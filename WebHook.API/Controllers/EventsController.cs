using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;

namespace WebHook.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(IEventService _eventService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Publish([FromBody] EventPublishDto eventPublishDto)
        {
            try
            {
                var eventId = await _eventService.PublishAsync(eventPublishDto);
                return Accepted(new { id = eventId, status = "Queued" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
