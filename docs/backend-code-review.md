# Backend Code Review — Chickquita

> Scope: `.NET 8` / Clean Architecture — Domain, Application, Infrastructure, API layers.
> Focus: best practices, DRY, KISS, YAGNI, antipatterns.
> Status: open findings to be triaged and addressed.

---

## Summary

| Category | Count |
|---|---|
| DRY violations | 5 |
| Antipatterns | 7 |
| Performance | 3 |
| Security | 3 |
| YAGNI | 4 |
| Architecture / design smells | 6 |
| **Total** | **28** |

---

## Critical

### [CR-01] Duplicated auth guard in every handler (DRY)

**Severity: High — affects every feature**

Every command and query handler starts with the same boilerplate:

```csharp
if (!_currentUserService.IsAuthenticated)
    return Result<T>.Failure(Error.Unauthorized("User is not authenticated"));

var tenantId = _currentUserService.TenantId;
if (!tenantId.HasValue)
    return Result<T>.Failure(Error.Unauthorized("Tenant not found"));
```

Appears verbatim in: `CreateCoopCommandHandler`, `UpdateCoopCommandHandler`, `CreateFlockCommandHandler`, `UpdateFlockCommandHandler`, `GetFlocksQueryHandler`, `GetFlockByIdQueryHandler`, `CreateDailyRecordCommandHandler`, `CreatePurchaseCommandHandler`, and all others.

**Problem:** DRY violation across the entire application layer. Any change (e.g. improved error messages, adding logging, adding metrics) requires touching every single handler.

**Fix options:**
1. `AuthorizationBehavior<TRequest, TResponse>` — MediatR pipeline behavior that runs before any handler, returns `Result<T>.Failure(Error.Unauthorized(...))` if not authenticated. Analogous to existing `ValidationBehavior`.
2. Base handler class with a protected helper method `GetAuthenticatedContext()` returning `(Guid tenantId)` or a failure `Result`.

---

### [CR-02] Stringly-typed error code matching in all endpoints (DRY + fragility)

**Severity: High — affects every endpoint**

Every endpoint method switches on hardcoded string literals:

```csharp
result.Error.Code switch
{
    "Error.Unauthorized" => Results.Unauthorized(),
    "Error.NotFound"     => Results.NotFound(new { error = result.Error }),
    "Error.Validation"   => Results.BadRequest(new { error = result.Error }),
    "Error.Conflict"     => Results.Conflict(new { error = result.Error }),
    _                    => Results.BadRequest(new { error = result.Error })
};
```

This pattern is copy-pasted ~30 times across `CoopsEndpoints`, `FlocksEndpoints`, `PurchasesEndpoints`, `DailyRecordsEndpoints`, etc.

**Problems:**
- DRY violation — same switch is duplicated everywhere.
- Fragile — if `Error.cs` renames a code constant (e.g. `"Error.Unauthorized"` → `"Unauthorized"`), all endpoints silently fall through to the `_ => BadRequest` branch. No compiler error.
- No single place to add a new error type (e.g. `"Error.RateLimit"`).

**Fix:** Extract a `ResultExtensions` (or similar) method:

```csharp
public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult>? onSuccess = null)
{
    if (result.IsSuccess)
        return onSuccess?.Invoke(result.Value!) ?? Results.Ok(result.Value);

    return result.Error.Code switch
    {
        ErrorCodes.Unauthorized  => Results.Unauthorized(),
        ErrorCodes.NotFound      => Results.NotFound(new { error = result.Error }),
        ErrorCodes.Validation    => Results.BadRequest(new { error = result.Error }),
        ErrorCodes.Conflict      => Results.Conflict(new { error = result.Error }),
        ErrorCodes.Forbidden     => Results.Forbid(),
        _                        => Results.BadRequest(new { error = result.Error })
    };
}
```

With string constants in `Error.cs` / `ErrorCodes`:

```csharp
public static class ErrorCodes
{
    public const string Unauthorized = "Error.Unauthorized";
    public const string NotFound     = "Error.NotFound";
    // ...
}
```

---

### [CR-03] `Exception.Message` exposed to callers (security)

**Severity: High — information disclosure**

Every handler has:

```csharp
catch (Exception ex)
{
    return Result<T>.Failure(Error.Failure($"Failed to create X: {ex.Message}"));
}
```

