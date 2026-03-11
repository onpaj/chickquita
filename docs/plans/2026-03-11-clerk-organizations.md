# Clerk Organizations Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Migrate Chickquita from a 1-user:1-tenant model to Clerk Organizations, so that one account can invite others to share flock management, while solo users experience no change.

**Architecture:** Every new user auto-gets a personal Clerk Organization (via Clerk dashboard setting). The `Tenant` entity's `ClerkUserId` is replaced by `ClerkOrgId`. `TenantResolutionMiddleware` reads the `org_id` JWT claim instead of `sub`. The webhook switches from `user.created` to `organization.created`. Frontend passes the active org in the Clerk token automatically — no UI change needed for solo users.

**Tech Stack:** Clerk Organizations API, `@clerk/clerk-react` (useOrganization hook), .NET 8 / EF Core / PostgreSQL (EF migration), xUnit + Moq + FluentAssertions

---

## Pre-flight Checklist (manual, before writing code)

1. In Clerk Dashboard → **Organizations** → enable Organizations feature.
2. Enable **"Automatic organization creation"** — every new user gets a personal org.
3. Add a new webhook endpoint (or update existing) to listen to `organization.created` event.
4. Note the `org_id` claim name in the Clerk JWT — it is literally `org_id`.

---

## Task 1: Update `Tenant` Domain Entity

**Goal:** Replace `ClerkUserId` / `Email` with `ClerkOrgId` / `Name` on the `Tenant` entity.

**Files:**
- Modify: `backend/src/Chickquita.Domain/Entities/Tenant.cs`

**Step 1: Write the failing test**

In `backend/tests/Chickquita.Domain.Tests/Entities/TenantTests.cs` (create file):

```csharp
using Chickquita.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Chickquita.Domain.Tests.Entities;

public class TenantTests
{
    [Fact]
    public void Create_WithValidOrgId_ReturnsTenantWithCorrectProperties()
    {
        var orgId = "org_abc123";
        var name = "Smith Farm";

        var tenant = Tenant.Create(orgId, name);

        tenant.ClerkOrgId.Should().Be(orgId);
        tenant.Name.Should().Be(name);
        tenant.Id.Should().NotBeEmpty();
        tenant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyOrgId_ThrowsArgumentException()
    {
        var act = () => Tenant.Create("", "Some Farm");
        act.Should().Throw<ArgumentException>().WithMessage("*org*");
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => Tenant.Create("org_abc", "");
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void UpdateName_WithValidName_UpdatesNameAndTimestamp()
    {
        var tenant = Tenant.Create("org_abc", "Old Name");
        var before = tenant.UpdatedAt;

        tenant.UpdateName("New Name");

        tenant.Name.Should().Be("New Name");
        tenant.UpdatedAt.Should().BeOnOrAfter(before);
    }
}
```

**Step 2: Run test to verify it fails**

```bash
cd backend && dotnet test tests/Chickquita.Domain.Tests --filter "TenantTests" -v minimal
```

Expected: FAIL — `ClerkOrgId` and `Name` don't exist yet.

**Step 3: Implement the entity**

Replace entire `backend/src/Chickquita.Domain/Entities/Tenant.cs`:

```csharp
namespace Chickquita.Domain.Entities;

public class Tenant
{
    public Guid Id { get; private set; }

    /// Clerk Organization ID (formerly ClerkUserId)
    public string ClerkOrgId { get; private set; } = string.Empty;

    /// Display name of the organization / farm
    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Tenant() { }

    public static Tenant Create(string clerkOrgId, string name)
    {
        if (string.IsNullOrWhiteSpace(clerkOrgId))
            throw new ArgumentException("Clerk org ID cannot be empty.", nameof(clerkOrgId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        var now = DateTime.UtcNow;
        return new Tenant
        {
            Id = Guid.NewGuid(),
            ClerkOrgId = clerkOrgId,
            Name = name,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Step 4: Run tests**

```bash
cd backend && dotnet test tests/Chickquita.Domain.Tests --filter "TenantTests" -v minimal
```

Expected: PASS (4 tests green).

**Step 5: Commit**

```bash
git add backend/src/Chickquita.Domain/Entities/Tenant.cs \
        backend/tests/Chickquita.Domain.Tests/Entities/TenantTests.cs
