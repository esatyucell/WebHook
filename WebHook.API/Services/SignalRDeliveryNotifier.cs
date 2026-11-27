using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebHook.API.Hubs;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;

namespace WebHook.API.Services
{
    public class SignalRDeliveryNotifier(IHubContext<DeliveryHub> hubContext) : IDeliveryNotifier
    {
        private readonly IHubContext<DeliveryHub> _hubContext = hubContext;

        public async Task NotifyDeliveryAsync(DeliveryLogDto deliveryLogDto, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveDeliveryLog", deliveryLogDto, cancellationToken);
        }
    }
}