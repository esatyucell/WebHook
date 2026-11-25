using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebHook.Domain.Entities;

namespace WebHook.Domain.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<Subscription> AddAsync(Subscription subscription);
        Task<Subscription> GetByIdAsync(Guid id);
        Task<List<Subscription>> GetAllAsync();
        Task<Subscription> UpdateAsync(Subscription subscription);

        Task<List<Subscription>> GetActiveByEventTypeAsync(string eventType);

        Task<int> GetActiveCountAsync();
    }
}