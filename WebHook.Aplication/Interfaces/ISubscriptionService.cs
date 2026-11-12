using System;
using System.Collections.Generic;
using System.Text;
using WebHook.Application.DTOs;

namespace WebHook.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<SubscriptionResponseDto> CreateAsync(SubscriptionCreateDto subscriptionCreateDto);
        Task<SubscriptionResponseDto> GetByIdAsync(Guid id);
        Task<List<SubscriptionResponseDto>> GetAllAsync();
    }
}
