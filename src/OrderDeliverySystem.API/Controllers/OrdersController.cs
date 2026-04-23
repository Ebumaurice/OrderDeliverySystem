using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderDeliverySystem.Application.DTOs.Orders;
using OrderDeliverySystem.Application.DTOs.Assignments;
using OrderDeliverySystem.Application.Interfaces;
using OrderDeliverySystem.Domain.Entities;

namespace OrderDeliverySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET /api/orders?status=Assigned&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetOrdersAsync(status, page, pageSize);
        return Ok(result);
    }

    // GET /api/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return order is null ? NotFound(new { Message = $"Order {id} not found." }) : Ok(order);
    }

    // POST /api/orders
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = await _orderService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
    }

    // PATCH /api/orders/{id}/status
    [HttpPatch("{id:guid}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, request);
        return Ok(order);
    }

    // POST /api/orders/{id}/assign
    [HttpPost("{id:guid}/assign")]
    [Authorize]
    public async Task<IActionResult> AssignAgent(Guid id, [FromBody] AssignAgentRequest request)
    {
        var order = await _orderService.AssignAgentAsync(id, request.DeliveryAgentId);
        return Ok(order);
    }
}