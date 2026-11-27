using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebHook.Application.Interfaces;
using WebHook.Application.Services;

namespace WebHook.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IDeliveryLogService, DeliveryLogService>();
            return services;
            
        }
    }
}
