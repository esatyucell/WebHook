using System;
using System.Collections.Generic;
using System.Text;

namespace WebHook.Application.DTOs
{
    public class SubscriptionCreateDto
    {
        public string Name { get; set; }
        public string EndpointUrl { get; set; }
        public string EventType { get; set; }
    }
}
