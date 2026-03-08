# Screen Layout Analysis & Workplan

**Date:** 2026-03-07
**Scope:** Frontend screen layout and spacing inconsistencies
**Branch:** `claude/analyze-screen-layouts-Ni2K1`

---

## Summary

After auditing all 11 main page components and supporting shared components, several layout and spacing inconsistencies were identified. This document captures the findings and provides a prioritized workplan to standardize the UI.

---

## Current Layout Architecture

### App Shell

```
AppBar (sticky, 64px height, px: 2 padding)
  ↓
OfflineBanner (conditional, ~60px)
  ↓
Main <Box pb="calc(64px + env(safe-area-inset-bottom))">
  └── Page Content (Container + inner Box)
</Box>
  ↓
BottomNavigation (fixed, 64px height)
```

### Page Content Template

All pages follow this wrapper pattern:

```tsx
<Container maxWidth="lg|md|sm">
  <Box sx={{ py: 3, pb: 10 }}>
    {/* Page content */}
  </Box>
</Container>
```

### Container maxWidth Convention

| Page type     | maxWidth | Pages                                           |
|---------------|----------|-------------------------------------------------|
| List / Grid   | `lg`     | Dashboard, DailyRecords, Statistics, Purchases  |
| Detail / Form | `sm`     | CoopDetail, FlockDetail, FlockHistory           |
| Settings      | `md`     | Settings                                        |

---

## Identified Issues

### Issue 1 — Double Bottom Padding

**Severity:** Medium
**Files affected:** Every page component

`App.tsx` already applies `pb: 'calc(64px + env(safe-area-inset-bottom))'` to the main content wrapper. Each page then adds another `pb: 10` (80px) inside its own `Box`. This creates ~160px of bottom space instead of the intended ~80px.

**Locations:**
- `src/App.tsx` line 78
- `src/pages/DashboardPage.tsx`
- `src/pages/CoopsPage.tsx`
- `src/pages/FlocksPage.tsx`
- `src/pages/CoopDetailPage.tsx`
- `src/pages/FlockDetailPage.tsx`
- `src/pages/DailyRecordsListPage.tsx`
- `src/pages/StatisticsPage.tsx`
- `src/pages/SettingsPage.tsx`
- `src/features/purchases/pages/PurchasesPage.tsx`
- `src/features/flocks/pages/FlockHistoryPage.tsx`

**Fix:** Remove `pb: 10` from all page-level `Box` wrappers and rely solely on the `App.tsx` shell padding.

---

### Issue 2 — Hardcoded Pixel Values in FAB Positioning

**Severity:** Medium
**Files affected:** 4 page components

FAB (Floating Action Button) `bottom` and `right` values are hardcoded in pixels instead of using MUI theme spacing tokens.

| File | Current | Expected |
|------|---------|----------|
| `DashboardPage.tsx` | `bottom: 80, right: 16` | `bottom: { xs: 10, sm: 2 }, right: 2` |
| `CoopsPage.tsx` | `bottom: { xs: 80, sm: 16 }, right: 16` | `bottom: { xs: 10, sm: 2 }, right: 2` |
| `FlocksPage.tsx` | `bottom: { xs: 80, sm: 16 }, right: 16` | `bottom: { xs: 10, sm: 2 }, right: 2` |
| `PurchasesPage.tsx` | `bottom: { xs: 80, sm: 16 }, right: 16` | `bottom: { xs: 10, sm: 2 }, right: 2` |

**Additional:** `DashboardPage.tsx` uses a static `bottom: 80` (non-responsive) while all other pages use a responsive object `{ xs: 80, sm: 16 }`.

---

### Issue 3 — Inconsistent Filter Section Components (Card vs Paper)

**Severity:** Low
**Files affected:** 2 page components

The filter/control section at the top of list pages uses different MUI components:

| File | Component | Elevation |
|------|-----------|-----------|
| `DailyRecordsListPage.tsx` | `<Card>` | `1` (default Card) |
| `StatisticsPage.tsx` | `<Paper>` | `0` (default Paper) |

This results in visually different filter bars on different screens.

**Fix:** Standardize to `<Card>` (consistent with other card usages in the app) with explicit `elevation={1}`.

---

### Issue 4 — Inconsistent Error / Empty State Padding

**Severity:** Low
**Files affected:** At least 2 page components

Error and loading fallback containers use `py: 4` (32px) while the standard page top padding is `py: 3` (24px).

**Locations:**
- `FlocksPage.tsx` error state container: `py: 4`
- `CoopsPage.tsx` error state container: `py: 4`

**Fix:** Change error state containers to `py: 3` to match the page standard, or extract to a shared `PageError` component with the correct value baked in.

---

### Issue 5 — Hardcoded px for Icon Container Sizes

**Severity:** Low
**Files affected:** Shared components

Several shared components use hardcoded pixel dimensions for icon containers instead of theme spacing:

