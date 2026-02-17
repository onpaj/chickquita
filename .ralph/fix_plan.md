# Ralph Fix Plan - E2E Test Failures Resolution

**PRD:** `tasks/prd-e2e-test-failures.md`
**Goal:** Investigate and resolve all 195 failing E2E tests across 10 spec files. Achieve a green test suite (100% pass rate for non-skipped tests). For each failure, determine whether the fix belongs in app code, test code, or test infrastructure. After each fix group, re-run the affected spec file to verify the fix resolves all related tests.
**Browser:** `http://localhost:3100` (must be running before starting)
**Viewport:** 375x812px (mobile portrait) for browser verification tasks

## Constraints

- **Run affected spec file after every fix** to verify the fix resolves all related tests
- **Do not introduce regressions** — the 81 currently passing tests must remain green
- **Shared root causes first** — fix the root cause, not individual symptoms; one fix may resolve dozens of user stories
- **Prefer test code fixes** over app code changes unless the app has a genuine bug
- **Never use `git push` without explicit user instruction**
- **Do not change unrelated code** — fix only what the failing tests require
- **Visual regression baselines** must be visually inspected before committing
- **Use Playwright MCP browser** for E2E verification when needed
- **For each FIX investigate root cause**. In case of invalid test, your goal is to fix the test. In case of application error, your goal is to fix that error. In both cases, do not mark story as completed unless test is successful
- **E2E Authentication — CRITICAL**: Tests authenticate via `@clerk/testing` (programmatic Clerk API). The setup project `e2e/clerk.setup.ts` runs automatically before tests and bypasses MFA. **NEVER navigate to `/sign-in` or authenticate manually in the browser** — manual auth triggers MFA and will hang indefinitely. The only valid auth method is `clerk.signIn()` from `@clerk/testing/playwright`. Requires `CLERK_SECRET_KEY` in `frontend/.env.test.local` (see `docs/architecture/E2E_AUTH_SETUP.md`).
---

## Priority Order

Stories are ordered by blast radius. P0 fixes unblock the most tests. Complete P0 first, then P1, then P2.

---

## P0 — Infrastructure & Setup (Unblock 100+ tests)

### FIX-001: Fix purchases-form.spec.ts — URL, MUI Select interactions, and form validity

**Source:** PRD Group 9 (US-178 through US-188) — 11 tests
**Common errors:** Hardcoded `localhost:5173`, `selectOption()` on MUI Select (custom combobox), 250 increment clicks for amount, submit button not disabled when form invalid
**Files:** `frontend/e2e/purchases-form.spec.ts`, `frontend/src/features/purchases/components/PurchaseForm.tsx`, `frontend/src/features/purchases/pages/PurchasesPage.tsx`

- [x] Rewrote `purchases-form.spec.ts`: replaced hardcoded URL with `page.goto('/purchases')`, replaced `selectOption()` with MUI Select `click()` + `getByRole('option')` pattern, replaced 250 increment clicks with `page.locator('input[aria-label="..."]').fill()`, simplified ARIA and keyboard tests
- [x] Added `onValidityChange` prop to `PurchaseForm` so form validity is emitted to parent
- [x] Updated `PurchasesPage.tsx` DialogActions submit button: `disabled={isSubmitting || !isFormValid}` (was only `disabled={isSubmitting}`)
- [x] TypeScript compiles clean (`npx tsc --noEmit`)

---

### FIX-002: Fix beforeEach "Add Coop" Button Selector Mismatch

**Source:** PRD Groups 5, 6, 8 (US-112–131, US-132–149, US-166–177) — 50 tests
**Common error:** Timeout waiting for `getByRole('button', { name: /add coop|přidat kurník|create coop/i }).first()` in beforeEach hooks
**Files:** `frontend/e2e/flocks-i18n.spec.ts`, `frontend/e2e/flocks.spec.ts`, `frontend/e2e/daily-records-quick-add.spec.ts`, app components rendering the "add coop" button

