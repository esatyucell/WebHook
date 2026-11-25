using System;
using System.Collections.Generic;
using System.Text;
using WebHook.Application.DTOs;

namespace WebHook.Application.Interfaces
{
    public interface IEventService
    {
        Task<Guid> PublishAsync(EventPublishDto eventPublishDto);
    }
}
