namespace OrderDeliverySystem.Domain.Entities;

public class DeliveryAgent
{
    public Guid DeliveryAgentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}