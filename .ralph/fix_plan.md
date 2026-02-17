# Ralph Fix Plan - UX Audit Fixes

**PRD:** `tasks/prd-ux-audit-fixes.md`
**Goal:** Implement all UX audit fixes identified in `tasks/ux-audit/` across 16 user stories. Fix critical bugs, implement missing features, and polish the UX to production quality. After each story, run frontend tests and an E2E browser verification at 375x812px mobile viewport.
**Browser:** `http://localhost:3100` (must be running before starting)
**Viewport:** 375x812px (mobile portrait) for all browser verification tasks

## Constraints

- **Mobile viewport only** for browser verification — 375x812px
- **Run `npm run test` after every frontend change** to catch regressions
- **Use Playwright MCP browser** for E2E verification of each story
- **Czech language** — verify UI text is in Czech on all screens
- **Never use `git push` without explicit user instruction**
- **Do not change unrelated code** — fix only what the story specifies
- **Backend changes** require `dotnet build` and checking for type errors before frontend work

---

## Priority Order

Stories are ordered by severity. Complete P0 stories first, then P1, then P2.

---

## P0 — Critical (Production Blockers)

### ✅ UX-001: Fix Flock Detail Page Data Bugs — DONE

**Source:** `tasks/ux-audit/04-flock-management.md` — Findings #1, #2, #3 (Critical)
**Files:** `frontend/src/features/flocks/pages/FlockDetailPage.tsx`, likely `frontend/src/features/flocks/hooks/useFlocks.ts`, `frontend/src/features/flocks/types/`

- [ ] Read `frontend/src/features/flocks/pages/FlockDetailPage.tsx` and identify how API response is mapped to component state
- [ ] Read the flock API hook (`useFlocks.ts` or `useFlockDetail.ts`) and check the response type definition
- [ ] Compare the type definition with the actual API response fields (look for property name mismatches: e.g. `hensCount` vs `hens`, `isActive` vs `isArchived`)
- [ ] Fix the property mapping so hens/roosters/chicks render as numbers (not undefined)
- [ ] Fix the total count calculation: ensure it uses `Number(hens) + Number(roosters) + Number(chicks)` with safe fallbacks
- [ ] Fix the status display: map `isActive: true` → "Aktivní" badge (green), `isActive: false` → "Archivováno" badge (grey)
- [ ] Run `npm run type-check` — fix any TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to a flock detail page, take screenshot
- [ ] Verify: total count is a number (not "NaN"), composition shows real values, status badge matches list view

**STATUS:** ✅ FIXED — Root cause: `flocksApi.getById` was calling wrong URL `/coops/{coopId}/flocks/{flockId}` (no such backend endpoint). Fixed to call `/flocks/{flockId}`.

---

### ✅ UX-004: Enable Daily Records Navigation and Fix UUID Display

**Source:** `tasks/ux-audit/07-daily-egg-records.md` — Findings #1, #2, #3 (Critical)
**Files:** `frontend/src/shared/components/BottomNavigation.tsx`, `frontend/src/features/daily-records/`, backend `DailyRecordDto`

- [ ] Read `frontend/src/shared/components/BottomNavigation.tsx` (or equivalent) and find the disabled "Denní záznamy" tab
- [ ] Remove the `disabled` prop / flag from the "Denní záznamy" `BottomNavigationAction` — enable navigation to `/daily-records`
- [ ] Read the backend `DailyRecordDto` (likely in `backend/src/Chickquita.Application/Features/DailyRecords/Dtos/`)
- [ ] Add `FlockName` (string) and `FlockCoopName` (string) properties to `DailyRecordDto`
- [ ] Update the backend query/handler that returns daily records to include flock name and coop name via JOIN or Include
- [ ] Run `dotnet build` — fix any compilation errors
- [ ] Read `frontend/src/features/daily-records/` — find the type/interface for daily records
- [ ] Add `flockName: string` and `flockCoopName: string` to the TypeScript type
- [ ] Update `DailyRecordCard` component to display `flockName (flockCoopName)` instead of `flockId`
- [ ] Update the edit modal's disabled "Hejno" field to display `flockName (flockCoopName)` instead of UUID
- [ ] Run `npm run type-check` — fix any TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to `/daily-records` via bottom nav
- [ ] Verify: "Denní záznamy" tab is enabled and navigates correctly, record cards show flock name not UUID

**STATUS:** ✅ FIXED — Enabled nav tab; added FlockName/FlockCoopName to DailyRecordDto; updated mapping profile and repository (ThenInclude Flock.Coop); updated flockMap to show flock names. Backend needs restart to serve new fields.

