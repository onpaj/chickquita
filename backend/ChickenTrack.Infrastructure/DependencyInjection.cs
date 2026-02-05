using ChickenTrack.Application.Interfaces;
using ChickenTrack.Infrastructure.Data;
using ChickenTrack.Infrastructure.Data.Interceptors;
using ChickenTrack.Infrastructure.Repositories;
using ChickenTrack.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
        // Register tenant interceptor
        services.AddScoped<TenantInterceptor>();

        // Register DbContext with Npgsql (PostgreSQL) and tenant interceptor
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var tenantInterceptor = serviceProvider.GetRequiredService<TenantInterceptor>();

            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .AddInterceptors(tenantInterceptor);
        });

        // Register repositories
        services.AddScoped<ITenantRepository, TenantRepository>();

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
