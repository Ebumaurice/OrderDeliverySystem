using OrderDeliverySystem.Domain.Entities;

namespace OrderDeliverySystem.Application.DTOs.Orders;

public class OrderResponse
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PickupLocation { get; set; } = string.Empty;
    public string DropoffLocation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? DeliveryAgentId { get; set; }
    public string? DeliveryAgentName { get; set; }
}