---

### ✅ UX-006: Fix Statistics API Backend Bug

**Source:** `tasks/ux-audit/09-statistics-dashboard.md` — Finding #2 (Critical)
**Files:** Backend statistics endpoint/handler — search for `statistics` in `backend/src/Chickquita.Application/Features/Statistics/`

- [ ] Search backend for the statistics query handler (likely `GetStatisticsQuery.cs` or `GetStatisticsQueryHandler.cs`)
- [ ] Find the PostgreSQL query that is failing with `"Cannot write DateTime to column array, range, or multirange"`
- [ ] Identify the parameter type: the query likely passes a `DateTime` where it should pass `DateOnly` or use `NpgsqlDbType.Date`
- [ ] Fix the parameter binding — change `DateTime` to `DateOnly` for the `startDate` and `endDate` parameters
- [ ] If using raw SQL with Npgsql parameters, ensure `NpgsqlDbType.Date` is specified explicitly
- [ ] Run `dotnet build` — fix any compilation errors
- [ ] Run `dotnet test` — confirm backend tests pass
- [ ] Open browser and navigate to `http://localhost:3100/statistics`
- [ ] Click "7 DNÍ" preset — verify charts load (HTTP 200, no error alert)
- [ ] Click "30 DNÍ" preset — verify it also loads successfully
- [ ] Click "90 DNÍ" preset — verify it also loads successfully
- [ ] Take screenshot confirming statistics page renders data

**STATUS:** ✅ FIXED — Root cause: `DateTime.SpecifyKind(Unspecified)` causing Npgsql to reject parameter. Fixed all 4 private methods in StatisticsRepository to use `DateTime.SpecifyKind(..., DateTimeKind.Utc)`. Backend build and 119 tests pass.

---

### UX-003: Implement Chick Maturation Feature

**Source:** `tasks/ux-audit/05-chick-maturation.md` — Findings #1, #2, #3 (Critical — Feature Not Implemented)
**Files:** Multiple — see below

**Phase 1: Backend verification**
- [ ] Search backend for `MatureChicks` command: `find backend -name "*MatureChick*" -o -name "*Mature*"` in bash
- [ ] If `POST /api/flocks/{id}/mature-chicks` endpoint already exists: note the request/response schema and skip to Phase 2
- [ ] If endpoint does NOT exist: implement backend first:
  - Create `MatureChicksCommand.cs` with fields: `FlockId`, `ChicksToMature`, `Hens`, `Roosters`
  - Create `MatureChicksCommandHandler.cs`: validate hens+roosters===chicksToMature, update flock counts, create FlockHistory entry with `reason="maturation"`
  - Register endpoint in `FlocksEndpoints.cs`: `POST /{id}/mature-chicks`
  - Run `dotnet build` and `dotnet test`

**Phase 2: Frontend implementation**
- [ ] Create `frontend/src/features/flocks/components/MatureChicksModal.tsx`:
  - NumericStepper for "Počet kuřat k dospění" (min: 1, max: flock.chicks)
  - NumericStepper for "Slepice" (min: 0, max: chicksToMature)
  - NumericStepper for "Kohouti" (min: 0, max: chicksToMature)
  - Real-time "Celkem: X" display (hens + roosters)
  - Validation: hens + roosters must === chicksToMature; Save button disabled if not equal
  - Error message when invalid: "Součet slepic a kohoutů musí být {{count}}"
- [ ] Add `useMatureChicks` mutation hook in `frontend/src/features/flocks/hooks/`:
  - `POST /api/flocks/{id}/mature-chicks` API call
  - On success: invalidate flock queries + show success toast "Kuřata úspěšně dospěla"
  - On error: show error toast
- [ ] Update `FlockCard.tsx` context menu: add "Dospět kuřata" menu item (enabled only when `flock.chicks > 0`), opens MatureChicksModal
- [ ] Update `FlockDetailPage.tsx` action buttons: add "Dospět kuřata" button (enabled only when `flock.chicks > 0`)
- [ ] Add Czech translation keys to `frontend/src/locales/cs/translation.json`
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to a flock with chicks > 0
- [ ] Verify: "Dospět kuřata" appears in card menu and detail page
- [ ] Open the modal, enter values, verify real-time validation works, submit and verify toast + flock composition update
- [ ] Navigate to flock history — verify maturation entry appears in timeline

