using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;

namespace WebHook.API.Controllers
{
    [ApiController]
    [Route("api/delivery-logs")]
    public class DeliveryLogsController(
        IDeliveryLogService deliveryLogService,
        IDeliveryNotifier deliveryNotifier) : ControllerBase
    {
        private readonly IDeliveryLogService _deliveryLogService = deliveryLogService;
        private readonly IDeliveryNotifier _deliveryNotifier = deliveryNotifier;

        [HttpGet]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 50)
        {
            var logs = await _deliveryLogService.GetRecentLogsAsync(count);
            return Ok(logs);
        }

        [HttpPost("notify")]
        public async Task<IActionResult> Notify(
            [FromBody] DeliveryLogDto deliveryLogDto,
            CancellationToken cancellationToken)
        {
            await _deliveryNotifier.NotifyDeliveryAsync(deliveryLogDto, cancellationToken);
            return Ok();
        }
    }
}