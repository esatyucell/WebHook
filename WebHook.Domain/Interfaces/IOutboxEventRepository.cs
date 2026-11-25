using System;
using System.Collections.Generic;
using System.Text;
using WebHook.Domain.Entities;

namespace WebHook.Domain.Interfaces
{
    public interface IOutboxEventRepository
    {
        Task<OutboxEvent> AddAsync(OutboxEvent outboxEvent);
        Task<List<OutboxEvent>> GetUnprocessedEventsAsync();
        Task UpdateAsync(OutboxEvent outboxEvent);
        Task<int> GetUnprocessedCountAsync();
    }
}
