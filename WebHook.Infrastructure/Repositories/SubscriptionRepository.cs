using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WebHook.Domain.Entities;
using WebHook.Domain.Interfaces;
using WebHook.Infrastructure.Data;

namespace WebHook.Infrastructure.Repositories
{
    public class SubscriptionRepository(AppDbContext context) : ISubscriptionRepository
    {
        private readonly AppDbContext _context = context;
        public async Task<Subscription> AddAsync(Subscription subscription)
        {
            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<List<Subscription>> GetActiveByEventTypeAsync(string eventType)
        {
            return await _context.Subscriptions.AsNoTracking().Where(x => x.Status && x.EventType == eventType).ToListAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.Subscriptions.CountAsync(x => x.Status);
        }

        public async Task<List<Subscription>> GetAllAsync()
        {
            return await _context.Subscriptions.AsNoTracking().ToListAsync();
            
        }

        public async Task<Subscription> GetByIdAsync(Guid id)
        {
            return await _context.Subscriptions.FindAsync(id);
            
        }

        public async Task<Subscription> UpdateAsync(Subscription subscription)
        {
             _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }
    }
}
