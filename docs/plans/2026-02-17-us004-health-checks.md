# US-004: Health Check Endpoints Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace the existing stub `/health` endpoint with three proper ASP.NET Core health check endpoints (`/health`, `/health/live`, `/health/ready`) that return structured JSON and integrate with the EF Core database.

**Architecture:** Use `builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>()` (tagged `"ready"`) for DB connectivity. Map three `MapHealthChecks` endpoints — aggregate, liveness (no checks), readiness (DB only) — all public (no auth). A shared JSON `ResponseWriter` formats `HealthReport` as structured JSON with per-entry detail.

**Tech Stack:** ASP.NET Core 8 built-in health checks, `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` NuGet, xunit + `WebApplicationFactory<Program>` for integration tests.

---

## Context

- **`Program.cs`**: `backend/src/Chickquita.Api/Program.cs` — has a stub `app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))` at line 145 to be replaced.
- **`ApplicationDbContext`**: lives in `Chickquita.Infrastructure.Data` namespace.
- **Test project**: `backend/tests/Chickquita.Api.Tests/` — uses `WebApplicationFactory<Program>` + InMemory EF Core DB (via `ReplaceWithInMemoryDatabase` helper pattern already in `CoopsEndpointsTests.cs`).
- **Version alignment**: Use `Version="8.0.2"` to match existing EF Core packages in the solution.

---

## Task 1: Add the EF Core health checks NuGet package

**Files:**
- Modify: `backend/src/Chickquita.Api/Chickquita.Api.csproj`

**Step 1: Add the package reference**

Open `backend/src/Chickquita.Api/Chickquita.Api.csproj` and add inside the existing `<ItemGroup>` with other `PackageReference` entries:

```xml
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="8.0.2" />
```

**Step 2: Restore packages**

```bash
cd /Users/pajgrtondrej/Work/GitHub/Chickquita/backend
dotnet restore
```

Expected: `Restore succeeded.` (no errors)

**Step 3: Verify build still passes**

```bash
dotnet build backend/src/Chickquita.Api/Chickquita.Api.csproj
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 2: Write failing tests for the three health endpoints

**Files:**
- Create: `backend/tests/Chickquita.Api.Tests/Endpoints/HealthCheckEndpointTests.cs`

**Step 1: Write the test file**

```csharp
using System.Net;
using System.Text.Json;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chickquita.Api.Tests.Endpoints;

public class HealthCheckEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real Postgres with InMemory so the DB health check works in tests
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("HealthCheckTestDb"));
            });
        });
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithJsonBody()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().Be("Healthy");
        doc.RootElement.TryGetProperty("entries", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("totalDuration", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetHealthLive_ReturnsOkWithJsonBody()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().Be("Healthy");
        // Liveness has no checks — entries should be empty
        doc.RootElement.TryGetProperty("entries", out var entries).Should().BeTrue();
        entries.EnumerateObject().Should().BeEmpty();
    }

    [Fact]
    public async Task GetHealthReady_ReturnsOkWhenDatabaseHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().Be("Healthy");
        doc.RootElement.TryGetProperty("entries", out var entries).Should().BeTrue();
        entries.TryGetProperty("database", out var dbEntry).Should().BeTrue();
        dbEntry.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthEndpoints_DoNotRequireAuthentication()
    {
        var client = _factory.CreateClient();

        // Hit all three without any Authorization header
        var healthResponse = await client.GetAsync("/health");
        var liveResponse = await client.GetAsync("/health/live");
        var readyResponse = await client.GetAsync("/health/ready");

        healthResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        liveResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        readyResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
```

**Step 2: Run tests to confirm they fail**

```bash
cd /Users/pajgrtondrej/Work/GitHub/Chickquita/backend
dotnet test tests/Chickquita.Api.Tests/Chickquita.Api.Tests.csproj \
  --filter "HealthCheckEndpointTests" -v normal
```

Expected: Tests fail — `/health/live` and `/health/ready` return 404, and `/health` returns wrong format (plain `{ status: "healthy" }` without `entries`/`totalDuration`).

---

## Task 3: Implement health checks in Program.cs

**Files:**
- Modify: `backend/src/Chickquita.Api/Program.cs`

### Step 1: Add the `using` for health checks

At the top of `Program.cs`, add:

```csharp
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
```

### Step 2: Register health checks (services section)

After `builder.Services.AddApplicationServices();` and `builder.Services.AddInfrastructureServices(...)`, add:

```csharp
// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Chickquita.Infrastructure.Data.ApplicationDbContext>(
        name: "database",
        tags: ["ready"]);
```

### Step 3: Add the JSON response writer helper

Add this static local function just before `app.Run()` (after all endpoint mapping):

```csharp
static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var entries = report.Entries.Select(e => new
    {
        key = e.Key,
        value = new
        {
            status = e.Value.Status.ToString(),
            duration = e.Value.Duration.ToString(),
            description = e.Value.Description,
            exception = e.Value.Exception?.Message
        }
    }).ToDictionary(e => e.key, e => e.value);

    var result = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.ToString(),
        entries
    };

    await context.Response.WriteAsync(
        JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
}
```

### Step 4: Replace the stub `/health` endpoint and add `/health/live` and `/health/ready`

Remove this block (lines ~145–148):

```csharp
// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .WithOpenApi()
    .Produces<object>(200);
