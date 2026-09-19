using Chickquita.Application.Interfaces;
using Chickquita.Infrastructure.Data;
using Chickquita.Infrastructure.Data.Interceptors;
using Chickquita.Infrastructure.Health;
using Chickquita.Infrastructure.Repositories;
using Chickquita.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Chickquita.Infrastructure;

/// <summary>
/// Configures Infrastructure layer services and dependencies
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Log category for authentication failures, kept outside the Microsoft.AspNetCore
    /// hierarchy so production log-level filtering does not hide them.
    /// </summary>
    private const string AuthenticationLoggerCategory = "Chickquita.Authentication";

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

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ICoopRepository, CoopRepository>();
        services.AddScoped<IFlockRepository, FlockRepository>();
        services.AddScoped<IFlockHistoryRepository, FlockHistoryRepository>();
        services.AddScoped<IStatisticsRepository, StatisticsRepository>();
        services.AddScoped<IDailyRecordRepository, DailyRecordRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IEggSaleRepository, EggSaleRepository>();

        // Register webhook validation service
        services.AddScoped<IClerkWebhookValidator, ClerkWebhookValidator>();

        // Register HttpContextAccessor (required for CurrentUserService)
        services.AddHttpContextAccessor();

        // Register current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Configure JWT Bearer Authentication — fail fast if required config is absent
        var clerkAuthority = configuration["Clerk:Authority"]
            ?? throw new InvalidOperationException(
                "Clerk:Authority is required but not configured. " +
                "Set the Clerk__Authority environment variable.");

        // Clerk issues tokens with an `iss` that has no trailing slash; normalise so a
        // trailing slash in configuration cannot break issuer matching.
        clerkAuthority = clerkAuthority.TrimEnd('/');

        var clerkAudience = configuration["Clerk:Audience"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = clerkAuthority;

                // A stale or empty key set must trigger a metadata refresh instead of
                // rejecting every request until the process is restarted.
                options.RefreshOnIssuerKeyNotFound = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    // Pinned explicitly so issuer validation does not depend on the discovery
                    // document being reachable.
                    ValidIssuer = clerkAuthority,
                    ValidateAudience = !string.IsNullOrEmpty(clerkAudience),
                    ValidAudience = clerkAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // Map the Clerk "sub" claim to ClaimTypes.NameIdentifier
                options.MapInboundClaims = false;

                // Log under our own category: Microsoft.AspNetCore is pinned to Warning in
                // production, which otherwise hides the reason behind every 401.
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger(AuthenticationLoggerCategory);

                        logger.LogWarning(
                            context.Exception,
                            "Clerk JWT authentication failed for {Path}: {Reason}",
                            context.HttpContext.Request.Path,
                            context.Exception.Message);

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        // Surface signing-key resolution failures instead of reporting a healthy app that
        // rejects every authenticated request. Deliberately untagged so it is covered by
        // /health (and CI smoke tests) without letting a transient Clerk outage flap
        // /health/ready and trigger container restarts.
        services.AddHealthChecks()
            .AddCheck<ClerkJwksHealthCheck>("clerk-jwks");

        return services;
    }
}
