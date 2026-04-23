using Microsoft.EntityFrameworkCore;
using OrderDeliverySystem.Domain.Entities;

namespace OrderDeliverySystem.Application.Interfaces;

public interface IOrderDeliveryContext
{
    DbSet<Order> Orders { get; }
    DbSet<DeliveryAgent> DeliveryAgents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}