namespace OrderDeliverySystem.Application.DTOs.Agents;

public class AgentResponse
{
    public Guid DeliveryAgentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ActiveOrderCount { get; set; }
}