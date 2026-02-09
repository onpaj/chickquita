# Coding Standards

**Chickquita (Chickquita)** - Code conventions and best practices for backend (C#) and frontend (TypeScript/React).

**Version:** 1.0
**Date:** February 5, 2026
**Status:** Approved

---

## Table of Contents

- [C# Backend Standards](#c-backend-standards)
- [Error Handling & Logging (C#)](#error-handling--logging-c)
- [TypeScript/React Standards](#typescriptreact-standards)
- [Code Organization Best Practices](#code-organization-best-practices)
- [Git Commit Conventions](#git-commit-conventions)

---

## C# Backend Standards

### Naming Conventions

```csharp
// PascalCase for classes, methods, properties, public fields
public class FlockService { }
public void MatureChicks() { }
public string FlockIdentifier { get; set; }

// camelCase for private fields (with underscore prefix)
private readonly IFlockRepository _flockRepository;
private readonly ILogger<FlockService> _logger;
private int _maxRetryCount;

// camelCase for parameters and local variables
public void CreateFlock(string identifier, int initialChicks)
{
    var flock = new Flock();
    var totalAnimals = initialChicks;
}

// UPPER_CASE for constants
public const int MAX_CHICKS_PER_FLOCK = 1000;
public const string DEFAULT_CURRENCY = "CZK";

// Async methods end with "Async"
public async Task<Flock> GetFlockByIdAsync(string id) { }
public async Task<Result<FlockDto>> CreateFlockAsync(CreateFlockCommand command) { }

// Interfaces start with 'I'
public interface IFlockRepository { }
public interface IAuthService { }
```

### File Organization

```csharp
// One class per file
// File name matches class name: FlockService.cs

// Namespace matches folder structure
namespace Chickquita.Application.Features.Flocks.Commands.MatureChicks;

// Using statements order:
// 1. System namespaces
// 2. Third-party namespaces
// 3. Project namespaces
using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using FluentValidation;
using Chickquita.Domain.Entities;
using Chickquita.Domain.Interfaces;
```

### Class Structure Order

```csharp
public class FlockService
{
    // 1. Constants
    private const int DEFAULT_RETRY_COUNT = 3;
    private const int MAX_CHICKS = 1000;

    // 2. Fields (private, readonly preferred)
    private readonly IFlockRepository _repository;
    private readonly ILogger<FlockService> _logger;
    private readonly ICurrentUserService _currentUser;

    // 3. Constructor
    public FlockService(
        IFlockRepository repository,
        ILogger<FlockService> logger,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _logger = logger;
        _currentUser = currentUser;
    }

    // 4. Public methods (alphabetical or logical order)
    public async Task<Result<Flock>> CreateFlockAsync(CreateFlockCommand command)
    {
        // Implementation
    }

    public async Task<Result<Flock>> MatureChicksAsync(
        string flockId,
        int chicksCount,
        int hens,
        int roosters)
    {
        // Implementation
    }

    // 5. Private methods (alphabetical or logical order)
    private void ValidateFlockComposition(Flock flock)
    {
        // Implementation
    }

    private async Task<bool> CheckUserHasAccessAsync(string flockId)
    {
        // Implementation
    }
}
```

### CQRS Command/Query Pattern

```csharp
// Command (mutation) - record type
public record MatureChicksCommand(
    string FlockId,
    DateTime Date,
    int ChicksCount,
    int ResultingHens,
    int ResultingRoosters,
    string? Notes
) : IRequest<Result<FlockDto>>;

// Command Handler
public class MatureChicksCommandHandler
    : IRequestHandler<MatureChicksCommand, Result<FlockDto>>
{
    private readonly IFlockRepository _repository;
    private readonly ILogger<MatureChicksCommandHandler> _logger;

    public MatureChicksCommandHandler(
        IFlockRepository repository,
        ILogger<MatureChicksCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<FlockDto>> Handle(
        MatureChicksCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch aggregate
        var flock = await _repository.GetByIdAsync(
            request.FlockId,
            cancellationToken);

        if (flock == null)
        {
            return Result<FlockDto>.Failure(
                new Error("NOT_FOUND", "Hejno nenalezeno"));
        }

        // 2. Execute domain logic
        try
        {
            flock.MatureChicks(
                request.ChicksCount,
                request.ResultingHens,
                request.ResultingRoosters,
                request.Notes);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error while maturing chicks");
            return Result<FlockDto>.Failure(
                new Error(ex.ErrorCode, ex.Message));
        }

        // 3. Persist
        await _repository.UpdateAsync(flock, cancellationToken);

        // 4. Return result
        return Result<FlockDto>.Success(flock.ToDto());
    }
}

// Query (read-only)
public record GetFlockByIdQuery(string FlockId) : IRequest<Result<FlockDto>>;

// Query Handler
public class GetFlockByIdQueryHandler
    : IRequestHandler<GetFlockByIdQuery, Result<FlockDto>>
{
    private readonly IFlockRepository _repository;

    public async Task<Result<FlockDto>> Handle(
        GetFlockByIdQuery request,
        CancellationToken cancellationToken)
    {
        var flock = await _repository.GetByIdAsync(
            request.FlockId,
            cancellationToken);

        if (flock == null)
        {
            return Result<FlockDto>.Failure(
                new Error("NOT_FOUND", "Hejno nenalezeno"));
        }

        return Result<FlockDto>.Success(flock.ToDto());
    }
}

// Validator
public class MatureChicksCommandValidator
    : AbstractValidator<MatureChicksCommand>
{
    public MatureChicksCommandValidator()
    {
        RuleFor(x => x.FlockId)
            .NotEmpty()
            .WithMessage("FlockId je povinný");

        RuleFor(x => x.ChicksCount)
            .GreaterThan(0)
            .WithMessage("Počet kuřat musí být větší než 0");

        RuleFor(x => x)
            .Must(x => x.ResultingHens + x.ResultingRoosters == x.ChicksCount)
            .WithMessage("Součet slepic a kohoutů musí odpovídat počtu kuřat");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Datum nesmí být v budoucnosti");
    }
}
```

---

## Error Handling & Logging (C#)

### Result Pattern (No Exceptions for Expected Failures)

```csharp
// Result type for operation outcomes
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value)
        => new(true, value, null);

    public static Result<T> Failure(Error error)
        => new(false, default, error);
}

public record Error(string Code, string Message);

// Usage in handlers
public async Task<Result<FlockDto>> Handle(
    CreateFlockCommand request,
    CancellationToken ct)
{
    // Check if coop exists
    if (!await _coopRepository.ExistsAsync(request.CoopId, ct))
    {
        return Result<FlockDto>.Failure(
            new Error("COOP_NOT_FOUND", "Kurník nebyl nalezen")
        );
    }

    // Success path
    var flock = Flock.Create(
        request.CoopId,
        request.Identifier,
        request.HatchDate,
        request.InitialChicks);

    await _repository.AddAsync(flock, ct);

    return Result<FlockDto>.Success(flock.ToDto());
}
```

### Domain Exceptions (Only for Invariant Violations)

```csharp
// Base domain exception
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

// Specific exceptions
public class InsufficientChicksException : DomainException
{
    public InsufficientChicksException(int available, int requested)
        : base("INSUFFICIENT_CHICKS",
               $"Nelze převést {requested} kuřat, dostupných pouze {available}")
    { }
}

public class InvalidFlockCompositionException : DomainException
{
    public InvalidFlockCompositionException(string message)
        : base("INVALID_COMPOSITION", message)
    { }
}

// Usage in domain entity
public class Flock
{
    public void MatureChicks(
        int chicksCount,
        int hens,
        int roosters,
        string? notes = null)
    {
        // Validate invariants
        if (chicksCount > CurrentChicks)
        {
            throw new InsufficientChicksException(CurrentChicks, chicksCount);
        }

        if (hens + roosters != chicksCount)
        {
            throw new InvalidFlockCompositionException(
                "Součet slepic a kohoutů musí odpovídat počtu kuřat");
        }

        // Update composition
        CurrentChicks -= chicksCount;
        CurrentHens += hens;
        CurrentRoosters += roosters;

        // Add history entry
        AddHistoryEntry(ChangeType.Maturation, CurrentHens, CurrentRoosters, CurrentChicks, notes);
    }
}
```

### Global Exception Handling Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception: {ErrorCode}", ex.ErrorCode);
            await HandleDomainExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }

    private static async Task HandleDomainExceptionAsync(
        HttpContext context,
        DomainException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = ex.ErrorCode,
                message = ex.Message
            }
        });
    }

    private static async Task HandleUnexpectedExceptionAsync(
        HttpContext context,
        Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "INTERNAL_ERROR",
                message = "Nastala neočekávaná chyba"
            }
        });
    }
}
```

### Structured Logging (Microsoft.Extensions.Logging)

```csharp
// Program.cs setup
var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddApplicationInsights();

