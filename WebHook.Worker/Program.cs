using WebHook.Application;
using WebHook.Application.Interfaces;
using WebHook.Infrastructure;
using WebHook.Worker;
using WebHook.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Clean Architecture Katmanları
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API Canlı Bildirim İstemcisi (SignalR Köprüsü)
builder.Services.AddHttpClient<IDeliveryNotifier, ApiDeliveryNotifier>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException("ApiSettings:BaseUrl ayarı bulunamadı.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Arka Plan Outbox İşçisi
builder.Services.AddHostedService<OutboxProcessorWorker>();

var host = builder.Build();
host.Run();