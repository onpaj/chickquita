using Chickquita.Application.Interfaces;
using Chickquita.Infrastructure.Data;
using Chickquita.Infrastructure.Data.Interceptors;
using Chickquita.Infrastructure.Repositories;
using Chickquita.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Chickquita.Infrastructure;

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
        // Register tenant interceptor
        services.AddScoped<TenantInterceptor>();

        // Determine which connection string to use based on environment
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        var connectionStringKey = environment == "E2ETests" ? "E2ETests" : "DefaultConnection";
        var connectionString = configuration.GetConnectionString(connectionStringKey);

        // Register DbContext with Npgsql (PostgreSQL) and tenant interceptor
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var tenantInterceptor = serviceProvider.GetRequiredService<TenantInterceptor>();

            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .AddInterceptors(tenantInterceptor);
        });

        // Register repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ICoopRepository, CoopRepository>();
        services.AddScoped<IFlockRepository, FlockRepository>();

        // Register webhook validation service
        services.AddScoped<IClerkWebhookValidator, ClerkWebhookValidator>();

        // Register HttpContextAccessor (required for CurrentUserService)
        services.AddHttpContextAccessor();

        // Register current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register tenant service
        services.AddScoped<ITenantService, TenantService>();

        // Configure JWT Bearer Authentication
        var clerkAuthority = configuration["Clerk:Authority"];
        var clerkAudience = configuration["Clerk:Audience"];

        if (!string.IsNullOrEmpty(clerkAuthority))
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = clerkAuthority;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = !string.IsNullOrEmpty(clerkAudience),
                        ValidAudience = clerkAudience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    // Map the Clerk "sub" claim to ClaimTypes.NameIdentifier
                    options.MapInboundClaims = false;
                });

            services.AddAuthorization();
        }

        return services;
    }
}
