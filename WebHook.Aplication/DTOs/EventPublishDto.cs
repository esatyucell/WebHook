using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace WebHook.Application.DTOs
{
    public class EventPublishDto
    {
        public string EventType { get; set; } = string.Empty;
        public JsonElement Payload { get; set; } 

    }
}
