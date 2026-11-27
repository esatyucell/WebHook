using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebHook.Application.Interfaces;
using WebHook.Domain.Interfaces;
using WebHook.Infrastructure.Caching;
using WebHook.Infrastructure.Data;
using WebHook.Infrastructure.Http;
using WebHook.Infrastructure.Messaging;
using WebHook.Infrastructure.Repositories;
using WebHook.Infrastructure.Security;

namespace WebHook.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Redis Distributed Cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                options.InstanceName = "WebHookEngine:";
            });

            // Decorator Pattern: SubscriptionRepository -> CachedSubscriptionRepository (Redis)
            services.AddScoped<SubscriptionRepository>();
            services.AddScoped<ISubscriptionRepository, CachedSubscriptionRepository>();

            services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();
            services.AddScoped<IDeliveryLogRepository, DeliveryLogRepository>();
            services.AddSingleton<IHmacSignatureGenerator, HmacSignatureGenerator>();
            services.AddSingleton<IOutboxMessagePublisher, RabbitMqOutboxPublisher>();

            services.AddHttpClient<IWebhookSender, HttpWebhookSender>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            return services;
        }
    }
}