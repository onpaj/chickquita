# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**IMPORTANT: All documentation and source code MUST be in English. The application UI is in Czech (primary language) with English as a switchable option for users.**

## Project Overview

**Chickquita** is a mobile-first Progressive Web Application (PWA) for tracking the financial profitability of chicken farming. The application features:

- Multi-tenant architecture with isolated data per farmer
- Offline-first design for use outdoors at chicken coops
- Real-time cost tracking and egg price calculation
- Flock management including chick maturation tracking

## Tech Stack

### Frontend
- **React 18+** with TypeScript
- **Vite** as build tool
- **React Router** for routing
- **Zustand** for state management
- **TanStack Query (React Query)** for server state and caching
- **Material-UI (MUI)** for component library
- **React Hook Form** + **Zod** for forms and validation
- **Recharts** for data visualization
- **react-i18next** for internationalization (Czech primary, English secondary)

### PWA Stack
- **Workbox** for service worker management
- **IndexedDB** (via Dexie.js) for offline storage
- **Background Sync API** for queued offline requests
- manifest.json with app icons and configuration

### Backend
- **.NET 8** Web API
- **ASP.NET Core** (Minimal APIs)
- **Entity Framework Core** (Code First)
- **AutoMapper** for DTO mapping
- **FluentValidation** for request validation
- **Microsoft.Extensions.Logging** for structured logging
- **MediatR** for CQRS pattern

### Authentication
- **Clerk.com** for authentication management
- Hosted UI components for sign-in/sign-up
- JWT token validation in .NET API
- Webhook integration for user sync
- Email + password authentication (MVP)
- Future: Social logins, MFA (requires Clerk Pro tier)

### Database
- **Neon Postgres** (serverless) for primary data storage
- **Row-Level Security (RLS)** for tenant isolation
- Partition strategy: `tenant_id` column on all tables
- PostgreSQL 16 features
- Automatic backups and point-in-time recovery

### Hosting
- **Azure Container Apps** (recommended) or **Azure Web App for Containers**
- **Docker** multi-stage builds
- **GitHub Actions** for CI/CD
- Single container deployment (frontend + backend)

## Architecture Principles

### Clean Architecture
- Onion/Clean Architecture pattern
- Dependency Injection throughout
- CQRS pattern with MediatR
- Vertical Slice Architecture for features

### Multi-tenancy
- Every user gets their own tenant on registration (via Clerk webhook)
- **Fallback**: If webhook fails, tenant is auto-created on first API request
- All data partitioned by `tenant_id` (UUID)
- Row-Level Security (RLS) enforced at database level
- EF Core global query filters as additional safety layer
- No cross-tenant data access allowed

### Offline-First Strategy
- **Static assets**: Cache-first strategy (30 days)
- **API GET requests**: Network-first with cache fallback (5 min)
- **API POST/PUT/DELETE**: Background sync queue (24h retention)
- IndexedDB for local data persistence
- Conflict resolution: Last-write-wins (MVP), UI-based merge (Phase 2)

## Core Domain Entities

### Hierarchy
```
Tenant (User Account - linked to Clerk user)
└── Coop (Chicken coop)
    └── Flock (Group of chickens)
        ├── FlockHistory (Composition change history)
        ├── DailyRecord (Daily egg production records)
        └── Individual Chickens (Phase 3)
```

### Key Concepts
- **Flock Composition**: Hens, Roosters, Chicks
- **Chick Maturation**: Converting chicks to adult hens/roosters with historical tracking
- **Purchases**: Feed, supplements, bedding, veterinary care, equipment
- **Daily Records**: Egg count per flock per day (offline-capable)
- **Egg Cost Calculation**: Total costs / Total eggs produced

## Development Commands

### Frontend
```bash
cd src/frontend

# Install dependencies
npm install

# Development server (with Vite HMR)
npm run dev

# Build for production
npm run build

# Type checking
npm run type-check

# Linting
npm run lint

# Run tests
npm run test
```

### Backend
```bash
cd backend

# Restore packages
dotnet restore

# Build
dotnet build

# Run locally
dotnet run --project backend/src/Chickquita.Api

# Run tests
dotnet test

# Create EF Core migration
dotnet ef migrations add <MigrationName> \
  --project backend/src/Chickquita.Infrastructure \
  --startup-project backend/src/Chickquita.Api

# Apply migrations to database
dotnet ef database update \
  --project backend/src/Chickquita.Infrastructure \
  --startup-project backend/src/Chickquita.Api
```

