using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebHook.Application.DTOs;
using WebHook.Application.Interfaces;

namespace WebHook.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController(ISubscriptionService subscriptionService) : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService = subscriptionService;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionCreateDto subscriptionCreateDto)
        {
            try
            {
                var createdSubscription = await _subscriptionService.CreateAsync(subscriptionCreateDto);
                return CreatedAtAction(nameof(GetById), new { id = createdSubscription.Id }, createdSubscription);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var subscription = await _subscriptionService.GetByIdAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }
            return Ok(subscription);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var subscriptions = await _subscriptionService.GetAllAsync();
            return Ok(subscriptions);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _subscriptionService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}