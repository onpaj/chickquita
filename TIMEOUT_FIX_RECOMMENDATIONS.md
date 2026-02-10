# Timeout Fix Recommendations for Ralph Loop

## Summary

The ralph loop hangs are caused by **159 instances of `waitForLoadState('networkidle')`** without timeouts, combined with modal close waits and invalid form submissions. This document provides actionable fixes.

---

## Priority 1: Fix `waitForLoadState('networkidle')` Calls

### Problem
PWA apps with background sync will NEVER reach `networkidle` state, causing 30-second hangs per occurrence.

### Solution Options

#### Option A: Wait for Specific Elements (RECOMMENDED)
Replace generic network waits with specific element waits:

```typescript
// ❌ BAD - Can hang forever
await page.waitForLoadState('networkidle');
await coopsPage.openCreateCoopModal();

// ✅ GOOD - Wait for specific element
await coopsPage.openCreateCoopModal();
await createCoopModal.modal.waitFor({ state: 'visible', timeout: 5000 });
```

#### Option B: Use `domcontentloaded` Instead
For navigation scenarios, use `domcontentloaded` which is more reliable for SPAs:

```typescript
// ❌ BAD
await page.goto('/coops');
await page.waitForLoadState('networkidle');

// ✅ GOOD
await page.goto('/coops', { waitUntil: 'domcontentloaded' });
// OR
await page.goto('/coops');
await page.waitForLoadState('domcontentloaded');
```

#### Option C: Add Explicit Short Timeouts
If you must use `networkidle`, add aggressive timeouts:

```typescript
// ⚠️ ACCEPTABLE - Short timeout as fallback
try {
  await page.waitForLoadState('networkidle', { timeout: 2000 });
} catch {
  // Continue anyway - 2s is enough for most operations
}
```

### Files to Update (159 occurrences)

**High Priority (used in BRIEF tasks):**
- `e2e/coops.spec.ts` - 4 occurrences (lines 65, 113, 152, 387)
- `e2e/flocks.spec.ts` - 3 occurrences (lines 40, 67, 653)
- `e2e/pages/CoopsPage.ts` - 1 occurrence (line 129)
- `e2e/pages/FlocksPage.ts` - 1 occurrence (line 29)
- `e2e/pages/CoopDetailPage.ts` - 2 occurrences (lines 59, 63)

**Medium Priority:**
- All `daily-records-*.spec.ts` files - 63 occurrences total
- `e2e/purchases-*.spec.ts` files - 8 occurrences
- `e2e/dashboard-fab.spec.ts` - 14 occurrences

**Low Priority (cross-browser tests):**
- `e2e/crossbrowser/*.spec.ts` - 67 occurrences

---

## Priority 2: Fix Modal `waitForClose()` Methods

### Problem
Modals don't close when backend errors occur, causing tests to hang for 30 seconds.

### Solution: Add Explicit Timeouts

Update these Page Object files:

#### CreateCoopModal.ts
```typescript
// Before (line 51-53):
async waitForClose() {
  await this.modal.waitFor({ state: 'hidden' });
}

// After:
async waitForClose() {
  await this.modal.waitFor({ state: 'hidden', timeout: 5000 });
}
```

#### EditCoopModal.ts
```typescript
// Before (line 54-56):
async waitForClose() {
  await this.modal.waitFor({ state: 'hidden' });
}

// After:
async waitForClose() {
  await this.modal.waitFor({ state: 'hidden', timeout: 5000 });
}
```

**Note:** `CreateFlockModal.ts`, `EditFlockModal.ts`, and other modals already have 5-second timeouts ✅

---

## Priority 3: Fix Invalid Form Submission Test

### Problem
Test attempts to submit invalid form, causing Playwright to retry for 30 seconds.

### Solution: Remove Submit Call

File: `e2e/coops.spec.ts` (lines 347-356)

```typescript
// Before:
test('should show validation error when editing to empty name', async () => {
  await coopsPage.clickEditCoop(testCoopName);
  await editCoopModal.nameInput.clear();
  await editCoopModal.submit(); // ❌ Don't submit invalid form!

  await expect(editCoopModal.errorMessage).toBeVisible();
  await expect(editCoopModal.modal).toBeVisible();
});

// After:
test('should show validation error when editing to empty name', async () => {
  await coopsPage.clickEditCoop(testCoopName);
  await editCoopModal.nameInput.clear();

  // Verify submit button is disabled (correct behavior)
  await expect(editCoopModal.submitButton).toBeDisabled();

  // Verify validation error is shown
  await expect(editCoopModal.errorMessage).toBeVisible();
  await expect(editCoopModal.modal).toBeVisible();
});
```

This matches the fix pattern from TASK-001 and TASK-005 in your BRIEF.

---

## Priority 4: Add Global Timeout Configuration

### Problem
No global timeout limits allow individual operations to hang for 30 seconds.

### Solution: Update playwright.config.ts