### Docker
```bash
# Build image (multi-stage: frontend + backend)
docker build -t chickquita .

# Run container locally
docker run -p 8080:80 chickquita

# Build and run with docker-compose (if using)
docker-compose up --build
```

## Authentication Flow (Clerk)

### User Sign-Up Flow
1. User accesses `/sign-up` → Clerk hosted UI
2. User enters email + password → Clerk validates and creates account
3. Clerk sends verification email
4. User verifies email → Clerk webhook fires `user.created` event
5. Backend receives webhook → Creates `Tenant` record in Neon
6. Links `ClerkUserId` → `TenantId` in database
7. User redirected to dashboard

### User Sign-In Flow
1. User accesses `/sign-in` → Clerk hosted UI
2. User enters credentials → Clerk validates
3. Clerk issues JWT session token (7 days default)
4. Frontend receives token via `@clerk/clerk-react` hooks
5. All API calls include `Authorization: Bearer <token>`
6. Backend validates Clerk JWT
7. Backend extracts `ClerkUserId`, looks up `TenantId`
8. **Fallback behavior**: If no tenant exists, automatically create one (handles webhook failures)
9. Backend sets RLS context: `SET app.current_tenant_id = <tenant_id>`
10. All queries automatically filtered by tenant via RLS policies

**Note**: The `TenantResolutionMiddleware` includes automatic tenant creation as a fallback. This ensures that even if the Clerk webhook fails or doesn't fire during sign-up, the tenant will be created on the user's first API request. The middleware logs this event for monitoring purposes.

### Frontend Integration
```tsx
// Use Clerk hooks for authentication
import { useAuth, useUser } from '@clerk/clerk-react';

function MyComponent() {
  const { isSignedIn, getToken } = useAuth();
  const { user } = useUser();

  // Make API call with Clerk token
  const fetchData = async () => {
    const token = await getToken();
    const response = await fetch('/api/coops', {
      headers: { Authorization: `Bearer ${token}` }
    });
  };
}
```

## Database Design

### Neon Postgres Schema

All tables include:
- `id` (UUID, primary key)
- `tenant_id` (UUID, foreign key to tenants)
- `created_at` (TIMESTAMPTZ)
- `updated_at` (TIMESTAMPTZ)

**Key Tables:**
- `tenants` - User accounts (linked to Clerk via `clerk_user_id`)
- `coops` - Chicken coops
- `flocks` - Chicken flocks with composition (hens, roosters, chicks)
- `flock_history` - Immutable history of flock composition changes
- `purchases` - Cost tracking (feed, bedding, etc.)
- `daily_records` - Daily egg production (offline-capable)

### Row-Level Security (RLS)

Every table has RLS policy:
```sql
CREATE POLICY tenant_isolation ON coops
  USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

Backend sets context before each query:
```csharp
await Database.ExecuteSqlRawAsync(
    "SELECT set_tenant_context({0})",
    currentUser.TenantId
);
```

## API Design Standards

### RESTful Conventions
- Use HTTP verbs correctly: GET, POST, PUT, DELETE
- URL-based versioning: `/api/v1/...`
- Consistent error response format with error codes
- CORS enabled for PWA origin

### Authentication
- JWT Bearer tokens (managed by Clerk)
- Automatic token refresh (Clerk SDK)
- Rate limiting on API endpoints:
  - Auth webhooks: Clerk managed
  - API: 100 requests / min / user

### Standard Error Format
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": [
      {
        "field": "email",
        "message": "Invalid email format"
      }
    ]
  }
}
```

## Performance Budget

- **Lighthouse Score**: > 90 (all categories)
- **First Contentful Paint**: < 1.5s
- **Time to Interactive**: < 3.5s
- **Largest Contentful Paint**: < 2.5s
- **Cumulative Layout Shift**: < 0.1
- **Bundle size**: < 200kb (gzipped)

## Mobile-First Design

### Breakpoints
```css
@media (min-width: 320px)  { /* Mobile portrait */ }
@media (min-width: 480px)  { /* Mobile landscape */ }
@media (min-width: 768px)  { /* Tablet */ }
@media (min-width: 1024px) { /* Desktop */ }
```