git commit -m "feat(domain): replace ClerkUserId/Email with ClerkOrgId/Name on Tenant entity"
```

---

## Task 2: Update EF Core Configuration & Migration

**Goal:** Rename `clerk_user_id` / `email` columns to `clerk_org_id` / `name` in the database.

**Files:**
- Modify: `backend/src/Chickquita.Infrastructure/Data/Configurations/TenantConfiguration.cs`
- Create: EF migration (auto-generated)

**Step 1: Update `TenantConfiguration.cs`**

Replace the entire file content:

```csharp
using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chickquita.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.ClerkOrgId)
            .HasColumnName("clerk_org_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(t => t.ClerkOrgId)
            .HasDatabaseName("ix_tenants_clerk_org_id")
            .IsUnique();
    }
}
```

**Step 2: Fix all compilation errors from changed properties**

Run build to see what breaks:

```bash
cd backend && dotnet build 2>&1 | grep -E "error|Error"
```

Fix each compiler error before proceeding (they will be in: `ITenantRepository`, `TenantRepository`, `SyncUserCommandHandler`, `TenantResolutionMiddleware`, `UserDto`, `AutoMapper profile`). Each is addressed in subsequent tasks.

**Step 3: Generate EF Core migration**

```bash
cd backend && dotnet ef migrations add RenameClerkUserIdToClerkOrgId \
  --project src/Chickquita.Infrastructure \
  --startup-project src/Chickquita.Api
```

Expected: Migration file created in `src/Chickquita.Infrastructure/Data/Migrations/`.

**Step 4: Review the generated migration**

Open the migration file and verify:
- It renames column `clerk_user_id` → `clerk_org_id`
- It renames column `email` → `name`
- It renames index `ix_tenants_clerk_user_id` → `ix_tenants_clerk_org_id`

If EF generated `DropColumn` + `AddColumn` instead of `RenameColumn`, edit the `Up()` method manually:

```csharp
migrationBuilder.RenameColumn(
    name: "email",
    table: "tenants",
    newName: "name");

migrationBuilder.RenameColumn(
    name: "clerk_user_id",
    table: "tenants",
    newName: "clerk_org_id");

migrationBuilder.RenameIndex(
    name: "ix_tenants_clerk_user_id",
    table: "tenants",
    newName: "ix_tenants_clerk_org_id");
```

And the `Down()` method (reverse of the above).

**Step 5: Commit**

```bash
git add backend/src/Chickquita.Infrastructure/Data/Configurations/TenantConfiguration.cs \
        backend/src/Chickquita.Infrastructure/Data/Migrations/
git commit -m "feat(infra): update tenant EF config and migration for Clerk org model"
```

---

## Task 3: Update `ITenantRepository` and `TenantRepository`

**Goal:** Replace `GetByClerkUserIdAsync` with `GetByClerkOrgIdAsync`.

**Files:**
- Modify: `backend/src/Chickquita.Application/Interfaces/ITenantRepository.cs`
- Modify: `backend/src/Chickquita.Infrastructure/Repositories/TenantRepository.cs`

**Step 1: Update interface**

In `ITenantRepository.cs`, replace `GetByClerkUserIdAsync` and `ExistsByClerkUserIdAsync`:

```csharp
using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByClerkOrgIdAsync(string clerkOrgId);
    Task<Tenant> AddAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task<bool> ExistsByClerkOrgIdAsync(string clerkOrgId);
}
```

**Step 2: Update repository implementation**

In `TenantRepository.cs`, rename the methods to match the new interface:

```csharp
public async Task<Tenant?> GetByClerkOrgIdAsync(string clerkOrgId)
    => await _context.Tenants
        .FirstOrDefaultAsync(t => t.ClerkOrgId == clerkOrgId);

public async Task<bool> ExistsByClerkOrgIdAsync(string clerkOrgId)
    => await _context.Tenants
        .AnyAsync(t => t.ClerkOrgId == clerkOrgId);
```

**Step 3: Build to confirm no errors**

```bash
cd backend && dotnet build 2>&1 | grep -E "error|Error"
```

Fix any remaining usages of `GetByClerkUserIdAsync` (will be in `SyncUserCommandHandler` and `TenantResolutionMiddleware` — addressed in Tasks 4 and 5).

**Step 4: Commit**

```bash
git add backend/src/Chickquita.Application/Interfaces/ITenantRepository.cs \
        backend/src/Chickquita.Infrastructure/Repositories/TenantRepository.cs
