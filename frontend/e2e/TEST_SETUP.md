# E2E Test Setup and Execution Guide

## Changes Made to Support E2E Tests

### 1. Authentication Setup
- **Removed automated login** - Tests now require manual authentication before running
- Created `save-auth.js` helper script to easily save authentication state
- Updated `playwright.config.ts` to remove auth setup dependency
- Tests will reuse saved authentication from `.auth/user.json`

### 2. Application Fixes

#### CoopCard Component (`src/features/coops/components/CoopCard.tsx`)
- ✅ Added `data-testid="coop-card"` attribute for test identification
- ✅ Added action menu with Edit, Archive, and Delete buttons
- ✅ Integrated modals (EditCoopModal, ArchiveCoopDialog, DeleteCoopDialog) directly in the component
- ✅ Actions now accessible from the list view (via menu) as tests expect

#### CreateCoopModal Component (`src/features/coops/components/CreateCoopModal.tsx`)
- ✅ Fixed location field label from `coopDescription` to `location`
- ✅ Now uses correct translation key that matches test expectations

#### EditCoopModal Component (`src/features/coops/components/EditCoopModal.tsx`)
- ✅ Fixed location field label from `coopDescription` to `location`
- ✅ Now uses correct translation key that matches test expectations

#### DashboardPage Page Object (`e2e/pages/DashboardPage.ts`)
- ✅ Fixed navigation button locator from `link` to `button` to match BottomNavigation implementation

#### Playwright Configuration (`playwright.config.ts`)
- ✅ Updated baseURL and webServer URL from port 5173 to 3100 (matches vite.config.ts)
- ✅ Removed auth setup dependency from test projects

#### New Files Created
- ✅ `e2e/pages/LoginPage.ts` - Page Object Model for Clerk login (for reference only, not used in tests)
- ✅ `e2e/save-auth.js` - Helper script to save authentication state
- ✅ `.auth/.gitignore` - Prevents committing sensitive auth tokens

## How to Run E2E Tests

### Step 1: Save Authentication State

Before running tests for the first time, you need to save your authentication state:

```bash
# Make sure dev server is running
npm run dev

# In another terminal, run the auth helper
npm run test:e2e:save-auth

# A browser will open automatically
# 1. Log in with your test account
# 2. Wait until you see the dashboard
# 3. Close the browser
# 4. Auth state will be saved to .auth/user.json
```

**Note**: You only need to do this once. The auth state will be reused for all subsequent test runs.

### Step 2: Run the Tests

```bash
# Run all e2e tests
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

### Issue: Tests fail with "Backend returned client errors" or "API returned 400 Bad Request"
**Solution**:
1. The e2e tests detected a backend validation issue
2. Check the test output for the exact error message and endpoint
3. Common causes:
   - Required query/route parameters without default values
   - Overly strict validation rules
   - Missing endpoint configuration
4. Fix the backend issue and restart the backend server
5. Run tests again to verify the fix

### Issue: Tests fail with "Authentication required"
**Solution**: Run the save-auth script again to refresh your authentication state

### Issue: Port conflict on 3100
**Solution**: Make sure no other service is using port 3100, or update `playwright.config.ts` and `vite.config.ts` to use a different port

### Issue: "data-testid not found"
**Solution**: Make sure you're running the latest version of the code with the fixes applied

### Issue: Backend API not responding
**Solution**: Ensure the backend API is running on `http://localhost:5100` (check `.env.development`)

## Test Coverage

The current test suite covers:

### ✅ API Integration
- **Load coops page without API errors** - Verifies the page loads successfully without validation errors
- **Handle API request without includeArchived parameter** - Ensures backend accepts requests without optional query parameters
- **Display empty state or coop list** - Verifies proper rendering based on data availability
- **Receive 200 OK from GET /api/coops endpoint** - Confirms API returns successful response (not 400 Bad Request)

### ✅ Coop Management (M2)
- Create coop with name only
- Create coop with name and location
- Form validation (empty name, max length)
- Cancel creation
- List coops (empty state, populated list)
- Navigate from dashboard
- Edit coop (name, location)
- Cancel edit
- Edit validation
- Archive coop (confirm, cancel)
- Delete coop (confirm, cancel)
- Mobile responsiveness
- Touch-friendly button sizes
- Tenant isolation

## Next Steps

1. **Run the tests** following the steps above
2. **Fix any remaining issues** that appear during test execution
3. **Add more test coverage** for future milestones (M3: Flocks, M4: Daily Records, etc.)

## Notes

- Tests run against the local development server (port 3100)
- Backend API must be running on port 5100
- Tests use real Clerk authentication (not mocked)
- Authentication state is persisted in `.auth/user.json` (gitignored)
- All tests are isolated and create their own test data with timestamps