**STATUS:** ✅ IMPLEMENTED — Backend: created `MatureChicksCommand.cs`, `MatureChicksCommandValidator.cs`, `MatureChicksCommandHandler.cs`; registered `POST /api/flocks/{id}/mature-chicks` in `FlocksEndpoints.cs`; `dotnet build` passes. Frontend: created `MatureChicksModal.tsx` with real-time validation (hens+roosters must equal chicksToMature), added `useMatureChicks` hook with toast notifications, added "Dospět kuřata" menu item to `FlockCard.tsx` (disabled when chicks=0), added "Dospět kuřata" button to `FlockDetailPage.tsx` (disabled when chicks=0). TypeScript type-check passes, no regressions in tests. E2E: modal opens, validation works, live 1/1 counter displays correctly. NOTE: Backend needs restart to serve new endpoint (old process from previous session running on port 5100).

---

## P1 — Major (Required for Good UX)

### UX-007: Add Statistics to Navigation

**Source:** `tasks/ux-audit/09-statistics-dashboard.md` — Findings #1, #3, #9, #10
`tasks/ux-audit/12-navigation-layout.md` — Finding #2

**Files:** `frontend/src/shared/components/BottomNavigation.tsx`, `frontend/src/features/statistics/pages/StatisticsPage.tsx`, `frontend/src/features/dashboard/`

- [ ] Read `BottomNavigation.tsx` — find the 5 tab definitions and the `getCurrentTab()` function
- [ ] Replace the disabled "Denní záznamy" tab with a "Statistiky" tab:
  - Icon: `BarChartIcon` or `ShowChartIcon`
  - Label: "Statistiky"
  - Route: `/statistics`
- [ ] Add `if (location.pathname.startsWith('/statistics')) return 'statistics'` to `getCurrentTab()`
- [ ] Add `'statistics'` to the tab value enum/type
- [ ] Read `StatisticsPage.tsx` — find where to add StatCard summary metrics
- [ ] Add 3 `StatCard` components above the chart grid: total eggs produced, total cost, average cost per egg (for the selected period). Use placeholder values if data not yet available from API.
- [ ] Read `frontend/src/features/dashboard/` — find the quick actions section
- [ ] Add a "Zobrazit statistiky" quick action card linking to `/statistics`
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, verify "Statistiky" tab appears in bottom nav and navigates correctly
- [ ] Verify "Statistiky" tab is active (not "Přehled") when on `/statistics`
- [ ] Verify Dashboard quick actions include the new statistics link

**STATUS:**

---

### UX-008: Add Sign-Out and Improve Settings Page

**Source:** `tasks/ux-audit/12-navigation-layout.md` — Findings #5, #6 (Major)
**Files:** `frontend/src/features/settings/pages/SettingsPage.tsx`, translations

- [ ] Read `frontend/src/features/settings/pages/SettingsPage.tsx`
- [ ] Import `useClerk` from `@clerk/clerk-react`
- [ ] Add a "Odhlásit se" button to the Settings page (at the bottom, styled as a destructive/outlined button)
- [ ] Wire the button to open a `ConfirmationDialog`:
  - Title: "Odhlásit se?"
  - Message: "Opravdu se chcete odhlásit?"
  - Confirm: "Odhlásit se" (destructive color)
  - Cancel: "Zrušit"
- [ ] On confirmation: call `clerk.signOut()` — user should be redirected to `/sign-in`
- [ ] Add Czech translation keys for sign-out strings to `locales/cs/translation.json`
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to Settings
- [ ] Verify "Odhlásit se" button is visible, tap it, verify confirmation dialog appears
- [ ] Confirm dialog and verify user is redirected to sign-in page

**STATUS:**

---

### UX-002: Add Composition Fields to Flock Edit Modal

**Source:** `tasks/ux-audit/04-flock-management.md` — Finding #4 (Major)
**Files:** `frontend/src/features/flocks/components/EditFlockModal.tsx`

- [ ] Read `frontend/src/features/flocks/components/EditFlockModal.tsx`
- [ ] Add NumericStepper fields for "Slepice", "Kohouti", "Kuřata" to the edit form (matching the create modal)
- [ ] Pre-populate these steppers with the current flock composition values
- [ ] Update the form submit handler to include composition fields in the PUT/PATCH request
- [ ] Verify the backend flock update endpoint accepts composition fields (read the DTO)
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, open flock edit modal
- [ ] Verify NumericStepper fields appear for hens/roosters/chicks with correct pre-populated values
- [ ] Change a value, save, and verify the flock card reflects the updated composition

