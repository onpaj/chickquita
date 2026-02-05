using ChickenTrack.Api.Endpoints;
using ChickenTrack.Api.Middleware;
using ChickenTrack.Application;
using ChickenTrack.Application.Interfaces;
using ChickenTrack.Infrastructure;
using ChickenTrack.Infrastructure.Data;
using ChickenTrack.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.StaticFiles;
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

// Serve default files (index.html) before static files
app.UseDefaultFiles();

// Serve static files with cache headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static assets for 30 days in production
        if (!app.Environment.IsDevelopment())
        {
            var path = ctx.File.PhysicalPath;
            // Cache JS, CSS, images, fonts for 30 days
            if (path != null && (path.EndsWith(".js") || path.EndsWith(".css") ||
                path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg") ||
                path.EndsWith(".svg") || path.EndsWith(".gif") || path.EndsWith(".webp") ||
                path.EndsWith(".woff") || path.EndsWith(".woff2") || path.EndsWith(".ttf") ||
                path.EndsWith(".eot")))
            {
                ctx.Context.Response.Headers.CacheControl = "public,max-age=2592000"; // 30 days
            }
            else if (path != null && path.EndsWith(".html"))
            {
                // Don't cache HTML files (especially index.html)
                ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                ctx.Context.Response.Headers.Pragma = "no-cache";
                ctx.Context.Response.Headers.Expires = "0";
            }
        }
    }
});

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

// SPA fallback - serve index.html for all non-API routes
// This must come AFTER API endpoint mapping to ensure API routes take precedence
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