// Set minimum log levels
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Chickquita", LogLevel.Information);

// Usage in handlers
public class MatureChicksCommandHandler
{
    private readonly ILogger<MatureChicksCommandHandler> _logger;

    public async Task<Result<FlockDto>> Handle(
        MatureChicksCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Maturing {ChicksCount} chicks for flock {FlockId}",
            request.ChicksCount, request.FlockId
        );

        var flock = await _repository.GetByIdAsync(request.FlockId, cancellationToken);

        if (flock == null)
        {
            _logger.LogWarning("Flock {FlockId} not found", request.FlockId);
            return Result<FlockDto>.Failure(new Error("NOT_FOUND", "Hejno nenalezeno"));
        }

        try
        {
            flock.MatureChicks(
                request.ChicksCount,
                request.ResultingHens,
                request.ResultingRoosters,
                request.Notes);

            await _repository.UpdateAsync(flock, cancellationToken);

            _logger.LogInformation(
                "Successfully matured {ChicksCount} chicks for flock {FlockId}",
                request.ChicksCount, request.FlockId
            );

            return Result<FlockDto>.Success(flock.ToDto());
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex,
                "Domain error while maturing chicks: {ErrorCode}",
                ex.ErrorCode
            );
            throw;
        }
    }
}
```

**Log Levels:**
- **Trace**: Very detailed debugging (rarely used)
- **Debug**: Internal system flow
- **Information**: General successful operations (default minimum)
- **Warning**: Unexpected but recoverable events
- **Error**: Failures that need attention
- **Critical**: System failures

---

## TypeScript/React Standards

### Naming Conventions

```typescript
// PascalCase for components, types, interfaces, enums
export const FlockCard: React.FC<FlockCardProps> = () => { };
export interface FlockDto { }
export type Result<T> = { };
export enum ChangeType { }