`ex.Message` from unhandled infrastructure exceptions can contain sensitive details: SQL queries, file paths, connection strings, stack traces, internal service names.

**Fix:** Never include `ex.Message` in results returned to the API. Log the exception with full details server-side, return a generic message to the caller:

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error creating purchase {Name}", request.Name);
    return Result<T>.Failure(Error.Failure("An unexpected error occurred."));
}
```

---

### [CR-04] `catch (ArgumentException)` as validation proxy (architecture smell)

**Severity: Medium**

Domain entities throw `ArgumentException` from `Create()` static methods for business rule violations. Handlers catch this and re-wrap as `Error.Validation`. Example:

```csharp
// In Flock.Create():
if (string.IsNullOrWhiteSpace(identifier))
    throw new ArgumentException("Identifier cannot be empty");

// In handler:
catch (ArgumentException ex)
    return Result<T>.Failure(Error.Validation(ex.Message));
```

**Problems:**
- Implicit contract: the handler has to know the domain throws on invalid input.
- `ArgumentException` is a general-purpose .NET exception — this coupling is fragile.
- FluentValidation validators already run before the handler. The domain's `ArgumentException` guard is a last-resort double-check without a proper domain exception type.

**Fix options:**
1. Make domain `Create()` methods return `Result<T>` instead of throwing (domain-driven approach).
2. Define a `DomainException` or `BusinessRuleException` type and catch that specifically instead of the generic `ArgumentException`.

---

## Performance

### [CR-05] N+1 query in `GetCoopsQueryHandler` (performance)

**Severity: Medium**

```csharp
// GetCoopsQueryHandler.cs
var coops = await _coopRepository.GetAllAsync(); // Query 1: all coops

foreach (var coop in coopsDto)
{
    coop.FlockCount = await _flockRepository.GetFlocksCountAsync(coop.Id); // Query per coop!
}
```

For 10 coops this is 11 queries. For 100 coops, 101 queries.

**Fix:** Add a JOIN or subquery at the repository level, or use a GROUP BY to count flocks per coop in a single query. Return the count as part of the initial projection.

---

### [CR-06] Blocking async in `TenantInterceptor` (deadlock risk)

**Severity: High**

```csharp
// TenantInterceptor.cs
public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
{
    SetTenantContextAsync(connection).GetAwaiter().GetResult(); // BLOCKING
}
```

Blocking an async method with `.GetAwaiter().GetResult()` in the synchronous override can cause deadlocks under ASP.NET's synchronization context (pre-.NET 6 or in certain configurations), and blocks a thread pool thread unnecessarily.

**Fix:** Override the async version instead:

```csharp
public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
{
    await SetTenantContextAsync(connection);
}
```

Leave the sync override calling `SetTenantContext` synchronously using a separate sync DB call, or accept that only async paths are supported.

---

### [CR-07] `GetDashboardStatsAsync` runs 5+ sequential DB queries (performance)

**Severity: Low-Medium**

```csharp
var flockStats  = await _context.Flocks...FirstOrDefaultAsync();
var totalCoops  = await _context.Coops...CountAsync();
var todayEggs   = await _context.DailyRecords...SumAsync();
var thisWeekEggs = await _context.DailyRecords...SumAsync();
var totalCosts  = await _context.Purchases...SumAsync();
var totalEggs   = await _context.DailyRecords...SumAsync();
```

These queries are all independent and run sequentially.

**Fix:** Use `Task.WhenAll` where queries are independent, or compose into fewer queries. Alternatively, cache dashboard stats with a short TTL since they don't need real-time accuracy.

---

### [CR-08] In-memory sort after AutoMapper in `GetFlocksQueryHandler` (performance)

```csharp
var result = _mapper.Map<List<FlockDto>>(flocks);
return Result<List<FlockDto>>.Success(result.OrderBy(f => f.Identifier).ToList());
```

Ordering happens in-memory after fetching all flocks from the DB. The `OrderBy` should be part of the LINQ query before `.ToListAsync()` to let the DB sort (index-friendly).

---

### [CR-09] `DeleteAsync` loads entity before removing (unnecessary roundtrip)

**Severity: Low**

All Delete implementations follow this pattern:

```csharp
var entity = await _context.Entities.FindAsync(id);
if (entity != null)
{
    _context.Entities.Remove(entity);
    await _context.SaveChangesAsync();
}
```

This is two DB roundtrips where one would do. EF Core 7+ supports:

```csharp
await _context.Entities.Where(e => e.Id == id).ExecuteDeleteAsync();
```

Or, attach a stub entity to avoid the SELECT:

```csharp
var stub = new Purchase { Id = id };
_context.Purchases.Remove(stub);
await _context.SaveChangesAsync();
```

---

## Security

### [CR-10] JWT auth silently skipped if `Clerk:Authority` is empty (security)

**Severity: High**

```csharp
// Infrastructure/DependencyInjection.cs
if (!string.IsNullOrEmpty(authority))
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(...);
}
// else: authentication is simply not configured — no warning, no exception
```

If `Clerk:Authority` is missing or misconfigured in a deployed environment, authentication silently does not register, and all `[Authorize]` endpoints become publicly accessible.

**Fix:** Throw on startup if the required config is missing:

```csharp
var authority = configuration["Clerk:Authority"]
    ?? throw new InvalidOperationException("Clerk:Authority is required but not configured.");
