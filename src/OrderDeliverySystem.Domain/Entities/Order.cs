namespace OrderDeliverySystem.Domain.Entities;

public class Order
{
    public Guid OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string PickupLocation { get; set; } = string.Empty;

    public string DropoffLocation { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Created;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? DeliveryAgentId { get; set; }

    public DeliveryAgent? DeliveryAgent { get; set; }
}