// camelCase for variables, functions, hooks
const flockId = '123';
const handleSubmit = () => { };
const useFlocks = () => { };

// UPPER_SNAKE_CASE for constants
const MAX_RETRY_ATTEMPTS = 3;
const API_BASE_URL = import.meta.env.VITE_API_URL;

// Prefix hooks with 'use'
const useAuth = () => { };
const useOnlineStatus = () => { };

// Props interfaces suffix with 'Props'
interface FlockCardProps {
  flock: FlockDto;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
}

// Avoid 'I' prefix for interfaces (TypeScript convention)
// ❌ interface IFlockDto
// ✅ interface FlockDto
```

### File Naming

```
// Components: PascalCase.tsx
FlockCard.tsx
QuickAddModal.tsx
DashboardPage.tsx

// Hooks: camelCase.ts
useFlocks.ts
useAuth.ts
useOnlineStatus.ts

// Utils: camelCase.ts
formatters.ts
validators.ts
calculations.ts

// Types: camelCase.types.ts
flock.types.ts
api.types.ts
common.types.ts

// Constants: camelCase.ts or UPPER_CASE.ts
routes.ts
API_ENDPOINTS.ts
config.ts
```

### Component Structure

```typescript
// 1. Imports (grouped and sorted)
import React, { useState, useEffect, useCallback } from 'react';
import { Box, Card, Button, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useFlocks } from '../hooks/useFlocks';
import { FlockDto } from '../types/flock.types';
import { formatDate } from '@/shared/utils/formatters';

// 2. Types/Interfaces
interface FlockCardProps {
  flock: FlockDto;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
}

