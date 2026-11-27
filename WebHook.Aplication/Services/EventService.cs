using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;
using WebHook.Domain.Entities;
using WebHook.Domain.Interfaces;

namespace WebHook.Application.Services
{
    public class EventService(
        IOutboxEventRepository outboxEventRepository,
        IOutboxMessagePublisher messagePublisher) : IEventService
    {
        private readonly IOutboxEventRepository _outboxEventRepository = outboxEventRepository;
        private readonly IOutboxMessagePublisher _messagePublisher = messagePublisher;

        public async Task<Guid> PublishAsync(EventPublishDto eventPublishDto)
        {
            ArgumentNullException.ThrowIfNull(eventPublishDto);

            if (string.IsNullOrWhiteSpace(eventPublishDto.EventType))
            {
                throw new ArgumentException("EventType alanı boş olamaz.");
            }

            if (eventPublishDto.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                throw new ArgumentException("Payload alanı boş olamaz.");
            }

            var outboxEvent = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = eventPublishDto.EventType.Trim(),
                Payload = eventPublishDto.Payload.GetRawText(),
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            };

            // 1. Önce SQL Server Outbox tablosuna kalıcı olarak yaz (veri kaybı garantisi)
            await _outboxEventRepository.AddAsync(outboxEvent);

            // 2. Worker'ı milisaniyeler içinde uyandırmak için RabbitMQ kuyruğuna haber ver
            await _messagePublisher.PublishOutboxEventAsync(outboxEvent.Id);

            return outboxEvent.Id;
        }
    }
}