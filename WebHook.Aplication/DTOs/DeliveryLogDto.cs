using System;
using System.Collections.Generic;
using System.Text;

namespace WebHook.Application.DTOs
{
    public class DeliveryLogDto
    {
        public Guid Id { get; set; }
        public Guid OutboxEventId { get; set; }
        public Guid SubscriptionId { get; set; }
        public string EventType { get; set; }
        public string EndpointUrl { get; set; }
        public string Payload { get; set; }
        public int? HttpStatusCode { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public int AttemptCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
