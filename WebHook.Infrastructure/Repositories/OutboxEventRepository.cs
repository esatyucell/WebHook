using System;
using System.Collections.Generic;
using WebHook.Domain.Entities;
using WebHook.Domain.Interfaces;
using WebHook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace WebHook.Infrastructure.Repositories
{
    public class OutboxEventRepository(AppDbContext context) : IOutboxEventRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<OutboxEvent> AddAsync(OutboxEvent outboxEvent)
        {
            await _context.OutboxEvents.AddAsync(outboxEvent);
            await _context.SaveChangesAsync();
            return outboxEvent;
        }

        public async Task<List<OutboxEvent>> GetUnprocessedEventsAsync()
        {
            return await _context.OutboxEvents.Where(x => x.ProcessedAt == null).OrderBy(x => x.CreatedAt).Take(50).ToListAsync();
            
        }
    }
}
