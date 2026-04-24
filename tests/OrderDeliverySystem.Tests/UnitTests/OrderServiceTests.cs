using Microsoft.EntityFrameworkCore;
using OrderDeliverySystem.Application.DTOs.Orders;
using OrderDeliverySystem.Application.Services;
using OrderDeliverySystem.Domain.Entities;
using OrderDeliverySystem.Infrastructure.Persistence;

namespace OrderDeliverySystem.Tests.UnitTests;

public class OrderServiceTests
{
    private static OrderDeliveryContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<OrderDeliveryContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OrderDeliveryContext(options);
    }

    // ─── Status Transition Tests ─────────────────────────────────────────────

    [Theory]
    [InlineData(OrderStatus.Created, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Assigned, OrderStatus.InTransit)]
    [InlineData(OrderStatus.Assigned, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.InTransit, OrderStatus.Delivered)]
    public async Task UpdateOrderStatus_ValidTransition_Succeeds(
        OrderStatus current, OrderStatus next)
    {
        // Arrange
        var context = CreateInMemoryContext();
        var agent = new DeliveryAgent
        {
            DeliveryAgentId = Guid.NewGuid(),
            Name = "Test Agent",
            IsActive = true
        };
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Abuja",
            Status = current,
            CreatedAt = DateTime.UtcNow,
            DeliveryAgentId = current == OrderStatus.Created ? null : agent.DeliveryAgentId
        };

        if (current != OrderStatus.Created)
            context.DeliveryAgents.Add(agent);

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.UpdateOrderStatusAsync(
            order.OrderId,
            new UpdateOrderStatusRequest { Status = next });

        // Assert
        Assert.Equal(next.ToString(), result.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Created, OrderStatus.Assigned)]
    [InlineData(OrderStatus.Created, OrderStatus.InTransit)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Assigned)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.InTransit)]
    [InlineData(OrderStatus.InTransit, OrderStatus.Created)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Assigned)]
    public async Task UpdateOrderStatus_InvalidTransition_ThrowsInvalidOperationException(
        OrderStatus current, OrderStatus next)
    {
        // Arrange
        var context = CreateInMemoryContext();
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Abuja",
            Status = current,
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateOrderStatusAsync(
                order.OrderId,
                new UpdateOrderStatusRequest { Status = next }));
    }

    [Fact]
    public async Task UpdateOrderStatus_OrderNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new OrderService(context);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateOrderStatusAsync(
                Guid.NewGuid(),
                new UpdateOrderStatusRequest { Status = OrderStatus.Assigned }));
    }

    // New test — setting Assigned via status update is blocked
    [Fact]
    public async Task UpdateOrderStatus_SetAssignedWithoutAgent_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Abuja",
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateOrderStatusAsync(
                order.OrderId,
                new UpdateOrderStatusRequest { Status = OrderStatus.Assigned }));
    }

    // ─── Assignment Tests ────────────────────────────────────────────────────

    [Fact]
    public async Task AssignAgent_ValidAssignment_Succeeds()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var agent = new DeliveryAgent
        {
            DeliveryAgentId = Guid.NewGuid(),
            Name = "Agent One",
            IsActive = true
        };
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Abuja",
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        context.DeliveryAgents.Add(agent);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.AssignAgentAsync(order.OrderId, agent.DeliveryAgentId);

        // Assert
        Assert.Equal(agent.DeliveryAgentId, result.DeliveryAgentId);
        Assert.Equal("Assigned", result.Status);
    }

    [Fact]
    public async Task AssignAgent_AgentAlreadyHasActiveOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var agent = new DeliveryAgent
        {
            DeliveryAgentId = Guid.NewGuid(),
            Name = "Busy Agent",
            IsActive = true
        };
        var existingOrder = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "Jane Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Ibadan",
            Status = OrderStatus.Assigned,
            DeliveryAgentId = agent.DeliveryAgentId,
            CreatedAt = DateTime.UtcNow
        };
        var newOrder = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PickupLocation = "Abuja",
            DropoffLocation = "Kano",
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        context.DeliveryAgents.Add(agent);
        context.Orders.AddRange(existingOrder, newOrder);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AssignAgentAsync(newOrder.OrderId, agent.DeliveryAgentId));
    }

    [Fact]
    public async Task AssignAgent_InactiveAgent_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var agent = new DeliveryAgent
        {
            DeliveryAgentId = Guid.NewGuid(),
            Name = "Inactive Agent",
            IsActive = false
        };
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Abuja",
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        context.DeliveryAgents.Add(agent);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AssignAgentAsync(order.OrderId, agent.DeliveryAgentId));
    }

    [Fact]
    public async Task AssignAgent_DeliveredOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var agent = new DeliveryAgent
        {
            DeliveryAgentId = Guid.NewGuid(),
            Name = "Agent One",
            IsActive = true
        };
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Abuja",
            Status = OrderStatus.Delivered,
            CreatedAt = DateTime.UtcNow
        };

        context.DeliveryAgents.Add(agent);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AssignAgentAsync(order.OrderId, agent.DeliveryAgentId));
    }
}