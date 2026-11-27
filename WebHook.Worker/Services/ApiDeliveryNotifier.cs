using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;

namespace WebHook.Worker.Services
{
    public class ApiDeliveryNotifier(
        HttpClient httpClient,
        ILogger<ApiDeliveryNotifier> logger) : IDeliveryNotifier
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<ApiDeliveryNotifier> _logger = logger;

        public async Task NotifyDeliveryAsync(DeliveryLogDto deliveryLogDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/delivery-logs/notify",
                    deliveryLogDto,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "API canlı bildirim isteği başarısız oldu. Durum Kodu: {StatusCode}",
                        response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // API o an kapalı olsa bile Worker'ın webhook dağıtım döngüsü durmamalıdır.
                _logger.LogWarning(ex, "Canlı bildirim için WebHook.API sunucusuna ulaşılamadı.");
            }
        }
    }
}