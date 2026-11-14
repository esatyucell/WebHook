using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;
using WebHook.Application.Mappings;
using WebHook.Domain.Interfaces;

namespace WebHook.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<SubscriptionResponseDto> CreateAsync(SubscriptionCreateDto subscriptionCreateDto)
        {
            ArgumentNullException.ThrowIfNull(subscriptionCreateDto);

            if (!Uri.TryCreate(subscriptionCreateDto.EndpointUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Geçersiz bir Webhook URL adresi girdiniz.");
            }

            var subscription = subscriptionCreateDto.ToEntity();
            var now = DateTime.UtcNow;

            subscription.Id = Guid.NewGuid();
            subscription.Status = true;
            subscription.CreatedAt = now;
            subscription.UpdatedAt = now;
            subscription.SecretKey = GenerateSecretKey();

            var createdSubscription = await _subscriptionRepository.AddAsync(subscription);

            return createdSubscription.ToResponseDto();
        }

        public async Task<SubscriptionResponseDto> GetByIdAsync(Guid id)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);
            if (subscription == null)
            {
                return null;
            }

            return subscription.ToResponseDto();
        }

        public async Task<List<SubscriptionResponseDto>> GetAllAsync()
        {
            var subscriptions = await _subscriptionRepository.GetAllAsync();
            return subscriptions.Select(s => s.ToResponseDto()).ToList();
        }

        private static string GenerateSecretKey()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(32);
            return $"whsec_{Convert.ToHexString(randomBytes).ToLowerInvariant()}";
        }
    }
}