# E2E Test Setup and Execution Guide

## Authentication

E2E tests use **`@clerk/testing`** for automated authentication. No manual browser login required.

### Setup

1. **Create `frontend/.env.test.local`** (gitignored — never commit this):
   ```env
   CLERK_SECRET_KEY=sk_test_your_key_here
   ```
   Get the key from Clerk Dashboard → API Keys → Secret keys (development instance only).

2. **Verify `frontend/.env.test`** contains:
   ```env
   CLERK_PUBLISHABLE_KEY=pk_test_...
   E2E_CLERK_USER_USERNAME=your-test-user@example.com
   E2E_CLERK_USER_PASSWORD=your-test-password
   ```

3. **Run tests** — authentication happens automatically:
   ```bash
   npm run test:e2e
   ```

Authentication state is saved to `.clerk/user.json` (gitignored). All test projects reuse this via `storageState: '.clerk/user.json'`.

## How to Run E2E Tests

### Step 1: Start the Dev Server

```bash
npm run dev
```

### Step 2: Run the Tests

```bash
# Run all e2e tests (auth setup runs automatically)
npm run test:e2e

# Run only chromium tests (faster)
npm run test:e2e -- --project=chromium

# Run with UI (interactive mode)
npm run test:e2e:ui

# Run in headed mode (see browser)
npm run test:e2e:headed

# Run in debug mode
npm run test:e2e:debug

# Run specific test file
npx playwright test coops.spec.ts

# Run specific test by name
npx playwright test -g "should create a coop with name only"
```

### Step 3: View Test Results

```bash
# View HTML report
npm run test:e2e:report

# View trace (if test failed)
npx playwright show-trace test-results/*/trace.zip
```

## Troubleshooting

### `CLERK_SECRET_KEY is not set`
Create `frontend/.env.test.local` with your Clerk dev secret key.

### Authentication fails
- Verify `E2E_CLERK_USER_USERNAME` and `E2E_CLERK_USER_PASSWORD` in `.env.test` are correct
- Ensure the test user exists in your Clerk dev instance with a verified email

### Tests fail with `Backend returned client errors`
1. Check the test output for the exact error and endpoint
2. Ensure backend is running on `http://localhost:5100`
3. Fix the backend issue and retry

### Port conflict on 3100
Ensure no other service uses port 3100, or update `playwright.config.ts` and `vite.config.ts`.

### Backend API not responding
Ensure backend is running on `http://localhost:5100` (check `.env.development`).

## Test Coverage

### ✅ API Integration
- Load coops page without API errors
- Handle API requests correctly
- Display empty state or coop list

### ✅ Coop Management (M2)
- Create coop (name only, name + location, validation, cancel)
- List coops (empty state, populated)
- Navigate from dashboard
- Edit coop (name, location, validation, cancel)
- Archive coop (confirm, cancel)
- Delete coop (confirm, cancel)
- Mobile responsiveness
- Tenant isolation

## Notes

- Tests run against the local development server (port 3100)
- Backend API must be running on port 5100
- Authentication uses `@clerk/testing` (programmatic, no browser interaction needed)
- Auth state is in `.clerk/user.json` (gitignored)
- All tests create their own test data with timestamps for isolation

For full authentication details, see [docs/architecture/E2E_AUTH_SETUP.md](../../docs/architecture/E2E_AUTH_SETUP.md).
