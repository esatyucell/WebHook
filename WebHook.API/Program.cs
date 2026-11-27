using WebHook.API.Hubs;
using WebHook.API.Services;
using WebHook.Application;
using WebHook.Application.Interfaces;
using WebHook.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// SignalR & Real-time Notifier
builder.Services.AddSignalR();
builder.Services.AddScoped<IDeliveryNotifier, SignalRDeliveryNotifier>();

// CORS (SignalR AllowCredentials ile uyumlu)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("ReactClient");

app.UseAuthorization();

app.MapControllers();
app.MapHub<DeliveryHub>("/hubs/delivery");

app.Run();