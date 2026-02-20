# Test Strategy

**Chickquita (Chickquita)** - Comprehensive testing approach for backend and frontend, covering unit, integration, and E2E tests.

**Version:** 1.1
**Date:** February 9, 2026
**Status:** Updated (Backend section revised to reflect EF Core/Postgres implementation)
**Test Count:** 730+ backend tests (166 Domain, 336 Application, 119 Infrastructure, 109 API)

---

## Table of Contents

- [Testing Philosophy](#testing-philosophy)
- [Backend Unit Tests](#backend-unit-tests)
- [Backend Integration Tests](#backend-integration-tests)
- [Frontend Unit Tests](#frontend-unit-tests)
- [E2E Tests with Playwright](#e2e-tests-with-playwright)
- [Test Data Management](#test-data-management)
- [CI/CD Integration](#cicd-integration)

---

## Testing Philosophy

### Testing Pyramid

```
         /\
        /E2E\          10% - End-to-End (Critical user flows)
       /------\
      /  Inte- \       30% - Integration (API + DB, Components + Hooks)
     /  gration \
    /------------\
   /    Unit      \    60% - Unit (Domain logic, utilities, pure functions)
  /________________\
```

### Core Principles

**1. Fast Feedback**
- Unit tests run in < 5 seconds
- Integration tests in < 30 seconds
- E2E tests in < 2 minutes

**2. Confidence Over Coverage**
- 80% meaningful coverage > 100% superficial
- Focus on critical paths and business logic
- Skip trivial getters/setters

**3. Test Behavior, Not Implementation**
- Refactoring shouldn't break tests
- Test public APIs, not private methods
- Mock external dependencies, not internal logic

**4. Arrange-Act-Assert (AAA)**
- Clear test structure
- One assertion per test (when possible)
- Descriptive test names

**5. No Flaky Tests**
- Fix or delete intermittent failures immediately
- No time-dependent tests without proper mocking
- Deterministic test data

### Coverage Targets

**Backend:**
- Overall: 80%
- Domain logic: 90%
- Command/Query handlers: 85%
- Infrastructure: 70%

**Frontend:**
- Overall: 70%
- Business logic (hooks, utils): 80%
- Components: 65%
- UI components (shared): 60%

**Critical Paths: 100%**
- Authentication flow
- Egg cost calculation
- Mature chicks logic
- Daily record creation (offline)
- Background sync

### What to Test

- ✅ Business logic (domain entities, calculations)
- ✅ Validation rules (FluentValidation, Zod)
- ✅ API endpoints (integration tests)
- ✅ Critical user flows (E2E)
- ✅ Error handling paths
- ✅ Edge cases and boundary conditions

### What NOT to Test

- ❌ Framework code (EF Core, React internals)
- ❌ Third-party libraries (MUI, MediatR)
- ❌ Simple DTOs/POCOs without logic
- ❌ Trivial getters/setters
- ❌ Generated code

### Test Naming Convention

```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public void MatureChicks_WithInsufficientChicks_ThrowsException() { }

[Fact]
public void CalculateEggCost_WithNoProduction_ReturnsZero() { }

[Fact]
public void CreateFlock_WithValidData_ReturnsSuccess() { }
```

---

## Backend Unit Tests

### Stack

- **xUnit** - Test framework
- **Moq** - Mocking library
- **AutoFixture** - Test data generation
- **FluentAssertions** - Fluent assertion syntax

### Test Project Setup

```xml
<!-- Chickquita.Tests/Application.Tests/Application.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xUnit" Version="2.6.0" />
    <PackageReference Include="xUnit.runner.visualstudio" Version="2.5.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Moq" Version="4.20.0" />
    <PackageReference Include="AutoFixture" Version="4.18.0" />
    <PackageReference Include="AutoFixture.Xunit2" Version="4.18.0" />
    <PackageReference Include="AutoFixture.AutoMoq" Version="4.18.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Chickquita.Application\Chickquita.Application.csproj" />
    <ProjectReference Include="..\..\Chickquita.Domain\Chickquita.Domain.csproj" />
  </ItemGroup>
</Project>
```

### AutoFixture Setup

```csharp
// TestHelpers/AutoMoqDataAttribute.cs
public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() => new Fixture().Customize(new AutoMoqCustomization()))
    {
    }
}

// TestHelpers/InlineAutoMoqDataAttribute.cs
public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] objects)
        : base(new AutoMoqDataAttribute(), objects)
    {
    }
}
```

### Domain Entity Tests

Domain entity tests use **pure unit tests** with no external dependencies:

```csharp
// Domain.Tests/Entities/CoopTests.cs (actual test patterns)
public class CoopTests
{
    private readonly Guid _validTenantId = Guid.NewGuid();
    private const string ValidName = "Main Coop";
    private const string ValidLocation = "North Field";

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);

        // Assert
        coop.Should().NotBeNull();
        coop.Id.Should().NotBeEmpty();
        coop.TenantId.Should().Be(_validTenantId);
        coop.Name.Should().Be(ValidName);
        coop.Location.Should().Be(ValidLocation);
        coop.IsActive.Should().BeTrue();
        coop.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceName_ShouldThrowArgumentException(
        string? invalidName)
    {
        // Arrange & Act
        var act = () => Coop.Create(_validTenantId, invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Coop name cannot be empty.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateNameAndLocation()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);
        var originalCreatedAt = coop.CreatedAt;
        Thread.Sleep(10); // Ensure time difference

        var newName = "Updated Coop";
        var newLocation = "South Field";

        // Act
        coop.Update(newName, newLocation);

        // Assert
        coop.Name.Should().Be(newName);
        coop.Location.Should().Be(newLocation);
        coop.CreatedAt.Should().Be(originalCreatedAt);
        coop.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName);
        var originalUpdatedAt = coop.UpdatedAt;
        Thread.Sleep(10);

        // Act
        coop.Deactivate();

        // Assert
        coop.IsActive.Should().BeFalse();
        coop.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}

// Domain.Tests/Entities/FlockTests.cs (composition tests)
public class FlockTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var hatchDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var flock = Flock.Create(
            tenantId, coopId, "Spring 2024",
            hatchDate, hens: 10, roosters: 2, chicks: 5);

        // Assert
        flock.Should().NotBeNull();
        flock.CurrentHens.Should().Be(10);
        flock.CurrentRoosters.Should().Be(2);
        flock.CurrentChicks.Should().Be(5);
        flock.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 0, 0)] // Negative hens
    [InlineData(0, -1, 0)] // Negative roosters
    [InlineData(0, 0, -1)] // Negative chicks
    public void Create_WithNegativeComposition_ShouldThrowArgumentException(
        int hens, int roosters, int chicks)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();

        // Act
        var act = () => Flock.Create(
            tenantId, coopId, "Test",
            DateTime.UtcNow, hens, roosters, chicks);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateComposition_WithValidData_ShouldSucceed()
    {
        // Arrange
        var flock = Flock.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Test",
            DateTime.UtcNow, hens: 10, roosters: 2, chicks: 5);

        // Act
        flock.UpdateComposition(hens: 12, roosters: 3, chicks: 8, reason: "Purchase");

        // Assert
        flock.CurrentHens.Should().Be(12);
        flock.CurrentRoosters.Should().Be(3);
        flock.CurrentChicks.Should().Be(8);
    }
}
```

**Actual Test Files:**
- `backend/tests/Chickquita.Domain.Tests/Entities/CoopTests.cs` - 24 tests
- `backend/tests/Chickquita.Domain.Tests/Entities/FlockTests.cs` - Flock domain logic
- `backend/tests/Chickquita.Domain.Tests/Entities/FlockHistoryTests.cs` - History immutability
- `backend/tests/Chickquita.Domain.Tests/Entities/DailyRecordTests.cs` - Daily record validation
- `backend/tests/Chickquita.Domain.Tests/Entities/PurchaseTests.cs` - Purchase domain logic

### Command Handler Tests (Application Layer)

Command handler tests use **Moq** for repository mocking and **AutoFixture** for test data generation:

```csharp
// Application.Tests/Features/Coops/Commands/CreateCoopCommandHandlerTests.cs (actual pattern)
public class CreateCoopCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreateCoopCommandHandler>> _mockLogger;
    private readonly CreateCoopCommandHandler _handler;

    public CreateCoopCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<CreateCoopCommandHandler>>>();

        _handler = new CreateCoopCommandHandler(
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateCoopSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        var createdCoop = Coop.Create(tenantId, command.Name, command.Location);
        _mockCoopRepository.Setup(x => x.AddAsync(It.IsAny<Coop>()))
            .ReturnsAsync(createdCoop);

        var expectedDto = new CoopDto
        {
            Id = createdCoop.Id,
            TenantId = tenantId,
            Name = command.Name,
            Location = command.Location,
            IsActive = true,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<CoopDto>(It.IsAny<Coop>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.TenantId.Should().Be(tenantId);

        _mockCoopRepository.Verify(
            x => x.ExistsByNameAsync(command.Name), Times.Once);
        _mockCoopRepository.Verify(
            x => x.AddAsync(It.IsAny<Coop>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnConflictError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(true); // Simulate duplicate name

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Message.Should().Be("A coop with this name already exists");

        _mockCoopRepository.Verify(
            x => x.AddAsync(It.IsAny<Coop>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Unauthorized");
        _mockCoopRepository.Verify(
            x => x.AddAsync(It.IsAny<Coop>()), Times.Never);
    }
}
```

**Actual Test Files:**
- `backend/tests/Chickquita.Application.Tests/Features/Coops/Commands/CreateCoopCommandHandlerTests.cs`
- `backend/tests/Chickquita.Application.Tests/Features/Flocks/Commands/CreateFlockCommandHandlerTests.cs`
- `backend/tests/Chickquita.Application.Tests/Features/Purchases/Commands/CreatePurchaseCommandHandlerTests.cs`
- `backend/tests/Chickquita.Application.Tests/Features/DailyRecords/Commands/CreateDailyRecordCommandHandlerTests.cs`

### Validator Tests (FluentValidation)

Validator tests use **FluentValidation.TestHelper** for concise assertion syntax:

```csharp
// Application.Tests/Features/Flocks/Commands/CreateFlockCommandValidatorTests.cs (actual pattern)
public class CreateFlockCommandValidatorTests
{
    private readonly CreateFlockCommandValidator _validator;

    public CreateFlockCommandValidatorTests()
    {
        _validator = new CreateFlockCommandValidator();
    }

    [Fact]
    public void Validate_WithEmptyCoopId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.Empty,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoopId)
            .WithErrorMessage("Coop ID is required.");
    }

    [Fact]
    public void Validate_WithEmptyIdentifier_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = string.Empty,
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Identifier)
            .WithErrorMessage("Flock identifier is required.");
    }

    [Theory]
    [InlineData(-1, 0, 0)] // Negative hens
    [InlineData(0, -1, 0)] // Negative roosters
    [InlineData(0, 0, -1)] // Negative chicks
    public void Validate_WithNegativeComposition_ShouldHaveValidationError(
        int hens, int roosters, int chicks)
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test",
            HatchDate = DateTime.UtcNow,
            InitialHens = hens,
            InitialRoosters = roosters,
            InitialChicks = chicks
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert - At least one validation error for negative values
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithAllZeroCounts_ShouldHaveValidationError()
    {
        // Arrange - Business rule: at least one count must be > 0
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test",
            HatchDate = DateTime.UtcNow,
            InitialHens = 0,
            InitialRoosters = 0,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("at least one count"));
    }
}
```

**Actual Test Files:**
- `backend/tests/Chickquita.Application.Tests/Features/Flocks/Commands/CreateFlockCommandValidatorTests.cs`
- `backend/tests/Chickquita.Application.Tests/Features/Flocks/Commands/UpdateFlockCommandValidatorTests.cs`
- `backend/tests/Chickquita.Application.Tests/Features/DailyRecords/Commands/CreateDailyRecordCommandValidatorTests.cs`
- `backend/tests/Chickquita.Application.Tests/Features/Purchases/Commands/CreatePurchaseCommandValidatorTests.cs`

---

## Backend Integration Tests

### Stack

- **WebApplicationFactory** - In-memory test server
- **EF Core InMemory Provider** - For fast API endpoint tests
- **SQLite In-Memory** - For repository integration tests
- **xUnit** with IClassFixture for test isolation

### Test Infrastructure Setup

**API Integration Tests** use `WebApplicationFactory` with EF Core InMemory database:

```csharp
// Api.Tests/Endpoints/CoopsEndpointsTests.cs (example)
public class CoopsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CoopsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // Helper: Replace production DbContext with InMemory database
    private static void ReplaceWithInMemoryDatabase(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Use unique database name for test isolation
        var databaseName = $"TestDb_{Guid.NewGuid()}";

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseInMemoryDatabase(databaseName);
            options.EnableSensitiveDataLogging();
        });

        // Bypass authentication for tests
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        });
    }

    // Helper: Mock current user service for tenant isolation
    private static void ReplaceCurrentUserService(
        IServiceCollection services,
        Mock<ICurrentUserService> mock)
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(ICurrentUserService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddScoped(_ => mock.Object);
    }
}
```

**Repository Integration Tests** use SQLite In-Memory database:

```csharp
// Infrastructure.Tests/Repositories/DailyRecordRepositoryTests.cs (example)
public class DailyRecordRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly DailyRecordRepository _repository;

    public DailyRecordRepositoryTests()
    {
        // Use SQLite in-memory database for realistic DB behavior
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated(); // Apply migrations/schema

        _repository = new DailyRecordRepository(_dbContext);

        // Seed required test data (tenants, coops, flocks)
        SeedTestData();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    private void SeedTestData()
    {
        var tenant = Tenant.Create("clerk_user_test", "test@example.com");
        _dbContext.Tenants.Add(tenant);
        // ... seed coops, flocks, etc.
        _dbContext.SaveChanges();
    }
}
```

### API Endpoint Tests

API integration tests use `WebApplicationFactory` with EF Core InMemory database and mock authentication:

```csharp
// Api.Tests/Endpoints/CoopsEndpointsTests.cs (actual implementation pattern)
public class CoopsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CoopsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateCoop_WithValidData_Returns201Created()
    {
        // Arrange - Setup test factory with InMemory DB and mock user
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        // Seed tenant data
        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        var command = new CreateCoopCommand
        {
            Name = "Test Coop",
            Location = "Test Location"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/coops", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CoopDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Coop");
        result.Location.Should().Be("Test Location");
        result.TenantId.Should().Be(tenantId);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCoop_WithInvalidData_Returns400BadRequest()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        // Command with empty name (validation should fail)
        var command = new CreateCoopCommand
        {
            Name = "",
            Location = "Test Location"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/coops", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TenantIsolation_UserCannotSeeOtherTenantCoops()
    {
        // Arrange - Create two tenants with separate data
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var mockCurrentUser1 = CreateMockCurrentUser("clerk_user_1", tenant1Id);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser1);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenant1Id, "clerk_user_1");
        await SeedTenant(scope, tenant2Id, "clerk_user_2");
        await SeedCoop(scope, tenant1Id, "Tenant 1 Coop", "Location 1");
        await SeedCoop(scope, tenant2Id, "Tenant 2 Coop", "Location 2");

        var client = factory.CreateClient();

        // Act - Request as tenant1 user
        var response = await client.GetAsync("/api/coops");

        // Assert - Only tenant1's coops returned
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<CoopDto>>();
        result.Should().NotBeNull();
        result!.Should().AllSatisfy(c => c.TenantId.Should().Be(tenant1Id));
        result.Should().NotContain(c => c.Name == "Tenant 2 Coop");
    }

    // Helper Methods
    private static Mock<ICurrentUserService> CreateMockCurrentUser(
        string clerkUserId, Guid tenantId)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(x => x.ClerkUserId).Returns(clerkUserId);
        mock.Setup(x => x.TenantId).Returns(tenantId);
        mock.Setup(x => x.IsAuthenticated).Returns(true);
        return mock;
    }

    private static async Task SeedTenant(
        IServiceScope scope, Guid tenantId, string clerkUserId)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenant = Tenant.Create(clerkUserId, $"{clerkUserId}@test.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<Guid> SeedCoop(
        IServiceScope scope, Guid tenantId, string name, string location)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var coop = Coop.Create(tenantId, name, location);
        dbContext.Coops.Add(coop);
        await dbContext.SaveChangesAsync();
        return coop.Id;
    }
}
```

### Repository Integration Tests (with SQLite In-Memory)

Repository tests use **SQLite In-Memory** databases for realistic relational database behavior:

```csharp
// Infrastructure.Tests/Repositories/DailyRecordRepositoryTests.cs (actual pattern)
public class DailyRecordRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly DailyRecordRepository _repository;
    private readonly Guid _tenantId;
    private readonly Guid _flockId;

    public DailyRecordRepositoryTests()
    {
        // Use SQLite in-memory database for testing
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated(); // Apply schema

        _repository = new DailyRecordRepository(_dbContext);

        // Seed required test data
        _tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("clerk_user_test", "test@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, _tenantId);
        _dbContext.Tenants.Add(tenant);

        var coop = Coop.Create(_tenantId, "Test Coop", "Test Location");
        _dbContext.Coops.Add(coop);
        _dbContext.SaveChanges();

        var flock = Flock.Create(
            _tenantId, coop.Id, "TEST-FLOCK",
            DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        _dbContext.SaveChanges();
        _flockId = flock.Id;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDailyRecords_OrderedByDateDescending()
    {
        // Arrange
        var record1 = DailyRecord.Create(
            _tenantId, _flockId, DateTime.UtcNow.AddDays(-2), 10, "Record 1");
        var record2 = DailyRecord.Create(
            _tenantId, _flockId, DateTime.UtcNow.AddDays(-1), 15, "Record 2");
        var record3 = DailyRecord.Create(
            _tenantId, _flockId, DateTime.UtcNow, 12, "Record 3");

        _dbContext.DailyRecords.AddRange(record1, record2, record3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].RecordDate.Should().Be(record3.RecordDate); // Most recent first
        result[1].RecordDate.Should().Be(record2.RecordDate);
        result[2].RecordDate.Should().Be(record1.RecordDate);
    }

    [Fact]
    public async Task AddAsync_AddsDailyRecord_Successfully()
    {
        // Arrange
        var record = DailyRecord.Create(
            _tenantId, _flockId, DateTime.UtcNow, 15, "Test record");

        // Act
        var result = await _repository.AddAsync(record);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.EggCount.Should().Be(15);

        var retrieved = await _repository.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Notes.Should().Be("Test record");
    }

    [Fact]
    public async Task ExistsForFlockAndDateAsync_ReturnsTrue_WhenRecordExists()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var record = DailyRecord.Create(_tenantId, _flockId, date, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsForFlockAndDateAsync(_flockId, date);

        // Assert
        exists.Should().BeTrue();
    }
}
```

**Data Integrity Tests** verify database constraints:

```csharp
// Infrastructure.Tests/Data/FlockDataIntegrityTests.cs (constraint testing)
[Fact]
public async Task Flock_CheckConstraint_HensCannotBeNegative()
{
    // Arrange
    var flock = Flock.Create(_tenantId, _coopId, "TEST", DateTime.UtcNow, 10, 2, 5, null);
    _dbContext.Flocks.Add(flock);
    await _dbContext.SaveChangesAsync();

    // Act - Try to set negative hens value
    var flockEntity = await _dbContext.Flocks.FindAsync(flock.Id);
    typeof(Flock).GetProperty("CurrentHens")!.SetValue(flockEntity, -1);

    // Assert - Should throw on SaveChanges due to CHECK constraint
    var act = async () => await _dbContext.SaveChangesAsync();
    await act.Should().ThrowAsync<DbUpdateException>();
}

[Fact]
public async Task Flock_UniqueConstraint_IdentifierMustBeUniquePerCoop()
{
    // Arrange
    var flock1 = Flock.Create(_tenantId, _coopId, "DUPLICATE", DateTime.UtcNow, 10, 2, 5, null);
    var flock2 = Flock.Create(_tenantId, _coopId, "DUPLICATE", DateTime.UtcNow, 8, 1, 3, null);

    // Act
    _dbContext.Flocks.Add(flock1);
    await _dbContext.SaveChangesAsync();

    _dbContext.Flocks.Add(flock2);
    var act = async () => await _dbContext.SaveChangesAsync();

    // Assert - Unique constraint violation
    await act.Should().ThrowAsync<DbUpdateException>();
}
```

---

## Frontend Unit Tests

### Stack

- **Vitest** - Test runner (faster than Jest)
- **React Testing Library** - Component testing
- **@testing-library/user-event** - User interactions
- **MSW (Mock Service Worker)** - API mocking

### Vitest Configuration

```typescript
// vitest.config.ts
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./tests/setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'tests/',
        '**/*.types.ts',
        '**/*.config.ts',
        '**/*.d.ts',
      ],
      thresholds: {
        lines: 70,
        functions: 70,
        branches: 65,
        statements: 70,
      },
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
```

### Test Setup

```typescript
// tests/setup.ts
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';

// Cleanup after each test
afterEach(() => {
  cleanup();
});

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// Mock IntersectionObserver
global.IntersectionObserver = class IntersectionObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  takeRecords() {
    return [];
  }
  unobserve() {}
} as any;
```

### Component Tests

```typescript
// features/flocks/components/FlockCard.test.tsx
import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { FlockCard } from './FlockCard';
import type { FlockDto } from '../types/flock.types';

describe('FlockCard', () => {
  const mockFlock: FlockDto = {
    id: 'flock-1',
    coopId: 'coop-1',
    coopName: 'Kurník 1',
    identifier: 'Hnědé 2024',
    hatchDate: new Date('2024-01-15'),
    currentHens: 15,
    currentRoosters: 2,
    currentChicks: 3,
    isActive: true,
  };

  it('renders flock information correctly', () => {
    // Arrange & Act
    render(<FlockCard flock={mockFlock} onEdit={vi.fn()} onDelete={vi.fn()} />);

    // Assert
    expect(screen.getByText('Hnědé 2024')).toBeInTheDocument();
    expect(screen.getByText(/15/)).toBeInTheDocument(); // Hens
    expect(screen.getByText(/2/)).toBeInTheDocument();  // Roosters
    expect(screen.getByText(/3/)).toBeInTheDocument();  // Chicks
  });

  it('calls onEdit when edit button is clicked', () => {
    // Arrange
    const onEdit = vi.fn();
    render(<FlockCard flock={mockFlock} onEdit={onEdit} onDelete={vi.fn()} />);

    // Act
    const editButton = screen.getByRole('button', { name: /upravit/i });
    fireEvent.click(editButton);

    // Assert
    expect(onEdit).toHaveBeenCalledWith('flock-1');
    expect(onEdit).toHaveBeenCalledTimes(1);
  });

  it('shows confirmation before delete', () => {
    // Arrange
    const onDelete = vi.fn();
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    render(<FlockCard flock={mockFlock} onEdit={vi.fn()} onDelete={onDelete} />);

    // Act
    const deleteButton = screen.getByRole('button', { name: /smazat/i });
    fireEvent.click(deleteButton);

    // Assert
    expect(window.confirm).toHaveBeenCalled();
    expect(onDelete).toHaveBeenCalledWith('flock-1');
  });

  it('does not delete if confirmation is cancelled', () => {
    // Arrange
    const onDelete = vi.fn();
    vi.spyOn(window, 'confirm').mockReturnValue(false);
    render(<FlockCard flock={mockFlock} onEdit={vi.fn()} onDelete={onDelete} />);

    // Act
    const deleteButton = screen.getByRole('button', { name: /smazat/i });
    fireEvent.click(deleteButton);

    // Assert
    expect(window.confirm).toHaveBeenCalled();
    expect(onDelete).not.toHaveBeenCalled();
  });
});
```

### Hook Tests

```typescript
// features/flocks/hooks/useFlocks.test.ts
import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useFlocks } from './useFlocks';
import * as flocksApi from '../api/flocksApi';

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('useFlocks', () => {
  it('fetches flocks successfully', async () => {
    // Arrange
    const mockFlocks = [
      { id: '1', identifier: 'Flock 1' },
      { id: '2', identifier: 'Flock 2' },
    ];
    vi.spyOn(flocksApi, 'getFlocks').mockResolvedValue(mockFlocks);

    // Act
    const { result } = renderHook(() => useFlocks('coop-1'), {
      wrapper: createWrapper(),
    });

    // Assert
    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.flocks).toEqual(mockFlocks);
    expect(result.current.error).toBeNull();
  });

  it('handles fetch error', async () => {
    // Arrange
    const mockError = new Error('Network error');
    vi.spyOn(flocksApi, 'getFlocks').mockRejectedValue(mockError);

    // Act
    const { result } = renderHook(() => useFlocks('coop-1'), {
      wrapper: createWrapper(),
    });

    // Assert
    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.flocks).toEqual([]);
    expect(result.current.error).toBeTruthy();
  });
});
```

---

## E2E Tests with Playwright

### Playwright Configuration

```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  timeout: 30000,

  use: {
    baseURL: 'http://localhost:3100',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    },
  ],

  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:3100',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});
```

### Page Object Model

```typescript
// tests/e2e/pages/LoginPage.ts
import { Page, Locator } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly submitButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.emailInput = page.getByLabel(/email/i);
    this.passwordInput = page.getByLabel(/heslo|password/i);
    this.submitButton = page.getByRole('button', { name: /přihlásit|login/i });
    this.errorMessage = page.getByRole('alert');
  }

  async goto() {
    await this.page.goto('/login');
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.submitButton.click();
  }

  async expectLoginSuccess() {
    await this.page.waitForURL('/');
  }
}

// tests/e2e/pages/DashboardPage.ts
export class DashboardPage {
  readonly page: Page;
  readonly quickAddButton: Locator;
  readonly todayEggCount: Locator;

  constructor(page: Page) {
    this.page = page;
    this.quickAddButton = page.getByRole('button', { name: /přidat/i });
    this.todayEggCount = page.getByTestId('today-egg-count');
  }

  async goto() {
    await this.page.goto('/');
  }

  async openQuickAdd() {
    await this.quickAddButton.click();
  }
}
```

### E2E Test Examples

```typescript
// tests/e2e/auth.spec.ts
import { test, expect } from '@playwright/test';
import { LoginPage } from './pages/LoginPage';

test.describe('Authentication', () => {
  test('user can register and login', async ({ page }) => {
    const email = `test-${Date.now()}@example.com`;
    const password = 'Test123!';

    // Register
    await page.goto('/register');
    await page.getByLabel(/email/i).fill(email);
    await page.getByLabel(/heslo/i).fill(password);
    await page.getByRole('button', { name: /registrovat/i }).click();

    // Should redirect to dashboard
    await expect(page).toHaveURL('/');
    await expect(page.getByText(/dashboard/i)).toBeVisible();
  });

  test('login fails with invalid credentials', async ({ page }) => {
    const loginPage = new LoginPage(page);

    await loginPage.goto();
    await loginPage.login('invalid@example.com', 'wrongpassword');

    await expect(loginPage.errorMessage).toBeVisible();
    await expect(loginPage.errorMessage).toContainText(/neplatné/i);
  });
});

// tests/e2e/daily-records.spec.ts
test.describe('Daily Records', () => {
  test.beforeEach(async ({ page }) => {
    // Register and login
    const email = `test-${Date.now()}@example.com`;
    await page.goto('/register');
    await page.getByLabel(/email/i).fill(email);
    await page.getByLabel(/heslo/i).fill('Test123!');
    await page.getByRole('button', { name: /registrovat/i }).click();
    await page.waitForURL('/');
  });

  test('user can add daily record via quick add in under 30 seconds', async ({ page }) => {
    const startTime = Date.now();

    // Open quick add
    await page.getByRole('button', { name: /přidat/i }).click();

    // Fill form
    await page.getByLabel(/hejno/i).selectOption({ index: 0 });
    await page.getByRole('button', { name: '+' }).click();
    await page.getByRole('button', { name: '+' }).click();
    await page.getByRole('button', { name: '+' }).click(); // Egg count = 3

    // Submit
    await page.getByRole('button', { name: /uložit/i }).click();

    // Verify success
    await expect(page.getByText(/úspěšně/i)).toBeVisible();

    const duration = Date.now() - startTime;
    expect(duration).toBeLessThan(30000); // Must be under 30 seconds
  });
});

// tests/e2e/offline-mode.spec.ts
test.describe('Offline Mode', () => {
  test('app queues record when offline', async ({ page, context }) => {
    // Setup auth
    const email = `test-${Date.now()}@example.com`;
    await page.goto('/register');
    await page.getByLabel(/email/i).fill(email);
    await page.getByLabel(/heslo/i).fill('Test123!');
    await page.getByRole('button', { name: /registrovat/i }).click();
    await page.waitForURL('/');

    // Go offline
    await context.setOffline(true);

    // Add record
    await page.getByRole('button', { name: /přidat/i }).click();
    await page.getByLabel(/hejno/i).selectOption({ index: 0 });
    await page.getByRole('button', { name: '+' }).click();
    await page.getByRole('button', { name: /uložit/i }).click();

    // Verify queued
    await expect(page.getByText(/uloží lokálně/i)).toBeVisible();
    await expect(page.getByText(/neuložené záznamy/i)).toBeVisible();

    // Go online
    await context.setOffline(false);

    // Sync
    await page.getByRole('button', { name: /synchronizovat/i }).click();

    // Verify synced
    await expect(page.getByText(/synchronizováno/i)).toBeVisible();
  });
});
```

---

## Test Data Management

### Backend Test Data

```csharp
// TestHelpers/TestDataBuilder.cs
public class TestDataBuilder
{
    public static Flock CreateFlock(
        string? id = null,
        int hens = 10,
        int roosters = 2,
        int chicks = 5)
    {
        return new Flock
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Identifier = $"Test Flock {Guid.NewGuid()}",
            CoopId = Guid.NewGuid().ToString(),
            HatchDate = DateTime.UtcNow.AddDays(-30),
            CurrentHens = hens,
            CurrentRoosters = roosters,
            CurrentChicks = chicks,
            IsActive = true,
            History = new List<FlockHistory>()
        };
    }

    public static Coop CreateCoop(string? id = null)
    {
        return new Coop
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = $"Test Coop {Guid.NewGuid()}",
            Location = "Test Location",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Frontend Test Data

```typescript
// tests/fixtures/flock.fixture.ts
export const createMockFlock = (overrides?: Partial<FlockDto>): FlockDto => ({
  id: 'flock-1',
  coopId: 'coop-1',
  coopName: 'Test Coop',
  identifier: 'Test Flock',
  hatchDate: new Date('2024-01-15'),
  currentHens: 10,
  currentRoosters: 2,
  currentChicks: 5,
  isActive: true,
  ...overrides,
});
```

---

## CI/CD Integration

### GitHub Actions Workflow

```yaml
# .github/workflows/test.yml
name: Tests

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

      - name: Upload coverage
        uses: codecov/codecov-action@v3

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci
        working-directory: ./src/frontend

      - name: Run unit tests
        run: npm run test:coverage
        working-directory: ./src/frontend

      - name: Upload coverage
        uses: codecov/codecov-action@v3

  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci
        working-directory: ./src/frontend

      - name: Install Playwright
        run: npx playwright install --with-deps
        working-directory: ./src/frontend

      - name: Run E2E tests
        run: npm run test:e2e
        working-directory: ./src/frontend

      - uses: actions/upload-artifact@v4
        if: always()
        with:
          name: playwright-report
          path: src/frontend/playwright-report/
          retention-days: 30
```

---

## Summary

This test strategy ensures:
- ✅ **Comprehensive coverage** - Unit, integration, E2E tests
- ✅ **Fast feedback** - Unit tests in seconds
- ✅ **Confidence** - Critical paths at 100% coverage
- ✅ **Maintainability** - Clear test structure and naming
- ✅ **CI/CD ready** - Automated testing in pipeline

**Test Distribution:**
- 60% Unit tests (fast, isolated)
- 30% Integration tests (API + DB)
- 10% E2E tests (critical user flows)

**Critical paths with 100% coverage:**
- Authentication flow
- Egg cost calculation
- Mature chicks logic
- Daily record creation with offline support
