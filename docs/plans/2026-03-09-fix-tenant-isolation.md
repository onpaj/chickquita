# Tenant Isolation Bug — Investigation & Fix Plan

**Date**: 2026-03-09
**Severity**: CRITICAL — production data leak
**Symptom**: User `andreabartosova1@gmail.com` can see records belonging to `ondra@anela.cz`
**Related**: US-007 (Tenant Isolation)

---

## Executive Summary

Multi-tenant data isolation is completely broken in production. Despite RLS policies being defined in PostgreSQL migrations and a `TenantInterceptor` class existing in the codebase, **none of the tenant isolation mechanisms actually work for read operations**. Every user with a valid JWT token can read all data from all tenants.

Four independent bugs combine to produce this failure. Any one of them alone would be enough to break isolation.

---

## Investigation

### Architecture Intent

The codebase documents a "defense in depth" multi-tenant strategy in `backend/tests/TENANT_ISOLATION_VERIFICATION.md`:

1. Authentication layer — `TenantResolutionMiddleware` resolves tenant from JWT
2. Authorization layer — all endpoints require `.RequireAuthorization()`
3. Application layer — handlers check `ICurrentUserService.TenantId`
4. Data access layer — EF Core global query filters + `TenantInterceptor`
5. Database layer — PostgreSQL Row-Level Security (RLS)

**In practice, layers 4 and 5 are both broken.** Layer 3 retrieves the tenant ID from the service but does not pass it to the repositories, which do not filter by tenant at all.

---

## Root Cause Analysis

### Bug 1 — `TenantInterceptor` only fires on writes, never on reads

**File**: `backend/src/Chickquita.Infrastructure/Data/Interceptors/TenantInterceptor.cs`

```csharp
public class TenantInterceptor : SaveChangesInterceptor  // ← SaveChangesInterceptor!
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
    {
        await SetTenantContextAsync(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(...);
    }
    // No override for read query interception!
}
```

`SaveChangesInterceptor` only fires when `SaveChanges()` / `SaveChangesAsync()` is called — that is, exclusively on INSERT / UPDATE / DELETE operations. When EF Core executes a SELECT query (e.g. `_context.Coops.ToListAsync()`), this interceptor is **not invoked at all**.

**Consequence**: `set_tenant_context()` is never called before any SELECT statement. The PostgreSQL session variable `app.current_tenant_id` is always NULL when reading data.

The `ApplicationDbContext.SetTenantContextAsync(Guid tenantId)` method exists and does the right thing, but it is never called anywhere during read operations.

---

### Bug 2 — Missing `FORCE ROW LEVEL SECURITY` on all tables

**Files**: All table-creation migrations under `backend/src/Chickquita.Infrastructure/Migrations/`

All tables with tenant data have RLS enabled:

```sql
ALTER TABLE coops ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON coops
    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);
```

But **none have `FORCE ROW LEVEL SECURITY`**. PostgreSQL documentation states:

> Row security policies are not applied when the query's role is the table owner or a superuser, unless the table has `FORCE ROW LEVEL SECURITY` set.

The application almost certainly connects to the database using the same user that owns the tables (the default in most `dotnet ef database update` setups). This means RLS is entirely bypassed at the PostgreSQL level for that user — regardless of whether `set_tenant_context()` is called.

**Affected tables**: `tenants`, `coops`, `flocks`, `flock_history`, `daily_records`, `purchases`

---

### Bug 3 — EF Core global query filters disabled

**File**: `backend/src/Chickquita.Infrastructure/Data/Configurations/CoopConfiguration.cs`

```csharp
// Temporarily disabled - rely on RLS only
// builder.HasQueryFilter(c => EF.Property<Guid>(c, "TenantId") == Guid.Empty);
```

Global query filters were the application-level safety net. They were commented out with the justification "rely on RLS only" — but RLS itself is broken (Bugs 1 and 2). No other entity configuration files (flocks, daily records, purchases, etc.) have query filters either.

The comment in `TENANT_ISOLATION_VERIFICATION.md` even notes "The global query filter currently uses `Guid.Empty` which needs to be updated" — confirming this was never properly implemented.

---

### Bug 4 — Repository read methods have no tenant filter

**Files**: All repository implementations

