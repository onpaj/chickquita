using System.Data.Common;
using Chickquita.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Chickquita.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that sets the PostgreSQL RLS tenant context on every
/// database connection before any command (SELECT or write) is executed on it.
///
/// Extends DbConnectionInterceptor instead of SaveChangesInterceptor so that
/// set_tenant_context() is called before reads, not only before writes.
/// </summary>
public class TenantInterceptor : DbConnectionInterceptor
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantInterceptor> _logger;

    public TenantInterceptor(ITenantService tenantService, ILogger<TenantInterceptor> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetTenantContextAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        SetTenantContextAsync(connection, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        base.ConnectionOpened(connection, eventData);
    }

    private async Task SetTenantContextAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        if (tenantId.HasValue)
        {
            _logger.LogDebug("Setting RLS context for tenant: {TenantId}", tenantId.Value);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT set_tenant_context(@tenantId)";
            var param = cmd.CreateParameter();
            param.ParameterName = "tenantId";
            param.Value = tenantId.Value;
            cmd.Parameters.Add(param);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            _logger.LogDebug("No tenant context available - skipping RLS context setup");
        }
    }
}
