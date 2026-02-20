# Technology Stack

**Chickquita (Chickquita)** - Technology choices and architectural decisions for the mobile-first PWA.

**Version:** 2.1
**Date:** February 9, 2026
**Status:** Approved

> **Note on Version Numbers:**
> This document shows baseline versions as of 2026-02-09. Versions may be newer than documented during active development - this is expected and normal. For exact current versions, refer to `frontend/package.json` and `backend/**/*.csproj` files. We recommend reviewing this document quarterly to keep versions current.

---

## Table of Contents

- [Frontend Technology Stack](#frontend-technology-stack)
- [UI Component Library & Styling](#ui-component-library--styling)
- [PWA Stack](#pwa-stack)
- [Backend Stack (.NET 8)](#backend-stack-net-8)
- [Authentication Stack](#authentication-stack)
- [Infrastructure & DevOps](#infrastructure--devops)

---

## Frontend Technology Stack

### Core Framework & Build Tools

**React 19.2.0 (as of 2026-02-09)**
- UI framework with concurrent features for better UX
- Excellent mobile performance
- Large ecosystem and community support
- Note: This is a forward-compatible major version upgrade from 18.2

**TypeScript 5.9.3 (as of 2026-02-09)**
- Strict mode enabled for maximum type safety
- Better IDE support and refactoring
- Catches errors at compile time

**Vite 7.2.4 (as of 2026-02-09)** - Build tool
- Lightning-fast HMR (Hot Module Replacement) for development
- Optimized production builds with automatic code splitting
- Native ES modules support
- Plugin ecosystem for PWA features
- Significantly faster than webpack
- Note: This is a forward-compatible major version upgrade from 5.0

### Routing & Navigation

**React Router 7.13.0 (as of 2026-02-09)**
- Declarative routing with data loading
- Lazy loading for code splitting
- Protected route wrappers for authentication
- Navigation guards for offline mode
- Nested routes support
- Note: This is a forward-compatible major version upgrade from 6.20

### State Management

**Zustand 5.0.11 (as of 2026-02-09)** - Client state management (preferred over Redux Toolkit)
- Simpler API with minimal boilerplate
- Built-in persistence middleware
- Better DevTools integration
- Smaller bundle size (~1KB vs ~12KB for Redux)
- Easy integration with React Query for server state
- No provider wrapper needed
- Note: This is a forward-compatible major version upgrade from 4.4

**TanStack Query 5.90.20 (as of 2026-02-09)** - Server state management
- Automatic caching and invalidation
- Background refetching for fresh data
- Optimistic updates for offline support
- Request deduplication
- Built-in loading/error states
- Polling and real-time updates support

### Forms & Validation

**React Hook Form 7.71.1 (as of 2026-02-09)**
- Uncontrolled components for better performance
- Native HTML5 validation integration
- Easy integration with Zod schemas
- Minimal re-renders (only affected fields)
- Built-in error handling

**Zod 4.3.6 (as of 2026-02-09)** - Schema validation
- TypeScript-first schema validation
- Runtime type safety
- Composable validation rules
- Shared schemas between frontend/backend
- Clear error messages
- Note: This is a forward-compatible major version upgrade from 3.22

---

## UI Component Library & Styling

### Component Library

**Material-UI (MUI) 7.3.7 (as of 2026-02-09)** - Selected over Chakra UI

**Why MUI:**
- Superior mobile touch optimization out of the box
- Comprehensive component set (Date pickers, Autocomplete, Data grids)
- Built-in theming with CSS-in-JS (@emotion)
- Accessibility (WCAG 2.1 AA) compliance by default
- Excellent TypeScript support
- Large community and active maintenance
- Better date/time picker components (critical for daily records)
- Touch ripple effects built-in for better mobile feel

### Styling Approach

- **MUI's `sx` prop** for component-level styles
- **Theme customization** for brand colors, spacing, breakpoints
- **CSS modules** for complex custom components (if needed)
- **No styled-components** library to reduce bundle size

### Icons

**@mui/icons-material**
- Tree-shakeable imports
- Consistent design language
- 2000+ icons covering farming/agriculture themes
- Perfect integration with MUI components

### Data Visualization

**Recharts 3.7.0 (as of 2026-02-09)** - Selected over Chart.js

**Why Recharts:**
- Declarative API (React-like)
- Responsive by default
- Touch-friendly charts for mobile
- Smaller bundle than Chart.js
- Composable chart components
- Good enough for MVP charts (egg cost trends, productivity graphs)
- Note: This is a forward-compatible major version upgrade from 2.10

---

## PWA Stack

### Service Worker Management

**Workbox 7.4.0 (as of 2026-02-09)**
- Google's production-ready service worker library
- Precaching strategies for static assets
- Runtime caching with customizable strategies
- Background sync queue with retry logic
- Built-in Vite plugin: `vite-plugin-pwa`
- Well-documented and battle-tested

### Offline Storage

**Dexie.js 4.3.0 (as of 2026-02-09)** - IndexedDB wrapper
- Clean, promise-based API
- TypeScript support out of the box
- Observable queries for reactivity
- Supports complex queries and indexing
- Built-in versioning for schema migrations
- Better than raw IndexedDB API
- Note: This is a forward-compatible major version upgrade from 3.2

### Configuration

**vite-plugin-pwa** configuration:
```javascript
{
  registerType: 'autoUpdate',
  workbox: {
    globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
    runtimeCaching: [
      // Network-first for API calls
      // Cache-first for static assets
      // Background sync for mutations
    ]
  }
}
```

### Background Sync

- **Workbox BackgroundSyncPlugin** for queued requests
- Custom retry logic with exponential backoff
- Conflict resolution hooks
- Sync status indicators in UI
- 24-hour retention for pending requests

### Web App Manifest

- Auto-generated by `vite-plugin-pwa`
- Icons in multiple sizes (72, 96, 128, 192, 512px)
- Maskable icons for Android adaptive icons
- Screenshots for app store listings (Phase 2)
- Theme color and display mode configuration

### Push Notifications (Phase 2)

- Web Push API
- Firebase Cloud Messaging (FCM) or Azure Notification Hubs
- Service worker notification handling

---

## Backend Stack (.NET 8)

### Core Framework

**.NET 8.0 LTS**
- Long-term support until November 2026
- Native AOT support for faster cold starts
- Improved performance over .NET 6/7
- Built-in minimal API improvements
- Enhanced JSON serialization

**ASP.NET Core 8.0**
- **Minimal APIs** for lightweight endpoints (preferred for MVP)
- Can migrate to Controllers if complexity grows
- Built-in OpenAPI/Swagger support
- Rate limiting middleware (new in .NET 7+)
- Enhanced middleware pipeline

### Data Access

**Entity Framework Core 8.0.2 (as of 2026-02-09)** - ORM for Neon Postgres
- Code-First approach with migrations
- LINQ query support
- Change tracking and automatic updates
- Connection pooling and retry logic
- Global query filters for tenant isolation
- Migration tooling via `dotnet ef`

**Npgsql.EntityFrameworkCore.PostgreSQL 8.0.2 (as of 2026-02-09)**
- PostgreSQL provider for EF Core
- Full support for PostgreSQL 16 features
- JSON column support
- Array types support
- Async/await throughout

**Database Design:**
- Row-Level Security (RLS) for tenant isolation
- `tenant_id` column on all tables
- Foreign key relationships for referential integrity
- Indexes on frequently queried columns
- Automatic `created_at`/`updated_at` triggers

### Validation & Mapping

**FluentValidation 12.1.1 (as of 2026-02-09)**
- Fluent API for validation rules
- Async validation support
- Custom validators
- Integration with ASP.NET Core (automatic validation)
- Clear, testable validation logic
- Note: This is a forward-compatible major version upgrade from 11.9

**AutoMapper 12.0.1 (as of 2026-02-09)**
- DTO to Entity mapping
- Profile-based configuration
- Projection support for queries
- Reduces boilerplate mapping code

### Logging

**Microsoft.Extensions.Logging** - Standard .NET logging
- Built-in console and debug providers
- Application Insights integration
- Structured logging support
- Log level filtering
- Scoped logging context

**Application Insights SDK**
- Automatic request tracking
- Exception logging
- Performance metrics
- Custom event tracking
- Distributed tracing

---

## Authentication Stack

### Clerk.com - Authentication Platform

**Why Clerk:**
- ✅ Battle-tested authentication (SOC 2 Type II certified)
- ✅ Pre-built UI components (sign-in, sign-up, user profile)
- ✅ Automatic token management and refresh
- ✅ Email verification built-in
- ✅ Password reset flows handled
- ✅ Rate limiting managed
- ✅ Security best practices enforced
- ✅ Free tier: 10,000 monthly active users

**Clerk.BackendAPI (NuGet)** - .NET integration
- JWT token validation
- Webhook signature verification
- User sync capabilities
- Middleware for ASP.NET Core

**@clerk/clerk-react (npm)** - Frontend integration
- React hooks: `useAuth()`, `useUser()`, `useSession()`
- Pre-built components: `<SignIn />`, `<SignUp />`, `<UserButton />`
- Automatic token refresh
- Protected route components

### Authentication Flow

**MVP (Phase 1):**
- Email + Password via Clerk hosted UI
- JWT Bearer tokens (managed by Clerk)
- Automatic token refresh (7 days default)
- Password reset via Clerk
- Email verification via Clerk

**Phase 2 - Social Logins:**
- Google OAuth (Clerk free tier)
- Facebook OAuth (Clerk free tier)
- GitHub OAuth (Clerk free tier)
- Multi-factor authentication (requires Clerk Pro tier - $25/month)

### Backend Integration

**JWT Validation:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{clerkDomain}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://{clerkDomain}",
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });
```

**Webhook Integration:**
- Event: `user.created` → Create tenant in database
- Event: `user.updated` → Update tenant information
- Event: `user.deleted` → Soft delete tenant
- Signature verification via Svix library

**User-to-Tenant Mapping:**
- Clerk `userId` stored as `clerk_user_id` in `tenants` table
- Automatic tenant creation via webhook on sign-up
- `ClerkUserId` → `TenantId` lookup on each request
- Row-Level Security context set based on tenant

---

## Infrastructure & DevOps

### Azure Services

**Azure Container Apps** - Primary hosting
- Managed container orchestration
- Auto-scaling (0-N replicas)
- HTTPS out of the box
- Custom domain support
- Cost-effective pay-per-use model
- ~10-30 EUR/month estimated

**Neon Postgres** - Serverless PostgreSQL database
- Cost-friendly (Free tier: 0.5GB storage, 1 project)
- Serverless auto-scaling and auto-pause
- PostgreSQL 16 with full feature support
- Row-Level Security (RLS) for tenant isolation
- Automatic backups and point-in-time recovery
- Connection pooling built-in
- Branch database feature for dev/staging
- Upgrade: ~$20/month for 10GB when needed

**Azure Blob Storage** - Static assets
- CDN integration (Phase 2)
- Image storage for future photo uploads
- Cost-effective

**Azure Application Insights** - Monitoring
- Request tracking
- Exception logging
- Performance metrics
- Custom events
- Distributed tracing

**Azure Key Vault** - Secrets management
- Neon connection string
- Clerk secret key and webhook secret
- API keys
- Managed identities integration

**Clerk** - Authentication service
- Hosted authentication UI
- User management
- JWT token issuing and validation
- Webhook integration for user sync
- Free tier: 10k monthly active users

### Containerization

**Container Strategy: Single Image Deployment**
- Monorepo builds into one container image
- Frontend: Built as static files, served by ASP.NET Core
- Backend: ASP.NET Core serves both API and static SPA
- Static files served from `wwwroot` folder
- Simpler deployment, single service

**Podman** - Container engine
- Docker-compatible container runtime
- Rootless containers for better security
- Drop-in replacement for Docker CLI
- No daemon requirement

**Multi-stage Dockerfile:**
```dockerfile
# Stage 1: Build frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY src/frontend/package*.json ./
RUN npm ci
COPY src/frontend ./
RUN npm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app
COPY backend/src/*.sln ./
COPY backend/src/**/*.csproj ./
RUN dotnet restore
COPY backend/src ./
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /app/frontend/dist ./wwwroot
EXPOSE 80
ENTRYPOINT ["dotnet", "Chickquita.Api.dll"]
```

### Container Registry

**Docker Hub** - Selected over Azure Container Registry
- Free public/private repositories
- Simpler setup than ACR
- `podman push docker.io/username/chickquita:latest`
- Azure Container Apps can pull from Docker Hub
- No additional Azure service costs

### Development Mode

**No Docker Compose needed** - Just one container for production

**Local Development:**
- **Frontend:** `npm run dev` (port 3100, Vite HMR)
- **Backend:** `dotnet run` (port 5000, hot reload)
- Frontend proxies API calls to backend in dev mode

### CI/CD Pipeline

**GitHub Actions** - Selected over Azure DevOps
- Simpler integration with GitHub
- Free for public/private repositories
- Workflow triggers: push to main, PR creation
- Environment-based secrets

**Pipeline Stages:**
1. **Build & Test** - Backend + Frontend tests
2. **Lighthouse CI** - PWA performance audit
3. **Docker Build** - Multi-stage image build
4. **Push to Docker Hub** - Tag with git SHA
5. **Deploy to Azure** - Container Apps deployment
6. **Smoke Tests** - Basic health checks

**Triggers:**
- Push to `main` → Deploy to production
- Pull request → Run tests and Lighthouse audit

### Development Tools

**Backend:**
- **Rider** - JetBrains .NET IDE
- **Azure CLI** - Resource management
- **.NET CLI** - Project commands

**Frontend:**
- **VS Code** - Primary frontend IDE
- Extensions: ESLint, Prettier, Volar (Vue/TS)

**Containers:**
- **Podman Desktop** - Container management UI
- **Podman CLI** - Container operations

---

## Version Control

**Git Strategy:**
- **Main branch** - Production-ready code
- **Feature branches** - `feature/123-description`
- **Fix branches** - `fix/456-description`
- No develop branch - trunk-based development
- Squash merge to main via PR

---

## Summary

This technology stack prioritizes:
- ✅ **Mobile-first performance** - Fast load times, smooth UX
- ✅ **Offline capabilities** - PWA with service workers and IndexedDB
- ✅ **Developer experience** - Modern tools, fast builds, great DX
- ✅ **Type safety** - TypeScript frontend, C# backend
- ✅ **Cost efficiency** - Neon Postgres free tier, Container Apps scaling
- ✅ **Security** - Clerk authentication (SOC 2), Row-Level Security (RLS)
- ✅ **Relational data** - PostgreSQL for complex queries and joins

**Bundle Size Budget:**
- Main bundle: < 150KB
- Vendor bundle: < 200KB
- Total: < 350KB (gzipped)

**Performance Targets:**
- Lighthouse Score: > 90 (all categories)
- First Contentful Paint: < 1.5s
- Time to Interactive: < 3.5s
