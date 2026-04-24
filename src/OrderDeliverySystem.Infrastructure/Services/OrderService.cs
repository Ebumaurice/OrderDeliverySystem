using Microsoft.EntityFrameworkCore;
using OrderDeliverySystem.Application.DTOs.Orders;
using OrderDeliverySystem.Application.Interfaces;
using OrderDeliverySystem.Domain.Entities;

namespace OrderDeliverySystem.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderDeliveryContext _context;

    public OrderService(IOrderDeliveryContext context)
    {
        _context = context;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            PickupLocation = request.PickupLocation,
            DropoffLocation = request.DropoffLocation,
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return MapToResponse(order);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.DeliveryAgent)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        return order is null ? null : MapToResponse(order);
    }

    public async Task<PagedResponse<OrderResponse>> GetOrdersAsync(
        OrderStatus? status, int page, int pageSize)
    {
        // Fix pagination
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

        var query = _context.Orders
            .Include(o => o.DeliveryAgent)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var totalCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<OrderResponse>
        {
            Data = orders.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(
        Guid id, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.DeliveryAgent)
            .FirstOrDefaultAsync(o => o.OrderId == id)
            ?? throw new KeyNotFoundException($"Order {id} not found.");

        ValidateStatusTransition(order.Status, request.Status);

        order.Status = request.Status;

        await _context.SaveChangesAsync();

        return MapToResponse(order);
    }

    public async Task<OrderResponse> AssignAgentAsync(Guid orderId, Guid agentId)
    {
        var order = await _context.Orders
            .Include(o => o.DeliveryAgent)
            .FirstOrDefaultAsync(o => o.OrderId == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        // Block invalid states
        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot assign agent to order in '{order.Status}' state.");
        }

        if (order.DeliveryAgentId == agentId)
        {
            throw new InvalidOperationException(
                "Order is already assigned to this agent.");
        }

        var agent = await _context.DeliveryAgents
            .FirstOrDefaultAsync(a => a.DeliveryAgentId == agentId)
            ?? throw new KeyNotFoundException($"Agent {agentId} not found.");

        if (!agent.IsActive)
        {
            throw new InvalidOperationException(
                $"Agent '{agent.Name}' is not active.");
        }

        // Prevent multiple active orders
        var hasActiveOrder = await _context.Orders.AnyAsync(o =>
            o.DeliveryAgentId == agentId &&
            o.Status != OrderStatus.Delivered &&
            o.Status != OrderStatus.Cancelled &&
            o.OrderId != orderId);

        if (hasActiveOrder)
        {
            throw new InvalidOperationException(
                $"Agent '{agent.Name}' already has an active order.");
        }

        // Reassignment rule
        if (order.DeliveryAgentId != null && order.DeliveryAgentId != agentId)
        {
            if (order.Status != OrderStatus.Assigned)
            {
                throw new InvalidOperationException(
                    "Reassignment is only allowed when order is in Assigned state.");
            }
        }

        order.DeliveryAgentId = agentId;

        if (order.Status == OrderStatus.Created)
        {
            order.Status = OrderStatus.Assigned;
        }

        await _context.SaveChangesAsync();

        order.DeliveryAgent = agent;

        return MapToResponse(order);
    }

    private static void ValidateStatusTransition(OrderStatus current, OrderStatus next)
    {
        var allowed = new Dictionary<OrderStatus, IEnumerable<OrderStatus>>
        {
            [OrderStatus.Created] = new[] { OrderStatus.Assigned, OrderStatus.Cancelled },
            [OrderStatus.Assigned] = new[] { OrderStatus.InTransit, OrderStatus.Cancelled },
            [OrderStatus.InTransit] = new[] { OrderStatus.Delivered },
            [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
        };

        if (!allowed[current].Contains(next))
        {
            throw new InvalidOperationException(
                $"Invalid status transition from '{current}' to '{next}'.");
        }
    }

    private static OrderResponse MapToResponse(Order order) => new()
    {
        OrderId = order.OrderId,
        CustomerName = order.CustomerName,
        PickupLocation = order.PickupLocation,
        DropoffLocation = order.DropoffLocation,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        DeliveryAgentId = order.DeliveryAgentId,
        DeliveryAgentName = order.DeliveryAgent?.Name
    };
}