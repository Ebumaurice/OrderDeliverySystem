namespace OrderDeliverySystem.Application.DTOs.Orders;

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string PickupLocation { get; set; } = string.Empty;
    public string DropoffLocation { get; set; } = string.Empty;
}