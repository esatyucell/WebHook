using System;
using System.Collections.Generic;
using System.Text;
using WebHook.Domain.Entities;

namespace WebHook.Domain.Interfaces
{
    public interface IDeliveryLogRepository
    {
        Task AddAsync(DeliveryLog deliveryLog);
        Task<List<DeliveryLog>> GetRecentAsync(int count = 50);
        Task<(int TotalCount, int SuccessCount)> GetStatsAsync();
    }
}