```

---

### [CR-11] CORS only configured in Development (security gap)

**Severity: Medium**

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
}
// No CORS for Production, Staging, etc.
```

In production, without explicit CORS policy, all cross-origin requests are blocked by browser — which may be the intent, but should be explicit and configurable (e.g. allow specific production domain).

---

### [CR-12] `WebhookVerificationException` caught by type name string (fragility)

**Severity: Low**

```csharp
// ClerkWebhookValidator.cs
catch (Exception ex) when (ex.GetType().Name == "WebhookVerificationException")
```

Catching exceptions by comparing `GetType().Name` to a string will silently stop working if the Svix library renames the exception class or moves it to a different namespace. Should reference the type directly:

```csharp
catch (Svix.Exceptions.WebhookVerificationException)
```

---

## Architecture & Design

### [CR-13] No Unit of Work — atomicity not possible (architecture)

**Severity: Medium**

Each repository calls `SaveChangesAsync()` internally:

```csharp
// FlockRepository.cs
public async Task<Flock> AddAsync(Flock flock)
{
    await _context.Flocks.AddAsync(flock);
    await _context.SaveChangesAsync(); // ← commits immediately
    return flock;
}
```

If a use case requires saving two aggregates atomically (e.g. create flock + create first history entry), there's no way to do it — each save commits independently, leaving the DB in a partially-updated state on error.

**Fix:** Either introduce a Unit of Work / explicit `SaveChangesAsync()` in handlers (not repositories), or accept the current approach but document transaction boundaries explicitly.

---

### [CR-14] `TenantService` and `CurrentUserService` duplicate logic (DRY)

**Severity: Low**

Both `TenantService` (`ITenantService`) and `CurrentUserService` (`ICurrentUserService`) read `TenantId` from `HttpContext.Items["TenantId"]`. The `ICurrentUserService` is used everywhere; `ITenantService` is injected into `TenantResolutionMiddleware`. The abstraction serves no clear purpose beyond the existing `ICurrentUserService`.

---

### [CR-15] Unnecessary entity fetch just to verify existence (YAGNI)

**Severity: Low-Medium**

Multiple handlers do:

```csharp
// CreateFlockCommandHandler.cs
var coop = await _coopRepository.GetByIdAsync(request.CoopId.Value);
if (coop == null)
    return Result.Failure(Error.NotFound(...));
// coop variable is never used again
```

The full entity is fetched (with all columns + potential includes) just to do an existence check. Global query filters already scope all queries to the current tenant, so an unauthorized `coopId` returns null anyway.

**Fix:** Add `ExistsByIdAsync(Guid id)` to repositories and use `AnyAsync(c => c.Id == id)` under the hood. Faster, less data transferred.

---

### [CR-16] `UpdateFlockCommand` has dual responsibility (SRP)

**Severity: Low**

`UpdateFlockCommandHandler` handles two distinct operations in one handler:

```csharp
// Update basic properties
flock.Update(request.Identifier, request.Description, request.Notes, request.IsActive);

// Optionally also update composition
if (request.Hens.HasValue || request.Roosters.HasValue || request.Chicks.HasValue)
    flock.UpdateComposition(...);
```