```csharp
// CoopRepository.cs — GetAllAsync
return await _context.Coops
    .OrderByDescending(c => c.CreatedAt)
    .ToListAsync();                          // ← no .Where(c => c.TenantId == tenantId)

// PurchaseRepository.cs — GetAllAsync
return await _context.Purchases
    .Include(p => p.Coop)
    .OrderByDescending(p => p.PurchaseDate)
    .ToListAsync();                          // ← same problem

// StatisticsRepository.cs — GetDashboardStatsAsync
var flockStats = await _context.Flocks
    .Where(f => f.IsActive)
    .GroupBy(f => 1)
    .Select(...)
    .FirstOrDefaultAsync();                  // ← queries all flocks from all tenants
```

**Affected methods**:

| Repository | Methods without tenant filter |
|---|---|
| `CoopRepository` | `GetAllAsync`, `GetByIdAsync`, `ExistsByNameAsync`, `HasFlocksAsync`, `GetFlocksCountAsync` |
| `FlockRepository` | `GetAllAsync`, `GetByIdAsync`, `GetByCoopIdAsync`, and others |
| `DailyRecordRepository` | `GetAllAsync`, `GetByIdAsync`, `GetByFlockIdAsync`, and others |
| `PurchaseRepository` | `GetAllAsync`, `GetWithFiltersAsync`, `GetByIdAsync`, `GetByDateRangeAsync`, `GetByTypeAsync`, `GetDistinctNamesAsync`, `GetDistinctNamesByQueryAsync` |
| `StatisticsRepository` | `GetDashboardStatsAsync`, `GetStatisticsAsync`, and all private helpers |

Note: `FlockHistoryRepository` only exposes `GetByIdAsync` (no list method), but it also lacks tenant filtering.

---

### How the Bugs Combine

When user B logs in and calls `GET /api/coops`:

1. `TenantResolutionMiddleware` correctly resolves tenant B → stored in `HttpContext.Items["TenantId"]` ✓
2. `GetCoopsQueryHandler` reads `tenantId` from `ICurrentUserService.TenantId` ✓
3. Handler calls `_coopRepository.GetAllAsync()` — no tenant ID is passed ✗
4. `CoopRepository.GetAllAsync()` executes `SELECT * FROM coops` with no tenant filter ✗
5. No `HasQueryFilter` applies (all commented out) ✗
6. `set_tenant_context()` was never called → `app.current_tenant_id` is NULL ✗
7. RLS policy `USING (tenant_id = NULL::UUID)` evaluates to NULL → would block all rows... ✗
8. BUT: if the DB user owns the tables → RLS is bypassed entirely → all rows returned ✗

All tenant data is returned to user B.

---

## Fix Plan

All four bugs must be fixed. They form independent safety layers — fixing only some leaves the system vulnerable if others fail.

### Fix 1 — Set tenant context before read operations

**Priority**: CRITICAL — immediate hotfix

**Approach**: Replace the current `SaveChangesInterceptor` with (or supplement it with) an `IDbConnectionInterceptor` that sets the tenant context each time a database connection is opened/checked out from the pool.

**File to change**: `backend/src/Chickquita.Infrastructure/Data/Interceptors/TenantInterceptor.cs`

```
Current:  public class TenantInterceptor : SaveChangesInterceptor
Proposed: public class TenantInterceptor : DbConnectionInterceptor
          (or: split into TenantConnectionInterceptor + TenantSaveInterceptor)
```

Override `ConnectionOpeningAsync` (fires when EF Core checks out a connection from the pool — before any command runs on it):

```csharp
public override async Task ConnectionOpenedAsync(
    DbConnection connection,
    ConnectionEndEventData eventData,
    CancellationToken cancellationToken = default)
{
    var tenantId = _tenantService.GetCurrentTenantId();
    if (tenantId.HasValue)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT set_tenant_context(@tenantId)";
        var param = cmd.CreateParameter();
        param.ParameterName = "tenantId";
        param.Value = tenantId.Value;
        cmd.Parameters.Add(param);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
```

This ensures `set_tenant_context()` runs for every connection before any SELECT or write command.

**Alternative approach**: Instead of the connection interceptor, call `context.SetTenantContextAsync(tenantId)` explicitly at the start of each repository method (or in a base repository class). This is more verbose but has no risk of interceptor ordering issues.

**Registration**: Update `DependencyInjection.cs` — the interceptor must remain registered as a scoped service so it has access to the per-request `ITenantService`.

---

### Fix 2 — Add `FORCE ROW LEVEL SECURITY` to all tables

**Priority**: CRITICAL — immediate hotfix