- [ ] Read `frontend/e2e/flocks.spec.ts` — find the `beforeEach` hook and note the exact selector used for the "add coop" button
- [ ] Read `frontend/e2e/flocks-i18n.spec.ts` — find the `beforeEach` hook selector
- [ ] Read `frontend/e2e/daily-records-quick-add.spec.ts` — find the `beforeEach` hook selector
- [ ] Open browser at 375x812px, navigate to coops page, take snapshot
- [ ] Identify the actual button text/ARIA label rendered for adding a coop (check FAB, empty state CTA, and page buttons)
- [ ] Compare actual label with test selector regex — determine the mismatch
- [ ] Decide: fix the app button label/ARIA to match tests, OR fix the test selectors to match the app
- [ ] Apply the fix consistently across all three spec files
- [ ] Run `npx playwright test flocks.spec.ts` — verify all 18 tests pass
- [ ] Run `npx playwright test flocks-i18n.spec.ts` — verify all 20 tests pass
- [ ] Run `npx playwright test daily-records-quick-add.spec.ts` — verify all 12 tests pass

---

### FIX-003: Fix Coops Page Heading Selector Mismatch

**Source:** PRD Group 3 (US-066–087) — 22 tests
**Common error:** `expect(locator).toBeVisible()` failed — heading `/coops|kurníky/i` not found
**Files:** `frontend/e2e/coops.spec.ts`, coops page component

- [ ] Read `frontend/e2e/coops.spec.ts` — find the heading selector pattern (`/coops|kurníky/i`)
- [ ] Open browser at 375x812px, navigate to coops page, take snapshot
- [ ] Identify the actual heading text rendered on the coops page (check h1, h2, etc.)
- [ ] Compare actual heading with test selector regex — determine the mismatch
- [ ] Decide: fix the app heading to match tests, OR fix the test selectors to match the app
- [ ] Apply the fix
- [ ] Run `npx playwright test coops.spec.ts` — verify all 22 tests pass

---

### FIX-004: Fix "Add Purchase" Button Selector Mismatch

**Source:** PRD Group 2 (US-039–065) — 27 tests
**Common error:** Timeout waiting for `getByLabel(/přidat nákup|add purchase/i).first()`
**Files:** `frontend/e2e/purchases-crud.spec.ts`, purchases page component

- [ ] Read `frontend/e2e/purchases-crud.spec.ts` — find all selectors for the "add purchase" button (note: uses `getByLabel` not `getByRole`)
- [ ] Open browser at 375x812px, navigate to purchases page, take snapshot
- [ ] Identify the actual button element: check ARIA label, role, visible text for the add purchase action (FAB or button)
- [ ] Compare actual ARIA label/role with test selector — determine the mismatch (likely `getByLabel` vs `getByRole`)
- [ ] Decide: fix the app button ARIA label, OR fix the test selector method
- [ ] Apply the fix consistently across all purchase test selectors
- [ ] Run `npx playwright test purchases-crud.spec.ts` — verify all 27 tests pass

---

## P1 — Page-Level Selector Fixes (40+ tests)

### FIX-005: Fix Responsive Layout Test Selectors (Strict Mode + Navigation)

**Source:** PRD Group 1 (US-001–038) — 38 tests
**Common error:** Strict mode violation — `getByRole('heading', { level: 1 })` resolves to 2 elements (app title "Chickquita" in AppBar + page heading). Also: bottom navigation element not found, timeouts.
**Files:** `frontend/e2e/crossbrowser/responsive-layout.crossbrowser.spec.ts`, app layout components

- [ ] Read `frontend/e2e/crossbrowser/responsive-layout.crossbrowser.spec.ts` — identify all selectors causing strict mode violations
- [ ] Determine the root cause: the AppBar added in UX-009 introduces a second h1 "Chickquita" alongside page headings
- [ ] Decide fix approach:
  - Option A (app fix): Change "Chickquita" in AppBar from h1 to a non-heading element (span, div)
  - Option B (test fix): Make selectors more specific (e.g. `page.getByRole('heading', { level: 1 }).filter(...)` or use `locator.first()`)
