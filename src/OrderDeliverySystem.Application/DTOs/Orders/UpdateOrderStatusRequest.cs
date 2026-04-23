using OrderDeliverySystem.Domain.Entities;

namespace OrderDeliverySystem.Application.DTOs.Orders;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}