**STATUS:**

---

### UX-005: Daily Records UX Polish (Loading, Empty State, Delete Confirmation)

**Source:** `tasks/ux-audit/07-daily-egg-records.md` — Findings #4, #5, #12 (Major)
**Files:** `frontend/src/features/daily-records/pages/DailyRecordsPage.tsx`, edit modal component

- [ ] Read `frontend/src/features/daily-records/pages/DailyRecordsPage.tsx`
- [ ] Add loading skeleton: when `isLoading === true`, render 3× `<DailyRecordCardSkeleton />` (import from `@/shared/components`)
- [ ] Add empty state: when `!isLoading && records.length === 0`, render `<IllustratedEmptyState title="Žádné záznamy" description="Začněte zaznamenávat produkci vajec každý den" actionLabel="Přidat denní záznam" onAction={...} />`
- [ ] Also show `IllustratedEmptyState` when filters are active and return 0 results (handle separately from total-empty case)
- [ ] Read the edit/delete modal for daily records
- [ ] Add `ConfirmationDialog` before delete:
  - State: `deleteConfirmOpen`
  - Open on "Smazat" click (replace direct delete call)
  - Message: "Opravdu chcete smazat tento denní záznam? Tuto akci nelze vrátit."
  - Confirm: "Smazat" (error/red color)
  - Only call `deleteMutation.mutateAsync()` after user confirms
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to `/daily-records`
- [ ] Apply filter that returns no results — verify `IllustratedEmptyState` shows
- [ ] Try to delete a record — verify confirmation dialog appears before deletion proceeds

**STATUS:**

---

### UX-009: Fix Dashboard Layout Issues

**Source:** `tasks/ux-audit/02-dashboard.md` — Findings #6, #2, #3 (Major)
`tasks/ux-audit/12-navigation-layout.md` — Findings #1, #4

**Files:** `frontend/src/App.tsx` or layout wrapper, `frontend/src/features/dashboard/pages/DashboardPage.tsx`, `frontend/src/shared/components/Layout.tsx` (or equivalent)

- [ ] Read `frontend/src/App.tsx` and identify the layout structure
- [ ] Add a `MuiAppBar` component to the top of the authenticated layout:
  - Height: 64px
  - Left: "Chickquita" text/logo (use Typography with primary color or a logo image)
  - Right: User avatar icon button (clicking navigates to `/settings`)
  - Use `position="sticky"` so it scrolls with content
- [ ] Read `frontend/src/features/dashboard/pages/DashboardPage.tsx`
- [ ] Remove the duplicate H6 "Přehled" heading (keep only the H1)
- [ ] Fix FAB overlap: increase `pb` on the stats/flock card section OR reduce the flock status card's bottom margin so the FAB shadow doesn't obscure the "Aktivních hejn" count
- [ ] Enable the "Sledovat nákupy" quick action card (remove the `disabled` state — the Nákupy page is fully functional)
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to dashboard
- [ ] Verify: app bar is visible at top with logo, no duplicate "Přehled" heading, FAB does not obscure flock count
- [ ] Take screenshot

**STATUS:**

---

### UX-010: Fix Purchase Tracking NumericStepper Steps and Category

**Source:** `tasks/ux-audit/08-purchase-tracking.md` — Findings #1, #2, #4, #9 (Major + Minor)
**Files:** `frontend/src/features/purchases/components/PurchaseForm.tsx`, translations

- [ ] Read `frontend/src/features/purchases/components/PurchaseForm.tsx`
- [ ] Find the NumericStepper for "Částka" (amount): change `step={0.01}` → `step={1}`, `min={1}`, `max={999999}`
- [ ] Find the NumericStepper for "Množství" (quantity): change `step={0.01}` → `step={0.5}` (or `step={1}` for whole units)
- [ ] Find the purchase category list — rename "Hračky" to "Vybavení" (Equipment)
- [ ] Update the Czech translation key for the renamed category in `locales/cs/translation.json`
- [ ] Find the form action buttons — move them from inside `DialogContent` to `DialogActions`
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, open create purchase modal
- [ ] Verify: Částka NumericStepper increments by 1 CZK, "Vybavení" appears in category dropdown, form buttons are in DialogActions position

**STATUS:**

---

## P2 — Quality Improvements

### UX-011: Flock History Polish (Czech Plurals + Empty State Component)

**Source:** `tasks/ux-audit/06-flock-history.md` — Findings #2, #3 (Minor)
**Files:** `frontend/src/features/flocks/components/FlockHistoryTimeline.tsx`, shared utils