This conflates "update flock metadata" with "update flock composition count." If they have different validation rules or business meaning, they should be separate commands.

---

### [CR-17] `GetFlocksQueryHandler` unnecessary coop existence check

**Severity: Low**

```csharp
// GetFlocksQueryHandler.cs
if (request.CoopId.HasValue)
{
    var coop = await _coopRepository.GetByIdAsync(request.CoopId.Value); // extra query
    if (coop == null)
        return Result.Failure(Error.NotFound(...));
}
var flocks = await _flockRepository.GetAllAsync(request.CoopId, request.IncludeInactive);
```

If `CoopId` doesn't belong to the tenant (or doesn't exist), the global query filter will scope the flocks query to return nothing. The coop existence check is a redundant roundtrip for security that the filter already handles. It's useful for a meaningful 404 response, but worth the trade-off only if that UX matters.

---

### [CR-18] FlockRepository EF Core change tracking workaround (leaky abstraction)

**Severity: Medium**

```csharp
// FlockRepository.cs — UpdateAsync
foreach (var entry in _context.ChangeTracker.Entries<FlockHistory>()
    .Where(e => e.State == EntityState.Modified))
{
    entry.State = EntityState.Added;
}
```

The repository needs to manipulate EF Core's change tracker internals because `GetByIdWithoutHistoryAsync()` loads the flock without its `History` collection, and when `UpdateComposition()` adds a new `FlockHistory`, EF incorrectly marks it `Modified` instead of `Added`.

This is a leak of EF Core internals into the repository layer. The root cause is loading the entity without the navigation property to save a query, then writing to it.

**Fix options:**
1. Always load with `History` included (slightly more data, simpler code).
2. Explicitly attach new `FlockHistory` entries to `_context.FlockHistory` in the repository rather than relying on tracking.

---

## YAGNI

### [CR-19] `Class1.cs` — empty scaffold file in Application project

**File:** `Chickquita.Application/Class1.cs`

A scaffolded empty class left from project creation. Delete it.

---

### [CR-20] `/ping` endpoint left in production

**Severity: Low**

```csharp
// Program.cs
app.MapGet("/ping", async (IMediator mediator) => { ... })
// This endpoint is for testing only and can be removed after verification
```

Comment says it can be removed. It should be.

---

### [CR-21] Unused repository methods — `GetByTypeAsync`, `GetByDateRangeAsync` in `PurchaseRepository`

`PurchaseRepository` exposes `GetByTypeAsync(PurchaseType)` and `GetByDateRangeAsync(DateTime, DateTime)`. Neither is called by any handler — all callers use `GetWithFiltersAsync()`. YAGNI — remove or consolidate.

---

### [CR-22] Redundant null guards in repository methods

```csharp
// PurchaseRepository.cs
public async Task<Purchase> AddAsync(Purchase purchase)
{
    if (purchase == null)
        throw new ArgumentNullException(nameof(purchase));
    ...
}
```

The callers are application handlers that always pass non-null entities. Validation already ran before the handler. These guards are defensive boilerplate for an impossible scenario in this architecture. Over-engineering; remove or leave as documented contract (but not both in every method).

---

## Code Style & Conventions

### [CR-23] `catch` on `WebhookVerificationException` by string name (see CR-12)

### [CR-24] Inconsistent `OpenAPI` documentation across endpoint groups

`PurchasesEndpoints` has detailed `WithOpenApi(op => { op.Summary = ...; return op; })` per endpoint. `FlocksEndpoints`, `CoopsEndpoints`, `DailyRecordsEndpoints` use just `WithOpenApi()` with no metadata. Should be consistent across all endpoint groups.

---

### [CR-25] Query + Handler in the same file only for some queries (inconsistency)

`GetCoopByIdQuery.cs` contains both `GetCoopByIdQuery` and `GetCoopByIdQueryHandler` in one file. Other queries (`GetFlocksQuery.cs` + `GetFlocksQueryHandler.cs`) are separate files. Should pick one convention and apply it uniformly.

---

### [CR-26] Hardcoded `"Manual update"` magic string in `UpdateFlockCommandHandler`

```csharp
flock.UpdateComposition(hens, roosters, chicks, "Manual update");
```

