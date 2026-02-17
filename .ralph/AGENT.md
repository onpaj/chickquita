# Agent Build Instructions — Chickquita

## Project Overview

Chickquita is a mobile-first PWA for tracking chicken farming profitability. It is a **monorepo** with:

- **Frontend**: React 19 + TypeScript + Vite + MUI (in `frontend/`)
- **Backend**: .NET 8 Minimal API + EF Core + MediatR (in `backend/`)
- **Database**: Neon Postgres (serverless) with Row-Level Security
- **Auth**: Clerk.com (JWT Bearer tokens)
- **Hosting**: Docker multi-stage → Azure Container Apps

## Project Setup

### Frontend

```bash
cd frontend
npm ci --prefer-offline --no-audit
```

### Backend

```bash
dotnet restore backend/Chickquita.slnx
```

### Full Build (Docker)

```bash
docker build -t chickquita .
docker run -p 8080:80 chickquita
```

## Development Server

### Frontend (Vite HMR)

```bash
cd frontend
npm run dev
```

### Backend (.NET)

```bash
dotnet run --project backend/src/Chickquita.Api
```

## Running Tests

### Backend Tests (xUnit + FluentAssertions + Moq)

```bash
# Run all backend tests
dotnet test backend/Chickquita.slnx

# Run with coverage (CI mode)
dotnet test backend/Chickquita.slnx --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test backend/tests/Chickquita.Api.Tests
dotnet test backend/tests/Chickquita.Application.Tests
dotnet test backend/tests/Chickquita.Domain.Tests
dotnet test backend/tests/Chickquita.Infrastructure.Tests
```

### Frontend Unit Tests (Vitest)

```bash
cd frontend

# Run tests (watch mode)
npm run test

# Run with coverage report
npm run test:coverage

# Run with UI
npm run test:ui
```

### E2E Tests (Playwright)

```bash
cd frontend

# Run all E2E tests (chromium) — auth via @clerk/testing runs automatically
npm run test:e2e

# Run with visible browser
npm run test:e2e:headed

# Run with Playwright UI
npm run test:e2e:ui

# Debug mode
npm run test:e2e:debug

# Show last report
npm run test:e2e:report
```

> **⚠️ Auth note:** E2E tests authenticate via `@clerk/testing` (programmatic Clerk sign-in that **bypasses MFA**). The setup project `e2e/clerk.setup.ts` runs automatically before all tests — no separate auth step is needed. **Never authenticate manually via the sign-in UI** — that triggers MFA and will block automated tests.

### Cross-Browser Tests (Playwright)

```bash
cd frontend

# Run all cross-browser tests
npm run test:crossbrowser

# By browser
npm run test:crossbrowser:chrome
npm run test:crossbrowser:firefox
npm run test:crossbrowser:safari

# Mobile and breakpoint tests
npm run test:crossbrowser:mobile
npm run test:crossbrowser:breakpoints

# Update snapshots
npm run test:crossbrowser:update
```

## Build Commands

### Frontend Production Build

```bash
cd frontend
npm run build          # Runs tsc -b && vite build
```

### Backend Release Build

```bash
dotnet build backend/Chickquita.slnx --configuration Release
```

### Type Checking & Linting (Frontend)

```bash
cd frontend
npx tsc --noEmit       # Type check
npm run lint           # ESLint
```

## Database Migrations (EF Core)

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project backend/src/Chickquita.Infrastructure \
  --startup-project backend/src/Chickquita.Api

# Apply migrations
dotnet ef database update \
  --project backend/src/Chickquita.Infrastructure \
  --startup-project backend/src/Chickquita.Api