// 3. Component
export const FlockCard: React.FC<FlockCardProps> = ({
  flock,
  onEdit,
  onDelete,
}) => {
  // 3a. Hooks (at top, in order: state, effects, queries, mutations)
  const [isExpanded, setIsExpanded] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    // Effect logic
  }, [flock.id]);

  const { updateFlock } = useFlocks();

  // 3b. Event handlers
  const handleEdit = useCallback(() => {
    onEdit(flock.id);
  }, [flock.id, onEdit]);

  const handleDelete = useCallback(async () => {
    if (window.confirm('Opravdu smazat?')) {
      setIsDeleting(true);
      await onDelete(flock.id);
      setIsDeleting(false);
    }
  }, [flock.id, onDelete]);

  // 3c. Computed values
  const totalAnimals = flock.hens + flock.roosters + flock.chicks;
  const productivity = flock.hens > 0 ? flock.eggCount / flock.hens : 0;

  // 3d. Early returns (loading, error, empty states)
  if (!flock) return null;

  // 3e. JSX
  return (
    <Card elevation={1}>
      <Box p={2}>
        <Typography variant="h6">{flock.identifier}</Typography>
        <Typography variant="body2" color="text.secondary">
          Celkem: {totalAnimals} • Produktivita: {productivity.toFixed(2)}
        </Typography>
        <Box mt={2} display="flex" gap={1}>
          <Button
            variant="outlined"
            onClick={handleEdit}
            disabled={isDeleting}
          >
            Upravit
          </Button>
          <Button
            variant="outlined"
            color="error"
            onClick={handleDelete}
            disabled={isDeleting}
          >
            {isDeleting ? 'Mazání...' : 'Smazat'}
          </Button>
        </Box>
      </Box>
    </Card>
  );
};

// 4. Export (default or named)
export default FlockCard;
```

### Custom Hooks Pattern

```typescript
// hooks/useFlocks.ts
export const useFlocks = (coopId?: string) => {
  // Query for fetching
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['flocks', coopId],
    queryFn: () => flocksApi.getFlocks(coopId),
    staleTime: 5 * 60 * 1000, // 5 minutes
    enabled: !!coopId, // Only fetch if coopId provided
  });

  // Mutation for creating
  const createMutation = useMutation({
    mutationFn: flocksApi.createFlock,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['flocks'] });
      toast.success('Hejno vytvořeno');
    },
    onError: (error) => {
      toast.error('Chyba při vytváření hejna');
      console.error('Create flock error:', error);
    },
  });

  // Mutation for updating
  const updateMutation = useMutation({
    mutationFn: flocksApi.updateFlock,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['flocks'] });
      toast.success('Hejno upraveno');
    },
  });

  // Return clean API
  return {
    flocks: data ?? [],
    isLoading,
    error,
    refetch,
    createFlock: createMutation.mutate,
    isCreating: createMutation.isPending,
    updateFlock: updateMutation.mutate,
    isUpdating: updateMutation.isPending,
  };
};
```

---

## Configuration & Environment Variables

### Frontend Configuration

**IMPORTANT: Never hardcode URLs, API endpoints, or environment-specific values in your code.**

```typescript
// ❌ BAD: Hardcoded URLs
const response = await fetch('http://localhost:5000/api/users/me', {
  headers: { Authorization: `Bearer ${token}` }
});

const apiUrl = 'https://api.chickquita.com';

// ✅ GOOD: Use environment variables
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// ✅ BEST: Use the configured apiClient
import apiClient from '@/lib/apiClient';

const response = await apiClient.get('/users/me');
```

### Environment Variable Guidelines

1. **Use `.env` files for configuration**
   - `.env.example` - Template with placeholder values (committed to git)
   - `.env.development` - Development values (committed to git)
   - `.env.local` - Local overrides (NOT committed, in `.gitignore`)

2. **Naming convention**
   - Frontend (Vite): `VITE_` prefix (e.g., `VITE_API_BASE_URL`)
   - Backend (.NET): Standard naming (e.g., `ConnectionStrings__DefaultConnection`)

3. **Always use the apiClient for API calls**
   ```typescript
   // The apiClient is pre-configured with:
   // - Base URL from VITE_API_BASE_URL
   // - Automatic JWT token injection
   // - Error handling interceptors
   import apiClient from '@/lib/apiClient';

   // GET request
   const { data } = await apiClient.get('/coops');

   // POST request
   const { data } = await apiClient.post('/coops', coopData);

   // PUT request
   const { data } = await apiClient.put(`/coops/${id}`, coopData);

   // DELETE request
   await apiClient.delete(`/coops/${id}`);
   ```

4. **Required environment variables**
   ```bash
   # Frontend (.env.example)
   VITE_CLERK_PUBLISHABLE_KEY=pk_test_...
   VITE_API_BASE_URL=http://localhost:5100/api
   VITE_APP_NAME=Chickquita
   VITE_APP_VERSION=0.0.0
   ```

5. **Security considerations**
   - NEVER commit `.env.local` files
   - NEVER include sensitive keys in frontend code (use backend for secrets)
   - Validate required env vars on application startup
   - Use Azure Key Vault for production secrets

---

## Code Organization Best Practices

### Import Organization

```typescript
// 1. External libraries (alphabetical)
import React, { useState, useEffect } from 'react';
import { Box, Button, Card } from '@mui/material';
import { useQuery } from '@tanstack/react-query';

