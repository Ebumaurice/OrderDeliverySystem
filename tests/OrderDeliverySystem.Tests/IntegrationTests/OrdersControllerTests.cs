using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using OrderDeliverySystem.Application.DTOs.Orders;
using OrderDeliverySystem.Domain.Entities;
using System.Net;
using System.Net.Http.Json;

namespace OrderDeliverySystem.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey"] = "test-api-key"
            });
        });

        builder.UseEnvironment("Testing");
    }
}

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _unauthenticatedClient;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Api-Key", "test-api-key");
        _unauthenticatedClient = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_Returns201()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "John Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Abuja"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(order);
        Assert.Equal("John Doe", order.CustomerName);
        Assert.Equal("Created", order.Status);
        Assert.Equal("Lagos", order.PickupLocation);
        Assert.Equal("Abuja", order.DropoffLocation);
    }

    [Fact]
    public async Task CreateOrder_WithoutApiKey_Returns401()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "John Doe",
            PickupLocation = "Lagos",
            DropoffLocation = "Abuja"
        };

        // Act
        var response = await _unauthenticatedClient.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_ReturnsPagedResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/orders?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResponse<OrderResponse>>();
        Assert.NotNull(result);
        Assert.True(result.PageSize == 10);
        Assert.True(result.Page == 1);
    }

    [Fact]
    public async Task GetOrderById_ExistingOrder_Returns200()
    {
        // Arrange — create an order first
        var createRequest = new CreateOrderRequest
        {
            CustomerName = "Jane Doe",
            PickupLocation = "Abuja",
            DropoffLocation = "Kano"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        // Act
        var response = await _client.GetAsync($"/api/orders/{createdOrder!.OrderId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(order);
        Assert.Equal("Jane Doe", order.CustomerName);
    }

    [Fact]
    public async Task GetOrderById_NonExistingOrder_Returns404()
    {
        // Act
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
}