git commit -m "feat(infra): rename tenant repository methods for Clerk org model"
```

---

## Task 4: Update `SyncOrgCommand` (replaces `SyncUserCommand`)

**Goal:** Replace the `user.created`-driven `SyncUserCommand` with `SyncOrgCommand` that handles `organization.created` webhooks.

**Files:**
- Create: `backend/src/Chickquita.Application/Features/Users/Commands/SyncOrgCommand.cs`
- Create: `backend/src/Chickquita.Application/Features/Users/Commands/SyncOrgCommandHandler.cs`
- Keep: `SyncUserCommand.cs` and `SyncUserCommandHandler.cs` — **delete** them after the new handler is wired up and tested.

**Step 1: Write the failing test**

Create `backend/tests/Chickquita.Application.Tests/Features/Users/Commands/SyncOrgCommandHandlerTests.cs`:

```csharp
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Users.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chickquita.Application.Tests.Features.Users.Commands;

public class SyncOrgCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<SyncOrgCommandHandler>> _loggerMock = new();

    private SyncOrgCommandHandler CreateHandler()
        => new(_repoMock.Object, _mapperMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_NewOrg_CreatesAndReturnsTenant()
    {
        var command = new SyncOrgCommand { ClerkOrgId = "org_abc", Name = "Smith Farm" };
        var created = Tenant.Create("org_abc", "Smith Farm");

        _repoMock.Setup(r => r.GetByClerkOrgIdAsync("org_abc")).ReturnsAsync((Tenant?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Tenant>())).ReturnsAsync(created);
        _mapperMock.Setup(m => m.Map<TenantDto>(created)).Returns(new TenantDto { ClerkOrgId = "org_abc" });

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.AddAsync(It.Is<Tenant>(t => t.ClerkOrgId == "org_abc")), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingOrg_UpdatesNameAndReturnsExisting()
    {
        var command = new SyncOrgCommand { ClerkOrgId = "org_abc", Name = "New Name" };
        var existing = Tenant.Create("org_abc", "Old Name");

        _repoMock.Setup(r => r.GetByClerkOrgIdAsync("org_abc")).ReturnsAsync(existing);
        _mapperMock.Setup(m => m.Map<TenantDto>(existing)).Returns(new TenantDto { ClerkOrgId = "org_abc" });

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existing.Name.Should().Be("New Name");
        _repoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingOrg_SameName_DoesNotCallUpdate()
    {
        var command = new SyncOrgCommand { ClerkOrgId = "org_abc", Name = "Same Name" };
        var existing = Tenant.Create("org_abc", "Same Name");

        _repoMock.Setup(r => r.GetByClerkOrgIdAsync("org_abc")).ReturnsAsync(existing);
        _mapperMock.Setup(m => m.Map<TenantDto>(existing)).Returns(new TenantDto());

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tenant>()), Times.Never);
    }
}
```

**Step 2: Run test to verify it fails**

```bash
cd backend && dotnet test tests/Chickquita.Application.Tests \
  --filter "SyncOrgCommandHandlerTests" -v minimal
```

Expected: FAIL — `SyncOrgCommand` doesn't exist yet.

**Step 3: Create `SyncOrgCommand.cs`**

```csharp
using Chickquita.Application.DTOs;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Users.Commands;

public record SyncOrgCommand : IRequest<Result<TenantDto>>
{
    public string ClerkOrgId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
```

**Step 4: Create `SyncOrgCommandHandler.cs`**

```csharp
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Users.Commands;

public sealed class SyncOrgCommandHandler : IRequestHandler<SyncOrgCommand, Result<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncOrgCommandHandler> _logger;

    public SyncOrgCommandHandler(ITenantRepository tenantRepository, IMapper mapper,
        ILogger<SyncOrgCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TenantDto>> Handle(SyncOrgCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _tenantRepository.GetByClerkOrgIdAsync(request.ClerkOrgId);

            if (existing is not null)
            {
                if (existing.Name != request.Name)
                {
                    existing.UpdateName(request.Name);
                    await _tenantRepository.UpdateAsync(existing);
                }
                return Result<TenantDto>.Success(_mapper.Map<TenantDto>(existing));
            }

            var tenant = Tenant.Create(request.ClerkOrgId, request.Name);
            var added = await _tenantRepository.AddAsync(tenant);

            _logger.LogInformation("Created tenant {TenantId} for org {OrgId}", added.Id, request.ClerkOrgId);
            return Result<TenantDto>.Success(_mapper.Map<TenantDto>(added));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing org {OrgId}", request.ClerkOrgId);
            return Result<TenantDto>.Failure(Error.Failure($"Failed to sync org: {ex.Message}"));
        }
    }
}
```

**Step 5: Add `TenantDto` with `ClerkOrgId`**

Update `backend/src/Chickquita.Application/DTOs/TenantDto.cs` (or `UserDto.cs` — whichever is mapped):

```csharp
namespace Chickquita.Application.DTOs;