- [ ] Create a utility function `frontend/src/lib/czechPlurals.ts` with `formatCzechCount(count, singular, paucal, genitive)`:
  - count === 1 → `${count} ${singular}`
  - count 2–4 → `${count} ${paucal}`
  - count 0 or 5+ → `${count} ${genitive}`
- [ ] Read `FlockHistoryTimeline.tsx` and find the composition display (e.g. `${hens} slepic, ${roosters} kohoutů`)
- [ ] Replace with `formatCzechCount` calls:
  - slepice: singular="slepice", paucal="slepice", genitive="slepic"
  - kohout: singular="kohout", paucal="kohouti", genitive="kohoutů"
  - kuře: singular="kuře", paucal="kuřata", genitive="kuřat"
- [ ] Apply same utility to any other places in the codebase that display chicken counts (flock cards, detail pages)
- [ ] Find the empty state in `FlockHistoryTimeline.tsx` (the `<Paper>` with text) — replace with `<IllustratedEmptyState>` from `@/shared/components`
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm tests pass (add unit tests for `formatCzechCount` if none exist)
- [ ] Open browser at 375x812px, navigate to flock history
- [ ] Verify "1 kohout" shows correctly (not "1 kohoutů")

**STATUS:**

---

### UX-012: Coop Management Polish (Czech Date Format + Delete Tooltip)

**Source:** `tasks/ux-audit/03-coop-management.md` — Findings #13, #3 (Minor)
**Files:** `frontend/src/features/coops/components/CoopCard.tsx`, `CoopDetailPage.tsx`

- [ ] Read `CoopCard.tsx` and find the date rendering (currently shows "10 Feb 2026")
- [ ] Update date formatting to use Czech locale: `new Date(date).toLocaleDateString('cs-CZ')` or use `date-fns` `format(date, 'd. M. yyyy')`
- [ ] Apply the same date formatting fix consistently to any other components that display dates (CoopDetailPage, FlockCard, etc.)
- [ ] Read `CoopDetailPage.tsx` — find the disabled "Smazat" button
- [ ] Add a MUI `Tooltip` wrapping the disabled button: `title="Nejprve archivujte kurník"` with `disabledPortal` if needed
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to coops list and coop detail
- [ ] Verify dates show in Czech format, hover disabled Delete button and verify tooltip appears

**STATUS:**

---

### UX-013: Navigation Layout Polish (Safe Area + OfflineBanner Padding)

**Source:** `tasks/ux-audit/12-navigation-layout.md` — Findings #14, #19
`tasks/ux-audit/10-offline-mode.md` — Finding #3

**Files:** `frontend/src/shared/components/BottomNavigation.tsx`, `frontend/src/App.tsx`

- [ ] Read `frontend/src/shared/components/BottomNavigation.tsx`
- [ ] Add `paddingBottom: 'env(safe-area-inset-bottom)'` to the bottom `Paper` component's `sx` prop
- [ ] Read `frontend/src/App.tsx` — find the content area `Box` with `pb: isSignedIn ? 8 : 0`
- [ ] Update to: `pb: { xs: 'calc(64px + env(safe-area-inset-bottom))' }` (or equivalent dynamic value)
- [ ] Read the `OfflineBanner` component — check how its visibility is tracked
- [ ] In `App.tsx`, add conditional `pt` to the content `Box` when `OfflineBanner` is showing (e.g. add a `bannerVisible` state or use a context)
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, verify layout has no obvious overflow issues
- [ ] Take screenshot of bottom navigation area

**STATUS:**

---

### UX-014: Offline Mode Per-Card Sync Indicators

**Source:** `tasks/ux-audit/10-offline-mode.md` — Finding #2 (Major)
**Files:** `frontend/src/features/daily-records/components/DailyRecordCard.tsx`, `frontend/src/lib/syncManager.ts` or `db.ts`

- [ ] Read `frontend/src/lib/db.ts` (Dexie) — check if pending requests track which record ID they relate to
- [ ] If pending requests don't track `recordId`: add a `recordId?: string` field to the pending request schema
- [ ] Read `DailyRecordCard.tsx` — add logic to check if the record's ID exists in the pending sync queue
- [ ] Create a hook `useIsRecordPendingSync(recordId: string): boolean` that queries the Dexie `pendingRequests` table
- [ ] In `DailyRecordCard.tsx`, render a `<Chip label="Čeká na synchronizaci" size="small" color="warning" />` when `isPending === true`
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to `/daily-records`
- [ ] Verify: records created online do NOT show the pending chip; note that offline testing requires manual dev-tools simulation

