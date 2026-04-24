using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderDeliverySystem.Api.Authentication;
using OrderDeliverySystem.API.Middleware;
using OrderDeliverySystem.Application.Interfaces;
using OrderDeliverySystem.Application.Services;
using OrderDeliverySystem.Infrastructure.Persistence;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter your API Key",
        Scheme = "ApiKey"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "ApiKey",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKey"
            },
            Array.Empty<string>()
        }
    });
});

// Infrastructure — use InMemory for Testing environment, SQL Server otherwise
var isTestingEnvironment = builder.Environment.EnvironmentName == "Testing";

builder.Services.AddDbContext<OrderDeliveryContext>(options =>
{
    if (isTestingEnvironment)
        options.UseInMemoryDatabase("TestDb");
    else
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IOrderDeliveryContext>(provider =>
    provider.GetRequiredService<OrderDeliveryContext>());

// Application services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDeliveryAgentService, DeliveryAgentService>();

// Authentication
builder.Services.AddAuthentication(ApiKeyConstants.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment() || isTestingEnvironment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }