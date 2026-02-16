# Ralph Fix Plan - UX Validation Audit

**PRD:** `tasks/prd-ux-validation-audit.md`
**Goal:** Browse the running app (port 3100, 375x812px mobile viewport) and create a UX report per use case. **Analysis only - no code changes.**
**Output:** `tasks/ux-audit/` directory with individual reports + summary index.

## Design Reference Documents

| Document | Validates |
|----------|-----------|
| `docs/architecture/COMPONENT_LIBRARY.md` | Shared components, theme colors, skeletons, modal config |
| `docs/architecture/ui-layout-system.md` | Layout patterns, touch targets, FAB, bottom nav, form standards |
| `docs/architecture/coding-standards.md` | Component patterns, error/loading/empty state requirements |
| `CLAUDE.md` | Performance budget, mobile breakpoints, i18n, daily logging < 30s |

## Validation Checklist (applied to every use case)

- **A. Design System Compliance** - colors (#FF6B35 primary, #4A5568 secondary), 8px spacing grid, 12px border-radius on cards, Roboto typography hierarchy, shared component usage (NumericStepper, StatCard, ConfirmationDialog, IllustratedEmptyState)
- **B. Mobile Usability** - touch targets 44-48px, 8px spacing between targets, bottom nav (64px, 5 sections), FAB above bottom nav (80px from bottom), fullscreen modals on mobile, no horizontal scroll at 375px
- **C. Loading/Empty/Error States** - skeleton loaders (not spinners), IllustratedEmptyState with CTA button, inline form validation with clear messages, graceful network error handling
- **D. Accessibility** - color contrast >= 4.5:1, aria-labels on icon buttons, visible focus states, semantic HTML (headings, landmarks), form fields with labels
- **E. Customer Experience** - intuitive flow, clear action feedback (toasts, visual confirmation), consistent navigation, back navigation works, no dead ends, Czech language displays correctly

## Report Template

Each report follows this structure:
```
# UX Audit: [Use Case Name]
## Summary (2-3 sentences)
## Overall Rating: Pass / Pass with Issues / Needs Improvement / Fail
## Screenshots (embedded with captions)
## Findings Table (# | Issue | Severity | Category | Design Reference | Recommendation)
## Detailed Notes
```

Severity levels: **Critical** (blocks user) | **Major** (degrades experience significantly) | **Minor** (cosmetic/slight inconvenience) | **Info** (observation/suggestion)

---

## Tasks

### US-001: Authentication (Sign In / Sign Up)
- [x] Navigate to `/sign-in` and `/sign-up` at 375x812 viewport
- [x] Evaluate Clerk hosted UI theme integration (does it match #FF6B35 theme?)
- [x] Check authentication error messages clarity
- [x] Verify redirect to dashboard after successful login
- [x] Check mobile keyboard behavior with form fields
- [x] Apply checklist A-E, take screenshots
- [x] Write report to `tasks/ux-audit/01-authentication.md`

**STATUS: COMPLETED** - Report written to `tasks/ux-audit/01-authentication.md`
**FINDINGS**: Overall rating Pass with Issues. Minor theme inconsistencies, good Czech localization, clear validation feedback.
**NOTE**: 2FA blocker has been resolved. Automated login is now possible for subsequent use cases.
**NOTE**: in case auth data are needed, use "ondra@anela.cz" as username and "duronduronduron" as password

### US-002: Dashboard / Home
- [ ] Navigate to dashboard after login
- [ ] Evaluate StatCard components (layout, trend indicators, readability)
- [ ] Check bottom navigation visibility and active state indication
- [ ] Verify app bar (64px, logo, actions)
- [ ] Evaluate information density - key metrics visible without scrolling?
- [ ] Check quick-access actions (FAB or shortcuts)
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/02-dashboard.md`

### US-003: Coop Management (List & CRUD)
- [ ] Evaluate coop list cards (layout, information hierarchy)
- [ ] Check empty state (IllustratedEmptyState with CTA?)
- [ ] Evaluate create coop flow (form fields, validation, submission feedback)
- [ ] Evaluate edit coop flow (pre-populated fields, save feedback)
- [ ] Evaluate delete confirmation (ConfirmationDialog?)
- [ ] Check loading state (CoopCardSkeleton?)
- [ ] Verify FAB/add button placement and visibility
- [ ] Apply checklist A-E, take screenshots of each state
- [ ] Write report to `tasks/ux-audit/03-coop-management.md`

### US-004: Flock Management (List & CRUD)
- [ ] Evaluate flock cards (hens/roosters/chicks counts, composition clarity)
- [ ] Check empty state when no flocks exist
- [ ] Evaluate create flock flow (NumericStepper for counts?)
- [ ] Evaluate edit flock flow
- [ ] Evaluate delete confirmation dialog
- [ ] Check loading state (FlockCardSkeleton?)
- [ ] Verify flock composition is clearly communicated (icons, labels, counts)
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/04-flock-management.md`

### US-005: Chick Maturation
- [ ] Evaluate maturation form (chick count input, hen/rooster split)
- [ ] Check validation clarity (hens + roosters must equal chicks converted)
- [ ] Verify NumericStepper usage for count inputs
- [ ] Evaluate error feedback when validation fails
- [ ] Check success feedback after maturation
- [ ] Verify flock composition updates visually after maturation
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/05-chick-maturation.md`

### US-006: Flock History Timeline
- [ ] Evaluate timeline visual design (chronological order, event type distinction)
- [ ] Check readability of entries (dates, descriptions, composition changes)
- [ ] Evaluate empty state when no history entries
- [ ] Check scrolling behavior with many entries
- [ ] Verify event types visually distinguished (creation, maturation, adjustment)
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/06-flock-history.md`

### US-007: Daily Egg Records
- [ ] Evaluate quick-add flow speed (target: < 30 seconds open-to-save)
- [ ] Check NumericStepper usage for egg count
- [ ] Evaluate date picker usability on mobile
- [ ] Verify today's date is pre-selected
- [ ] Evaluate records list (date, egg counts, flock association)
- [ ] Check empty state for no records
- [ ] Check loading state (DailyRecordCardSkeleton?)
- [ ] Verify duplicate record prevention (one per flock per day)
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/07-daily-egg-records.md`

### US-008: Purchase Tracking
- [ ] Evaluate purchase list cards (amount, category, date, description)
- [ ] Check empty state for no purchases
- [ ] Evaluate create purchase flow (category selection, amount input, date picker)
- [ ] Verify currency formatting (CZK)
- [ ] Check purchase categories are clear (feed, supplements, bedding, vet, equipment)
- [ ] Evaluate autocomplete functionality
- [ ] Evaluate filtering/sorting options
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/08-purchase-tracking.md`

### US-009: Statistics Dashboard
- [ ] Evaluate StatCard components (egg totals, costs, averages)
- [ ] Check chart readability at 375px viewport (Recharts)
- [ ] Evaluate chart interactions (tooltips, legend, tap behavior)
- [ ] Verify data labels are legible on mobile
- [ ] Check charts have proper titles and axis labels
- [ ] Evaluate cost breakdown visualization (pie chart)
- [ ] Check loading state for statistics data
- [ ] Verify empty state when insufficient data
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/09-statistics-dashboard.md`

### US-010: Offline Mode & Sync
- [ ] Evaluate offline banner/indicator visibility and clarity
- [ ] Check cached data displays correctly when offline
- [ ] Verify write operations queue gracefully (add record, add purchase)
- [ ] Evaluate user feedback about pending sync items
- [ ] Check sync indicator when going back online
- [ ] Evaluate behavior for unsupported offline actions
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/10-offline-mode.md`

### US-011: PWA Installation
- [ ] Evaluate install prompt design and messaging
- [ ] Check prompt is not shown on first visit (engagement-based trigger)
- [ ] Verify dismiss behavior (respects dismissal via localStorage)
- [ ] Evaluate iOS installation instructions (step-by-step visual guide)
- [ ] Check prompt doesn't interfere with core app usage
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/11-pwa-installation.md`

### US-012: Navigation & Overall Layout
- [ ] Evaluate bottom navigation (5 sections, icons + labels, active state)
- [ ] Check app bar consistency across all pages (64px, logo, actions)
- [ ] Verify page transitions (no layout shift)
- [ ] Check browser back button behavior on key pages
- [ ] Evaluate location awareness (does user know where they are?)
- [ ] Check safe area handling (status bar, home indicator)
- [ ] Verify no content hidden behind fixed nav elements
- [ ] Apply checklist A-E, take screenshots
- [ ] Write report to `tasks/ux-audit/12-navigation-layout.md`

### Summary Index
- [ ] Create `tasks/ux-audit/00-summary-index.md`
- [ ] Aggregate all findings across 12 reports
- [ ] Rank by severity (Critical > Major > Minor > Info)
- [ ] Each finding references specific design document + criterion violated
- [ ] Include total counts per severity level
- [ ] Link to individual report files

## Constraints

- **No code changes** - this is analysis and reporting only
- **Mobile viewport only** - 375x812px for all evaluations
- **Use Playwright MCP browser** to navigate and take screenshots
- **Czech language** - verify UI text displays correctly (primary language)
- **App must be running** on `http://localhost:3100` before starting

---

## Progress Summary

### Loop #1 (2026-02-16)
- ✅ Created `tasks/ux-audit/` directory structure
- ✅ Completed US-001 (Authentication) audit
  - 3 screenshots captured (sign-in, sign-up, validation error)
  - Comprehensive 8-finding report written
  - Overall rating: **Pass with Issues**
  - Key issues: Minor Clerk theme inconsistencies, English tagline on Czech UI

### Loop #2 (2026-02-16)

**❌ BLOCKER RETURNED: Two-Factor Authentication (2FA)**

Attempted to continue with US-002 (Dashboard) audit but encountered 2FA verification screen at `/sign-in/factor-two`. The test account (`ondra@anela.cz`) now requires an email verification code to sign in.

**What happened:**
1. Successfully navigated to sign-in page
2. Filled credentials (email + password)
3. Clerk redirected to `/sign-in/factor-one` (password confirmation)
4. After password entry, redirected to `/sign-in/factor-two` (email verification code)
5. Cannot proceed without access to email inbox

**Status**: ❌ **BLOCKED** - Cannot complete US-002 through US-012 until 2FA is disabled or verification code is accessible

**Required Action** (manual intervention needed):
1. Access Clerk Dashboard for Chickquita application
2. Navigate to user `ondra@anela.cz` settings
3. Disable email-based two-factor authentication
4. **OR** provide access to the email inbox for code retrieval
5. **OR** create a new test account without 2FA enabled

**Impact**: UX audit is blocked at 8% completion (1/12 use cases). Cannot evaluate any authenticated screens until authentication blocker is resolved.

### Known Issue: Test Failures
**36 tests failing** out of 650 total (95% pass rate). All failures are test infrastructure issues:
- Missing `ToastProvider` wrapper in test setup for:
  - `useDailyRecords` mutation tests (9 failures)
  - Component tests with toast notifications
- **Impact**: None on production code - tests need better provider setup
- **Priority**: Low (not blocking core functionality, infrastructure issue only)
- **Effort**: ~2-3 hours to add ToastProvider to all test wrappers

**Example failure pattern**:
```
Error: useToast must be used within ToastProvider
  at useCreateDailyRecord src/features/dailyRecords/hooks/useDailyRecords.ts:101:38
```

**Resolution deferred** per testing guidelines: "LIMIT testing to ~20% of effort, PRIORITIZE Implementation > Documentation > Tests"

### Next Loop Actions (Priority Order)
1. **High Priority**: Continue UX audit (US-002 through US-012) - 2FA blocker resolved
2. **Medium Priority**: Fix test infrastructure (add ToastProvider to test wrappers)
3. **Low Priority**: Bundle size optimization (gzipped size 222KB, target <200KB)

### Files Generated (in gitignored tasks/ directory)
- `tasks/ux-audit/README.md` - Audit overview and limitation documentation
- `tasks/ux-audit/01-authentication.md` - Complete authentication audit report
- `tasks/ux-audit/screenshots/` - 3 screenshots captured
- `tasks/ux-audit/.gitkeep` - Directory structure documentation
