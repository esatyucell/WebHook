using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WebHook.Domain.Entities;

namespace WebHook.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<OutboxEvent> OutboxEvents { get; set; }
        public DbSet<DeliveryLog> DeliveryLog { get; set; }
    }
}
