# Filesystem Structure

**Chickquita (Chickquita)** - Complete directory layout for the monorepo with vertical slice architecture.

**Version:** 2.0
**Date:** February 5, 2026
**Status:** Approved

---

## Table of Contents

- [Monorepo Root Structure](#monorepo-root-structure)
- [Backend Structure (DDD + Vertical Slices)](#backend-structure-ddd--vertical-slices)
- [Frontend Structure (Feature-Based)](#frontend-structure-feature-based)
- [Documentation & Scripts](#documentation--scripts)

---

## Monorepo Root Structure

```
chickquita/
├── .github/
│   └── workflows/              # GitHub Actions CI/CD pipelines
│       ├── ci.yml              # Build, test, lint
│       ├── deploy.yml          # Deploy to Azure Container Apps
│       └── lighthouse.yml      # PWA performance checks
├── docs/
│   ├── technology-stack.md     # Technology choices and rationale
│   ├── filesystem-structure.md # This document
│   ├── ui-layout-system.md     # UI design system and patterns
│   ├── coding-standards.md     # Code conventions
│   ├── test-strategy.md        # Testing approach
│   ├── plans/                  # Implementation plans
│   ├── api/                    # API documentation, OpenAPI specs
│   ├── architecture/           # Architecture diagrams, ADRs
│   └── Chickquita_PRD.md     # Product Requirements Document
├── backend/                    # .NET 8 API
│   ├── src/                    # Production code
│   └── tests/                  # Test projects
├── frontend/                   # React PWA
├── scripts/
│   ├── setup-dev.sh            # Local development environment setup
│   ├── seed-data.sh            # Test data seeding for development
│   └── docker-build.sh         # Container build script
├── .gitignore                  # Git ignore patterns
├── .editorconfig               # Code style settings (cross-editor)
├── Dockerfile                  # Single-image multi-stage build
├── README.md                   # Project overview, setup instructions
├── CLAUDE.md                   # AI assistant context
└── Chickquita.slnx             # Visual Studio solution file
```

### Key Principles

- **Separation:** Backend and frontend in separate root directories
- **Test Isolation:** Test projects separated in `backend/tests/` directory
- **Documentation:** Centralized in `docs/` with logical subcategories
- **Automation:** Scripts for common development tasks in `scripts/`
- **CI/CD:** GitHub Actions workflows at repository root
- **Single Container:** One Dockerfile builds both frontend and backend

---

## Backend Structure (DDD + Vertical Slices)

### Overview

The backend follows **Domain-Driven Design (DDD) with Vertical Slices**:
- Features organized by use case, not by technical layer
- CQRS pattern with MediatR
- Light DDD - aggregates, value objects, some domain logic in entities
- Clean separation: API → Application → Domain → Infrastructure

```
backend/
├── src/                              # Production code
│   ├── Chickquita.Api/              # Web API layer (entry point)
│   ├── Chickquita.Application/      # Application layer (features)
│   ├── Chickquita.Domain/           # Domain layer (business logic)
│   └── Chickquita.Infrastructure/   # Infrastructure (data access)
└── tests/                           # Test projects
    ├── Chickquita.Api.Tests/        # API integration tests
    └── Chickquita.Infrastructure.Tests/ # Infrastructure tests
```

---

### Chickquita.Api (Entry Point)

```
Chickquita.Api/
├── Program.cs                      # Application entry, middleware setup, DI
├── appsettings.json                # Configuration (non-secrets)
├── appsettings.Development.json    # Development overrides
├── appsettings.Production.json     # Production overrides
├── Middleware/
│   ├── TenantResolutionMiddleware.cs   # Extract TenantId from JWT
│   ├── ExceptionHandlingMiddleware.cs  # Global exception handler
│   └── RequestLoggingMiddleware.cs     # HTTP request logging
└── wwwroot/                        # Frontend static files (after build)
    ├── index.html
    ├── assets/
    │   ├── index-[hash].js
    │   └── index-[hash].css
    ├── icons/
    └── manifest.json
```

**Notes:**
- Minimal APIs registered in `Program.cs` via feature endpoints
- Middleware order matters: Exception → Logging → Auth → Tenant → Endpoints
- Static files served from `wwwroot` for production build

---

### Chickquita.Application (Vertical Slices)

```
Chickquita.Application/
├── Common/                         # Shared application concerns
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs   # MediatR pipeline - validates commands
│   │   ├── LoggingBehavior.cs      # MediatR pipeline - logs requests
│   │   └── UnitOfWorkBehavior.cs   # Transaction management (if needed)
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs    # Database context abstraction
│   │   ├── ICurrentUserService.cs      # Current user context
│   │   └── IDateTimeService.cs         # Testable time provider
│   ├── Models/
│   │   ├── Result.cs               # Result pattern for errors
│   │   ├── Error.cs                # Error type
│   │   └── PaginatedList.cs        # Pagination helper
│   └── Exceptions/
│       └── ValidationException.cs  # Validation failure exception
└── Features/                       # Feature slices (vertical)
    ├── Users/
    │   ├── Commands/
    │   │   └── SyncUser/
    │   │       ├── SyncUserCommand.cs       # Clerk webhook handler
    │   │       ├── SyncUserCommandHandler.cs
    │   │       └── SyncUserCommandValidator.cs
    │   ├── Queries/
    │   │   └── GetCurrentUser/
    │   │       ├── GetCurrentUserQuery.cs
    │   │       └── GetCurrentUserQueryHandler.cs
    │   ├── DTOs/
    │   │   ├── UserDto.cs
    │   │   └── ClerkWebhookDto.cs
    │   └── UsersEndpoints.cs        # Minimal API endpoint registration
    ├── Coops/
    │   ├── Commands/
    │   │   ├── CreateCoop/
    │   │   │   ├── CreateCoopCommand.cs
    │   │   │   ├── CreateCoopCommandHandler.cs
    │   │   │   └── CreateCoopCommandValidator.cs
    │   │   ├── UpdateCoop/
    │   │   │   ├── UpdateCoopCommand.cs
    │   │   │   ├── UpdateCoopCommandHandler.cs
    │   │   │   └── UpdateCoopCommandValidator.cs
    │   │   ├── DeleteCoop/
    │   │   │   ├── DeleteCoopCommand.cs
    │   │   │   └── DeleteCoopCommandHandler.cs
    │   │   └── ArchiveCoop/
    │   │       ├── ArchiveCoopCommand.cs
    │   │       └── ArchiveCoopCommandHandler.cs
    │   ├── Queries/
    │   │   ├── GetCoops/
    │   │   │   ├── GetCoopsQuery.cs
    │   │   │   └── GetCoopsQueryHandler.cs
    │   │   └── GetCoopById/
    │   │       ├── GetCoopByIdQuery.cs
    │   │       └── GetCoopByIdQueryHandler.cs
    │   ├── DTOs/
    │   │   └── CoopDto.cs
    │   └── CoopsEndpoints.cs
    ├── Flocks/
    │   ├── Commands/
    │   │   ├── CreateFlock/
    │   │   │   ├── CreateFlockCommand.cs
    │   │   │   ├── CreateFlockCommandHandler.cs
    │   │   │   └── CreateFlockCommandValidator.cs
    │   │   ├── UpdateFlock/
    │   │   ├── MatureChicks/           # Special domain action
    │   │   │   ├── MatureChicksCommand.cs
    │   │   │   ├── MatureChicksCommandHandler.cs
    │   │   │   └── MatureChicksCommandValidator.cs
    │   │   └── UpdateFlockComposition/
    │   │       ├── UpdateFlockCompositionCommand.cs
    │   │       ├── UpdateFlockCompositionCommandHandler.cs
    │   │       └── UpdateFlockCompositionCommandValidator.cs
    │   ├── Queries/
    │   │   ├── GetFlocks/
    │   │   ├── GetFlockById/
    │   │   └── GetFlockHistory/
    │   ├── DTOs/
    │   │   ├── FlockDto.cs
    │   │   └── FlockHistoryDto.cs
    │   └── FlocksEndpoints.cs
    ├── Purchases/
    │   ├── Commands/
    │   │   ├── CreatePurchase/
    │   │   ├── UpdatePurchase/
    │   │   └── DeletePurchase/
    │   ├── Queries/
    │   │   ├── GetPurchases/
    │   │   └── GetPurchaseById/
    │   ├── DTOs/
    │   │   └── PurchaseDto.cs
    │   └── PurchasesEndpoints.cs
    ├── DailyRecords/
    │   ├── Commands/
    │   │   ├── CreateDailyRecord/
    │   │   ├── UpdateDailyRecord/
    │   │   └── DeleteDailyRecord/
    │   ├── Queries/
    │   │   ├── GetDailyRecords/
    │   │   └── GetDailyRecordById/
    │   ├── DTOs/
    │   │   └── DailyRecordDto.cs
    │   └── DailyRecordsEndpoints.cs
    └── Statistics/
        ├── Queries/
        │   ├── GetDashboard/
        │   │   ├── GetDashboardQuery.cs
        │   │   └── GetDashboardQueryHandler.cs
        │   ├── GetEggCost/
        │   │   ├── GetEggCostQuery.cs
        │   │   └── GetEggCostQueryHandler.cs
        │   └── GetFlockEvolution/
        │       ├── GetFlockEvolutionQuery.cs
        │       └── GetFlockEvolutionQueryHandler.cs
        ├── DTOs/
        │   ├── DashboardDto.cs
        │   ├── EggCostDto.cs
        │   └── FlockEvolutionDto.cs
        └── StatisticsEndpoints.cs
```

**Key Points:**
- Each feature is self-contained with Commands, Queries, DTOs, Endpoints
- MediatR handles command/query dispatch
- FluentValidation validates requests via pipeline behavior
- Endpoints register routes in `Program.cs`

---

### Chickquita.Domain (Core Business Logic)

```
Chickquita.Domain/
├── Entities/                       # Aggregates and entities
│   ├── Tenant.cs                   # Aggregate root
│   ├── Coop.cs                     # Aggregate root
│   ├── Flock.cs                    # Aggregate root
│   │   └── Methods:
│   │       - MatureChicks(int chicksCount, int hens, int roosters)
│   │       - UpdateComposition(int hens, int roosters, int chicks)
│   │       - CalculateProductivity()
│   │       - AddHistoryEntry(ChangeType type, ...)
│   ├── FlockHistory.cs             # Entity (owned by Flock aggregate)
│   ├── Purchase.cs                 # Aggregate root
│   └── DailyRecord.cs              # Aggregate root
├── ValueObjects/                   # Immutable value objects
│   ├── Email.cs                    # Email with validation
│   ├── FlockComposition.cs         # Hens, Roosters, Chicks
│   ├── Money.cs                    # Amount + Currency (CZK)
│   └── DateRange.cs                # Start + End dates
├── Enums/
│   ├── ChangeType.cs               # Adjustment, Maturation
│   └── PurchaseType.cs             # Krmivo, Vitamíny, Stelivo, etc.
├── Exceptions/                     # Domain exceptions
│   ├── DomainException.cs          # Base domain exception
│   ├── InvalidFlockCompositionException.cs
│   ├── InsufficientChicksException.cs
│   └── FlockNotFoundException.cs
└── Interfaces/                     # Repository contracts
    ├── ICoopRepository.cs          # Per aggregate root
    ├── IFlockRepository.cs
    ├── IPurchaseRepository.cs
    ├── IDailyRecordRepository.cs
    └── ITenantRepository.cs
```

**DDD Principles:**
- **Aggregates:** Flock controls its FlockHistory (consistency boundary)
- **Value Objects:** FlockComposition, Money are immutable and validated
- **Domain Logic:** Lives in entities (e.g., `Flock.MatureChicks()`)
- **Repository per Aggregate:** Not per entity
- **No database concerns** in domain layer

---

### Chickquita.Infrastructure (Data Access)

```
Chickquita.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs         # EF Core DbContext
│   ├── Configurations/                 # Entity configurations (Fluent API)
│   │   ├── TenantConfiguration.cs
│   │   ├── CoopConfiguration.cs
│   │   ├── FlockConfiguration.cs
│   │   ├── FlockHistoryConfiguration.cs
│   │   ├── PurchaseConfiguration.cs
│   │   └── DailyRecordConfiguration.cs
│   ├── Repositories/                   # Repository implementations
│   │   ├── TenantRepository.cs
│   │   ├── CoopRepository.cs
│   │   ├── FlockRepository.cs
│   │   ├── PurchaseRepository.cs
│   │   └── DailyRecordRepository.cs
│   ├── Migrations/                     # EF Core migrations
│   │   └── YYYYMMDDHHMMSS_InitialCreate.cs
│   └── Interceptors/
│       └── TenantInterceptor.cs        # Auto-apply RLS context
├── Authentication/
│   ├── ClerkJwtValidator.cs            # Validates Clerk JWT tokens
│   └── ClerkWebhookValidator.cs        # Validates webhook signatures
├── Services/                           # Service implementations
│   ├── CurrentUserService.cs           # Resolves current user from HttpContext
│   └── DateTimeService.cs              # Testable DateTime.UtcNow
└── DependencyInjection.cs              # Infrastructure service registration
```

**Layer Dependencies:**
- Infrastructure implements Domain interfaces
- Infrastructure depends on Domain
- Domain has no dependencies (pure business logic)

---

### Chickquita.Tests (Testing)

```
Chickquita.Tests/
├── Application.Tests/              # Feature/slice tests
│   ├── Features/
│   │   ├── Users/
│   │   │   ├── SyncUserCommandTests.cs
│   │   │   ├── GetCurrentUserQueryTests.cs
│   │   │   └── SyncUserValidatorTests.cs
│   │   ├── Coops/
│   │   │   ├── CreateCoopCommandTests.cs
│   │   │   ├── GetCoopsQueryTests.cs
│   │   │   └── DeleteCoopCommandTests.cs
│   │   ├── Flocks/
│   │   │   ├── MatureChicksCommandTests.cs
│   │   │   ├── CreateFlockCommandTests.cs
│   │   │   └── GetFlockHistoryQueryTests.cs
│   │   └── Statistics/
│   │       ├── GetDashboardQueryTests.cs
│   │       └── GetEggCostQueryTests.cs
│   └── Common/
│       └── Behaviors/
│           ├── ValidationBehaviorTests.cs
│           └── LoggingBehaviorTests.cs
├── Domain.Tests/                   # Domain logic tests
│   ├── Entities/
│   │   ├── FlockTests.cs           # Test MatureChicks() logic
│   │   ├── CoopTests.cs
│   │   └── DailyRecordTests.cs
│   └── ValueObjects/
│       ├── FlockCompositionTests.cs
│       ├── MoneyTests.cs
│       └── EmailTests.cs
├── Integration.Tests/              # API + Database integration
│   ├── Features/
│   │   ├── Users/
│   │   │   └── UsersEndpointsTests.cs
│   │   ├── Coops/
│   │   │   └── CoopsEndpointsTests.cs
│   │   └── Flocks/
│   │       └── FlocksEndpointsTests.cs
│   ├── Infrastructure/
│   │   └── Repositories/
│   │       ├── FlockRepositoryTests.cs  # Postgres test database
│   │       ├── CoopRepositoryTests.cs
│   │       └── DailyRecordRepositoryTests.cs
│   └── TestUtilities/
│       ├── ChickquitaWebApplicationFactory.cs  # Test server
│       ├── IntegrationTestBase.cs
│       ├── PostgresFixture.cs          # Test database setup
│       └── AutoMoqDataAttribute.cs     # AutoFixture + Moq
└── README.md                       # Test execution guide
```

**Test Libraries:**
- xUnit
- Moq
- AutoFixture (+ AutoFixture.Xunit2, AutoFixture.AutoMoq)
- FluentAssertions

---

## Frontend Structure (Feature-Based)

### Overview

The frontend follows **feature-based organization** aligned with backend vertical slices:
- Each feature contains components, hooks, API calls, and types
- Pages are located at `src/pages/` for better separation of concerns
- Shared code in `shared/` directory and `components/` at root level
- React Context providers in `src/contexts/` directory (alongside Zustand)
- Configuration and setup in `lib/`

**Note:** Frontend structure evolved during implementation for better separation of concerns - pages at root level, feature-specific components in features/.

```
src/frontend/
├── public/
├── src/
├── tests/
└── configuration files
```

---

### Public Assets

```
public/
├── icons/                          # PWA icons
│   ├── icon-72x72.png
│   ├── icon-192x192.png
│   └── icon-512x512.png
├── manifest.json                   # Generated by vite-plugin-pwa
└── robots.txt
```

---

### Source Code

```
src/
├── main.tsx                        # App entry point
├── App.tsx                         # Root component, routing setup
├── vite-env.d.ts                   # Vite TypeScript types
├── pages/                          # Page components (routing destinations)
│   ├── SignInPage.tsx              # Clerk <SignIn /> component
│   ├── SignUpPage.tsx              # Clerk <SignUp /> component
│   ├── DashboardPage.tsx           # Main dashboard page
│   ├── CoopsPage.tsx               # Coops list page
│   ├── CoopDetailPage.tsx          # Single coop detail
│   ├── FlocksPage.tsx              # Flocks list page (within coop)
│   ├── FlockDetailPage.tsx         # Single flock detail
│   ├── DailyRecordsListPage.tsx    # Daily records list
│   ├── SettingsPage.tsx            # User settings
│   └── NotFoundPage.tsx            # 404 page
├── features/                       # Feature modules (vertical slices)
│   ├── coops/
│   │   ├── components/
│   │   │   ├── CoopCard.tsx
│   │   │   ├── CreateCoopModal.tsx
│   │   │   ├── EditCoopModal.tsx
│   │   │   ├── DeleteCoopDialog.tsx
│   │   │   ├── ArchiveCoopDialog.tsx
│   │   │   └── CoopsEmptyState.tsx
│   │   ├── hooks/
│   │   │   ├── useCoops.ts
│   │   │   ├── useCreateCoop.ts
│   │   │   ├── useUpdateCoop.ts
│   │   │   ├── useDeleteCoop.ts
│   │   │   └── useArchiveCoop.ts
│   │   ├── api/
│   │   │   └── coopsApi.ts
│   │   └── types/
│   │       └── coop.types.ts
│   ├── flocks/
│   │   ├── components/
│   │   │   ├── FlockCard.tsx
│   │   │   ├── CreateFlockModal.tsx
│   │   │   ├── EditFlockModal.tsx
│   │   │   ├── FlockHistoryTimeline.tsx
│   │   │   ├── MatureChicksModal.tsx
│   │   │   └── UpdateCompositionModal.tsx
│   │   ├── hooks/
│   │   │   ├── useFlocks.ts
│   │   │   ├── useMatureChicks.ts
│   │   │   ├── useUpdateComposition.ts
│   │   │   └── useFlockHistory.ts
│   │   ├── api/
│   │   │   └── flocksApi.ts
│   │   └── types/
│   │       └── flock.types.ts
│   ├── purchases/
│   │   ├── components/
│   │   │   ├── PurchaseForm.tsx
│   │   │   ├── PurchaseList.tsx
│   │   │   └── PurchaseListSkeleton.tsx
│   │   ├── hooks/
│   │   │   ├── usePurchases.ts
│   │   │   ├── useCreatePurchase.ts
│   │   │   ├── useUpdatePurchase.ts
│   │   │   ├── useDeletePurchase.ts
│   │   │   └── usePurchaseAutocomplete.ts
│   │   ├── api/
│   │   │   └── purchasesApi.ts
│   │   ├── types/
│   │   │   └── purchase.types.ts
│   │   └── pages/                  # Feature-specific page
│   │       └── PurchasesPage.tsx
│   ├── dailyRecords/
│   │   ├── components/
│   │   │   ├── QuickAddModal.tsx       # FAB modal (< 30 sec target)
│   │   │   ├── DailyRecordForm.tsx
│   │   │   ├── DailyRecordCard.tsx
│   │   │   ├── DailyRecordCardSkeleton.tsx
│   │   │   ├── DeleteDailyRecordDialog.tsx
│   │   │   └── RecordsList.tsx
│   │   ├── hooks/
│   │   │   ├── useDailyRecords.ts
│   │   │   ├── useCreateDailyRecord.ts
│   │   │   ├── useUpdateDailyRecord.ts
│   │   │   ├── useDeleteDailyRecord.ts
│   │   │   └── useQuickAdd.ts
│   │   ├── api/
│   │   │   └── dailyRecordsApi.ts
│   │   └── types/
│   │       └── dailyRecord.types.ts
│   └── dashboard/
│       ├── components/
│       │   ├── DashboardWidgets.tsx
│       │   ├── EggCostChart.tsx
│       │   ├── ProductivityChart.tsx
│       │   └── CostBreakdownChart.tsx
│       ├── hooks/
│       │   ├── useDashboard.ts
│       │   └── useEggCost.ts
│       ├── api/
│       │   └── statisticsApi.ts
│       └── types/
│           └── dashboard.types.ts
├── components/                     # Root-level shared components
│   ├── BottomNavigation.tsx        # Main bottom navigation
│   ├── ProtectedRoute.tsx          # Route wrapper for auth
│   ├── ResourceNotFound.tsx        # Resource not found message
│   └── ToastProvider.tsx           # Toast notification provider
├── contexts/                       # React Context providers
│   └── ToastContext.ts             # Toast context definition
├── shared/                         # Shared across features
│   ├── components/                 # Reusable UI components
│   │   ├── NumericStepper.tsx      # +/- buttons for numbers
│   │   ├── IllustratedEmptyState.tsx   # Empty state with illustration
│   │   ├── StatCard.tsx            # Dashboard statistics card
│   │   ├── ConfirmationDialog.tsx  # Standardized confirmation dialog
│   │   ├── CoopCardSkeleton.tsx    # Loading skeleton
│   │   ├── FlockCardSkeleton.tsx   # Loading skeleton
│   │   └── CoopDetailSkeleton.tsx  # Loading skeleton
│   └── constants/
│       └── modalConfig.ts          # Modal configuration constants
├── hooks/                          # Shared custom hooks
│   ├── useDebounce.ts
│   ├── useLocalStorage.ts
│   ├── useOnlineStatus.ts
│   └── useMediaQuery.ts
├── lib/                            # Third-party library setup
│   └── api/
│       ├── apiClient.ts            # Axios instance with Clerk token interceptor
│       └── queryClient.ts          # TanStack Query configuration
├── theme/                          # Material-UI theme
│   ├── index.ts                    # Theme exports
│   └── theme.ts                    # MUI theme customization
├── locales/                        # Internationalization
│   ├── cs/
│   │   └── translation.json        # Czech translations (primary)
│   └── en/
│       └── translation.json        # English translations
└── assets/                         # Static assets
    └── illustrations/              # SVG illustrations
        ├── empty-coops.svg
        ├── empty-flocks.svg
        ├── empty-purchases.svg
        └── empty-daily-records.svg
```

---

### Tests

```
tests/
├── unit/                           # Unit tests (Vitest)
│   ├── components/
│   │   ├── FlockCard.test.tsx
│   │   └── QuickAddModal.test.tsx
│   ├── hooks/
│   │   ├── useFlocks.test.ts
│   │   └── useAuth.test.ts
│   └── utils/
│       ├── formatters.test.ts
│       └── calculations.test.ts
├── integration/                    # Integration tests
│   └── features/
│       ├── auth.integration.test.tsx
│       └── daily-records.integration.test.tsx
├── e2e/                            # E2E tests (Playwright)
│   ├── auth.spec.ts
│   ├── daily-records.spec.ts
│   ├── offline-mode.spec.ts
│   ├── pages/                      # Page Object Models
│   │   ├── SignInPage.ts           # Clerk sign-in page
│   │   ├── DashboardPage.ts
│   │   └── FlockDetailPage.ts
│   └── helpers/
│       └── auth.ts                 # Clerk auth helpers for tests
└── setup.ts                        # Test environment setup
```

---

### Configuration Files

```
frontend/
├── .env.example                    # Environment variables template
├── .env.development                # Dev environment vars
├── .eslintrc.json                  # ESLint configuration
├── .prettierrc                     # Prettier configuration
├── tsconfig.json                   # TypeScript configuration
├── tsconfig.node.json              # TypeScript for Vite config
├── vite.config.ts                  # Vite configuration + PWA plugin
├── playwright.config.ts            # Playwright E2E configuration
├── vitest.config.ts                # Vitest unit test configuration
├── package.json                    # Dependencies & scripts
└── README.md                       # Frontend setup guide
```

---

## Documentation & Scripts

### Documentation Structure

```
docs/
├── technology-stack.md             # This stack and rationale
├── filesystem-structure.md         # Directory layout
├── ui-layout-system.md             # UI design system
├── coding-standards.md             # Code conventions
├── test-strategy.md                # Testing approach
├── plans/                          # Implementation plans
│   └── YYYY-MM-DD-feature-name.md
├── api/                            # API documentation
│   ├── openapi.yaml                # OpenAPI specification
│   └── endpoints.md                # Endpoint descriptions
├── architecture/                   # Architecture diagrams
│   ├── system-overview.md
│   ├── data-model.md
│   └── ADRs/                       # Architecture Decision Records
│       └── 001-vertical-slices.md
└── Chickquita_PRD.md             # Product Requirements Document
```

### Scripts

```
scripts/
├── setup-dev.sh                    # Local dev environment setup
│   └── Installs Azurite, sets up databases, seeds data
├── seed-data.sh                    # Test data seeding
│   └── Creates test tenants, coops, flocks, records
└── docker-build.sh                 # Container build script
    └── Builds and tags Docker image
```

---

## Summary

This filesystem structure provides:
- ✅ **Clear separation** - Backend (DDD slices) and Frontend (feature modules)
- ✅ **Scalability** - Easy to add new features without touching existing code
- ✅ **Testability** - Tests mirror source structure
- ✅ **Maintainability** - Feature cohesion, easy to find related code
- ✅ **Consistency** - Both backend and frontend use vertical slicing

**Backend:** DDD + Vertical Slices with CQRS (MediatR)
**Frontend:** Feature-based with shared components and utilities
**Testing:** Unit, Integration, E2E with clear separation