### Touch Targets
- Minimum: 44x44px (iOS standard)
- Preferred: 48x48px (Material Design)
- Spacing between targets: 8px minimum

### Critical UX Flows
1. **Daily egg logging**: < 30 seconds from open to save
2. **Quick Add**: FAB button → Modal → 3 fields → Save
3. **Offline mode**: Automatic queue + sync when online
4. **Chick maturation**: Validation that hens + roosters = chicks converted

## Internationalization (i18n)

### Language Support
- **Primary language**: Czech (cs-CZ)
- **Secondary language**: English (en-US)
- User can switch language in settings
- Translation files: `src/frontend/src/locales/{cs|en}/translation.json`

### Implementation
```tsx
import { useTranslation } from 'react-i18next';

function MyComponent() {
  const { t } = useTranslation();

  return (
    <h1>{t('dashboard.title')}</h1> // "Přehled" in Czech, "Dashboard" in English
  );
}
```

## Security Considerations

- Authentication: Managed by Clerk (SOC 2 Type II certified)
- HTTPS/TLS 1.3 required
- Input validation on both frontend (Zod) and backend (FluentValidation)
- Parameterized queries only via EF Core (prevent SQL injection)
- XSS protection via DOMPurify for user inputs
- GDPR compliance for EU users
- Row-Level Security (RLS) for tenant data isolation

## Testing Strategy

### Frontend
- Unit tests for utilities and business logic
- Component tests with React Testing Library
- E2E tests with Playwright (critical flows)
- Mobile device testing required (iOS Safari 15+, Android Chrome 90+)

### Backend
- Unit tests for domain logic
- Integration tests for API endpoints
- Repository tests with EF Core
- Validation tests for FluentValidation rules

### PWA Testing
- Lighthouse CI in pipeline
- Offline functionality testing
- Background sync verification
- Install prompt testing on mobile devices

## Important Notes

### Language Requirements
- **All code, comments, and documentation**: English
- **Variable names, function names, class names**: English
- **Commit messages**: English
- **UI text**: Czech (primary), with i18n support for English
- **User-facing content**: Czech by default, switchable to English

### Data Model Specifics
- **Chicks** are counted in costs (feed consumption)
- **Chicks do NOT count** in egg production (only hens lay eggs)
- Flock history is immutable except for notes field
- First history entry = initial flock composition

### Phase 1 MVP Scope
Focus only on:
1. Authentication (Clerk email + password)
2. Coops and Flocks CRUD (including chicks)
3. Chick maturation action with history
4. Purchases tracking
5. Daily records (offline-capable)
6. Dashboard with basic stats
7. Egg cost calculation
8. PWA features (install, offline, manifest)

## Development Workflow

1. Feature branches from `main`
2. Conventional commits preferred (in English)
3. PR required for merge to main
4. CI/CD pipeline runs tests and Lighthouse
5. Deploy to Azure Container Apps on merge

## File Structure

```
chickquita/
├── docs/
│   ├── ChickenTrack_PRD.md          # Product requirements (English)
│   ├── technology-stack.md          # Tech stack documentation
│   ├── filesystem-structure.md      # Project structure
│   └── migration-clerk-neon.md      # Migration guide
├── backend/                         # .NET 8 API
│   ├── src/                         # Production code
│   │   ├── Chickquita.Api/          # Entry point, endpoints
│   │   ├── Chickquita.Application/  # Features (CQRS + MediatR)
│   │   ├── Chickquita.Domain/       # Entities, value objects
│   │   └── Chickquita.Infrastructure/ # EF Core, Clerk integration
│   └── tests/                       # Test projects
│       ├── Chickquita.Api.Tests/    # API integration tests
│       └── Chickquita.Infrastructure.Tests/ # Infrastructure tests
├── frontend/                        # React PWA
│   ├── src/
│   │   ├── features/                # Feature modules
│   │   ├── shared/                  # Shared components
│   │   ├── lib/                     # Third-party setup (Clerk, API)
│   │   └── locales/                 # i18n translations (cs, en)
│   └── public/                      # Static assets, manifest.json
├── Dockerfile                       # Multi-stage build
├── .github/workflows/               # CI/CD pipelines
└── README.md                        # Project overview
```