// 2. Internal absolute imports (by proximity: shared → features)
import { formatDate, formatCurrency } from '@/shared/utils/formatters';
import { useAuth } from '@/features/auth/hooks/useAuth';

// 3. Relative imports (parent → sibling → child)
import { FlockCard } from '../components/FlockCard';
import { useFlocks } from './hooks/useFlocks';
import { FlockDto } from './types/flock.types';

// 4. Types-only imports (if needed separately)
import type { FC, ReactNode } from 'react';
```

### DRY Principles

```typescript
// ❌ BAD: Repeated logic
const FlockCard = ({ flock }: FlockCardProps) => {
  const total = flock.hens + flock.roosters + flock.chicks;
  return <div>{total}</div>;
};

const FlockSummary = ({ flock }: FlockSummaryProps) => {
  const total = flock.hens + flock.roosters + flock.chicks;
  return <span>{total}</span>;
};

// ✅ GOOD: Shared utility
// utils/flock.utils.ts
export const calculateTotalAnimals = (flock: FlockDto): number =>
  flock.hens + flock.roosters + flock.chicks;

// Usage
const total = calculateTotalAnimals(flock);
```

### Component Composition

```typescript
// ✅ GOOD: Small, focused components
const FlockCard: FC<FlockCardProps> = ({ flock }) => (
  <Card>
    <FlockHeader flock={flock} />
    <FlockStats flock={flock} />
    <FlockActions flock={flock} />
  </Card>
);

// Each subcomponent has single responsibility
const FlockHeader: FC<{ flock: FlockDto }> = ({ flock }) => (
  <Box p={2}>
    <Typography variant="h6">{flock.identifier}</Typography>
    <Typography variant="caption" color="text.secondary">
      {flock.coopName}
    </Typography>
  </Box>
);

const FlockStats: FC<{ flock: FlockDto }> = ({ flock }) => {
  const total = calculateTotalAnimals(flock);

  return (
    <Box px={2}>
      <Chip label={`🐔 ${flock.hens}s`} size="small" />
      <Chip label={`🐓 ${flock.roosters}k`} size="small" />
      <Chip label={`🐣 ${flock.chicks}k`} size="small" />
      <Typography variant="caption">Celkem: {total}</Typography>
    </Box>
  );
};
```

### Avoid Prop Drilling

```typescript
// ❌ BAD: Passing props through many levels
<Dashboard user={user}>
  <Sidebar user={user}>
    <UserMenu user={user} />
  </Sidebar>
</Dashboard>

// ✅ GOOD: Use Zustand store
// store/authStore.ts
export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  setUser: (user) => set({ user }),
  logout: () => set({ user: null }),
}));

// Components access directly
const UserMenu: FC = () => {
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  if (!user) return null;

  return (
    <Menu>
      <MenuItem>{user.email}</MenuItem>
      <MenuItem onClick={logout}>Odhlásit se</MenuItem>
    </Menu>
  );
};
```

### Conditional Rendering

```typescript
// ✅ GOOD: Early returns for readability
const FlockDetail: FC<FlockDetailProps> = ({ flockId }) => {
  const { flock, isLoading, error } = useFlock(flockId);

  if (isLoading) return <Spinner />;
  if (error) return <ErrorMessage error={error} />;
  if (!flock) return <NotFound />;

  return <FlockContent flock={flock} />;
};

// ❌ BAD: Nested ternaries (hard to read)
return isLoading ? (
  <Spinner />
) : error ? (
  <Error />
) : !flock ? (
  <NotFound />
) : (
  <Content />
);
```

### Type Safety

```typescript
// ✅ GOOD: Explicit types
interface CreateFlockRequest {
  coopId: string;
  identifier: string;
  hatchDate: Date;
  initialHens: number;
  initialRoosters: number;
  initialChicks: number;
}

const createFlock = async (
  request: CreateFlockRequest
): Promise<FlockDto> => {
  const response = await api.post('/flocks', request);
  return response.data;
};

// ❌ BAD: 'any' types (avoid)
const createFlock = async (request: any): Promise<any> => { };
```

### Avoid Magic Numbers/Strings

```typescript
// ❌ BAD
if (user.role === 'admin') { }
setTimeout(() => {}, 5000);

// ✅ GOOD
// constants/roles.ts
export const USER_ROLES = {
  ADMIN: 'admin',
  USER: 'user',
} as const;