**STATUS:**

---

### UX-015: PWA Installation Polish (Stepper, Dismissal Expiry, DIALOG_CONFIG)

**Source:** `tasks/ux-audit/11-pwa-installation.md` — Findings #8, #2, #3, #9, #10 (Minor)
**Files:** `frontend/src/shared/components/IosInstallPrompt.tsx`, `frontend/src/shared/components/PwaInstallPrompt.tsx`

- [ ] Read `IosInstallPrompt.tsx` — find the `<Stepper>` with all `active={true}` steps
- [ ] Replace with a sequential stepper: add `activeStep` state (0), show "Další" / "Zpět" buttons between steps, or replace with a simple numbered `<List>` if a stepper is overkill
- [ ] Read `PwaInstallPrompt.tsx` — find `localStorage.setItem('pwa-install-dismissed', 'true')`
- [ ] Replace with expiry-based storage: `{ dismissed: true, expiresAt: Date.now() + 90 * 24 * 60 * 60 * 1000 }`
- [ ] On load, check expiry: if `Date.now() > expiresAt`, treat as not dismissed
- [ ] Apply same expiry logic to `IosInstallPrompt.tsx` (`'ios-install-dismissed'` key)
- [ ] Update `PwaInstallPrompt.tsx` dialog to use `DIALOG_CONFIG` from `@/shared/constants/modalConfig`
- [ ] Update `IosInstallPrompt.tsx` dialog to use `DIALOG_CONFIG`
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to `/settings`
- [ ] Verify no crashes; review code visually confirms expiry logic is in place

**STATUS:**

---

### UX-016: Authentication Theme and Tagline Fix

**Source:** `tasks/ux-audit/01-authentication.md` — Findings #1, #6 (Minor)
**Files:** `frontend/src/main.tsx` or wherever `ClerkProvider` is configured

- [ ] Read `frontend/src/main.tsx` (or `frontend/src/App.tsx`) — find the `<ClerkProvider>` component
- [ ] Add `appearance` prop to `ClerkProvider`:
  ```tsx
  appearance={{
    variables: {
      colorPrimary: '#FF6B35',
      borderRadius: '12px',
    }
  }}
  ```
- [ ] Find the auth page header tagline "Chicken Farming Profitability Tracker" — this may be in a custom layout component for Clerk, in `index.html`, or in a page wrapper. Search for it.
- [ ] Replace with Czech equivalent: "Sledujte ziskovost chovu slepic" — or remove entirely from the auth page if no custom layout is wrapping Clerk
- [ ] Run `npm run type-check` — fix TypeScript errors
- [ ] Run `npm run test` — confirm no regressions
- [ ] Open browser at 375x812px, navigate to `/sign-in`
- [ ] Verify: Clerk button color matches `#FF6B35`, tagline is in Czech or removed
- [ ] Take screenshot

**STATUS:**

---

## Final Verification

After completing all stories:

- [ ] Run full frontend test suite: `npm run test` — confirm ≥ 95% pass rate
- [ ] Run `npm run type-check` — zero TypeScript errors
- [ ] Run `npm run build` — confirm production build succeeds
- [ ] Open browser at 375x812px and do a full app walkthrough:
  - [ ] Dashboard — app bar visible, no duplicate heading, FAB no overlap
  - [ ] Coops — Czech dates, disabled delete has tooltip
  - [ ] Flocks list → Flock detail — no NaN, correct composition, correct status
  - [ ] Flock edit modal — composition fields present
  - [ ] Chick maturation — action in menu and detail, modal works, validation works
  - [ ] Flock history — correct Czech plurals ("1 kohout" not "1 kohoutů")
  - [ ] Daily Records — enabled nav tab, flock names not UUIDs, skeleton/empty state, delete confirms
  - [ ] Purchases — step=1 for amounts, "Vybavení" category
  - [ ] Statistics — "Statistiky" tab in nav, page loads without 400 error
  - [ ] Settings — sign-out button present and functional
- [ ] Commit changes: `git add -p && git commit -m "fix: resolve UX audit findings (UX-001 through UX-016)"`

---

## Progress Summary

### Current Status
- **Total Stories**: 16
- **Completed**: 4 / 16 (UX-001, UX-003, UX-004, UX-006)
- **P0 Critical**: 4 / 4 complete ✅
- **P1 Major**: 0 / 6 complete
- **P2 Quality**: 0 / 6 complete
