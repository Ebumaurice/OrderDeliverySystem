using Microsoft.EntityFrameworkCore;
using OrderDeliverySystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderDeliverySystem.Infrastructure.Persistence
{
    public class OrderDeliveryContext : DbContext
    {
        public OrderDeliveryContext(DbContextOptions<OrderDeliveryContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<DeliveryAgent> DeliveryAgents => Set<DeliveryAgent>();
    }
}
