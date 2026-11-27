using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebHook.Application.Interfaces;
using WebHook.Application.Mappings;
using WebHook.Domain.Entities;
using WebHook.Domain.Interfaces;
using WebHook.Infrastructure.Messaging;

namespace WebHook.Worker
{
    public class OutboxProcessorWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OutboxProcessorWorker> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<OutboxProcessorWorker> _logger = logger;
        private readonly ConcurrentDictionary<Guid, byte> _inFlightEvents = new();

        private const int MaxRetries = 3;
        private static readonly TimeSpan FallbackSweepInterval = TimeSpan.FromSeconds(30);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxProcessorWorker başlatıldı (RabbitMQ Anlık Dinleyici + Outbox Güvenlik Ağı).");

            var consumerTask = RunRabbitMqConsumerAsync(stoppingToken);
            var sweepTask = RunFallbackSweepAsync(stoppingToken);

            await Task.WhenAll(consumerTask, sweepTask);
        }

        private async Task RunRabbitMqConsumerAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                        Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                        UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                        Password = _configuration["RabbitMQ:Password"] ?? "guest",
                        ClientProvidedName = "WebHook.Worker.Consumer"
                    };

                    await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                    await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                    await channel.QueueDeclareAsync(
                        queue: RabbitMqOutboxPublisher.QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: stoppingToken);

                    await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += async (_, ea) =>
                    {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                        if (Guid.TryParse(message, out var outboxEventId))
                        {
                            _logger.LogInformation("RabbitMQ mesajı yakalandı -> OutboxEventId: {OutboxEventId}", outboxEventId);
                            await ProcessSingleEventByIdAsync(outboxEventId, stoppingToken);
                        }

                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    };

                    await channel.BasicConsumeAsync(
                        queue: RabbitMqOutboxPublisher.QueueName,
                        autoAck: false,
                        consumer: consumer,
                        cancellationToken: stoppingToken);

                    _logger.LogInformation("RabbitMQ kuyruğu dinleniyor: {QueueName}", RabbitMqOutboxPublisher.QueueName);

                    // Bağlantı açık kaldığı sürece iptal sinyaline kadar bekle
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ bağlantısı kesildi, 5 saniye içinde yeniden bağlanılacak...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task RunFallbackSweepAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingEventsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox güvenlik ağı taramasında hata oluştu.");
                }

                await Task.Delay(FallbackSweepInterval, stoppingToken);
            }
        }

        private async Task ProcessSingleEventByIdAsync(Guid outboxEventId, CancellationToken cancellationToken)
        {
            if (!_inFlightEvents.TryAdd(outboxEventId, 0))
                return;

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();

                var events = await outboxRepo.GetByIdsAsync([outboxEventId]);
                var outboxEvent = events.FirstOrDefault();

                if (outboxEvent is null || outboxEvent.ProcessedAt is not null)
                    return;

                await DeliverEventAsync(scope.ServiceProvider, outboxRepo, outboxEvent, cancellationToken);
            }
            finally
            {
                _inFlightEvents.TryRemove(outboxEventId, out _);
            }
        }

        private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();

            var pendingEvents = await outboxRepo.GetUnprocessedEventsAsync();
            if (pendingEvents.Count == 0)
                return;

            foreach (var outboxEvent in pendingEvents)
            {
                if (!_inFlightEvents.TryAdd(outboxEvent.Id, 0))
                    continue;

                try
                {
                    await DeliverEventAsync(scope.ServiceProvider, outboxRepo, outboxEvent, cancellationToken);
                }
                finally
                {
                    _inFlightEvents.TryRemove(outboxEvent.Id, out _);
                }
            }
        }

        private async Task DeliverEventAsync(
            IServiceProvider serviceProvider,
            IOutboxEventRepository outboxRepo,
            OutboxEvent outboxEvent,
            CancellationToken cancellationToken)
        {
            var subscriptionRepo = serviceProvider.GetRequiredService<ISubscriptionRepository>();
            var deliveryLogRepo = serviceProvider.GetRequiredService<IDeliveryLogRepository>();
            var hmacGenerator = serviceProvider.GetRequiredService<IHmacSignatureGenerator>();
            var webhookSender = serviceProvider.GetRequiredService<IWebhookSender>();
            var deliveryNotifier = serviceProvider.GetRequiredService<IDeliveryNotifier>();

            var subscribers = await subscriptionRepo.GetActiveByEventTypeAsync(outboxEvent.EventType);

            foreach (var subscription in subscribers)
            {
                await DeliverWithRetryAsync(
                    outboxEvent,
                    subscription,
                    hmacGenerator,
                    webhookSender,
                    deliveryLogRepo,
                    deliveryNotifier,
                    cancellationToken);
            }

            outboxEvent.ProcessedAt = DateTime.UtcNow;
            await outboxRepo.UpdateAsync(outboxEvent);
        }

        private async Task DeliverWithRetryAsync(
            OutboxEvent outboxEvent,
            Subscription subscription,
            IHmacSignatureGenerator hmacGenerator,
            IWebhookSender webhookSender,
            IDeliveryLogRepository deliveryLogRepo,
            IDeliveryNotifier deliveryNotifier,
            CancellationToken cancellationToken)
        {
            string signature = hmacGenerator.GenerateSignature(outboxEvent.Payload, subscription.SecretKey);

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                var (isSuccess, statusCode, errorMessage) = await webhookSender.SendAsync(
                    subscription.EndpointUrl,
                    outboxEvent.EventType,
                    outboxEvent.Payload,
                    signature,
                    cancellationToken);

                var log = new DeliveryLog
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    OutboxEventId = outboxEvent.Id,
                    IsSuccessful = isSuccess,
                    StatusCode = statusCode,
                    ErrorMessage = errorMessage ?? string.Empty,
                    AttemptNumber = attempt,
                    CreatedAt = DateTime.UtcNow
                };

                await deliveryLogRepo.AddAsync(log);

                var logDto = log.ToDto(outboxEvent, subscription);
                await deliveryNotifier.NotifyDeliveryAsync(logDto, cancellationToken);

                if (isSuccess)
                {
                    _logger.LogInformation(
                        "Teslimat başarılı -> Event: {EventType}, Hedef: {Url}, Deneme: {Attempt}",
                        outboxEvent.EventType,
                        subscription.EndpointUrl,
                        attempt);
                    return;
                }

                _logger.LogWarning(
                    "Teslimat başarısız -> Event: {EventType}, Hedef: {Url}, Deneme: {Attempt}/{Max}, Hata: {Error}",
                    outboxEvent.EventType,
                    subscription.EndpointUrl,
                    attempt,
                    MaxRetries,
                    errorMessage);

                if (attempt < MaxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
    }
}