## Azure & External Services

### Neon Postgres
- Database: `chickquita`
- Connection string stored in Azure Key Vault
- Free tier: 0.5GB storage, 1 project
- Upgrade to paid when needed (~$20/month for 10GB)

### Clerk
- Application name: `Chickquita`
- Plan: Free tier (10k MAU)
- Features: Email/password, hosted UI
- Publishable Key: Frontend env var
- Secret Key: Backend env var (Azure Key Vault)
- Webhook: `POST /api/webhooks/clerk` for user sync

### Azure Resources
- Resource Group: `chickquita-rg`
- Container Apps: For hosting the application
- Application Insights: For monitoring and logging
- Key Vault: For secrets (Neon connection string, Clerk keys)

## Monitoring

- **Application Insights** for telemetry
- Structured logging with Microsoft.Extensions.Logging
- Custom events for business metrics:
  - User registration (via Clerk webhook)
  - Daily record created
  - Chick maturation
  - Offline sync completed
- Alert on error rate > 5% or response time > 1s (p95)

## Common Patterns

### Backend - Creating a New Feature

1. **Domain Entity** (`backend/src/Chickquita.Domain/Entities/`)
```csharp
public class Flock
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    // ... properties

    public void MatureChicks(int count, int hens, int roosters)
    {
        // Domain logic here
    }
}
```

2. **Command/Query** (`backend/src/Chickquita.Application/Features/Flocks/Commands/`)
```csharp
public record MatureChicksCommand : IRequest<Result<FlockDto>>
{
    public Guid FlockId { get; init; }
    public int ChicksCount { get; init; }
    // ...
}
```

3. **Handler** (same directory)
```csharp
public class MatureChicksCommandHandler : IRequestHandler<MatureChicksCommand, Result<FlockDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public async Task<Result<FlockDto>> Handle(...)
    {
        // Implementation
    }
}
```

4. **Endpoint** (`backend/src/Chickquita.Api/Endpoints/FlocksEndpoints.cs`)
```csharp
public static class FlocksEndpoints
{
    public static RouteGroupBuilder MapFlocksEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/{id}/mature-chicks", async (Guid id, MatureChicksCommand cmd, IMediator mediator) =>
        {
            cmd.FlockId = id;
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        return group;
    }
}
```

### Frontend - Creating a New Feature

1. **API Client** (`src/features/flocks/api/flocksApi.ts`)
```typescript
export const flocksApi = {
  matureChicks: async (flockId: string, data: MatureChicksDto): Promise<FlockDto> => {
    const response = await apiClient.post(`/flocks/${flockId}/mature-chicks`, data);
    return response.data;
  },
};
```

2. **Hook** (`src/features/flocks/hooks/useMatureChicks.ts`)
```typescript
export function useMatureChicks() {
  return useMutation({
    mutationFn: ({ flockId, data }: { flockId: string; data: MatureChicksDto }) =>
      flocksApi.matureChicks(flockId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['flocks'] });
    },
  });
}
```

3. **Component** (`src/features/flocks/components/MatureChicksModal.tsx`)
```tsx
export function MatureChicksModal({ flock, open, onClose }: Props) {
  const { t } = useTranslation();
  const { mutate, isPending } = useMatureChicks();

  const onSubmit = (data: FormData) => {
    mutate({ flockId: flock.id, data });
  };

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>{t('flocks.matureChicks.title')}</DialogTitle>
      {/* Form implementation */}
    </Dialog>
  );
}
```

## IMPORTANT REMINDERS

1. **Always write code and documentation in English**
2. **Use Clerk for all authentication** - don't implement custom auth
3. **Use EF Core** for database access - no raw SQL unless necessary
4. **Set RLS context** before queries in backend
5. **Use global query filters** in EF Core as safety layer
6. **Offline-first** - ensure daily records work offline
7. **Mobile-first** - design for mobile, enhance for desktop
8. **i18n ready** - use translation keys, not hardcoded Czech text
9. **Type-safe** - use TypeScript strict mode, C# nullable reference types
10. **Test critical flows** - especially offline sync and tenant isolation

---

## API Documentation

For detailed API specifications, see:
- `/docs/API_SPEC_COOPS.md` - Coops endpoints documentation
- `/docs/ChickenTrack_PRD.md` - Product requirements document
