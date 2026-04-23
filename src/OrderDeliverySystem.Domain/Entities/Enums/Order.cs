namespace OrderDeliverySystem.Domain.Entities;

public enum OrderStatus
{
    Created,
    Assigned,
    InTransit,
    Delivered,
    Cancelled
}
