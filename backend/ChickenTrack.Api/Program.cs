using ChickenTrack.Api.Endpoints;
using ChickenTrack.Api.Middleware;
using ChickenTrack.Application;
using ChickenTrack.Application.Interfaces;
using ChickenTrack.Infrastructure;
using ChickenTrack.Infrastructure.Data;
using ChickenTrack.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Application layer services (MediatR, AutoMapper, FluentValidation)
builder.Services.AddApplicationServices();

// Register Infrastructure layer services (DbContext, Repositories, Services)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure CORS for development only
if (builder.Environment.IsDevelopment())
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevelopmentPolicy");
}

app.UseHttpsRedirection();

// Authentication & Authorization middleware
app.UseAuthentication();

// Tenant resolution middleware - must come after UseAuthentication
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .WithOpenApi()
    .Produces<object>(200);

// MediatR test endpoint (can be removed after verification)
app.MapGet("/ping", async (IMediator mediator) =>
    {
        var result = await mediator.Send(new ChickenTrack.Application.Features.System.PingQuery());
        return Results.Ok(new { message = result });
    })
    .WithName("Ping")
    .WithOpenApi()
    .Produces<object>(200);

// Map webhook endpoints
app.MapWebhooksEndpoints();

// Map users endpoints
app.MapUsersEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