| Component | Location | Current | Expected |
|-----------|----------|---------|----------|
| `StatCard.tsx` | Icon wrapper | `width: 40, height: 40` | `width: 5, height: 5` (40px) |
| `QuickActionCard.tsx` | Icon box | `width: 48, height: 48` | `width: 6, height: 6` (48px) |

Note: The actual rendered size remains the same — this is a code consistency issue.

---

### Issue 6 — Missing Responsive FAB on DashboardPage

**Severity:** Medium
**Files affected:** `DashboardPage.tsx`

`DashboardPage.tsx` FAB uses `bottom: 80` (static, non-responsive), while every other page with a FAB uses `bottom: { xs: 80, sm: 16 }`. On desktop/tablet this means the FAB stays 80px from the bottom even when BottomNavigation is not visible.

---

## Workplan

### Phase 1 — Critical Fixes (High Impact)

#### Task 1.1 — Fix Double Bottom Padding

**Effort:** Small
**Files:** `App.tsx` + all page components (11 files)

Decide on one of these approaches:
- **Option A (recommended):** Remove `pb: 10` from all page `Box` wrappers. Let `App.tsx` shell handle the bottom clearance universally.
- **Option B:** Remove the `pb` from `App.tsx` shell and keep `pb: 10` on each page (less DRY).

The preferred approach is Option A, as it centralizes the concern in one place and avoids repetition.

#### Task 1.2 — Fix Non-Responsive FAB on DashboardPage

**Effort:** Trivial
**File:** `src/pages/DashboardPage.tsx`

Change:
```tsx
// Before
bottom: 80, right: 16

// After
bottom: { xs: 80, sm: 16 }, right: 16
```

---

### Phase 2 — Standardization (Medium Impact)

#### Task 2.1 — Replace Hardcoded px with Theme Spacing in FABs

**Effort:** Small
**Files:** `DashboardPage.tsx`, `CoopsPage.tsx`, `FlocksPage.tsx`, `PurchasesPage.tsx`

Replace raw pixel values with MUI theme spacing tokens in all FAB `sx` props:

```tsx
// Before
sx={{ position: 'fixed', bottom: { xs: 80, sm: 16 }, right: 16 }}

// After
sx={{ position: 'fixed', bottom: { xs: 10, sm: 2 }, right: 2 }}
```

Consider extracting a `fabPositionSx` constant to `src/shared/constants/fabConfig.ts` to avoid future drift.

#### Task 2.2 — Standardize Filter Section Component

**Effort:** Trivial
**Files:** `StatisticsPage.tsx`

Change `<Paper>` to `<Card elevation={1}>` in the filter section to match `DailyRecordsListPage.tsx`.

---

### Phase 3 — Polish (Low Impact)

#### Task 3.1 — Standardize Error State Padding

**Effort:** Trivial
**Files:** `FlocksPage.tsx`, `CoopsPage.tsx`

Change `py: 4` to `py: 3` in error/loading state containers.

Alternatively, extract a shared `<PageErrorState>` component that encapsulates the correct layout.

#### Task 3.2 — Replace Hardcoded px in Icon Containers

**Effort:** Trivial
**Files:** `src/shared/components/StatCard.tsx`, `src/features/dashboard/components/QuickActionCard.tsx`

```tsx
// Before
<Box sx={{ width: 40, height: 40 }}>

// After
<Box sx={{ width: 5, height: 5 }}>  // 5 * 8px = 40px
```

---

## Affected Files Reference

| File | Issues |
|------|--------|
| `src/App.tsx` | Issue 1 (double padding — source) |
| `src/pages/DashboardPage.tsx` | Issue 1, 2, 6 |
| `src/pages/CoopsPage.tsx` | Issue 1, 2, 4 |
| `src/pages/FlocksPage.tsx` | Issue 1, 2, 4 |
| `src/pages/CoopDetailPage.tsx` | Issue 1 |
| `src/pages/FlockDetailPage.tsx` | Issue 1 |
| `src/pages/DailyRecordsListPage.tsx` | Issue 1 |
| `src/pages/StatisticsPage.tsx` | Issue 1, 3 |
| `src/pages/SettingsPage.tsx` | Issue 1 |
| `src/features/purchases/pages/PurchasesPage.tsx` | Issue 1, 2 |
| `src/features/flocks/pages/FlockHistoryPage.tsx` | Issue 1 |
| `src/shared/components/StatCard.tsx` | Issue 5 |
| `src/features/dashboard/components/QuickActionCard.tsx` | Issue 5 |

---

## Priority Order

1. **Issue 1** — Double bottom padding (all pages, visual gap too large)
2. **Issue 6** — Non-responsive FAB on Dashboard (broken on desktop)
3. **Issue 2** — Hardcoded FAB px values (consistency + maintainability)
4. **Issue 3** — Filter section Card vs Paper (visual inconsistency)
5. **Issue 4** — Error state padding (minor visual difference)
6. **Issue 5** — Icon container px values (code quality only)
