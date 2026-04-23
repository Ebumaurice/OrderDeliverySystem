using OrderDeliverySystem.Application.DTOs.Orders;
using OrderDeliverySystem.Domain.Entities;

namespace OrderDeliverySystem.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<PagedResponse<OrderResponse>> GetOrdersAsync(OrderStatus? status, int page, int pageSize);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request);
    Task<OrderResponse> AssignAgentAsync(Guid orderId, Guid agentId);
}