**Approach**: New EF Core migration that adds `FORCE ROW LEVEL SECURITY` to every tenant-isolated table.

**File to create**: `backend/src/Chickquita.Infrastructure/Migrations/YYYYMMDDHHMMSS_ForceRowLevelSecurity.cs`

```sql
-- Up
ALTER TABLE tenants FORCE ROW LEVEL SECURITY;
ALTER TABLE coops FORCE ROW LEVEL SECURITY;
ALTER TABLE flocks FORCE ROW LEVEL SECURITY;
ALTER TABLE flock_history FORCE ROW LEVEL SECURITY;
ALTER TABLE daily_records FORCE ROW LEVEL SECURITY;
ALTER TABLE purchases FORCE ROW LEVEL SECURITY;

-- Down
ALTER TABLE tenants NO FORCE ROW LEVEL SECURITY;
ALTER TABLE coops NO FORCE ROW LEVEL SECURITY;
ALTER TABLE flocks NO FORCE ROW LEVEL SECURITY;
ALTER TABLE flock_history NO FORCE ROW LEVEL SECURITY;
ALTER TABLE daily_records NO FORCE ROW LEVEL SECURITY;
ALTER TABLE purchases NO FORCE ROW LEVEL SECURITY;
```

**Important**: This fix alone (without Fix 1) would cause every user to see zero records, because `current_setting('app.current_tenant_id', true)` would return NULL and the policy `tenant_id = NULL::UUID` evaluates to false. Fix 1 and Fix 2 must be deployed together.

**Also consider**: Creating a dedicated non-superuser application database role that does not own the tables. This is a deeper infrastructure change but provides proper privilege separation. The application role should have `SELECT`, `INSERT`, `UPDATE`, `DELETE` on all tables but not `OWNERSHIP`. With a non-owner role, `ENABLE ROW LEVEL SECURITY` (without FORCE) would already be sufficient.

---

### Fix 3 — Re-enable EF Core global query filters

**Priority**: HIGH — defense in depth layer

**Approach**: Inject the current tenant ID into `ApplicationDbContext` at construction time (captured from DI scope) and configure global query filters for all tenant-owned entities.

**File to change**: `backend/src/Chickquita.Infrastructure/Data/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : DbContext
{
    private readonly Guid? _currentTenantId;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)  // ← add this
        : base(options)
    {
        _currentTenantId = currentUserService.TenantId;  // captured once at scope creation
    }
}
```

**Files to change**: All entity configuration files must add a query filter:
- `CoopConfiguration.cs`
- `FlockConfiguration.cs` (if exists, or inline in `OnModelCreating`)
- `FlockHistoryConfiguration.cs`
- `DailyRecordConfiguration.cs`
- `PurchaseConfiguration.cs`

Filter pattern for each:
```csharp
builder.HasQueryFilter(c => _tenantId == null || c.TenantId == _tenantId.Value);
```

The `_tenantId == null` guard allows migrations (which run without a request context) and integration tests to function without a tenant in scope. In production API requests, `_tenantId` will always be set.

**Note**: Because `OnModelCreating` is called once and the model is cached, this approach requires the `ApplicationDbContext` to use `_currentTenantId` as a captured value (not a property call), and it must be accessed via `EF.Property` or a field captured in the lambda. The standard pattern: pass `Func<Guid?>` to the context, or capture the value in a field at constructor time.

See: https://learn.microsoft.com/en-us/ef/core/querying/filters

---

### Fix 4 — Add explicit tenant filter in all repository read methods

**Priority**: HIGH — final defense layer, also most explicit and auditable

**Approach**: Inject `ICurrentUserService` into every repository that reads tenant-scoped data. Add `.Where(x => x.TenantId == tenantId)` to every read query.

**Files to change**:
- `CoopRepository.cs`
- `FlockRepository.cs`
- `DailyRecordRepository.cs`
- `PurchaseRepository.cs`
- `StatisticsRepository.cs`
- `FlockHistoryRepository.cs`

Example for `CoopRepository`:

```csharp
public class CoopRepository : ICoopRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CoopRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<Coop>> GetAllAsync(bool includeArchived = false)
    {
        var tenantId = _currentUserService.TenantId
            ?? throw new InvalidOperationException("No tenant context");

        var query = _context.Coops
            .Where(c => c.TenantId == tenantId);  // ← explicit tenant filter

        if (!includeArchived)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Same pattern for GetByIdAsync, ExistsByNameAsync, etc.
}
```

