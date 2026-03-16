using Chickquita.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Chickquita.Api.Tests.Helpers;

/// <summary>
/// Startup filter that ensures the SQLite schema is created before any requests are handled.
/// Required because each WebApplicationFactory child instance gets its own in-memory SQLite
/// connection, and EnsureCreated() must be called on every instance.
/// </summary>
internal sealed class DatabaseInitializerStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            next(app);
        };
    }
}