`"Manual update"` is a magic string passed as the `reason` parameter to `UpdateComposition`. If the frontend eventually sends a reason (e.g. "Mortality", "Purchase"), this will silently override it. The command should carry the reason, or the domain should define an enum.

---

### [CR-27] `Program.cs` static file caching uses `path.EndsWith()` (brittle)

```csharp
if (path.EndsWith(".js") || path.EndsWith(".css") || ...)
```

Should use `Path.GetExtension(path)` for proper extension matching (handles edge cases like `file.min.js`, query strings stripped, case sensitivity on Windows).

---

### [CR-28] `PurchasesEndpoints.GetPurchases` accepts `flockId` but `GetPurchasesQuery` has no `FlockId` filter

The endpoint accepts `[FromQuery] Guid? flockId` and assigns it to `query.FlockId`, but `GetPurchasesQuery` / `GetPurchasesQueryHandler` / `PurchaseRepository.GetWithFiltersAsync` only filter by `coopId`. The `flockId` parameter is silently ignored.

---

## Summary Table

| ID | Area | Issue | Severity |
|---|---|---|---|
| CR-01 | DRY | Auth guard duplicated in every handler | High |
| CR-02 | DRY | Stringly-typed error switch in all endpoints | High |
| CR-03 | Security | `ex.Message` exposed in error responses | High |
| CR-04 | Architecture | `ArgumentException` as domain validation proxy | Medium |
| CR-05 | Performance | N+1 in `GetCoopsQueryHandler` | Medium |
| CR-06 | Correctness | Blocking async in `TenantInterceptor` | High |
| CR-07 | Performance | 5+ sequential queries in `GetDashboardStatsAsync` | Low |
| CR-08 | Performance | In-memory sort after DB fetch in `GetFlocksQueryHandler` | Low |
| CR-09 | Performance | DELETE loads entity before removing | Low |
| CR-10 | Security | JWT auth silently skipped if config missing | High |
| CR-11 | Security | CORS only configured in Development | Medium |
| CR-12 | Fragility | Exception caught by type name string | Low |
| CR-13 | Architecture | No Unit of Work — atomicity not possible | Medium |
| CR-14 | DRY | `TenantService` duplicates `CurrentUserService` logic | Low |
| CR-15 | YAGNI | Full entity fetch just for existence check | Low |
| CR-16 | SRP | `UpdateFlockCommand` has dual responsibility | Low |
| CR-17 | Performance | Extra coop exists check in `GetFlocksQueryHandler` | Low |
| CR-18 | Architecture | EF Core change tracker leak in `FlockRepository.UpdateAsync` | Medium |
| CR-19 | YAGNI | `Class1.cs` empty scaffold in Application project | Low |
| CR-20 | YAGNI | `/ping` endpoint left in production | Low |
| CR-21 | YAGNI | Unused `GetByTypeAsync`, `GetByDateRangeAsync` in `PurchaseRepository` | Low |
| CR-22 | YAGNI | Redundant null guards in repository methods | Low |
| CR-23 | Fragility | (see CR-12) | Low |
| CR-24 | Convention | Inconsistent OpenAPI docs across endpoint groups | Low |
| CR-25 | Convention | Query + Handler in same file only sometimes | Low |
| CR-26 | Convention | `"Manual update"` magic string | Low |
| CR-27 | Convention | `path.EndsWith()` for file extension matching | Low |
| CR-28 | Bug | `flockId` query param silently ignored in Purchases endpoint | Medium |

---

## Recommended Priority Order

1. **CR-10** — Auth bypass on misconfiguration (security, startup-time fix)
2. **CR-03** — `ex.Message` in responses (security)
3. **CR-01** — Auth guard boilerplate (DRY, high leverage — one fix removes ~15 copy-pastes)
4. **CR-02** — Error code string switch (DRY, high leverage)
5. **CR-06** — Blocking async in interceptor (correctness)
6. **CR-05** — N+1 on coops list (performance, user-visible)
7. **CR-28** — Silent `flockId` ignore in purchases (bug)
8. **CR-13** — No Unit of Work (architectural — assess if atomicity is needed now or later)
9. **CR-04** — `ArgumentException` as validation proxy (architecture)
10. Everything else in whatever order makes sense during normal feature work.