```

Replace it with:

```csharp
// Health check endpoints — unauthenticated, publicly accessible
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse,
    AllowCachingResponses = false
}).AllowAnonymous();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Liveness: no dependency checks — just proves the app is running
    Predicate = _ => false,
    ResponseWriter = WriteHealthCheckResponse,
    AllowCachingResponses = false
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Readiness: only checks tagged "ready" (database)
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse,
    AllowCachingResponses = false
}).AllowAnonymous();
```

> **Placement:** These three `MapHealthChecks` calls must come **before** `app.MapFallbackToFile("index.html")` so the SPA fallback doesn't intercept them. Place them immediately after the existing `app.MapGet("/ping", ...)` block.

### Step 5: Build to confirm no compile errors

```bash
cd /Users/pajgrtondrej/Work/GitHub/Chickquita/backend
dotnet build backend/src/Chickquita.Api/Chickquita.Api.csproj
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 4: Run tests and verify all pass

**Step 1: Run the health check tests**

```bash
cd /Users/pajgrtondrej/Work/GitHub/Chickquita/backend
dotnet test tests/Chickquita.Api.Tests/Chickquita.Api.Tests.csproj \
  --filter "HealthCheckEndpointTests" -v normal
```

Expected: All 4 tests pass.

**Step 2: Run the full test suite to check for regressions**

```bash
cd /Users/pajgrtondrej/Work/GitHub/Chickquita/backend
dotnet test --verbosity normal
```

Expected: All tests pass, 0 failures.

---

## Task 5: Update the PRD to mark US-004 as done

**Files:**
- Modify: `tasks/prd-go-live.md`

Mark all US-004 acceptance criteria checkboxes as `[x]` and add `✅ DONE` to the heading:

```markdown
### US-004: Add health check endpoints to .NET API ✅ DONE

**Acceptance Criteria:**
- [x] `/health` — Aggregate health (all checks: healthy/degraded/unhealthy) → HTTP 200/503
- [x] `/health/live` — Liveness check (app is alive, no external dependencies) → HTTP 200
- [x] `/health/ready` — Readiness check (includes DB connectivity check) → HTTP 200/503
- [x] Health checks registered in `Program.cs` using `builder.Services.AddHealthChecks()` with EF Core DB check
- [x] Health check middleware mapped in `Program.cs` with `app.MapHealthChecks()`
- [x] Response includes JSON body with component status details
- [x] Endpoints work without authentication (publicly accessible)
- [x] Typecheck/build passes
```

---

## Task 6: Commit

```bash
cd /Users/pajgrtondrej/Work/GitHub/Chickquita
git add \
  backend/src/Chickquita.Api/Chickquita.Api.csproj \
  backend/src/Chickquita.Api/Program.cs \
  backend/tests/Chickquita.Api.Tests/Endpoints/HealthCheckEndpointTests.cs \
  tasks/prd-go-live.md \
  docs/plans/2026-02-17-us004-health-checks.md

git commit -m "feat: add health check endpoints (US-004)

- Add /health (aggregate), /health/live (liveness), /health/ready (db) endpoints
- Register AddHealthChecks with EF Core DbContextCheck tagged 'ready'
- Custom JSON ResponseWriter returns status, totalDuration, entries
- All endpoints unauthenticated via AllowAnonymous()
- Add Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 8.0.2
- Integration tests covering all three endpoints and no-auth requirement"
```

---

## Verification Checklist

After the commit, verify manually:

```bash
# Start the API locally
cd /Users/pajgrtondrej/Work/GitHub/Chickquita/backend
dotnet run --project src/Chickquita.Api

# In another terminal:
curl http://localhost:5000/health | jq .
# Expected: { "status": "Healthy", "totalDuration": "...", "entries": { "database": { "status": "Healthy", ... } } }

curl http://localhost:5000/health/live | jq .
# Expected: { "status": "Healthy", "totalDuration": "...", "entries": {} }

curl http://localhost:5000/health/ready | jq .
# Expected: { "status": "Healthy", "totalDuration": "...", "entries": { "database": { "status": "Healthy", ... } } }
```
