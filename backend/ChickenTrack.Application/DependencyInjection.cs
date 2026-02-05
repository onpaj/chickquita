using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChickenTrack.Application;

/// <summary>
/// Configures Application layer services and dependencies
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer services into the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR with all handlers from this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register AutoMapper with profiles from this assembly
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