public sealed class TenantDto
{
    public Guid Id { get; set; }
    public string ClerkOrgId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

Also update the AutoMapper profile to map `Tenant` → `TenantDto` using `ClerkOrgId` and `Name`. Find the mapping profile in `backend/src/Chickquita.Infrastructure/Mapping/` or `Chickquita.Application/Mapping/` and update:

```csharp
CreateMap<Tenant, TenantDto>();
```

(AutoMapper maps by convention — property names match so no custom config needed.)

**Step 6: Run tests**

```bash
cd backend && dotnet test tests/Chickquita.Application.Tests \
  --filter "SyncOrgCommandHandlerTests" -v minimal
```

Expected: PASS (3 tests green).

**Step 7: Delete old SyncUserCommand files**

```bash
rm backend/src/Chickquita.Application/Features/Users/Commands/SyncUserCommand.cs
rm backend/src/Chickquita.Application/Features/Users/Commands/SyncUserCommandHandler.cs
```

**Step 8: Commit**

```bash
git add -A
git commit -m "feat(app): add SyncOrgCommand and handler, replace SyncUserCommand"
```

---

## Task 5: Update Webhook Endpoint

**Goal:** Handle `organization.created` (and `organization.updated`) instead of `user.created`.

**Files:**
- Modify: `backend/src/Chickquita.Api/Endpoints/WebhooksEndpoints.cs`

**Step 1: Write the failing test**

In `backend/tests/Chickquita.Api.Tests/Endpoints/ClerkWebhookEndpointTests.cs`, add two new test cases (existing tests will break — update them too):

```csharp
[Fact]
public async Task ClerkWebhook_WithOrganizationCreatedEvent_DispatchesSyncOrgCommand()
{
    // Arrange
    var orgId = "org_abc123";
    var orgName = "Smith Farm";

    var webhookPayload = new ClerkWebhookDto
    {
        Type = "organization.created",
        Data = new ClerkWebhookDataDto
        {
            Id = orgId,
            Name = orgName
        }
    };

    // validator mock returns success with above DTO
    // mediator mock captures the command

    // Assert: mediator.Send called with SyncOrgCommand { ClerkOrgId = orgId, Name = orgName }
}
```

> Note: Full test setup will mirror existing `ClerkWebhookEndpointTests` patterns. Read the existing test file for the exact mock setup pattern before writing.

**Step 2: Update the webhook handler**

In `WebhooksEndpoints.cs`, replace the `user.created` handler block:

```csharp
// Handle organization.created event
if (webhookDto.Type == "organization.created" || webhookDto.Type == "organization.updated")
{
    var syncCommand = new SyncOrgCommand
    {
        ClerkOrgId = webhookDto.Data.Id,
        Name = webhookDto.Data.Name ?? webhookDto.Data.Id  // fallback to ID if no name
    };

    await mediator.Send(syncCommand);
}
```

**Step 3: Update `ClerkWebhookDataDto`**

The existing DTO may only have `Id`, `EmailAddresses`, `PrimaryEmailAddressId`. Add `Name`:

Find the DTO file (likely `backend/src/Chickquita.Application/DTOs/ClerkWebhookDto.cs` or similar) and add:

```csharp
public string? Name { get; set; }
```

**Step 4: Build and verify tests pass**

```bash
cd backend && dotnet build && dotnet test tests/Chickquita.Api.Tests \
  --filter "ClerkWebhookEndpointTests" -v minimal
```

**Step 5: Commit**

```bash
git add backend/src/Chickquita.Api/Endpoints/WebhooksEndpoints.cs
git commit -m "feat(api): handle organization.created webhook instead of user.created"
```

---

## Task 6: Update `TenantResolutionMiddleware`

**Goal:** Resolve tenant from `org_id` JWT claim instead of `sub`.

**Files:**
- Modify: `backend/src/Chickquita.Api/Middleware/TenantResolutionMiddleware.cs`
- Modify: `backend/tests/Chickquita.Api.Tests/Middleware/TenantResolutionMiddlewareTests.cs`

**Step 1: Update tests first**

Replace the existing test file. Key changes:
- JWT claim is `org_id` (not `sub`)
- Fallback: if no tenant found for `org_id`, auto-create with name = `org_id` (not email)
- Remove email-related fallback tests, add org-fallback tests

```csharp
[Fact]
public async Task InvokeAsync_WithValidOrgId_FetchesTenantByOrgId()
{
    var clerkOrgId = "org_abc123";
    var tenant = Tenant.Create(clerkOrgId, "Smith Farm");

    var repoMock = new Mock<ITenantRepository>();
    repoMock.Setup(r => r.GetByClerkOrgIdAsync(clerkOrgId)).ReturnsAsync(tenant);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim("sub", "user_xyz"),     // user claim still present
        new Claim("org_id", clerkOrgId),  // active org claim
    }, "TestAuth"));

    var next = new Mock<RequestDelegate>();
    next.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

    var logger = new Mock<ILogger<TenantResolutionMiddleware>>();
    var middleware = new TenantResolutionMiddleware(next.Object);

    await middleware.InvokeAsync(httpContext, repoMock.Object, logger.Object);

    repoMock.Verify(r => r.GetByClerkOrgIdAsync(clerkOrgId), Times.Once);
    httpContext.Items["TenantId"].Should().Be(tenant.Id);
}

[Fact]
public async Task InvokeAsync_WithNoOrgIdClaim_DoesNotSetTenantId()
{
    var repoMock = new Mock<ITenantRepository>();
    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim("sub", "user_xyz"),   // user authenticated but no org active
    }, "TestAuth"));

    var next = new Mock<RequestDelegate>();
    next.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

    var logger = new Mock<ILogger<TenantResolutionMiddleware>>();
    var middleware = new TenantResolutionMiddleware(next.Object);

    await middleware.InvokeAsync(httpContext, repoMock.Object, logger.Object);

    httpContext.Items.Should().NotContainKey("TenantId");
    repoMock.Verify(r => r.GetByClerkOrgIdAsync(It.IsAny<string>()), Times.Never);
}

[Fact]
public async Task InvokeAsync_WhenTenantNotFound_AutoCreatesTenant()
{
    var clerkOrgId = "org_new";
    var created = Tenant.Create(clerkOrgId, clerkOrgId);

    var repoMock = new Mock<ITenantRepository>();
    repoMock.Setup(r => r.GetByClerkOrgIdAsync(clerkOrgId)).ReturnsAsync((Tenant?)null);
    repoMock.Setup(r => r.AddAsync(It.IsAny<Tenant>())).ReturnsAsync(created);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim("sub", "user_xyz"),
        new Claim("org_id", clerkOrgId),
    }, "TestAuth"));

    var next = new Mock<RequestDelegate>();
    next.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

    var logger = new Mock<ILogger<TenantResolutionMiddleware>>();
    var middleware = new TenantResolutionMiddleware(next.Object);

    await middleware.InvokeAsync(httpContext, repoMock.Object, logger.Object);

    repoMock.Verify(r => r.AddAsync(It.Is<Tenant>(t => t.ClerkOrgId == clerkOrgId)), Times.Once);
    httpContext.Items["TenantId"].Should().Be(created.Id);
}
```

**Step 2: Run tests to verify they fail**

```bash
cd backend && dotnet test tests/Chickquita.Api.Tests \
  --filter "TenantResolutionMiddlewareTests" -v minimal
```

Expected: Several FAIL.

**Step 3: Update middleware implementation**

Replace `TenantResolutionMiddleware.cs`:

```csharp
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Chickquita.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenantRepository,
        ILogger<TenantResolutionMiddleware> logger)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var clerkOrgId = context.User.FindFirst("org_id")?.Value;

            if (!string.IsNullOrEmpty(clerkOrgId))
            {
                var tenant = await tenantRepository.GetByClerkOrgIdAsync(clerkOrgId);

                if (tenant == null)
                {
                    logger.LogWarning(
                        "Tenant not found for org {ClerkOrgId}. Auto-creating.", clerkOrgId);

                    tenant = Tenant.Create(clerkOrgId, clerkOrgId); // name = orgId as fallback
                    tenant = await tenantRepository.AddAsync(tenant);

                    logger.LogInformation(
                        "Auto-created tenant {TenantId} for org {ClerkOrgId}", tenant.Id, clerkOrgId);
                }

                context.Items["TenantId"] = tenant.Id;
            }
        }

        await _next(context);
    }
}
```

**Step 4: Run tests**

```bash
cd backend && dotnet test tests/Chickquita.Api.Tests \
  --filter "TenantResolutionMiddlewareTests" -v minimal
```

Expected: PASS.

**Step 5: Run all backend tests**

```bash
cd backend && dotnet test -v minimal
```

Fix any remaining failures before committing.

**Step 6: Commit**

```bash
git add backend/src/Chickquita.Api/Middleware/TenantResolutionMiddleware.cs \
        backend/tests/Chickquita.Api.Tests/Middleware/TenantResolutionMiddlewareTests.cs
git commit -m "feat(api): resolve tenant from org_id JWT claim"
```

---

## Task 7: Update `GetCurrentUserQuery` and `UserDto`

**Goal:** Return `ClerkOrgId` and `Name` from `/api/users/me` instead of `ClerkUserId` and `Email`.

**Files:**
- Modify: `backend/src/Chickquita.Application/DTOs/UserDto.cs`
- Modify or replace with `TenantDto` mapping in `GetCurrentUserQueryHandler.cs`

**Step 1: Update `UserDto.cs`**

```csharp
namespace Chickquita.Application.DTOs;

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string ClerkOrgId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Step 2: Build and fix**

```bash
cd backend && dotnet build 2>&1 | grep -E "error|Error"
```

Fix any mapping issues in `GetCurrentUserQueryHandler.cs` — update it to use `GetByClerkOrgIdAsync` if needed. Check what claim it reads — update from `ClerkUserId` lookup to find the tenant via `TenantId` stored in `HttpContext.Items` (already set by middleware).

**Step 3: Run all tests**

```bash
cd backend && dotnet test -v minimal
```

**Step 4: Commit**

```bash
git add backend/src/Chickquita.Application/DTOs/UserDto.cs
git commit -m "feat(app): update UserDto to expose ClerkOrgId and Name"
```

---

## Task 8: Frontend — Pass Active Organization in JWT

**Goal:** Ensure Clerk sends `org_id` in the JWT. For solo users (auto-created org), this happens automatically once the org is active. No UI change is needed for solo users, but the frontend must request a token with the active org.

**Files:**
- Modify: `frontend/src/lib/useApiClient.ts`

**Step 1: Check Clerk's token template**

By default, Clerk does NOT include `org_id` in the JWT unless you configure a **JWT template** in the Clerk dashboard, OR you pass `{ template: 'with-org' }` to `getToken()`.

In Clerk Dashboard → **JWT Templates** → create a template named `chickquita` with:

```json
{
  "org_id": "{{org.id}}",
  "org_role": "{{org.role}}"
}
```

**Step 2: Update `useApiClient.ts` to request org-scoped token**

Find the `getToken()` call in `useApiClient.ts` or `apiClient.ts`:

```typescript
// Before:
const token = await getToken();

// After:
const token = await getToken({ template: 'chickquita' });
```

**Step 3: Verify in browser**

1. Run `npm run dev` in `frontend/`
2. Sign in
3. Open DevTools → Network tab → any API request
4. Check `Authorization: Bearer <token>` header
5. Decode the JWT at jwt.io — verify `org_id` claim is present

**Step 4: Commit**

```bash
git add frontend/src/lib/useApiClient.ts
git commit -m "feat(frontend): request org-scoped JWT token"
```

---

## Task 9: Frontend — Invite Members UI (Minimal)

**Goal:** Add a simple "Invite Member" button in Settings using Clerk's `useOrganization` hook. This is optional for solo users — it just doesn't show unless they want it.

**Files:**
- Modify: `frontend/src/features/settings/` (or wherever Settings page lives — check `frontend/src/pages/` or `frontend/src/features/`)

**Step 1: Find the Settings page**

```bash
find frontend/src -name "*etting*" -o -name "*Setting*" | head -10
```

**Step 2: Add invite UI**

In the Settings page component, add:

```tsx
import { useOrganization } from '@clerk/clerk-react';
import { useTranslation } from 'react-i18next';
import Button from '@mui/material/Button';

// Inside component:
const { organization, invitations } = useOrganization();
const { t } = useTranslation();

// In JSX (e.g., inside a "Members" section):
{organization && (
  <Button
    variant="outlined"
    onClick={() => organization.inviteMember({ emailAddress: inviteEmail, role: 'org:member' })}
  >
    {t('settings.inviteMember')}
  </Button>
)}
```

For MVP, use Clerk's hosted `<OrganizationProfile />` component to avoid building full invite UI:

```tsx
import { OrganizationProfile } from '@clerk/clerk-react';

// Add a route or modal:
<OrganizationProfile />
```

This gives invite + member management out of the box.

**Step 3: Add i18n keys**

In `frontend/src/locales/cs/translation.json`:
```json
"settings": {
  "members": "Členové farmy",
  "inviteMember": "Pozvat člena"
}
```

In `frontend/src/locales/en/translation.json`:
```json
"settings": {
  "members": "Farm Members",
  "inviteMember": "Invite Member"
}
```

**Step 4: Commit**

```bash
git add frontend/src/
git commit -m "feat(frontend): add farm member invite UI via Clerk OrganizationProfile"
```

---

## Task 10: Apply Database Migration

**Step 1: Apply migration to dev database**

```bash
cd backend && dotnet ef database update \
  --project src/Chickquita.Infrastructure \
  --startup-project src/Chickquita.Api
```

Expected: Migration runs successfully, no errors.

**Step 2: Verify columns in DB (optional)**

Connect to Neon and run:
```sql
\d tenants
```

Expected: columns `clerk_org_id`, `name` (instead of `clerk_user_id`, `email`).

**Step 3: Handle existing data (if dev DB has users)**

If you have existing rows in `tenants`, you need to populate `clerk_org_id` with actual Clerk org IDs. For a dev environment, it's easiest to:
1. Drop all rows from `tenants` (dev only — no production data yet)
2. Re-register user accounts (they'll get auto-created orgs via new webhook)

```sql
-- Dev only!
DELETE FROM daily_records;
DELETE FROM purchases;
DELETE FROM flock_history;
DELETE FROM flocks;
DELETE FROM coops;
DELETE FROM tenants;
```

**Step 4: Commit migration if not already committed**

```bash
git add backend/src/Chickquita.Infrastructure/Data/Migrations/
git commit -m "feat(infra): apply clerk org migration to database"
```

---

## Task 11: Run Full Test Suite

**Step 1: Run all backend tests**

```bash
cd backend && dotnet test -v normal 2>&1 | tail -30
```

Expected: All green. Fix any remaining failures.

**Step 2: Run frontend tests**

```bash
cd frontend && npm test -- --run
```

Expected: All green.

**Step 3: Manual smoke test**

1. Start backend: `cd backend && dotnet run --project src/Chickquita.Api`
2. Start frontend: `cd frontend && npm run dev`
3. Sign up with a new account
4. Verify auto-org is created (check DB: `SELECT * FROM tenants`)
5. Verify dashboard loads with data
6. Go to Settings → verify member invite UI appears

**Step 4: Final commit**

```bash
git add -A
git commit -m "feat: clerk organizations migration complete"
```

---

## Summary of Changes

| Area | Change |
|------|--------|
| `Tenant` entity | `ClerkUserId`/`Email` → `ClerkOrgId`/`Name` |
| DB schema | `clerk_user_id`/`email` → `clerk_org_id`/`name` |
| `ITenantRepository` | `GetByClerkUserIdAsync` → `GetByClerkOrgIdAsync` |
| `SyncUserCommand` | Deleted — replaced by `SyncOrgCommand` |
| Webhook handler | `user.created` → `organization.created` |
| `TenantResolutionMiddleware` | Reads `org_id` claim instead of `sub` |
| `UserDto` | `ClerkUserId`/`Email` → `ClerkOrgId`/`Name` |
| Frontend JWT | `getToken({ template: 'chickquita' })` includes `org_id` |
| Settings UI | `<OrganizationProfile />` for invite management |
| Clerk Dashboard | Enable Organizations + auto-creation + JWT template |

## Clerk Dashboard Checklist

- [ ] Organizations feature enabled
- [ ] Automatic organization creation enabled
- [ ] Webhook: `organization.created` event added
- [ ] Webhook: `organization.updated` event added
- [ ] JWT Template `chickquita` created with `org_id` and `org_role` claims
- [ ] Webhook URL updated if endpoint path changed
