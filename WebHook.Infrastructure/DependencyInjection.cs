using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebHook.Application.Interfaces;
using WebHook.Domain.Interfaces;
using WebHook.Infrastructure.Data;
using WebHook.Infrastructure.Http;
using WebHook.Infrastructure.Repositories;
using WebHook.Infrastructure.Security;

namespace WebHook.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();
            services.AddScoped<IDeliveryLogRepository, DeliveryLogRepository>();
            services.AddSingleton<IHmacSignatureGenerator, HmacSignatureGenerator>();

            services.AddHttpClient<IWebhookSender, HttpWebhookSender>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            return services;
        }
    }
}
