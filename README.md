# Chickquita (Chickquita)

> Mobile-first Progressive Web Application for tracking the financial profitability of chicken farming

## Overview

Chickquita is a comprehensive solution for chicken farmers to track costs, egg production, and flock management. Built as a PWA for offline-first mobile use, the application helps farmers answer the key question: "What is the real cost per egg?"

### Key Features

- **Multi-tenant Architecture**: Each farmer gets isolated data via Clerk authentication
- **Offline-First Design**: Record daily egg counts and purchases even without internet
- **Flock Management**: Track chickens (hens, roosters, chicks) with maturation history
- **Purchase Tracking**: Comprehensive expense management with categorization (feed, vitamins, bedding, toys, veterinary, other)
- **Cost Analysis**: Monitor all expenses with autocomplete, filtering, and consumption tracking
- **Real-time Analytics**: Calculate egg cost based on total expenses and production
- **Mobile-Optimized**: Touch-friendly UI designed for outdoor use at chicken coops

## Tech Stack

### Frontend
- **React 18+** with TypeScript
- **Vite** for blazing-fast development
- **Material-UI (MUI)** component library
- **TanStack Query** for server state management
- **Zustand** for client state
- **Workbox** for PWA capabilities
- **IndexedDB** for offline storage

### Backend
- **.NET 8** Web API
- **Entity Framework Core** with Code First approach
- **MediatR** for CQRS pattern
- **FluentValidation** for request validation
- **ASP.NET Core Minimal APIs**

### Infrastructure
- **Clerk.com**: Authentication and user management
- **Neon Postgres**: Serverless PostgreSQL database with Row-Level Security
- **Azure Container Apps**: Container hosting
- **GitHub Actions**: CI/CD pipeline
- **Docker**: Multi-stage containerized deployment

## Project Structure

```
chickquita/
├── backend/           # .NET 8 Web API
│   ├── src/          # Production code
│   └── tests/        # Test projects
├── frontend/          # React PWA application
├── docs/              # Comprehensive project documentation
├── .github/           # GitHub Actions workflows
│   └── workflows/     # CI/CD pipelines
├── Dockerfile         # Multi-stage build (frontend + backend)
├── CLAUDE.md          # Claude Code assistant instructions
└── README.md          # This file
```

## Documentation

Comprehensive documentation is available in the `/docs` directory:

- **[Chickquita_PRD.md](docs/Chickquita_PRD.md)** - Product Requirements Document
- **[technology-stack.md](docs/technology-stack.md)** - Detailed technology choices and rationale
- **[filesystem-structure.md](docs/filesystem-structure.md)** - Complete project structure
- **[coding-standards.md](docs/coding-standards.md)** - Code quality guidelines
- **[test-strategy.md](docs/test-strategy.md)** - Testing approach and coverage
- **[ui-layout-system.md](docs/ui-layout-system.md)** - Mobile-first design system
- **[neon-database-setup.md](docs/neon-database-setup.md)** - Neon Postgres database creation guide
- **[database-connection-guide.md](docs/database-connection-guide.md)** - Database connection configuration
- **[API_SPEC_COOPS.md](docs/API_SPEC_COOPS.md)** - Coops API specification
- **[API_SPEC_PURCHASES.md](docs/API_SPEC_PURCHASES.md)** - Purchases API specification

## Getting Started

### Prerequisites

- **Node.js** 18+ and npm
- **.NET 8 SDK**
- **Docker** (for containerized deployment)
- **PostgreSQL** (or Neon account for cloud database)
- **Clerk** account for authentication

### Frontend Development

```bash
cd frontend
npm install
npm run dev
```

The application will start at `http://localhost:5173`

### Backend Development

First, configure the database connection:

```bash
cd backend/src/Chickquita.Api
cp .env.example .env
# Edit .env and add your Neon connection string
```

See [database-connection-guide.md](docs/database-connection-guide.md) for detailed setup instructions.

Then start the backend:

```bash
cd backend
dotnet restore
dotnet run --project src/Chickquita.Api
```

The API will start at `http://localhost:5000`

### Docker Deployment

```bash
# Build multi-stage image
docker build -t chickquita .

# Run container
docker run -p 8080:80 chickquita
```

## Environment Variables

### Frontend
- `VITE_CLERK_PUBLISHABLE_KEY` - Clerk public key
- `VITE_API_BASE_URL` - Backend API URL

### Backend
- `ConnectionStrings__DefaultConnection` - Neon Postgres connection string
- `Clerk__SecretKey` - Clerk secret key for JWT validation
- `Clerk__WebhookSecret` - Clerk webhook signing secret

Store secrets securely in Azure Key Vault for production.

## Key Concepts

### Multi-tenancy
Every user gets their own tenant on registration (via Clerk webhook). All data is partitioned by `tenant_id` with Row-Level Security enforced at the database level.

### Offline-First
- Static assets cached for 30 days
- API GET requests use network-first with cache fallback
- POST/PUT/DELETE requests queue in background sync (24h retention)
- IndexedDB stores local data for offline use

### Flock Composition
Tracks three chicken types:
- **Hens**: Adult females that lay eggs
- **Roosters**: Adult males
- **Chicks**: Young chickens (counted in feed costs, not egg production)

Chicks can be "matured" into hens/roosters with immutable historical tracking.

### Purchase Management
Comprehensive expense tracking system:
- **Categories**: Feed, Vitamins, Bedding, Toys, Veterinary, Other
- **Units**: Support for Kg, Pieces, Liters, Packages, and custom units
- **Date Tracking**: Purchase date and optional consumption date
- **Autocomplete**: Smart suggestions based on previous purchases
- **Filtering**: Filter by date range, type, and associated flock
- **Notes**: Optional notes for each purchase (max 500 characters)
- **Coop Association**: Link purchases to specific coops or keep them general

## Authentication Flow

1. User signs up via Clerk hosted UI (email + password)
2. Clerk webhook creates tenant in Neon database
3. User receives JWT token for API access
4. Backend validates JWT and sets Row-Level Security context
5. All queries automatically filtered by tenant

## Development Workflow

1. Create feature branch from `main`
2. Implement feature with tests
3. Run quality checks and Lighthouse CI
4. Create Pull Request
5. Merge triggers CI/CD deployment to Azure

## Quality Standards

- **Lighthouse Score**: > 90 (all categories)
- **Test Coverage**: > 80% for backend, > 70% for frontend
- **Bundle Size**: < 200kb gzipped
- **Performance**: FCP < 1.5s, TTI < 3.5s

## Internationalization

- **Primary Language**: Czech (cs-CZ)
- **Secondary Language**: English (en-US)
- User-switchable in application settings
- All code and documentation in English

## License

Proprietary - All rights reserved

## Support

For issues, questions, or feature requests, please contact the development team.

---

**Built with by the Chickquita team**
