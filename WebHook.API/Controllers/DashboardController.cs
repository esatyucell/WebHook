using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebHook.Application.Interfaces;

namespace WebHook.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController(IDeliveryLogService deliveryLogService) : ControllerBase
    {
        private readonly IDeliveryLogService _deliveryLogService = deliveryLogService;

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _deliveryLogService.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}