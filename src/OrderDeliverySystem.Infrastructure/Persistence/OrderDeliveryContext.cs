using Microsoft.EntityFrameworkCore;
using OrderDeliverySystem.Application.Interfaces;
using OrderDeliverySystem.Domain.Entities;

namespace OrderDeliverySystem.Infrastructure.Persistence;

public class OrderDeliveryContext : DbContext, IOrderDeliveryContext
{
    public OrderDeliveryContext(DbContextOptions<OrderDeliveryContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<DeliveryAgent> DeliveryAgents => Set<DeliveryAgent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.OrderId);

            entity.Property(o => o.CustomerName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(o => o.PickupLocation)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(o => o.DropoffLocation)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(o => o.Status)
                  .HasConversion<string>()
                  .IsRequired();

            entity.Property(o => o.CreatedAt)
                  .IsRequired();

            entity.HasOne(o => o.DeliveryAgent)
                  .WithMany(a => a.Orders)
                  .HasForeignKey(o => o.DeliveryAgentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DeliveryAgent>(entity =>
        {
            entity.HasKey(a => a.DeliveryAgentId);

            entity.Property(a => a.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.HasData(
                new DeliveryAgent
                {
                    DeliveryAgentId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    Name = "Yakubu Kachiro",
                    IsActive = true
                },
                new DeliveryAgent
                {
                    DeliveryAgentId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                    Name = "Fola Adeyemi",
                    IsActive = true
                },
                new DeliveryAgent
                {
                    DeliveryAgentId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                    Name = "Chinedu Okonkwo",
                    IsActive = true
                }
            );
        });
    }
}