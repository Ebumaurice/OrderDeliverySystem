using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderDeliverySystem.Application.DTOs.Agents;
using OrderDeliverySystem.Application.Interfaces;

namespace OrderDeliverySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IDeliveryAgentService _agentService;

    public AgentsController(IDeliveryAgentService agentService)
    {
        _agentService = agentService;
    }

    // GET /api/agents
    [HttpGet]
    public async Task<IActionResult> GetAgents()
    {
        var agents = await _agentService.GetAllAgentsAsync();
        return Ok(agents);
    }

    // GET /api/agents/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAgent(Guid id)
    {
        var agent = await _agentService.GetAgentByIdAsync(id);
        return agent is null ? NotFound(new { Message = $"Agent {id} not found." }) : Ok(agent);
    }

    // POST /api/agents
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest request)
    {
        var agent = await _agentService.CreateAgentAsync(request);
        return CreatedAtAction(nameof(GetAgent), new { id = agent.DeliveryAgentId }, agent);
    }
}