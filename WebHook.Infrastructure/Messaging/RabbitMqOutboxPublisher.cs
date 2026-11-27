using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using WebHook.Application.Interfaces;

namespace WebHook.Infrastructure.Messaging
{
    public sealed class RabbitMqOutboxPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqOutboxPublisher> logger) : IOutboxMessagePublisher, IAsyncDisposable
    {
        public const string QueueName = "webhook.outbox";

        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<RabbitMqOutboxPublisher> _logger = logger;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private IConnection? _connection;

        public async Task PublishOutboxEventAsync(Guid outboxEventId, CancellationToken cancellationToken = default)
        {
            try
            {
                var connection = await GetOrCreateConnectionAsync(cancellationToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                await channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                var body = Encoding.UTF8.GetBytes(outboxEventId.ToString());
                var properties = new BasicProperties { Persistent = true };

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: QueueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Olay RabbitMQ kuyruğuna gönderildi -> OutboxEventId: {OutboxEventId}", outboxEventId);
            }
            catch (Exception ex)
            {
                // Kayıt zaten SQL Server Outbox tablosunda güvende; Worker güvenlik ağı devralacaktır.
                _logger.LogWarning(ex, "RabbitMQ'ya mesaj gönderilemedi (Outbox yedek akışı devrede) -> {OutboxEventId}", outboxEventId);
            }
        }

        private async Task<IConnection> GetOrCreateConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (_connection is { IsOpen: true })
                    return _connection;

                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                    UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest",
                    ClientProvidedName = "WebHook.API.Publisher"
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                return _connection;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
            _connectionLock.Dispose();
        }
    }
}