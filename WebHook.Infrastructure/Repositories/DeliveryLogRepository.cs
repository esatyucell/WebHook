using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebHook.Domain.Entities;
using WebHook.Domain.Interfaces;
using WebHook.Infrastructure.Data;

namespace WebHook.Infrastructure.Repositories
{
    public class DeliveryLogRepository(AppDbContext context) : IDeliveryLogRepository
    {
        private readonly AppDbContext _context = context;

        public async Task AddAsync(DeliveryLog deliveryLog)
        {
            await _context.DeliveryLog.AddAsync(deliveryLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DeliveryLog>> GetRecentAsync(int count = 50)
        {
            return await _context.DeliveryLog
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<(int TotalCount, int SuccessCount)> GetStatsAsync()
        {
            var totalCount = await _context.DeliveryLog.CountAsync();
            var successCount = await _context.DeliveryLog.CountAsync(x => x.IsSuccessful);
            return (totalCount, successCount);
        }
    }
}