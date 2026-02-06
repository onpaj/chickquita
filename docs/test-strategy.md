# Test Strategy

**Chickquita (Chickquita)** - Comprehensive testing approach for backend and frontend, covering unit, integration, and E2E tests.

**Version:** 1.0
**Date:** February 5, 2026
**Status:** Approved

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

```csharp
// Domain.Tests/Entities/FlockTests.cs
public class FlockTests
{
    [Fact]
    public void MatureChicks_WithValidInput_UpdatesComposition()
    {
        // Arrange
        var flock = new Flock
        {
            Id = "flock-1",
            CurrentHens = 10,
            CurrentRoosters = 2,
            CurrentChicks = 20,
            History = new List<FlockHistory>()
        };

        // Act
        flock.MatureChicks(
            chicksCount: 15,
            resultingHens: 12,
            resultingRoosters: 3);

        // Assert
        flock.CurrentChicks.Should().Be(5);    // 20 - 15
        flock.CurrentHens.Should().Be(22);     // 10 + 12
        flock.CurrentRoosters.Should().Be(5);  // 2 + 3
        flock.History.Should().HaveCount(1);
        flock.History.First().ChangeType.Should().Be(ChangeType.Maturation);
    }

    [Fact]
    public void MatureChicks_WithInsufficientChicks_ThrowsException()
    {
        // Arrange
        var flock = new Flock { CurrentChicks = 5 };

        // Act
        var act = () => flock.MatureChicks(
            chicksCount: 10,
            resultingHens: 8,
            resultingRoosters: 2);

        // Assert
        act.Should().Throw<InsufficientChicksException>()
            .WithMessage("*pouze 5*");
    }

    [Theory]
    [InlineData(10, 8, 3)]  // Sum = 11, not 10
    [InlineData(10, 5, 4)]  // Sum = 9, not 10
    [InlineData(10, 0, 11)] // Sum = 11, not 10
    public void MatureChicks_WithInvalidSum_ThrowsException(
        int chicksCount,
        int hens,
        int roosters)
    {
        // Arrange
        var flock = new Flock { CurrentChicks = 20 };

        // Act
        var act = () => flock.MatureChicks(chicksCount, hens, roosters);

        // Assert
        act.Should().Throw<InvalidFlockCompositionException>();
    }

    [Fact]
    public void CalculateProductivity_WithNoHens_ReturnsZero()
    {
        // Arrange
        var flock = new Flock
        {
            CurrentHens = 0,
            DailyRecords = new List<DailyRecord>
            {
                new() { Date = DateTime.UtcNow, EggCount = 10 }
            }
        };

        // Act
        var productivity = flock.CalculateProductivity(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Assert
        productivity.Should().Be(0);
    }
}
```

### Command Handler Tests (with AutoFixture)

```csharp
// Application.Tests/Features/Flocks/MatureChicksCommandTests.cs
public class MatureChicksCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_WithValidCommand_ReturnsSuccess(
        [Frozen] Mock<IFlockRepository> repositoryMock,
        MatureChicksCommandHandler sut,
        Flock flock,
        MatureChicksCommand command)
    {
        // Arrange - AutoFixture generates test data
        flock.Id = command.FlockId;
        flock.CurrentChicks = 20;
        flock.CurrentHens = 10;
        flock.CurrentRoosters = 2;

        repositoryMock
            .Setup(r => r.GetByIdAsync(command.FlockId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flock);

        var validCommand = command with
        {
            ChicksCount = 15,
            ResultingHens = 12,
            ResultingRoosters = 3
        };

        // Act
        var result = await sut.Handle(validCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CurrentChicks.Should().Be(5);
        result.Value.CurrentHens.Should().Be(22);
        result.Value.CurrentRoosters.Should().Be(5);

        repositoryMock.Verify(
            r => r.UpdateAsync(flock, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory, AutoMoqData]
    public async Task Handle_WithNonExistentFlock_ReturnsFailure(
        [Frozen] Mock<IFlockRepository> repositoryMock,
        MatureChicksCommandHandler sut,
        MatureChicksCommand command)
    {
        // Arrange
        repositoryMock
            .Setup(r => r.GetByIdAsync(command.FlockId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Flock?)null);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("NOT_FOUND");
    }

    [Theory]
    [InlineAutoMoqData(10, 8, 3)]  // Invalid sum
    [InlineAutoMoqData(10, 5, 4)]  // Invalid sum
    public async Task Handle_WithInvalidSum_ThrowsDomainException(
        int chicksCount,
        int hens,
        int roosters,
        [Frozen] Mock<IFlockRepository> repositoryMock,
        MatureChicksCommandHandler sut,
        Flock flock,
        MatureChicksCommand command)
    {
        // Arrange
        flock.Id = command.FlockId;
        flock.CurrentChicks = 20;

        repositoryMock
            .Setup(r => r.GetByIdAsync(command.FlockId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flock);

        var invalidCommand = command with
        {
            ChicksCount = chicksCount,
            ResultingHens = hens,
            ResultingRoosters = roosters
        };

        // Act
        var act = async () => await sut.Handle(invalidCommand, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidFlockCompositionException>();
    }
}
```

