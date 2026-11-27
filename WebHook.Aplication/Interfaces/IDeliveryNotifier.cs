using System.Threading;
using System.Threading.Tasks;
using WebHook.Application.DTOs;

namespace WebHook.Application.Interfaces
{
    public interface IDeliveryNotifier
    {
        Task NotifyDeliveryAsync(DeliveryLogDto deliveryLogDto, CancellationToken cancellationToken = default);

    }
}