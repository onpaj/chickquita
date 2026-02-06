using Chickquita.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Chickquita.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that sets the PostgreSQL RLS context before SaveChanges operations
/// </summary>
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantInterceptor> _logger;

    public TenantInterceptor(ITenantService tenantService, ILogger<TenantInterceptor> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await SetTenantContextAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            SetTenantContextAsync(eventData.Context, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        return base.SavingChanges(eventData, result);
    }

    private async Task SetTenantContextAsync(DbContext context, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        if (tenantId.HasValue)
        {
            _logger.LogDebug("Setting RLS context for tenant: {TenantId}", tenantId.Value);

            await context.Database.ExecuteSqlRawAsync(
                "SELECT set_tenant_context({0})",
                new object[] { tenantId.Value },
                cancellationToken);
        }
        else
        {
            _logger.LogDebug("No tenant context available - skipping RLS context setup");
        }
    }
}
