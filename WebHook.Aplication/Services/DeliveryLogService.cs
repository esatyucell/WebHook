using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;
using WebHook.Application.Mappings;
using WebHook.Domain.Interfaces;

namespace WebHook.Application.Services
{
    public class DeliveryLogService(
        IDeliveryLogRepository deliveryLogRepository,
        ISubscriptionRepository subscriptionRepository,
        IOutboxEventRepository outboxEventRepository) : IDeliveryLogService
    {
        private readonly IDeliveryLogRepository _deliveryLogRepository = deliveryLogRepository;
        private readonly ISubscriptionRepository _subscriptionRepository = subscriptionRepository;
        private readonly IOutboxEventRepository _outboxEventRepository = outboxEventRepository;

        public async Task<List<DeliveryLogDto>> GetRecentLogsAsync(int count = 50)
        {
            var logs = await _deliveryLogRepository.GetRecentAsync(count);

            if (logs.Count == 0)
            {
                return [];
            }

            var eventIds = logs.Select(x => x.OutboxEventId).Distinct();
            var events = await _outboxEventRepository.GetByIdsAsync(eventIds);
            var eventDict = events.ToDictionary(x => x.Id);

            var subscriptions = await _subscriptionRepository.GetAllAsync();
            var subDict = subscriptions.ToDictionary(x => x.Id);

            return logs
                .Select(log => log.ToDto(
                    eventDict.GetValueOrDefault(log.OutboxEventId),
                    subDict.GetValueOrDefault(log.SubscriptionId)))
                .ToList();
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var activeSubscriptions = await _subscriptionRepository.GetActiveCountAsync();
            var pendingEvents = await _outboxEventRepository.GetUnprocessedCountAsync();
            var (totalDeliveries, successCount) = await _deliveryLogRepository.GetStatsAsync();

            double successRate = totalDeliveries == 0
                ? 0
                : Math.Round((double)successCount / totalDeliveries * 100, 2);

            return new DashboardStatsDto
            {
                ActiveSubscriptions = activeSubscriptions,
                PendingEvents = pendingEvents,
                TotalDeliveries = totalDeliveries,
                SuccessRate = successRate
            };
        }
    }
}