// constants/timeouts.ts
export const TIMEOUTS = {
  API_REQUEST: 5000,
  DEBOUNCE: 300,
  TOAST_DURATION: 3000,
} as const;

// Usage
if (user.role === USER_ROLES.ADMIN) { }
setTimeout(() => {}, TIMEOUTS.API_REQUEST);
```

---

## Git Commit Conventions

### Commit Message Format (Conventional Commits)

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `style`: Formatting, whitespace (no code behavior change)
- `docs`: Documentation only changes
- `test`: Adding or updating tests
- `chore`: Build process, dependencies, tooling
- `perf`: Performance improvements

### Scopes (Optional)

- `auth`: Authentication feature
- `coops`: Coops feature
- `flocks`: Flocks feature
- `purchases`: Purchases feature
- `daily-records`: Daily records feature
- `statistics`: Statistics feature
- `ui`: UI components
- `api`: API layer
- `db`: Database/repository layer
- `pwa`: PWA features

### Examples

```bash
# Simple feature
git commit -m "feat(flocks): add mature chicks command"

# Bug fix with description
git commit -m "fix(daily-records): prevent negative egg count

- Add validation to ensure egg count >= 0
- Update form validation schema with Zod
- Add error message for invalid input
- Update tests"

# Breaking change
git commit -m "feat(api): change authentication to JWT

BREAKING CHANGE: Old session-based auth no longer supported.
Users must re-authenticate after this update."

# Multiple related changes
git commit -m "feat(pwa): add offline support

- Implement service worker with Workbox
- Add IndexedDB for local storage via Dexie.js
- Create sync queue for offline mutations
- Add offline indicator banner
- Add sync status indicator"
```

### Commit Best Practices

```bash
# ✅ GOOD: Small, focused commits
git commit -m "feat(flocks): add FlockCard component"
git commit -m "test(flocks): add FlockCard tests"
git commit -m "style(flocks): format FlockCard with prettier"

# ❌ BAD: Large, unfocused commit
git commit -m "Add flock feature with tests and styling"

# ✅ GOOD: Present tense, imperative mood
"Add feature" NOT "Added feature" or "Adds feature"

# ✅ GOOD: Lowercase subject (after type/scope)
feat(auth): add login form

# ✅ GOOD: No period at end of subject
feat(auth): add login form

# ❌ BAD: Period at end
feat(auth): add login form.
```

### Branch Naming

```bash
# Format: <type>/<short-description>
feature/123-mature-chicks-command
feature/456-offline-sync
fix/789-daily-record-validation
refactor/clean-architecture
docs/update-readme

# Protected branches
main          # Production-ready code only
```

### Git Workflow

```bash
# 1. Create feature branch from main
git checkout main
git pull origin main
git checkout -b feature/123-mature-chicks

# 2. Work on feature (small, frequent commits)
git add .
git commit -m "feat(flocks): add MatureChicksCommand"
git commit -m "feat(flocks): add MatureChicksCommandHandler"
git commit -m "test(flocks): add MatureChicks tests"

# 3. Push and create PR
git push origin feature/123-mature-chicks

# 4. After PR approval, squash and merge to main
# (GitHub will squash commits into one)

# 5. Delete branch after merge
git branch -d feature/123-mature-chicks
git push origin --delete feature/123-mature-chicks
```

### Pull Request Title Format

```
# Same as commit message format
feat(flocks): add mature chicks functionality

# Description in PR body:
## Changes
- Added MatureChicksCommand and handler
- Added FluentValidation rules
- Updated Flock entity with MatureChicks method
- Added domain exception for insufficient chicks

## Testing
- Unit tests for domain logic
- Integration tests for API endpoint
- Manual testing with 3 different flocks

## Screenshots
[If UI changes - attach screenshots]
```

---

## Summary

These coding standards ensure:
- ✅ **Consistency** - Code looks like one person wrote it
- ✅ **Readability** - Easy to understand and maintain
- ✅ **Type Safety** - Catch errors at compile time
- ✅ **Testability** - Easy to write and maintain tests
- ✅ **Scalability** - Code organization supports growth

**Key Principles:**
- SOLID principles (backend)
- DRY (Don't Repeat Yourself)
- KISS (Keep It Simple, Stupid)
- YAGNI (You Aren't Gonna Need It)
- Explicit over implicit
- Type safety everywhere
- Small, focused functions/components
- Clear naming conventions
