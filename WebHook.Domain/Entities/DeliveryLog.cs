using System;
using System.Collections.Generic;
using System.Text;

namespace WebHook.Domain.Entities
{
    public class DeliveryLog
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid OutboxEventId { get; set; }
        public bool IsSuccessful { get; set; }
        public int? StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