### Validator Tests

```csharp
// Application.Tests/Features/Flocks/MatureChicksCommandValidatorTests.cs
public class MatureChicksCommandValidatorTests
{
    private readonly MatureChicksCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_Succeeds()
    {
        // Arrange
        var command = new MatureChicksCommand(
            FlockId: "flock-1",
            Date: DateTime.UtcNow,
            ChicksCount: 10,
            ResultingHens: 8,
            ResultingRoosters: 2,
            Notes: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidChicksCount_Fails(int chicksCount)
    {
        // Arrange
        var command = new MatureChicksCommand(
            "flock-1",
            DateTime.UtcNow,
            chicksCount,
            8,
            2,
            null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ChicksCount));
    }

    [Fact]
    public void Validate_WithInvalidSum_Fails()
    {
        // Arrange - Sum is 11, but chicksCount is 10
        var command = new MatureChicksCommand(
            "flock-1",
            DateTime.UtcNow,
            10,
            8,
            3,
            null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Součet"));
    }
}
```

---

## Backend Integration Tests

### Stack

- **WebApplicationFactory** - In-memory test server
- **Azurite** - Azure Table Storage Emulator
- **xUnit** with collection fixtures

### Test Setup

```csharp
// Integration.Tests/TestUtilities/ChickquitaWebApplicationFactory.cs
public class ChickquitaWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production Table Storage
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TableServiceClient));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add Azure Table Storage Emulator (Azurite)
            services.AddSingleton<TableServiceClient>(_ =>
                new TableServiceClient("UseDevelopmentStorage=true"));

            // Override configuration
            services.Configure<JwtSettings>(options =>
            {
                options.Secret = "test-secret-key-for-testing-only-minimum-256-bits";
                options.Issuer = "ChickquitaTest";
                options.Audience = "ChickquitaTest";
            });
        });

        builder.UseEnvironment("Testing");
    }
}

// Integration.Tests/TestUtilities/IntegrationTestBase.cs
public abstract class IntegrationTestBase : IClassFixture<ChickquitaWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly ChickquitaWebApplicationFactory Factory;
    protected readonly IFixture Fixture;

    protected IntegrationTestBase(ChickquitaWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    protected async Task<string> GetAuthTokenAsync()
    {
        // Create test user and get JWT token
        var email = $"test-{Guid.NewGuid()}@example.com";
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Test123!"
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.AccessToken;
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
```

### API Endpoint Tests