- [ ] Fix the strict mode violation for Dashboard grid layout tests (line 42) — affects US-001, US-004, US-007, US-009 (5 viewports)
- [ ] Fix the bottom navigation visibility tests (line 73) — identify actual bottom nav selector and update tests — affects US-002, US-003, US-005, US-006, US-008, US-010 (6 viewports)
- [ ] Fix the Coops list layout tests (line 125) — affects US-011 through US-016 (6 viewports)
- [ ] Fix the Create coop modal tests (line 179) — affects US-017 through US-022 (6 viewports)
- [ ] Fix the Bottom navigation interaction tests (line 265) — affects US-023 through US-028 (6 viewports)
- [ ] Fix the Vertical scrolling tests (line 296) — affects US-029 through US-034 (6 viewports)
- [ ] Fix the Device-specific user journey tests (line 331) — affects US-035 through US-038 (4 devices)
- [ ] Run `npx playwright test crossbrowser/responsive-layout.crossbrowser.spec.ts` — verify all 38 tests pass

---

### FIX-006: Fix Purchases Page Routing and Element Tests

**Source:** PRD Group 7 (US-150–165) — 16 tests
**Common error:** Timeouts and elements not found when navigating to purchases page
**Files:** `frontend/e2e/purchases-page.spec.ts`, purchases page components

- [ ] Read `frontend/e2e/purchases-page.spec.ts` — identify all failing selectors and navigation patterns
- [ ] Open browser at 375x812px, navigate to `/purchases`, take snapshot
- [ ] Compare actual page structure (headings, buttons, FAB, bottom nav) with test expectations
- [ ] Fix route navigation test (line 22) — US-150
- [ ] Fix bottom navigation to purchases test (line 30) — US-151: check if "Nákupy" tab still exists in bottom nav (it may have been replaced by "Statistiky" in UX-007)
- [ ] Fix authentication protection test (line 48) — US-152
- [ ] Fix FAB button tests (lines 63, 69, 81) — US-153, US-154, US-155
- [ ] Fix CRUD flow tests (lines 97, 143, 186, 203, 228) — US-156 through US-160
- [ ] Fix viewport tests (lines 247, 267) — US-161, US-162
- [ ] Fix PurchaseList component tests (lines 282, 287) — US-163, US-164
- [ ] Fix bottom nav highlight test (line 302) — US-165: update expected tab if nav changed
- [ ] Run `npx playwright test purchases-page.spec.ts` — verify all 16 tests pass

---

### FIX-007: Fix Daily Records List Page Heading Selector

**Source:** PRD Group 10 (US-189–198) — 10 tests
**Common error:** `expect(locator).toBeVisible()` failed — heading `/denní záznamy/i` not found
**Files:** `frontend/e2e/daily-records-list.spec.ts`, daily records list page component

- [ ] Read `frontend/e2e/daily-records-list.spec.ts` — find the heading selector pattern (`/denní záznamy/i`)
- [ ] Open browser at 375x812px, navigate to daily records page, take snapshot
- [ ] Identify the actual heading text rendered on the daily records page
- [ ] Compare actual heading with test selector regex — determine the mismatch
- [ ] Decide: fix the app heading to match tests, OR fix the test selectors to match the app
- [ ] Apply the fix
- [ ] Fix remaining test selectors if needed (filter options, quick filters, date range, skeletons)
- [ ] Run `npx playwright test daily-records-list.spec.ts` — verify all 10 tests pass

---

## P2 — Visual Regression Baselines (25 tests)

### FIX-008: Generate Visual Regression Baseline Snapshots

**Source:** PRD Group 4 (US-088–111) — 25 tests
**Common error:** `A snapshot doesn't exist at ... writing actual.` — baseline images not yet generated
**Files:** `frontend/e2e/crossbrowser/visual-regression.crossbrowser.spec.ts`, snapshot directory

