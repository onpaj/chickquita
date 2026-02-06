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
├── auth.setup.ts       # Authentication setup (runs before all tests)
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

E2E tests require an authenticated session. Since we skip automated login, you need to save your authentication state manually once:

**Option 1: Using the helper script (Recommended)**
```bash
# Make sure dev server is running
npm run dev

# In another terminal, run the helper script
node e2e/save-auth.js

# A browser will open - log in to the app
# After login, close the browser
# Auth state will be saved to .auth/user.json
```

**Option 2: Using Playwright codegen**
```bash
# Start dev server
npm run dev

# Save auth state while interacting with the app
npx playwright codegen --save-storage=.auth/user.json http://localhost:3100

# Log in through the opened browser
# Close the browser when done
```

**Option 3: Manual storage state**
```bash
# Log in manually via browser
# Then save the storage state using browser DevTools
# Copy cookies and localStorage to .auth/user.json
```

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

Tests use a shared authentication state to avoid logging in for each test:

1. `auth.setup.ts` runs once before all tests
2. Creates authenticated session
3. Saves session to `.auth/user.json`
4. All tests reuse this session

To re-authenticate, delete `.auth/user.json` and run tests again.

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
