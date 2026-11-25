using System;
using System.Collections.Generic;
using System.Text;

namespace WebHook.Application.DTOs
{
    public class DashboardStatsDto
    {
        public int ActiveSubscriptions { get; set; }
        public int PendingEvents { get; set; }
        public int TotalDeliveries { get; set; }
        public double SuccessRate { get; set; }
    }
}