- [ ] Read `frontend/e2e/crossbrowser/visual-regression.crossbrowser.spec.ts` to understand what pages/components are screenshotted
- [ ] Ensure the app is running and all pages render correctly before generating baselines
- [ ] Run `npx playwright test crossbrowser/visual-regression.crossbrowser.spec.ts --update-snapshots` to generate all baseline images
- [ ] Review generated snapshot files in the snapshots directory:
  - [ ] `sign-in-page` baseline — verify it looks correct (US-088)
  - [ ] `dashboard-page` baseline — verify (US-089)
  - [ ] `coops-page` baseline — verify (US-090)
  - [ ] `settings-page` baseline — verify (US-091)
  - [ ] `bottom-navigation` baseline — verify (US-092)
  - [ ] `loading-skeleton` baseline — verify (US-093)
  - [ ] `create-coop-modal` baseline — verify (US-094)
  - [ ] Breakpoint baselines (320px, 480px, 768px, 1024px, 1920px) for dashboard and coops — verify (US-095 through US-104)
  - [ ] `FAB-touch-target` baseline — verify touch target size is adequate (US-105)
  - [ ] `text-contrast` baseline — verify WCAG contrast ratios (US-106)
  - [ ] `czech-layout` baseline — verify Czech text renders correctly (US-107)
  - [ ] `english-layout` baseline — verify English text renders correctly (US-108)
  - [ ] `coop-card` baseline — verify (US-109)
  - [ ] `empty-state` baseline — verify (US-110)
  - [ ] `bottom-nav-touch-target` baseline — verify (US-111)
- [ ] Run `npx playwright test crossbrowser/visual-regression.crossbrowser.spec.ts` (without --update-snapshots) — verify all 25 tests pass
- [ ] Stage snapshot files for commit

---

## Final Verification

After completing all fix groups:

- [ ] Run full E2E test suite: `npx playwright test` — confirm 0 failures (195 previously failing tests now pass)
- [ ] Verify 81 previously passing tests still pass (0 regressions)
- [ ] Verify 18 skipped tests are still skipped (not newly broken)
- [ ] Review total test count: expect 332 total, ~314 passing, 18 skipped
- [ ] Run `npx tsc --noEmit` — zero TypeScript errors (if app code was changed)
- [ ] Run `npm run build` — confirm production build succeeds (if app code was changed)
- [ ] Open browser at 375x812px and spot-check key pages:
  - [ ] Dashboard — renders without errors
  - [ ] Coops — list and create modal work
  - [ ] Flocks — list, create, edit work
  - [ ] Purchases — list and create work
  - [ ] Daily Records — list works
  - [ ] Statistics — page loads
  - [ ] Settings — page loads
- [ ] Commit changes with appropriate message

---

## Progress Summary

### Current Status
- **Total Failing Tests**: 195 (across 10 spec files)
- **Fix Groups**: 8
- **Completed**: 0 / 8
- **P0 Infrastructure**: 0 / 4 (FIX-001 through FIX-004 — unblock ~110 tests)
- **P1 Page-Level**: 0 / 3 (FIX-005 through FIX-007 — fix ~64 tests)
- **P2 Baselines**: 0 / 1 (FIX-008 — generate 25 baselines)

### Root Cause → Test Count Mapping
| Fix Group | Root Cause | Tests Affected |
|-----------|-----------|----------------|
| FIX-001 | Hardcoded `localhost:5173` | 11 |
| FIX-002 | "Add coop" button selector in beforeEach | 50 |
| FIX-003 | Coops page heading mismatch | 22 |
| FIX-004 | "Add purchase" button selector | 27 |
| FIX-005 | Strict mode (dual h1) + nav selectors | 38 |
| FIX-006 | Purchases page element selectors | 16 |
| FIX-007 | Daily records heading mismatch | 10 |
| FIX-008 | Missing visual regression baselines | 25 |
| **Total** | | **199** (some overlap with shared root causes) |
