# Bug Fix: API /coops Endpoint Returns 400 Bad Request

## Issue Description

When navigating to the Coops page, the frontend displayed a validation error:
- **Frontend Error**: "Formulář obsahuje chyby" (Form contains errors) / "Validation error occurred"
- **Backend Response**: HTTP 400 Bad Request
- **API Endpoint**: `GET /api/coops`

## Root Cause

The backend endpoint `GetCoops` required a `bool includeArchived` query parameter without a default value:

```csharp
// BEFORE (broken)
private static async Task<IResult> GetCoops(
    [FromQuery] bool includeArchived,  // ❌ Required parameter
    [FromServices] IMediator mediator)
```

When the frontend called `GET /api/coops` without the `includeArchived` parameter, ASP.NET Core's model binding failed to bind the boolean value, resulting in a validation error (400 Bad Request).

## Fix Applied

Added a default value to make the parameter optional:

```csharp
// AFTER (fixed)
private static async Task<IResult> GetCoops(
    [FromServices] IMediator mediator,
    [FromQuery] bool includeArchived = false)  // ✅ Optional with default
```

**File**: `backend/src/Chickquita.Api/Endpoints/CoopsEndpoints.cs` (line 65)

## E2E Tests Added

Added comprehensive tests that automatically detect backend validation issues:

### New Test Suite: API Integration (`e2e/coops.spec.ts`)

1. **`should not return 4xx validation errors when loading coops`**
   - ❌ **Would FAIL before fix**: Detects 400 Bad Request errors
   - ✅ **Passes after fix**: No 4xx errors detected
   - Monitors all API responses and throws detailed error if 4xx detected
   - Catches missing default parameters, strict validation, config issues

2. **`should successfully GET /api/coops and return valid data structure`**
   - ❌ **Would FAIL before fix**: Expected 200 OK but got 400 Bad Request
   - ✅ **Passes after fix**: Returns 200 OK with valid array
   - Intercepts API response and validates status code
   - Verifies response structure (array with proper fields)

3. **`should handle empty coops list gracefully`**
   - Ensures proper UI rendering regardless of data
   - Verifies no generic error pages are shown
   - Tests both empty state and populated list scenarios

4. **`should not fail with missing optional query parameters`**
   - ❌ **Would FAIL before fix**: Detects 400 errors in console and network
   - ✅ **Passes after fix**: No 400 errors for missing parameters
   - Monitors console errors and network responses
   - Throws detailed error explaining the cause (missing default values)

### Backend Health Check

Added `beforeAll` hook that verifies backend is running before any tests execute:
- Checks `/health` endpoint
- Fails fast with clear error message if backend is down
- Prevents confusing test failures from connectivity issues

## How to Verify

### 1. Restart Backend (Required!)
```bash
cd backend
dotnet run --project src/Chickquita.Api
```

### 2. Run E2E Tests (They will catch the issue automatically)
```bash
cd frontend

# Save auth state (first time only)
npm run test:e2e:save-auth

# Run tests - they will FAIL if backend has validation issues
npm run test:e2e -- --project=chromium
```

**Before the fix**: Tests would fail with messages like:
- "Backend returned client errors: 400 Bad Request"
- "Expected 200 OK but got 400 Bad Request"
- "API should not return 400 errors for GET requests without query params"

**After the fix**: All tests pass ✅

## Impact

- **Before Fix**: Coops page was completely broken, showing validation error
- **After Fix**: Coops page loads successfully, showing empty state or list of coops
- **Test Coverage**: 4 new tests specifically cover this scenario
- **Regression Prevention**: Tests will catch this issue if it happens again

## Related Files

### Backend
- `backend/src/Chickquita.Api/Endpoints/CoopsEndpoints.cs` - Fixed endpoint

### Frontend
- `frontend/e2e/coops.spec.ts` - Added API integration tests
- `frontend/e2e/verify-backend-fix.sh` - Verification script
- `frontend/e2e/TEST_SETUP.md` - Updated documentation
- `frontend/e2e/README.md` - Updated test instructions

## Lessons Learned

1. **Always provide default values** for optional query parameters in ASP.NET Core endpoints
2. **E2E tests should verify API integration** at the HTTP level, not just UI interactions
3. **Use Playwright MCP tools** for debugging - they allow real-time testing during development
4. **Backend changes require server restart** - C# is compiled, not interpreted

## Testing Checklist

- [x] Backend endpoint accepts requests without `includeArchived` parameter
- [x] Backend endpoint returns 200 OK (not 400 Bad Request)
- [x] Frontend displays coops page without validation errors
- [x] E2E tests verify API integration
- [x] Verification script created for quick testing
- [x] Documentation updated
- [x] Regression tests added to prevent future issues
