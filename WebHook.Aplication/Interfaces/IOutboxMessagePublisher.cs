using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebHook.Application.Interfaces
{
    public interface IOutboxMessagePublisher
    {
        Task PublishOutboxEventAsync(Guid outboxEventId, CancellationToken cancellationToken = default);
    }
}