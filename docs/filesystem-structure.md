# Filesystem Structure

**ChickenTrack (Chickquita)** - Complete directory layout for the monorepo with vertical slice architecture.

**Version:** 1.0
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
│   └── ChickenTrack_PRD.md     # Product Requirements Document
├── src/
│   ├── backend/                # .NET 8 API
│   └── frontend/               # React PWA
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

- **Separation:** Backend and frontend in separate `src/` subdirectories
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
src/backend/
├── ChickenTrack.sln                    # .NET solution file
├── ChickenTrack.Api/                   # Web API layer (entry point)
├── ChickenTrack.Application/           # Application layer (features)
├── ChickenTrack.Domain/                # Domain layer (business logic)
├── ChickenTrack.Infrastructure/        # Infrastructure (data access)
└── ChickenTrack.Tests/                 # Test projects
```

---

### ChickenTrack.Api (Entry Point)

```
ChickenTrack.Api/
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

### ChickenTrack.Application (Vertical Slices)

```
ChickenTrack.Application/
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
    ├── Auth/
    │   ├── Commands/
    │   │   ├── Register/
    │   │   │   ├── RegisterCommand.cs
    │   │   │   ├── RegisterCommandHandler.cs
    │   │   │   └── RegisterCommandValidator.cs
    │   │   ├── Login/
    │   │   │   ├── LoginCommand.cs
    │   │   │   ├── LoginCommandHandler.cs
    │   │   │   └── LoginCommandValidator.cs
    │   │   ├── RefreshToken/
    │   │   │   ├── RefreshTokenCommand.cs
    │   │   │   └── RefreshTokenCommandHandler.cs
    │   │   └── ForgotPassword/
    │   │       ├── ForgotPasswordCommand.cs
    │   │       └── ForgotPasswordCommandHandler.cs
    │   ├── Queries/
    │   │   └── GetCurrentUser/
    │   │       ├── GetCurrentUserQuery.cs
    │   │       └── GetCurrentUserQueryHandler.cs
    │   ├── DTOs/
    │   │   ├── AuthResponse.cs
    │   │   ├── UserDto.cs
    │   │   └── TokenDto.cs
    │   └── AuthEndpoints.cs        # Minimal API endpoint registration
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

### ChickenTrack.Domain (Core Business Logic)

```
ChickenTrack.Domain/
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
│   ├── PurchaseType.cs             # Krmivo, Vitamíny, Stelivo, etc.
│   └── AuthProvider.cs             # Email, Google, Facebook, Microsoft
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

### ChickenTrack.Infrastructure (Data Access)

```
ChickenTrack.Infrastructure/
├── Persistence/
│   ├── TableStorage/
│   │   ├── TableStorageContext.cs      # Azure Table client wrapper
│   │   ├── Repositories/               # Repository implementations
│   │   │   ├── TenantRepository.cs
│   │   │   ├── CoopRepository.cs
│   │   │   ├── FlockRepository.cs
│   │   │   ├── PurchaseRepository.cs
│   │   │   └── DailyRecordRepository.cs
│   │   └── Converters/                 # Entity ↔ TableEntity mapping
│   │       ├── CoopConverter.cs        # Convert Coop to/from TableEntity
│   │       ├── FlockConverter.cs
│   │       └── EntityMapperBase.cs     # Shared mapping logic
│   └── Configurations/
│       └── TableNames.cs               # Centralized table name constants
├── Identity/
│   ├── ApplicationUser.cs              # Identity user entity
│   ├── TableStorageUserStore.cs        # Custom UserStore for Table Storage
│   └── JwtTokenService.cs              # JWT generation and validation
├── Services/                           # Service implementations
│   ├── CurrentUserService.cs           # Resolves current user from HttpContext
│   ├── EmailService.cs                 # Email sending (password reset)
│   └── DateTimeService.cs              # Testable DateTime.UtcNow
└── DependencyInjection.cs              # Infrastructure service registration
```

