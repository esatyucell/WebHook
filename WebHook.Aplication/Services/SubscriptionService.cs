using System;
using System.Collections.Generic;
using System.Text;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;
using WebHook.Domain.Entities;
using WebHook.Domain.Interfaces;
using WebHook.Application.Mappings;
using System.Linq;

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
            if (!Uri.TryCreate(subscriptionCreateDto.EndpointUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Geçersiz bir Webhook URL adresi girdiniz.");
            }

            // 1. Gelen veriyi veritabanı nesnesine çevir
            var subscription = subscriptionCreateDto.ToEntity();

            // 2. Kullanıcının giremeyeceği sistem bilgilerini ata
            subscription.Id = Guid.NewGuid();
            subscription.Status = true;
            subscription.CreatedAt = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;
            subscription.SecretKey = $"whsec_{Guid.NewGuid():N}";

            // 3. Veritabanına kaydet ve kaydın son halini al
            var createdSubscription = await _subscriptionRepository.AddAsync(subscription);

            // 4. Kullanıcıya döneceğimiz güvenli modele çevirip bitir
            return createdSubscription.ToResponseDto();
        }

        public async Task<SubscriptionResponseDto> GetByIdAsync(Guid id)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);
            if (subscription == null) {
                return null;
            }
            return subscription.ToResponseDto();
        }
        public async Task<List<SubscriptionResponseDto>> GetAllAsync()
        {
            var subscriptions = await _subscriptionRepository.GetAllAsync();
            return subscriptions.Select(s => s.ToResponseDto()).ToList();
            

        }
    }
}