```

## Key Learnings

- **E2E auth**: Tests use `@clerk/testing` (programmatic Clerk sign-in, bypasses MFA). Auth runs automatically via `e2e/clerk.setup.ts` — **never sign in manually via the browser UI** as that triggers MFA. Requires `CLERK_SECRET_KEY` in `frontend/.env.test.local`. See `docs/architecture/E2E_AUTH_SETUP.md`.
- **RLS context**: Backend must call `SET app.current_tenant_id` before every query. EF Core global query filters provide additional safety.
- **Tenant auto-creation**: `TenantResolutionMiddleware` auto-creates tenants if Clerk webhook failed — no manual intervention needed.
- **Offline-first**: Daily records must work offline. Changes queue via Background Sync API (24h retention).
- **i18n**: UI is Czech primary, English secondary. All code/docs/commits in English. Use `t()` keys, never hardcoded Czech.
- **Docker port**: Container runs on port 8080 (non-privileged, user `appuser:1000`).
- **Solution file**: Use `backend/Chickquita.slnx` (not `.sln`) for all dotnet commands.
- **CI/CD**: GitHub Actions pipeline in `.github/workflows/ci-cd.yml` runs backend tests, frontend tests, E2E tests, coverage checks, and Docker build.
- **Cross-browser CI**: Separate workflow `.github/workflows/crossbrowser-tests.yml` runs weekly (Monday 6 AM UTC) and on dispatch.

## Architecture Documentation Reference

| Document | Description |
|----------|-------------|
| `docs/architecture/technology-stack.md` | Complete tech stack with versions and rationale |
| `docs/architecture/filesystem-structure.md` | Project directory structure and patterns |
| `docs/architecture/COMPONENT_LIBRARY.md` | Shared components, design system, theme config |
| `docs/architecture/ui-layout-system.md` | Layout patterns, typography, spacing, breakpoints |
| `docs/architecture/coding-standards.md` | C# and TypeScript naming conventions, patterns |
| `docs/architecture/test-strategy.md` | Testing approach, coverage targets, test pyramid |
| `docs/architecture/E2E_AUTH_SETUP.md` | Playwright auth setup for E2E tests |
| `docs/spec/` | API specifications (Coops, Purchases, Daily Records, DB Schema, i18n Keys) |

## Feature Development Quality Standards

**CRITICAL**: All new features MUST meet the following mandatory requirements before being considered complete.

### Testing Requirements

- **Backend Coverage**: 85% minimum line coverage (enforced by CI)
- **Frontend Coverage**: 70% minimum line coverage (enforced by CI)
- **Test Pass Rate**: 100% — all tests must pass, no exceptions
- **Test Pyramid** (per `docs/architecture/test-strategy.md`):
  - **60% Unit Tests**: Domain logic, services, validators, utilities
  - **30% Integration Tests**: API endpoints, EF Core repositories, middleware
  - **10% E2E Tests**: Critical user workflows via Playwright
- **Coverage Validation**: Run before marking features complete:
  ```bash
  # Backend coverage
  dotnet test backend/Chickquita.slnx --collect:"XPlat Code Coverage"

  # Frontend coverage
  cd frontend && npm run test:coverage

  # E2E tests
  cd frontend && npm run test:e2e
  ```
- **Test Quality**: Tests must validate behavior, not just achieve coverage metrics
- **Test Documentation**: Complex test scenarios must include comments explaining the test strategy

### Testing Stack Reference

| Layer | Backend | Frontend |
|-------|---------|----------|
| **Framework** | xUnit | Vitest |
| **Assertions** | FluentAssertions | Vitest built-in |
| **Mocking** | Moq | MSW (Mock Service Worker) |
| **Test Data** | AutoFixture | — |
| **Components** | — | React Testing Library |
| **E2E** | — | Playwright |
| **Accessibility** | — | @axe-core/playwright |
| **DB** | EF Core InMemory / SQLite | — |
| **Coverage** | XPlat Code Coverage | Vitest --coverage |

### Coding Standards

Follow the conventions documented in `docs/architecture/coding-standards.md`:

- **C# Backend**: PascalCase for public members, camelCase for private fields with `_` prefix, CQRS pattern with MediatR, FluentValidation for requests, AutoMapper for DTOs
- **TypeScript Frontend**: camelCase for variables/functions, PascalCase for components/types, feature-based folder structure, React Hook Form + Zod for forms
- **UI Components**: Use shared components from `@/shared/components` (see `docs/architecture/COMPONENT_LIBRARY.md`)
- **All code, comments, docs**: English only. UI text uses i18n keys (`t('key')`)

### Git Workflow Requirements

Before moving to the next feature, ALL changes must be:

1. **Committed with Clear Messages**:
   ```bash
   git add <specific-files>
   git commit -m "feat(module): descriptive message following conventional commits"
   ```
   - Conventional commit format: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, etc.
   - Include scope: `feat(api):`, `fix(ui):`, `test(auth):`, `feat(coops):`
   - Descriptive messages explaining WHAT changed and WHY

2. **Pushed to Remote Repository**:
   ```bash
   git push origin <branch-name>
   ```
   - Never leave completed features uncommitted
   - Push regularly to maintain backup and enable collaboration
   - Ensure CI/CD pipeline passes before considering feature complete

3. **Branch Hygiene**:
   - Work on feature branches, never directly on `main`
   - Branch naming: `feature/<feature-name>`, `fix/<issue-name>`, `docs/<doc-update>`
   - Create pull requests for all significant changes

4. **Ralph Integration**:
   - Update `.ralph/fix_plan.md` with new tasks before starting work
   - Mark items complete in `.ralph/fix_plan.md` upon completion
   - Update `.ralph/PROMPT.md` if development patterns change
   - Test features work within Ralph's autonomous loop

### Documentation Requirements

**ALL implementation documentation MUST remain synchronized with the codebase**:

1. **Code Documentation**:
   - C#: XML doc comments on public APIs
   - TypeScript: JSDoc where needed for complex logic
   - Update inline comments when implementation changes
   - Remove outdated comments immediately

2. **Architecture Documentation**:
   - Update relevant docs in `docs/architecture/` when patterns change
   - Keep `docs/architecture/technology-stack.md` versions current
   - Update `docs/architecture/COMPONENT_LIBRARY.md` when adding shared components
   - Update `docs/architecture/test-strategy.md` when test patterns evolve

3. **API Specifications**:
   - Update specs in `docs/spec/` when endpoints change
   - Keep request/response schemas current

4. **AGENT.md Maintenance**:
   - Add new build patterns to relevant sections
   - Update "Key Learnings" with new insights
   - Keep command examples accurate and tested
   - Document new testing patterns or quality gates

### Feature Completion Checklist

Before marking ANY feature as complete, verify:

- [ ] Backend tests pass: `dotnet test backend/Chickquita.slnx`
- [ ] Frontend tests pass: `cd frontend && npm run test`
- [ ] E2E tests pass: `cd frontend && npm run test:e2e`
- [ ] Backend coverage meets 85% minimum threshold
- [ ] Frontend coverage meets 70% minimum threshold
- [ ] Type checking passes: `cd frontend && npx tsc --noEmit`
- [ ] Linting passes: `cd frontend && npm run lint`
- [ ] Code follows conventions in `docs/architecture/coding-standards.md`
- [ ] All changes committed with conventional commit messages
- [ ] All commits pushed to remote repository
- [ ] `.ralph/fix_plan.md` task marked as complete
- [ ] Architecture docs updated (if new patterns introduced)
- [ ] i18n keys added for all user-facing text
- [ ] Mobile-first responsive design verified
- [ ] Breaking changes documented

### Rationale

These standards ensure:
- **Quality**: High test coverage and pass rates prevent regressions
- **Traceability**: Git commits and `.ralph/fix_plan.md` provide clear history of changes
- **Maintainability**: Current documentation reduces onboarding time and prevents knowledge loss
- **Collaboration**: Pushed changes enable team visibility and code review
- **Reliability**: Consistent quality gates maintain production stability
- **Automation**: Ralph integration ensures continuous development practices

**Enforcement**: AI agents should automatically apply these standards to all feature development tasks without requiring explicit instruction for each task.