**Layer Dependencies:**
- Infrastructure implements Domain interfaces
- Infrastructure depends on Domain
- Domain has no dependencies (pure business logic)

---

### ChickenTrack.Tests (Testing)

```
ChickenTrack.Tests/
├── Application.Tests/              # Feature/slice tests
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── RegisterCommandTests.cs
│   │   │   ├── LoginCommandTests.cs
│   │   │   └── RegisterValidatorTests.cs
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
│   │   ├── Auth/
│   │   │   └── AuthEndpointsTests.cs
│   │   ├── Coops/
│   │   │   └── CoopsEndpointsTests.cs
│   │   └── Flocks/
│   │       └── FlocksEndpointsTests.cs
│   ├── Infrastructure/
│   │   └── Repositories/
│   │       ├── FlockRepositoryTests.cs  # Azure Table Storage Emulator
│   │       ├── CoopRepositoryTests.cs
│   │       └── DailyRecordRepositoryTests.cs
│   └── TestUtilities/
│       ├── ChickenTrackWebApplicationFactory.cs  # Test server
│       ├── IntegrationTestBase.cs
│       ├── TableStorageFixture.cs
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
- Each feature contains components, hooks, API calls, types, pages
- Shared code in `shared/` directory
- Configuration and setup in `lib/`

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
├── features/                       # Feature modules (vertical slices)
│   ├── auth/
│   │   ├── components/
│   │   │   ├── LoginForm.tsx
│   │   │   ├── RegisterForm.tsx
│   │   │   └── PasswordResetForm.tsx
│   │   ├── hooks/
│   │   │   ├── useAuth.ts
│   │   │   ├── useLogin.ts
│   │   │   └── useRegister.ts
│   │   ├── api/
│   │   │   └── authApi.ts          # API calls for auth
│   │   ├── types/
│   │   │   └── auth.types.ts
│   │   ├── pages/
│   │   │   ├── LoginPage.tsx
│   │   │   └── RegisterPage.tsx
│   │   └── store/
│   │       └── authStore.ts        # Zustand slice for auth state
│   ├── coops/
│   │   ├── components/
│   │   │   ├── CoopCard.tsx
│   │   │   ├── CoopForm.tsx
│   │   │   └── CoopList.tsx
│   │   ├── hooks/
│   │   │   ├── useCoops.ts
│   │   │   ├── useCreateCoop.ts
│   │   │   └── useUpdateCoop.ts
│   │   ├── api/
│   │   │   └── coopsApi.ts
│   │   ├── types/
│   │   │   └── coop.types.ts
│   │   └── pages/
│   │       ├── CoopsPage.tsx
│   │       └── CoopDetailPage.tsx
│   ├── flocks/
│   │   ├── components/
│   │   │   ├── FlockCard.tsx
│   │   │   ├── FlockForm.tsx
│   │   │   ├── FlockHistoryTimeline.tsx
│   │   │   └── MatureChicksModal.tsx
│   │   ├── hooks/
│   │   │   ├── useFlocks.ts
│   │   │   ├── useMatureChicks.ts
│   │   │   └── useFlockHistory.ts
│   │   ├── api/
│   │   │   └── flocksApi.ts
│   │   ├── types/
│   │   │   └── flock.types.ts
│   │   └── pages/
│   │       └── FlockDetailPage.tsx
│   ├── purchases/
│   │   ├── components/
│   │   │   ├── PurchaseForm.tsx
│   │   │   └── PurchaseList.tsx
│   │   ├── hooks/
│   │   │   └── usePurchases.ts
│   │   ├── api/
│   │   │   └── purchasesApi.ts
│   │   └── pages/
│   │       └── PurchasesPage.tsx
│   ├── daily-records/
│   │   ├── components/
│   │   │   ├── QuickAddModal.tsx       # FAB modal (< 30 sec target)
│   │   │   ├── DailyRecordForm.tsx
│   │   │   └── RecordsList.tsx
│   │   ├── hooks/
│   │   │   ├── useDailyRecords.ts
│   │   │   └── useQuickAdd.ts
│   │   ├── api/
│   │   │   └── dailyRecordsApi.ts
│   │   └── pages/
│   │       └── DailyRecordsPage.tsx
│   ├── statistics/
│   │   ├── components/
│   │   │   ├── DashboardWidgets.tsx
│   │   │   ├── EggCostChart.tsx
│   │   │   ├── ProductivityChart.tsx
│   │   │   └── CostBreakdownChart.tsx
│   │   ├── hooks/
│   │   │   ├── useDashboard.ts
│   │   │   └── useEggCost.ts
│   │   └── pages/
│   │       ├── DashboardPage.tsx
│   │       └── StatisticsPage.tsx
│   └── dashboard/
│       └── pages/
│           └── DashboardPage.tsx
├── shared/                         # Shared across features
│   ├── components/                 # Reusable UI components
│   │   ├── layout/
│   │   │   ├── AppLayout.tsx       # Main layout with bottom nav
│   │   │   ├── Header.tsx
│   │   │   ├── BottomNavigation.tsx
│   │   │   └── Sidebar.tsx         # Desktop sidebar (lg+)
│   │   ├── ui/                     # Generic UI components
│   │   │   ├── Button.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── Modal.tsx
│   │   │   ├── FAB.tsx             # Floating Action Button
│   │   │   ├── Spinner.tsx
│   │   │   └── Skeleton.tsx
│   │   ├── forms/
│   │   │   ├── FormInput.tsx
│   │   │   ├── FormSelect.tsx
│   │   │   ├── FormDatePicker.tsx
│   │   │   └── NumberStepper.tsx   # +/- buttons for numbers
│   │   └── feedback/
│   │       ├── Toast.tsx
│   │       ├── OfflineBanner.tsx
│   │       └── SyncIndicator.tsx
│   ├── hooks/                      # Shared custom hooks
│   │   ├── useDebounce.ts
│   │   ├── useLocalStorage.ts
│   │   ├── useOnlineStatus.ts
│   │   └── useMediaQuery.ts
│   ├── utils/                      # Utility functions
│   │   ├── formatters.ts           # Date, number, currency
│   │   ├── validators.ts           # Common validations
│   │   └── calculations.ts         # Egg cost, productivity
│   ├── types/                      # Shared TypeScript types
│   │   ├── api.types.ts            # API response types
│   │   ├── common.types.ts
│   │   └── index.ts
│   └── constants/
│       ├── routes.ts               # Route paths
│       ├── api.ts                  # API endpoints
│       └── config.ts               # App configuration
├── lib/                            # Third-party library setup
│   ├── api/
│   │   ├── axios.ts                # Axios instance with interceptors
│   │   └── queryClient.ts          # TanStack Query configuration
│   ├── pwa/
│   │   ├── serviceWorker.ts        # SW registration
│   │   ├── syncQueue.ts            # Background sync queue
│   │   └── installPrompt.ts        # PWA install prompt logic
│   └── db/
│       ├── dexie.ts                # IndexedDB setup (Dexie.js)
│       └── schema.ts               # DB schema definition
├── store/                          # Global Zustand store
│   ├── index.ts                    # Combined store
│   ├── slices/
│   │   ├── authSlice.ts            # Auth state
│   │   ├── uiSlice.ts              # UI state (modals, toasts)
│   │   └── syncSlice.ts            # Offline sync state
│   └── middleware.ts               # Persistence middleware
└── styles/
    ├── theme.ts                    # MUI theme customization
    ├── global.css                  # Global styles
    └── breakpoints.ts              # Responsive breakpoints
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
│   │   ├── LoginPage.ts
│   │   ├── DashboardPage.ts
│   │   └── FlockDetailPage.ts
│   └── helpers/
│       └── auth.ts                 # Auth helpers for tests
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
└── ChickenTrack_PRD.md             # Product Requirements Document
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
