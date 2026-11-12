using System;
using System.Collections.Generic;
using System.Text;

namespace WebHook.Application.DTOs
{
    public class SubscriptionResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EndpointUrl { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string SecretKey { get; set; }
        public string EventType { get; set; }
    }
}
