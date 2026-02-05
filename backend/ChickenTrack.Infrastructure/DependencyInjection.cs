using ChickenTrack.Application.Interfaces;
using ChickenTrack.Infrastructure.Data;
using ChickenTrack.Infrastructure.Repositories;
using ChickenTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChickenTrack.Infrastructure;

/// <summary>
/// Configures Infrastructure layer services and dependencies
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services into the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext with Npgsql (PostgreSQL)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register repositories
        services.AddScoped<ITenantRepository, TenantRepository>();

        // Register webhook validation service
        services.AddScoped<IClerkWebhookValidator, ClerkWebhookValidator>();

        return services;
    }
}