For `GetByIdAsync`, an explicit tenant check prevents cross-tenant access:
```csharp
public async Task<Coop?> GetByIdAsync(Guid id)
{
    var tenantId = _currentUserService.TenantId
        ?? throw new InvalidOperationException("No tenant context");

    return await _context.Coops
        .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
}
```

`ExistsByNameAsync` must also filter by tenant — otherwise a tenant can't reuse a name that another tenant already uses, and cross-tenant name pollution is possible.

---

## Fix Priority and Deployment Order

| Fix | Priority | Reason |
|---|---|---|
| Fix 2 — `FORCE ROW LEVEL SECURITY` | Deploy 1st | Database migration, safe independently |
| Fix 1 — `DbConnectionInterceptor` | Deploy 2nd (with Fix 2) | Must be paired — Fix 2 alone blocks all reads |
| Fix 4 — Repository tenant filters | Deploy 3rd | Most explicit layer, requires code review |
| Fix 3 — EF Core global query filters | Deploy 4th | Needs `ApplicationDbContext` refactor |

**Recommended hotfix order**:
1. Create `ForceRowLevelSecurity` migration (Fix 2) — this alone won't affect existing (broken) behavior because the DB user likely bypasses RLS already
2. Implement `DbConnectionInterceptor` (Fix 1) — this enables RLS to actually filter reads
3. Deploy both together — after deployment, each user will only see their own data

Fixes 3 and 4 follow as defense-in-depth improvements.

---

## What's NOT Broken

- `TenantResolutionMiddleware` — correctly resolves tenant from JWT and stores in `HttpContext.Items`
- `CurrentUserService.TenantId` — correctly reads from `HttpContext.Items`
- Entity creation — `TenantId` is set correctly on INSERT (the write interceptor works)
- Authentication/authorization — endpoints are properly protected, unauthenticated access returns 401
- The `set_tenant_context()` PostgreSQL function itself — works correctly
- RLS policy definitions — policies are syntactically correct, they just never get triggered

---

## Testing Requirements

After implementing the fix, the following tests should be added or verified:

### Integration tests (new)

1. **Cross-tenant read isolation**: User A creates entity → User B logs in → User B's GET request returns empty list (not User A's data)
2. **Cross-tenant ID access**: User A creates entity → User B calls GET with User A's entity ID → returns 404
3. **Cross-tenant statistics isolation**: Statistics endpoint returns only the current tenant's aggregated data
4. **Write isolation still works**: User A cannot create entity in User B's tenant via POST with manipulated body

### Unit tests (update)

1. `TenantInterceptor` (renamed `TenantConnectionInterceptor`): verify `set_tenant_context()` is called before connection is used for read
2. `CoopRepository.GetAllAsync`: verify generates SQL with `WHERE tenant_id = @tenantId`
3. All repository `GetByIdAsync` methods: verify tenant ID is included in the query

---

## Files Affected Summary

| File | Change |
|---|---|
| `Infrastructure/Data/Interceptors/TenantInterceptor.cs` | Change base class to `DbConnectionInterceptor`, override `ConnectionOpenedAsync` |
| `Infrastructure/Data/ApplicationDbContext.cs` | Add `ICurrentUserService` constructor param, capture `_currentTenantId` |
| `Infrastructure/Data/Configurations/CoopConfiguration.cs` | Re-enable `HasQueryFilter` with actual tenant ID |
| `Infrastructure/Data/Configurations/*.cs` (all entity configs) | Add `HasQueryFilter` |
| `Infrastructure/Repositories/CoopRepository.cs` | Add `ICurrentUserService`, add `.Where(TenantId == tenantId)` to all reads |
| `Infrastructure/Repositories/FlockRepository.cs` | Same |
| `Infrastructure/Repositories/DailyRecordRepository.cs` | Same |
| `Infrastructure/Repositories/PurchaseRepository.cs` | Same |
| `Infrastructure/Repositories/StatisticsRepository.cs` | Same |
| `Infrastructure/Repositories/FlockHistoryRepository.cs` | Same |
| `Infrastructure/DependencyInjection.cs` | Update interceptor registration if class renamed |
| New migration `*_ForceRowLevelSecurity.cs` | `ALTER TABLE ... FORCE ROW LEVEL SECURITY` for all 6 tables |
| `backend/tests/TENANT_ISOLATION_VERIFICATION.md` | Update — current claims are incorrect |
