# E2E Tests - Chickquita

End-to-end tests for Chickquita application using Playwright.

## Overview

This directory contains E2E tests following the **Page Object Model (POM)** pattern as specified in the test strategy document (`docs/test-strategy.md`).

## Directory Structure

```
e2e/
├── pages/              # Page Object Models
│   ├── LoginPage.ts
│   ├── SignUpPage.ts
│   ├── DashboardPage.ts
│   ├── CoopsPage.ts
│   ├── CreateCoopModal.ts
│   ├── EditCoopModal.ts
│   └── CoopDetailPage.ts
├── fixtures/           # Test data fixtures
│   └── coop.fixture.ts
├── clerk.setup.ts      # Authentication setup via @clerk/testing (runs before all tests)
├── coops.spec.ts       # M2: Coop Management tests
└── README.md           # This file
```

## Prerequisites

1. **Node.js** (v20+)
2. **Playwright** installed (see installation below)
3. **Test user account** (Clerk)

## Installation

```bash
# Install dependencies
npm install

# Install Playwright browsers
npx playwright install

# Install system dependencies (Linux only)
npx playwright install-deps
```

## Configuration

### Authentication Setup

E2E tests use **`@clerk/testing`** for automated authentication — no manual browser login required.

**Setup (one-time):**

1. Create `frontend/.env.test.local` (gitignored):
   ```env
   CLERK_SECRET_KEY=sk_test_your_key_here
   ```

2. Verify `frontend/.env.test` has test user credentials:
   ```env
   CLERK_PUBLISHABLE_KEY=pk_test_...
   E2E_CLERK_USER_USERNAME=your-test-user@example.com
   E2E_CLERK_USER_PASSWORD=your-test-password
   ```

3. Run tests — authentication happens automatically:
   ```bash
   npm run test:e2e
   ```

The `clerk.setup.ts` project signs in programmatically and saves state to `.clerk/user.json`.
All test projects reuse this state via `storageState: '.clerk/user.json'`.

For full details, see [docs/architecture/E2E_AUTH_SETUP.md](../../docs/architecture/E2E_AUTH_SETUP.md).

### Database Configuration

E2E tests use a **separate database** to avoid interfering with development data. Configure this in your user secrets:

```bash
# Navigate to the API project
cd ../../backend/src/Chickquita.Api

# Set the E2E database connection string in user secrets
dotnet user-secrets set "ConnectionStrings:E2ETests" "postgresql://username:password@endpoint/chickquita_e2e?sslmode=require"
```

Apply migrations to the E2E database:

```bash
cd ../../backend/src/Chickquita.Api

# Run migrations for E2E database
ASPNETCORE_ENVIRONMENT=E2ETests dotnet ef database update \
  --project ../Chickquita.Infrastructure \
  --startup-project .
```

### Starting the Backend for E2E Tests

Before running E2E tests, start the backend in E2ETests mode:

**Option 1: Using dotnet run**
```bash
# Terminal 1: Start backend in E2ETests mode
cd backend/src/Chickquita.Api
dotnet run --launch-profile E2ETests
```

**Option 2: Using the helper script**
```bash
# Terminal 1: Start backend using helper script
cd frontend/e2e
./start-backend-e2e.sh
```

The backend will use the `ConnectionStrings:E2ETests` connection string from user secrets.

### Environment Variables

Optional environment variables:

```bash
# Application URL (optional, defaults to http://localhost:3100)
VITE_APP_URL=http://localhost:3100
```

## Running Tests

### Run all tests
```bash
npm run test:e2e
```

### Run specific test file
```bash
npx playwright test coops.spec.ts
```

### Run in headed mode (see browser)
```bash
npx playwright test --headed
```

### Run in debug mode
```bash
npx playwright test --debug
```

### Run specific test by name
```bash
npx playwright test -g "should create a coop with name only"
```

### Run on specific browser
```bash
npx playwright test --project=chromium
npx playwright test --project="Mobile Chrome"
```

### Run with UI mode
```bash
npx playwright test --ui
```

## Test Coverage

### Milestone 2: Coop Management (`coops.spec.ts`)

✅ **Create Coop**
- Create with name only
- Create with name and location
- Validation: Empty name error
- Validation: Max length enforcement
- Cancel creation

✅ **List Coops**
- Display empty state
- Display list of coops
- Navigate from dashboard

