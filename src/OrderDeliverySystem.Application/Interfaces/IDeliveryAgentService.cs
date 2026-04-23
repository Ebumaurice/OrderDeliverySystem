using OrderDeliverySystem.Application.DTOs.Agents;

namespace OrderDeliverySystem.Application.Interfaces;

public interface IDeliveryAgentService
{
    Task<AgentResponse> CreateAgentAsync(CreateAgentRequest request);
    Task<IEnumerable<AgentResponse>> GetAllAgentsAsync();
    Task<AgentResponse?> GetAgentByIdAsync(Guid id);
}