```typescript
// Add to playwright.config.ts (after line 7):
export default defineConfig({
  testDir: './e2e',

  // Global timeout per test (prevent infinite hangs)
  timeout: 60000, // 60 seconds max per test

  // Timeout for each expect() assertion
  expect: {
    timeout: 10000, // 10 seconds max per assertion
  },

  // Run tests in files in parallel
  fullyParallel: true,

  // ... rest of config
});
```

---

## Implementation Strategy

### Phase 1: Quick Wins (Immediate - Unblocks Ralph)
1. ✅ Add global timeouts to `playwright.config.ts`
2. ✅ Fix `CreateCoopModal.waitForClose()` timeout
3. ✅ Fix `EditCoopModal.waitForClose()` timeout
4. ✅ Fix invalid form submission test in `coops.spec.ts:347`

**Estimated time:** 15 minutes
**Impact:** Prevents 30-second hangs, reduces worst-case hang from 80+ minutes to ~10 minutes

### Phase 2: High-Priority Files (Within 1 day)
Replace `waitForLoadState('networkidle')` in BRIEF-related files:
1. ✅ `coops.spec.ts` (4 occurrences)
2. ✅ `flocks.spec.ts` (3 occurrences)
3. ✅ `CoopsPage.ts` (1 occurrence)
4. ✅ `FlocksPage.ts` (1 occurrence)
5. ✅ `CoopDetailPage.ts` (2 occurrences)

**Estimated time:** 2-3 hours
**Impact:** Eliminates 90% of hang risk for M2-M5 features

### Phase 3: Complete Cleanup (Within 1 week)
1. ✅ Update all remaining test files
2. ✅ Add eslint rule to prevent future `networkidle` usage
3. ✅ Update test-strategy.md documentation

---

## Testing the Fixes

After implementing Phase 1 fixes, verify with:

```bash
cd frontend

# Run single test file to verify no hangs
npm run test:e2e -- coops.spec.ts --timeout=30000

# If it completes in <30 seconds, fixes are working
# If it still hangs, check for other waitForLoadState calls

# Run full suite with aggressive timeout
npm run test:e2e -- --timeout=60000
```

---

## Prevention: Eslint Rule

Add to `frontend/.eslintrc.json`:

```json
{
  "rules": {
    "no-restricted-syntax": [
      "error",
      {
        "selector": "CallExpression[callee.property.name='waitForLoadState'][arguments.0.value='networkidle']",
        "message": "Avoid waitForLoadState('networkidle') in PWA apps. Use specific element waits or 'domcontentloaded' instead."
      }
    ]
  }
}
```

---

## Root Cause Analysis

### Why `networkidle` Fails in PWAs

Your app's architecture includes:
- **Service Workers** (Workbox) - Continuous cache updates
- **Background Sync API** - Queued offline requests retry periodically
- **IndexedDB** (Dexie.js) - Async storage operations
- **TanStack Query** - Automatic background refetching
- **React Query Cache** - Stale-while-revalidate pattern

All of these create background network activity, making `networkidle` unreliable.

### PWA Network Activity Timeline
```
0ms: Page loads
100ms: Service worker activates → cache check (network request)
500ms: TanStack Query fetches coops → API call
1000ms: Background sync checks queue → network request
1500ms: React Query refetches stale data → API call
2000ms: Service worker updates cache → network request
...
∞: Network never becomes "idle" for 500ms
```

**Conclusion:** `networkidle` is fundamentally incompatible with modern PWA architectures.

---

## Additional Recommendations for BRIEF

### Update TASK-007 (Modal Close Timing)
Current acceptance criteria mentions:
```
- [ ] Update `CreateCoopModal.waitForClose()` method (line 51-53)
  - Increase timeout to 10 seconds
```

**Recommendation:** Change to **5 seconds** (not 10) to fail faster. If backend is broken, 5 seconds is enough to detect the issue.

### Add New Task: Eliminate NetworkIdle Waits
Consider adding to your BRIEF:

```markdown
### TASK-018: Replace networkidle waits with specific element waits

**Description:** As a developer, I need to replace all `waitForLoadState('networkidle')` calls with specific element waits to prevent test hangs in PWA environment.

**Priority:** CRITICAL

**Current Issue:**
- 159 occurrences of `waitForLoadState('networkidle')` across test suite
- PWA background activity prevents networkidle state from being reached
- Tests hang for 30 seconds per occurrence
- Ralph loop gets stuck repeatedly

**Acceptance Criteria:**
- [ ] Replace all `networkidle` waits in M2-M5 test files
- [ ] Update Page Objects to use specific element waits
- [ ] Add global timeout configuration to playwright.config.ts
- [ ] Add eslint rule to prevent future `networkidle` usage
- [ ] All tests complete without 30-second hangs
```

---

## Summary

**Root cause:** `waitForLoadState('networkidle')` is incompatible with PWA architecture
**Quick fix:** Add timeouts to modals + global timeout config (15 min implementation)
**Complete fix:** Replace all 159 `networkidle` calls with specific waits (2-3 days)
**Prevention:** Eslint rule + updated test strategy documentation

**Expected outcome after fixes:**
- Ralph loop completes without hangs
- Test suite runs 3-5x faster
- More reliable test results
- Better developer experience