✅ **Edit Coop**
- Edit coop name
- Edit coop location
- Cancel edit
- Validation errors

✅ **Archive Coop**
- Archive confirmation
- Cancel archive

✅ **Delete Coop**
- Delete empty coop (with confirmation)
- Cancel delete
- _Note: Delete with active flocks requires M3_

✅ **Mobile Responsiveness**
- Mobile viewport functionality
- Touch-friendly button sizes (44x44px minimum)

✅ **Tenant Isolation**
- User only sees their own coops

## Authentication Setup

Tests use a shared authentication state via `@clerk/testing`:

1. `clerk.setup.ts` runs once before all tests
2. Signs in programmatically using Clerk Testing Token
3. Saves session to `.clerk/user.json`
4. All tests reuse this session

To re-authenticate, delete `.clerk/user.json` and run tests again.

## Page Object Model (POM)

Each page/component has a corresponding POM class:

```typescript
// Example: CoopsPage.ts
export class CoopsPage {
  readonly page: Page;
  readonly createCoopButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.createCoopButton = page.getByRole('button', { name: /add coop/i });
  }

  async openCreateCoopModal() {
    await this.createCoopButton.click();
  }
}
```

### Benefits:
- **Maintainability**: UI changes only require updating POM
- **Reusability**: Share page logic across tests
- **Readability**: Tests read like user actions

## Test Data Fixtures

Use fixtures from `fixtures/coop.fixture.ts` for consistent test data:

```typescript
import { testCoops } from './fixtures/coop.fixture';

// Generate test coop data
const coop = testCoops.withLocation();
await createCoopModal.createCoop(coop.name, coop.location);
```

## CI/CD Integration

Tests run automatically on:
- Pull requests to `main`
- Pushes to `main`

GitHub Actions workflow: `.github/workflows/test.yml`

### CI Configuration
- Retries: 2 (on failure)
- Workers: 1 (sequential on CI)
- Browsers: Chromium only (on CI)
- Screenshots: On failure
- Videos: On failure
- Trace: On first retry

## Debugging

### View test report
```bash
npx playwright show-report
```

### View trace
```bash
npx playwright show-trace trace.zip
```

### Slow motion mode
```bash
npx playwright test --headed --slow-mo=1000
```

### Inspect locators
```bash
npx playwright codegen http://localhost:5173
```

## Best Practices

1. **Use semantic locators**: Prefer `getByRole`, `getByLabel` over CSS selectors
2. **Wait for visibility**: Use `toBeVisible()` instead of `waitForTimeout()`
3. **Unique test data**: Use timestamps in test data to avoid conflicts
4. **Cleanup**: Tests should be independent (create their own data)
5. **Mobile testing**: Test critical flows on mobile viewports
6. **Accessibility**: Use ARIA roles for reliable selectors

## Common Issues

### Issue: Database connection error during E2E tests
**Solution**:
- Verify `ConnectionStrings:E2ETests` is set in user secrets
- Ensure the E2E database exists and migrations are applied
- Check that the backend is running in E2ETests mode (`--launch-profile E2ETests`)
- Verify connection string format: `postgresql://user:pass@host/dbname?sslmode=require`

### Issue: Test user credentials not found
**Solution**: Set `TEST_USER_EMAIL` and `TEST_USER_PASSWORD` environment variables

### Issue: Clerk rate limiting
**Solution**: Use different test users or wait between test runs

### Issue: Flaky tests
**Solution**:
- Avoid `waitForTimeout()`, use explicit waits
- Ensure proper cleanup between tests
- Check for race conditions in async operations

### Issue: Tests fail on CI but pass locally
**Solution**:
- Check CI environment variables
- Verify database state on CI
- Review CI logs and screenshots

## Performance Targets

- E2E suite runtime: < 2 minutes (per test-strategy.md)
- Individual test: < 30 seconds
- Mobile tests: < 45 seconds

## Next Milestones

- **M3: Basic Flock Creation** - Add tests for flock CRUD operations
- **M4: Daily Egg Records** - Add tests for daily record creation
- **M10: Offline Mode** - Add tests for offline functionality

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [Test Strategy](../../docs/test-strategy.md)
- [PRD](../../docs/ChickenTrack_PRD.md)

## Maintenance

- Review and update POMs when UI changes
- Add new test cases for bug fixes
- Keep test data fixtures up to date
- Update README when adding new test files
