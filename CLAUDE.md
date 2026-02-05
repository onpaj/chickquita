# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**ChickenTrack** (Chickquita) is a mobile-first Progressive Web Application (PWA) for tracking the financial profitability of chicken farming. The application features:

- Multi-tenant architecture with isolated data per farmer
- Offline-first design for use outdoors at chicken coops
- Real-time cost tracking and egg price calculation
- Flock management including chicks maturation tracking

## Tech Stack

### Frontend
- **React 18+** with TypeScript
- **Vite** as build tool
- **React Router** for routing
- **Zustand** or **Redux Toolkit** for state management
- **TanStack Query (React Query)** for server state and caching
- **Material-UI (MUI)** or **Chakra UI** for component library
- **React Hook Form** + **Zod** for forms and validation
- **Recharts** or **Chart.js** for data visualization

### PWA Stack
- **Workbox** for service worker management
- **IndexedDB** (via Dexie.js) for offline storage
- **Background Sync API** for queued offline requests
- manifest.json with app icons and configuration

### Backend
- **.NET 8** Web API
- **ASP.NET Core** (Minimal APIs or Controllers)
- **Entity Framework Core** (Code First)
- **AutoMapper** for DTO mapping
- **FluentValidation** for request validation
- **Serilog** for structured logging

### Database
- **Azure Table Storage** (primary) for cost-efficiency and scalability
- Partition key: TenantId for data isolation
- Row key: Reverse timestamp for chronological queries

### Hosting
- **Azure Container Apps** (recommended) or **Azure Web App for Containers**
- **Docker** multi-stage builds
- **Azure CDN** for static assets (Phase 2)

## Architecture Principles

### Clean Architecture
- Onion/Clean Architecture pattern
- Dependency Injection throughout
- CQRS pattern with MediatR (optional)
- Repository + Unit of Work pattern (optional)

### Multi-tenancy
- Every user gets their own tenant on registration
- All data partitioned by TenantId
- JWT tokens include tenant claims
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
Tenant (User Account)
└── Coop (Kurník)
    └── Flock (Hejno)
        ├── FlockHistory (Historie změn složení)
        ├── DailyRecord (Denní záznamy vajec)
        └── Individual Chickens (Phase 3)
```

### Key Concepts
- **Flock Composition**: Hens (slepice), Roosters (kohouti), Chicks (kuřata)
- **Chick Maturation**: Converting chicks to adult hens/roosters with historical tracking
- **Purchases**: Feed, supplements, bedding, veterinary care, equipment
- **Daily Records**: Egg count per flock per day (offline-capable)
- **Egg Cost Calculation**: Total costs / Total eggs produced

## Development Commands

### Frontend (when created)
```bash
# Install dependencies
npm install

# Development server
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

### Backend (when created)
```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run locally
dotnet run --project src/ChickenTrack.Api

# Run tests
dotnet test

# Create migration (EF Core)
dotnet ef migrations add <MigrationName> --project src/ChickenTrack.Infrastructure

# Update database
dotnet ef database update --project src/ChickenTrack.Infrastructure
```

### Docker
```bash
# Build image
docker build -t chickentrack .

# Run container
docker run -p 8080:80 chickentrack

# Build and run with docker-compose
docker-compose up --build
```

## API Design Standards

### RESTful Conventions
- Use HTTP verbs correctly: GET, POST, PUT, DELETE
- URL-based versioning: `/api/v1/...`
- Consistent error response format with error codes
- CORS enabled for PWA origin

### Authentication
- JWT Bearer tokens (15 min expiration)
- Refresh tokens (30 days sliding expiration)
- Rate limiting on auth endpoints:
  - Login: 5 attempts / 15 min / IP
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

## Security Considerations

- Password hashing: bcrypt (cost factor 12)
- HTTPS/TLS 1.3 required
- Input validation on both frontend (Zod) and backend (FluentValidation)
- Parameterized queries only (prevent SQL injection)
- XSS protection via DOMPurify for user inputs
- GDPR compliance for EU users

## Testing Strategy

### Frontend
- Unit tests for utilities and business logic
- Component tests with React Testing Library
- E2E tests with Playwright (critical flows)
- Mobile device testing required (iOS Safari 15+, Android Chrome 90+)

### Backend
- Unit tests for domain logic
- Integration tests for API endpoints
- Repository tests (if using repository pattern)
- Validation tests for FluentValidation rules

### PWA Testing
- Lighthouse CI in pipeline
- Offline functionality testing
- Background sync verification
- Install prompt testing on mobile devices

## Important Notes

### Language
- PRD and documentation in **Czech** (Čeština)
- Code, comments, and variable names in **English**
- UI text in Czech (with i18n support planned for Phase 2/3)

### Data Model Specifics
- **Chicks (kuřata)** are counted in costs but NOT in egg production
- Only **hens (slepice)** contribute to egg count
- Flock history is immutable except for notes field
- First history entry = initial flock composition

### Phase 1 MVP Scope
Focus only on:
1. Authentication (JWT + refresh tokens)
2. Coops and Flocks CRUD (including chicks)
3. Chick maturation action with history
4. Purchases tracking
5. Daily records (offline-capable)
6. Dashboard with basic stats
7. Egg cost calculation
8. PWA features (install, offline, manifest)

## Development Workflow

1. Feature branches from `main`
2. Conventional commits preferred
3. PR required for merge to main
4. CI/CD pipeline runs tests and Lighthouse
5. Deploy to Azure Container Apps on merge

## File Structure (Target)

```
/
├── src/
│   ├── frontend/          # React PWA
│   │   ├── src/
│   │   │   ├── components/
│   │   │   ├── pages/
│   │   │   ├── hooks/
│   │   │   ├── services/
│   │   │   ├── store/
│   │   │   └── utils/
│   │   ├── public/
│   │   └── package.json
│   └── backend/           # .NET API
│       ├── ChickenTrack.Api/
│       ├── ChickenTrack.Core/
│       ├── ChickenTrack.Infrastructure/
│       └── ChickenTrack.Tests/
├── docs/
│   └── ChickenTrack_PRD.md
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Azure Resources

- Resource Group: `chickquita-rg`
- Table Storage: For all application data
- Container Apps: For hosting the application
- Application Insights: For monitoring and logging
- CDN: For static assets (Phase 2)

## Monitoring

- **Application Insights** for telemetry
- Structured logging with Serilog
- Custom events for business metrics:
  - User registration
  - Daily record created
  - Chick maturation
  - Offline sync completed
- Alert on error rate > 5% or response time > 1s (p95)
