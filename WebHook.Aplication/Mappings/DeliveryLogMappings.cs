using WebHook.Application.DTOs;
using WebHook.Domain.Entities;

namespace WebHook.Application.Mappings
{
    public static class DeliveryLogMappings
    {
        public static DeliveryLogDto ToDto(
            this DeliveryLog log,
            OutboxEvent? outboxEvent = null,
            Subscription? subscription = null)
        {
            return new DeliveryLogDto
            {
                Id = log.Id,
                OutboxEventId = log.OutboxEventId,
                SubscriptionId = log.SubscriptionId,
                EventType = outboxEvent?.EventType ?? string.Empty,
                EndpointUrl = subscription?.EndpointUrl ?? string.Empty,
                Payload = outboxEvent?.Payload ?? string.Empty,
                HttpStatusCode = log.StatusCode,
                IsSuccessful = log.IsSuccessful,
                ErrorMessage = log.ErrorMessage,
                AttemptCount = log.AttemptNumber,
                CreatedAt = log.CreatedAt
            };
        }
    }
}