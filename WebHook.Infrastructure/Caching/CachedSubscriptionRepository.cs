using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using WebHook.Domain.Entities;
using WebHook.Domain.Interfaces;
using WebHook.Infrastructure.Repositories;

namespace WebHook.Infrastructure.Caching
{
    public class CachedSubscriptionRepository(
        SubscriptionRepository innerRepository,
        IDistributedCache cache,
        ILogger<CachedSubscriptionRepository> logger) : ISubscriptionRepository
    {
        private readonly SubscriptionRepository _innerRepository = innerRepository;
        private readonly IDistributedCache _cache = cache;
        private readonly ILogger<CachedSubscriptionRepository> _logger = logger;

        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        private static string GetCacheKey(string eventType) => $"subs:active:{eventType.Trim().ToLowerInvariant()}";

        public async Task<List<Subscription>> GetActiveByEventTypeAsync(string eventType)
        {
            var cacheKey = GetCacheKey(eventType);

            try
            {
                var cachedJson = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedJson))
                {
                    var cachedList = JsonSerializer.Deserialize<List<Subscription>>(cachedJson);
                    if (cachedList is not null)
                    {
                        _logger.LogInformation("Redis Cache HIT -> {CacheKey} ({Count} abonelik)", cacheKey, cachedList.Count);
                        return cachedList;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis okuma hatası, SQL Server'a düşülüyor -> {CacheKey}", cacheKey);
            }

            _logger.LogInformation("Redis Cache MISS -> SQL Server'dan okunuyor: {CacheKey}", cacheKey);
            var subscriptions = await _innerRepository.GetActiveByEventTypeAsync(eventType);

            try
            {
                var serialized = JsonSerializer.Serialize(subscriptions);
                await _cache.SetStringAsync(cacheKey, serialized, CacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis yazma hatası -> {CacheKey}", cacheKey);
            }

            return subscriptions;
        }

        public async Task<Subscription> AddAsync(Subscription subscription)
        {
            var created = await _innerRepository.AddAsync(subscription);
            await InvalidateCacheAsync(subscription.EventType);
            return created;
        }

        public async Task<Subscription> UpdateAsync(Subscription subscription)
        {
            var updated = await _innerRepository.UpdateAsync(subscription);
            await InvalidateCacheAsync(subscription.EventType);
            return updated;
        }

        public Task<Subscription> GetByIdAsync(Guid id) => _innerRepository.GetByIdAsync(id);

        public Task<List<Subscription>> GetAllAsync() => _innerRepository.GetAllAsync();

        public Task<int> GetActiveCountAsync() => _innerRepository.GetActiveCountAsync();

        private async Task InvalidateCacheAsync(string eventType)
        {
            if (string.IsNullOrWhiteSpace(eventType)) return;

            var cacheKey = GetCacheKey(eventType);
            try
            {
                await _cache.RemoveAsync(cacheKey);
                _logger.LogInformation("Redis Cache temizlendi (Invalidated) -> {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis önbellek temizlenemedi -> {CacheKey}", cacheKey);
            }
        }
    }
}