```csharp
// Integration.Tests/Features/Flocks/FlocksEndpointsTests.cs
public class FlocksEndpointsTests : IntegrationTestBase
{
    public FlocksEndpointsTests(ChickquitaWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateFlock_WithValidData_Returns201()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthToken(token);

        // Create a coop first
        var coopResponse = await Client.PostAsJsonAsync("/api/coops", new
        {
            Name = "Test Kurník",
            Location = "Test Location"
        });
        var coop = await coopResponse.Content.ReadFromJsonAsync<CoopDto>();

        var request = new
        {
            CoopId = coop!.Id,
            Identifier = "Test Hejno",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 0,
            InitialRoosters = 0,
            InitialChicks = 20
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/flocks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var flock = await response.Content.ReadFromJsonAsync<FlockDto>();
        flock.Should().NotBeNull();
        flock!.Identifier.Should().Be("Test Hejno");
        flock.CurrentChicks.Should().Be(20);
        flock.CurrentHens.Should().Be(0);
    }

    [Fact]
    public async Task MatureChicks_WithValidData_Returns200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthToken(token);

        var flock = await CreateTestFlockAsync();

        var request = new
        {
            Date = DateTime.UtcNow,
            ChicksCount = 15,
            ResultingHens = 12,
            ResultingRoosters = 3,
            Notes = "First maturation"
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/flocks/{flock.Id}/mature-chicks",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<FlockDto>();
        result.Should().NotBeNull();
        result!.CurrentChicks.Should().Be(5);  // 20 - 15
        result.CurrentHens.Should().Be(12);
        result.CurrentRoosters.Should().Be(3);
    }

    [Fact]
    public async Task MatureChicks_WithInsufficientChicks_Returns400()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthToken(token);

        var flock = await CreateTestFlockAsync();

        var request = new
        {
            ChicksCount = 50,  // More than available (20)
            ResultingHens = 40,
            ResultingRoosters = 10
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/flocks/{flock.Id}/mature-chicks",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Code.Should().Be("INSUFFICIENT_CHICKS");
    }

    [Fact]
    public async Task GetFlocks_WithoutAuth_Returns401()
    {
        // Act
        var response = await Client.GetAsync("/api/flocks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<FlockDto> CreateTestFlockAsync()
    {
        var coopResponse = await Client.PostAsJsonAsync("/api/coops", new
        {
            Name = $"Test Coop {Guid.NewGuid()}",
            Location = "Test"
        });
        var coop = await coopResponse.Content.ReadFromJsonAsync<CoopDto>();

        var flockResponse = await Client.PostAsJsonAsync("/api/flocks", new
        {
            CoopId = coop!.Id,
            Identifier = $"Test-{Guid.NewGuid()}",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialChicks = 20
        });

        return (await flockResponse.Content.ReadFromJsonAsync<FlockDto>())!;
    }
}
```

### Repository Tests (with Azurite)

```csharp
// Integration.Tests/Infrastructure/Repositories/FlockRepositoryTests.cs
public class FlockRepositoryTests : IAsyncLifetime
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly FlockRepository _sut;
    private readonly string _testTableName;

    public FlockRepositoryTests()
    {
        // Use Azurite (Azure Table Storage Emulator)
        _tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
        _testTableName = $"TestFlocks{Guid.NewGuid():N}";
        _sut = new FlockRepository(_tableServiceClient, _testTableName);
    }

    public async Task InitializeAsync()
    {
        await _tableServiceClient.CreateTableIfNotExistsAsync(_testTableName);
    }

    public async Task DisposeAsync()
    {
        await _tableServiceClient.DeleteTableAsync(_testTableName);
    }

    [Theory, AutoMoqData]
    public async Task AddAsync_WithValidFlock_StoresFlock(Flock flock)
    {
        // Arrange - AutoFixture generates flock

        // Act
        await _sut.AddAsync(flock, CancellationToken.None);

        // Assert
        var retrieved = await _sut.GetByIdAsync(flock.Id, CancellationToken.None);
        retrieved.Should().NotBeNull();
        retrieved!.Identifier.Should().Be(flock.Identifier);
        retrieved.CurrentChicks.Should().Be(flock.CurrentChicks);
    }

    [Theory, AutoMoqData]
    public async Task UpdateAsync_WithModifiedFlock_UpdatesStorage(Flock flock)
    {
        // Arrange
        await _sut.AddAsync(flock, CancellationToken.None);

        // Act
        flock.MatureChicks(10, 8, 2);
        await _sut.UpdateAsync(flock, CancellationToken.None);

        // Assert
        var retrieved = await _sut.GetByIdAsync(flock.Id, CancellationToken.None);
        retrieved.Should().NotBeNull();
        retrieved!.CurrentChicks.Should().Be(flock.CurrentChicks);
        retrieved.CurrentHens.Should().Be(flock.CurrentHens);
    }
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
    baseURL: 'http://localhost:5173',
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
    url: 'http://localhost:5173',
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

      - name: Start Azurite
        run: |
          npm install -g azurite
          azurite --silent --location azurite --debug azurite/debug.log &

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
