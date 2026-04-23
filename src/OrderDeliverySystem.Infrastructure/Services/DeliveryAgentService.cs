using Microsoft.EntityFrameworkCore;
using OrderDeliverySystem.Application.DTOs.Agents;
using OrderDeliverySystem.Application.Interfaces;
using OrderDeliverySystem.Domain.Entities;
using OrderDeliverySystem.Infrastructure.Persistence;

namespace OrderDeliverySystem.Application.Services;

public class DeliveryAgentService : IDeliveryAgentService
{
    private readonly IOrderDeliveryContext _context;

    public DeliveryAgentService(IOrderDeliveryContext context)
    {
        _context = context;
    }

    public async Task<AgentResponse> CreateAgentAsync(CreateAgentRequest request)
    {
        var agent = new DeliveryAgent
        {
            DeliveryAgentId = Guid.NewGuid(),
            Name = request.Name,
            IsActive = true
        };

        _context.DeliveryAgents.Add(agent);
        await _context.SaveChangesAsync();

        return MapToResponse(agent, 0);
    }

    public async Task<IEnumerable<AgentResponse>> GetAllAgentsAsync()
    {
        var agents = await _context.DeliveryAgents
            .Include(a => a.Orders)
            .ToListAsync();

        return agents.Select(a =>
            MapToResponse(a, a.Orders.Count(o =>
                o.Status != OrderStatus.Delivered &&
                o.Status != OrderStatus.Cancelled)));
    }

    public async Task<AgentResponse?> GetAgentByIdAsync(Guid id)
    {
        var agent = await _context.DeliveryAgents
            .Include(a => a.Orders)
            .FirstOrDefaultAsync(a => a.DeliveryAgentId == id);

        if (agent is null) return null;

        var activeOrderCount = agent.Orders.Count(o =>
            o.Status != OrderStatus.Delivered &&
            o.Status != OrderStatus.Cancelled);

        return MapToResponse(agent, activeOrderCount);
    }

    private static AgentResponse MapToResponse(DeliveryAgent agent, int activeOrderCount) => new()
    {
        DeliveryAgentId = agent.DeliveryAgentId,
        Name = agent.Name,
        IsActive = agent.IsActive,
        ActiveOrderCount = activeOrderCount
    };
}