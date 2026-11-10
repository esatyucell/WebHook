using System;
using System.Collections.Generic;
using System.Text;

namespace WebHook.Domain.Entities
{
    public class OutboxEvent
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
