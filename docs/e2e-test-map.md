# Chickquita E2E Test Map

> **Goal**: Comprehensive map of all existing Playwright E2E tests, their coverage, overlap analysis, and value assessment.
>
> **Status**: Cleanup completed 2026-02-18 — removed 6 files (~87 tests) and reduced 4 files (~35 tests).

---

## Overview (Post-Cleanup)

| File | Tests | Feature Area | Value |
|------|-------|-------------|-------|
| `coops.spec.ts` | 17 | Coop CRUD + API integration | ✅ High |
| `flocks.spec.ts` | 16 + 1 skipped | Flock CRUD + API integration | ✅ High |
| `flocks-i18n.spec.ts` | 16 | Flock internationalization | ✅ High |
| `flocks-performance.spec.ts` | 3 | FCP, API response time, CLS | ✅ Medium |
| `dashboard-fab.spec.ts` | 2 | FAB a11y label + opens modal | ✅ Medium |
| `purchases-crud.spec.ts` | 25 | Purchase full CRUD lifecycle | ✅ High |
| `purchases-autocomplete.spec.ts` | 4 | Autocomplete regression | ✅ High |
| `daily-records-quick-add.spec.ts` | 12 | Quick Add modal | ✅ High |
| `daily-records-full-workflow.spec.ts` | 7 | Daily records CRUD (desktop+mobile+tablet) | ✅ High |
| `daily-records-list.spec.ts` | 9 | Daily records list + filters | ✅ High |
| `daily-records-accessibility.spec.ts` | 10 | Axe a11y scans + keyboard navigation | ✅ High |
| `daily-records-mobile-ui.spec.ts` | 4 | Bottom nav touch targets on 4 devices | ✅ Medium |
| `daily-records-quick-add-performance.spec.ts` | 3 | Quick Add on Fast 3G (1 run + mobile + breakdown) | ✅ Medium |
| `crossbrowser/visual-regression.crossbrowser.spec.ts` | 23 | Screenshot diffing | ⚠️ Medium |
| `crossbrowser/responsive-layout.crossbrowser.spec.ts` | 37 | Layout across 6 breakpoints | ⚠️ Medium |

**Total: ~188 tests across 15 files** (reduced from ~282 across 21 files)

### Removed Files

| File | Tests removed | Reason |
|------|--------------|--------|
| `purchases.spec.ts` | 11 | Raw HTTP API tests — no UI; belongs in backend test projects |
| `purchases-list.spec.ts` | 13 | All meaningful tests conditional on pre-existing data (`if count > 0`) |
| `purchases-page.spec.ts` | 15 | Full overlap with `purchases-crud.spec.ts` |
| `purchases-form.spec.ts` | 11 | Full overlap with `purchases-crud.spec.ts` + `purchases-autocomplete.spec.ts` |
| `daily-records-edit.spec.ts` | 11 | All tests skip without test data; edit flow covered by `daily-records-full-workflow.spec.ts` |
| `crossbrowser/browser-compatibility.crossbrowser.spec.ts` | 26 | ~12 tests test JS language features (Promises, array methods); rest overlap dedicated specs |

---

## Detailed Test Inventory

### `coops.spec.ts` — Coop Management (M2)

**Setup**: Health check on `localhost:5100` before all; navigates to `/coops` before each.

| Group | Test | What it verifies |
|-------|------|-----------------|
| API Integration | no 4xx on load | Backend returns 200 for coops list |
| API Integration | GET returns valid structure | `id`, `name`, `isActive` fields present |
| API Integration | empty list handled | Shows empty state or list, never error |
| API Integration | no 400 for missing params | Default params work |
| Create Coop | name only | POST 201, coop appears in list |
| Create Coop | name + location | Location displayed on card |
| Create Coop | empty name | Submit disabled + validation error |
| Create Coop | cancel | No coop created |
| Create Coop | max length (>100 chars) | Submit disabled + validation error |
| List Coops | empty state | Shows empty state UI (conditional) |
| List Coops | list display | 3 created coops all visible |
| List Coops | nav from dashboard | URL = `/coops`, title visible |
| Edit Coop | edit name | Old name gone, new name visible |
| Edit Coop | edit location | Location updated on card |
| Edit Coop | cancel edit | Original name unchanged |
| Edit Coop | empty name validation | Submit disabled + error |
| Archive Coop | archive action | Confirm dialog → archived |
| Archive Coop | cancel archive | Coop still visible |
| Delete Coop | delete empty coop | Coop removed from list |
| Delete Coop | cancel delete | Coop still visible |
| Mobile | create/edit on 375×667 | Full workflow works |
| Mobile | touch target size | More button ≥ 44×44px |
| Tenant Isolation | only own coops | Creates unique coop, verifies visible |

---

### `flocks.spec.ts` — Flock Management (CRUD Journey)

**Setup**: Creates a fresh coop before each test, uses captured coop ID.

| Group | Test | What it verifies |
|-------|------|-----------------|
| Navigation | nav to flocks from coop detail | URL = `/coops/{id}/flocks`, empty state |
| Create | valid flock data | Hens/roosters/chicks displayed, status Active |
| Create | empty identifier | Submit disabled |
| Create | future hatch date | Error "future/budoucnosti" |
| Create | all counts zero | Submit disabled |
| Create | cancel | Flock count unchanged |
| List | empty state | Empty state visible, count = 0 |
| List | multiple flocks (3) | All cards visible, composition correct, status Active |
| Edit | edit identifier + hatch date | New identifier visible, old gone, composition unchanged |
| Edit | cancel edit | Original identifier unchanged |
| Edit | empty identifier validation | Submit disabled |
| Archive | archive after confirmation | Not in Active filter, visible in All filter, status Archived |
| Archive | cancel archive | Still visible, status Active |
| Filter | active/all filter | Active hides archived; All shows both |
| Mobile | 375×667 create/edit | Modal works, FAB ≥ 44×44px |
| API | GET returns valid structure | Array with `id`, `identifier`, `hens`, etc. |
| API | client validation blocks submit | No 4xx API calls for invalid form |
| API | no 4xx on load | GET flocks returns 200 |
| API | (skipped) handle API errors | Skipped due to timing issues |

---

### `flocks-i18n.spec.ts` — Flock Internationalization

**Setup**: Creates a coop + navigates to flocks page per test. Language switched via `localStorage.i18nextLng`.

| Group | Test | What it verifies |
|-------|------|-----------------|
| Czech | page elements | Title "Hejna", empty state, filter buttons, FAB aria-label |
| Czech | create modal | Modal title, labels (Identifikátor, Datum líhnutí), buttons |
| Czech | validation errors | "Toto pole je povinné", "Datum nemůže být v budoucnosti" |
| Czech | flock card labels | Slepice, Kohouti, Kuřata, Celkem, Aktivní |
| Czech | archive dialog | Title "Archivovat hejno?", dialog text |
| English | page elements | Title "Flocks", empty state, filter buttons, FAB aria-label |
| English | create modal | "Add Flock", field labels, buttons |
| English | validation errors | "This field is required", "Date cannot be in the future" |
| English | flock card labels | Hens, Roosters, Chicks, Total, Active |
| English | archive dialog | "Archive Flock?", dialog text |
| Language Switching | CS→EN→CS | Title changes correctly |
| Language Switching | persist across reloads | English persists after page reload |
| Number Formatting | integer counts | No decimals, values 123/45/67/235 |
| Number Formatting | non-negative inputs | Negative values rejected or corrected |
| Date Formatting | HTML date input type | `type="date"` attribute |
| Date Formatting | no future dates | Error "Datum nemůže být v budoucnosti" |
| No Hardcoded Text | no English in Czech mode | 10 English phrases must not appear |
| No Hardcoded Text | no Czech in English mode | 10 Czech phrases must not appear |
| ARIA Labels (Czech) | localized aria-labels | FAB "Přidat hejno", 3×Zvýšit/Snížit buttons |
| ARIA Labels (English) | localized aria-labels | FAB "Add Flock", 3×Increase/Decrease buttons |

---

### `flocks-performance.spec.ts` — Flock List Performance

**Setup**: Creates 50 flocks in `beforeAll` using a real API. Tests measure wall-clock and browser API times.

| Test | What it verifies | Budget |
|------|-----------------|--------|
| list renders within 1s | Wall-clock from navigation to first card visible | < 1000ms |
| performance metrics via API | FCP, LCP, DOM Interactive via `performance` API | FCP < 1000ms |
| scroll through large list | Scroll to bottom + last card visible | < 1000ms |
| filter without degradation | Click "All" filter, re-render visible | < 500ms |
| API response time | Network timing of GET flocks | < 2000ms |
| no layout shift (CLS) | PerformanceObserver for layout-shift entries | CLS ≤ 0.1 |
| card rendering efficiency | JS query of 50 DOM nodes | < 1000ms |
| rapid navigation (5×) | Average round-trip to flocks page | < 1000ms avg, max ≤ 2× min |

---

### `dashboard-fab.spec.ts` — Dashboard FAB Button

**Setup**: Creates a coop + flock before each test.

| Test | What it verifies |
|------|-----------------|
| FAB visible when user has flocks | FAB is visible on dashboard |
| correct a11y label | `aria-label` matches "Přidat denní záznam\|Add daily record" |
| opens QuickAddModal | Modal title visible after click |
| positioned above bottom nav (mobile 375×667) | `fabBox.y + fabBox.height < navBox.y` |
| positioned right side with spacing | 12–24px from right edge |
| fixed positioning | `position: fixed` via `getComputedStyle` |
| close modal on cancel | Modal title no longer visible |

---

### `purchases-crud.spec.ts` — Purchase CRUD Lifecycle ✅ Primary Reference

**Setup**: Navigates to `/purchases` before each.

| Group | Test | What it verifies |
|-------|------|-----------------|
| Create | all required fields | Purchase card visible, name matches |
| Create | minimal fields | Created without optional date/notes |
| Create | empty name validation | Submit disabled |
| Create | zero amount validation | Submit disabled |
| Create | future date validation | Error message "future\|budoucnosti" |
| Create | cancel | Count unchanged |
| Create | different types (Feed/Vitamins/Bedding) | All 3 types created and visible |
| Filter | by date range | Feb purchase visible, Mar not visible |
| Filter | by type (Feed) | Feed visible, Vitamins not |
| Filter | clear type filter | All 3 purchases visible again |
| Filter | combine date + type | Only Feb Feed visible |
| Edit | name | New name visible, old gone |
| Edit | amount + quantity | Card still visible |
| Edit | type | Card still visible |
| Edit | cancel | Original name unchanged |
| Edit | empty name validation | Submit disabled |
| Edit | multiple fields at once | Fully updated purchase visible |
| Delete | after confirmation | Card gone, count decremented |
| Delete | cancel | Card still there, count unchanged |
| Delete | dialog content | Title, content, both buttons visible |
| Autocomplete | suggestions based on existing | Options appear after typing "Premium" |
| Autocomplete | free text entry | Submit enabled for unique name |
| Autocomplete | select suggestion | Name field populated |
| Mobile (iPhone SE 375×667) | create/edit/delete | Full workflow + FAB ≥ 44×44px |
| Mobile (Pixel 5 393×851) | page title + create | Purchase created and visible |
| Desktop (1280×720) | create + filter | Purchase created, filter works |
| Empty State | when no purchases | Empty state message visible |
| Empty State | hide after first purchase | Empty state gone, count > 0 |

---

### `purchases-autocomplete.spec.ts` — Autocomplete Regression

**Regression for**: `TypeError: options.filter is not a function`

| Test | What it verifies |
|------|-----------------|
| no crash on open | Modal opens, no console "options.filter" errors |
| create after autocomplete typing | Form submits successfully, purchase in list |
| rapid typing stability | 50ms delay typing, form still functional, value correct |
| clear and retype | Second autocomplete query doesn't crash form |

---

### `purchases.spec.ts` — Purchases Raw API ❌ Candidate for Removal

Direct `request.post/get/put/delete` to `http://localhost:5100/api/v1/purchases` — **no browser/UI involved**.

| Group | Test | What it verifies |
|-------|------|-----------------|
| Happy Path | create → 201 + Location header | API response structure |
| Happy Path | get by ID → 200 | Response has correct fields |
| Happy Path | list includes created | Array contains created purchase |
| Happy Path | update → 200 | Updated fields match |
| Happy Path | delete → 204 | Success status |
| Happy Path | get after delete → 404 | Item not found |
| Error Path | empty name → 400 | Error structure in response |
| Error Path | non-existent ID → 404 | Not found |
| Error Path | ID mismatch on PUT → 400 | Bad request |
| Names endpoint | GET names list | Returns array of strings |
| Names endpoint | filter by query param | Array returned |

---

### `purchases-list.spec.ts` — Purchase List Page ❌ Candidate for Removal

**Pattern**: Most tests use `if (count > 0)` — silently pass when no data exists.

| Group | Test | What it verifies |
|-------|------|-----------------|
| List | empty state | Heading, Filters section, empty text |
| List | all filter options | Date inputs, type dropdown, flock filter visible |
| List | filter by date range | Date inputs accept values |
| List | filter by type | Type dropdown selection |
| List | monthly summary (conditional) | Creates purchase inline if button available |
| List | purchase cards (conditional) | Edit/delete buttons on first card |
| List | delete confirmation (conditional) | Dialog appears on delete click |
| List | cancel delete (conditional) | Dialog closes on cancel |
| List | delete confirmed (conditional) | Card removed |
| List | keyboard navigation (conditional) | Tab between edit and delete buttons |
| List | clear type filter | Select "All" after "Feed" |
| Accessibility | ARIA labels | Labels on filters and card buttons |
| Accessibility | screen reader headings | h2/h3/h6 count > 0, "Filtry" heading |

---

### `purchases-page.spec.ts` — Purchase Page & Routing ❌ Candidate for Removal

Overlaps heavily with `purchases-crud.spec.ts`.

| Group | Test | What it verifies |
|-------|------|-----------------|
| Navigation | route works | URL contains `/purchases`, heading visible |
| Navigation | via bottom nav | Click nav button → purchases page |
| Navigation | auth protection | Clear storage → redirect to sign-in |
| FAB | visible on page | FAB button visible |
| FAB | opens modal | Modal title visible |
| FAB | cancel closes modal | Modal title gone |
| Create Flow | create and see in list | Form fill → submit → purchase in list |
| Create Flow | validation disabled submit | Submit disabled without required fields |
| Edit Flow | open with pre-filled data | Name/amount/quantity pre-filled |
| Edit Flow | update and close | Updated name in list |
| Edit Flow | cancel closes modal | Modal title gone |
| Responsive | mobile 375×667 | Heading, FAB, bottom nav visible |
| Responsive | desktop 1280×720 | Create button visible |
| PurchaseList Integration | renders component | Filters section visible |
| PurchaseList Integration | filters visible | Date/type inputs visible |
| Bottom Nav | purchases tab selected | `Mui-selected` class present |

---

### `purchases-form.spec.ts` — Purchase Form Standalone ❌ Candidate for Removal

| Group | Test | What it verifies |
|-------|------|-----------------|
| Create Flow | create full form | All fields → submit → visible in list |
| Create Flow | autocomplete suggestions | Options appear for "Kr" input |
| Create Flow | type icons/options | All 6 purchase types in dropdown |
| Validation | empty required fields | Submit disabled |
| Validation | future purchase date | Error message visible |
| Validation | zero amount | Submit disabled |
| Validation | zero quantity | Submit disabled |
| Accessibility | ARIA labels | Labels present and have `aria-label` attr |
| Accessibility | keyboard navigation | Focus moves to date, notes fields |
| Mobile | mobile viewport | Form visible, NumericStepper buttons visible |
| Mobile | touch-friendly inputs | Name input height ≥ 40px |

---

### `daily-records-list.spec.ts` — Daily Records List

**Setup**: Navigates to `/daily-records` before each test.

| Test | What it verifies |
|------|-----------------|
| empty state | Heading, "Filtrovat" section, empty text |
| all filter options | Flock, from-date, to-date, Today/Last Week/Last Month chips |
| "Today" quick filter | Both date inputs = today's date |
| "Last Week" quick filter | End date = today, start date not empty |
| "Last Month" quick filter | End date = today, start date not empty |
| clear all filters | After applying Today, clear → both inputs empty |
| manual date range | Input accepts `2024-02-01` and `2024-02-15` |
| loading skeletons | Skeleton appears briefly (flaky by design) |
| responsive grid layout | MUI Grid container exists |
| mobile viewport accessibility | Heading/filters visible at 375×667 |
| filter state not persisted | Navigating away resets filter to empty |

---

### `daily-records-quick-add.spec.ts` — Quick Add Modal

**Setup**: Creates a coop + flock before each test via API intercept.

| Test | What it verifies |
|------|-----------------|
| open from dashboard | FAB visible + enabled, modal title visible |
| all form fields | Flock, Date, Egg count stepper, Notes, buttons |
| today as default date | Date input = today's ISO date |
| increment/decrement | 0 → 1 → 2 → 1, decrement disabled at 0 |
| future date validation | Error "nemůže být v budoucnosti" |
| notes max length | 501 chars → "maximální délka" error |
| character count | "0/500 znaků" → "9/500 znaků" |
| submit + close | POST 201 received, modal closes, success toast |
| reset after close | Reopen shows egg=0, notes empty |
| disable during submit | Flock/date/notes disabled during 1s delay |
| < 30 second workflow | FAB click → fill → submit < 30s |
| mobile 375×667 | Dialog visible, all fields, save button ≥ 44px |

---

### `daily-records-full-workflow.spec.ts` — Full CRUD Workflow

**Setup**: Creates coop + flock before each test.

| Test | What it verifies |
|------|-----------------|
| full CRUD desktop | CREATE (12 eggs, notes) → READ (filter Today, card visible) → UPDATE (15 eggs, different notes) → DELETE (confirm, card gone), all < 60s |
| full CRUD mobile (375×667) | Same as above with 3 eggs, update to 5, then delete |
| full CRUD tablet (768×1024) | Same with 8 eggs, update to 10 |
| performance budget | Create → read → delete in < 60s |
| screenshot capability | Just navigates to `/daily-records` and checks heading |
| all breakpoints (5) | 320→568→1024→1920 each shows heading + filters |
| validation errors | Future date → error + save disabled |

---

### `daily-records-accessibility.spec.ts` — Accessibility Tests

| Test | What it verifies |
|------|-----------------|
| no axe violations on list page | WCAG 2.1 AA scan with zero violations |
| no axe violations on Quick Add modal | Zero violations (skipped if FAB not visible) |
| keyboard nav on list page | Flock/date/clear-filter chips all focusable |
| keyboard nav in modal | All fields focusable; Escape closes modal |
| labels on all interactive elements | All filter fields labeled; FAB has aria-label |
| focus management on modal open/close | Escape closes modal |
| focus with validation errors | `aria-invalid="true"` on future date field |
| ARIA on modal dialogs | `role="dialog"`, `aria-labelledby` or `aria-label` |
| color contrast | WCAG AA color-contrast violations = 0 |
| semantic HTML | `<h1>` for main heading, filter heading present |

---

### `daily-records-edit.spec.ts` — Edit Modal ❌ Candidate for Removal

**Pattern**: Every test uses `if (await editButton.isVisible())` — silently skips if no same-day records exist. Does not create test data.

| Test | What it verifies |
|------|-----------------|
| edit button visible only same-day | Checks count ≥ 0 (always passes) |
| modal opens with pre-filled data | Creates record inline via FAB, then edits |
| date field read-only | `disabled` attribute + helper text |
| flock field read-only | `disabled` attribute + helper text |
| egg count + notes editable | Fill 20 eggs, "Updated notes" |
| successfully updates record | PUT closes modal, card updates |
| cancel closes modal | Modal title gone |
| validate negative egg count | Error "Musí být kladné číslo" |
| validate notes > 500 chars | Error "maximální délka je 500 znaků" |
| mobile fullscreen modal | dialog visible at 375×667 |
| character count for notes | "10/500 znaků" counter |

---

### `daily-records-mobile-ui.spec.ts` — Mobile UI Tests

Tests run for 4 devices: iPhone SE (320), iPhone 14 (390), Samsung A52 (412), Pixel 5 (393). All tests skip if FAB not visible.

| Group | Test per device | What it verifies |
|-------|----------------|-----------------|
| FAB Positioning (4×) | position | Right-aligned (≤100px from edge), above bottom nav (60–90px), ≥44×44px |
| FAB Positioning (4×) | stays fixed during scroll | X/Y unchanged after `scrollBy(0, 300)` |
| Modal Fullscreen (4×) | fullscreen on mobile | Width ≥ device width, height ≥ 90%, top ≤ 10px |
| Touch Targets (4×) | FAB size | ≥44px AND ≥56px (Material Design FAB) |
| Touch Targets (4×) | modal buttons | Save/cancel height ≥ 44px |
| Touch Targets (4×) | bottom nav buttons | Height ≥ 44px, width ≥ 40px |
| iOS vs Android | iOS consistency | All iOS FABs ≥ 56px |
| iOS vs Android | Android consistency | All Android FABs ≥ 56px |

---

### `daily-records-quick-add-performance.spec.ts` — Quick Add Performance

**Setup**: Creates coop + flock before each test. Fast 3G emulated via CDP `Network.emulateNetworkConditions`.

| Test | What it verifies |
|------|-----------------|
| Run 1/3 on Fast 3G | Full workflow < 30 seconds |
| Run 2/3 on Fast 3G | Full workflow < 30 seconds |
| Run 3/3 on Fast 3G | Full workflow < 30 seconds |
| mobile 375×667 on Fast 3G | Full workflow < 30 seconds |
| detailed breakdown | Logs per-step timings (navigation, modal open, form fill, submit) |

---

### `crossbrowser/visual-regression.crossbrowser.spec.ts` — Visual Regression

Screenshot-based tests using `toHaveScreenshot()`. Require baseline images to be committed.

| Group | Tests | What it captures |
|-------|-------|-----------------|
| Page Layouts (4) | sign-in, dashboard, coops, settings | Full page per browser |
| Component States (4) | bottom nav, coop card, empty state, loading skeleton | Element/page screenshots |
| Modal Dialogs (2) | create coop modal, confirmation dialog (conditional) | Page with modal open |
| Responsive Breakpoints (10) | dashboard + coops at 5 breakpoints | Full page per breakpoint per browser |
| Touch Target Verification (2) | FAB size, bottom nav button sizes | Size assertions + FAB screenshot |
| Accessibility Color Contrast (1) | dashboard screenshot | Color contrast via axe (indirect) |
| i18n Layout Stability (2) | Czech + English dashboard | Screenshots after language switch |

---

### `crossbrowser/browser-compatibility.crossbrowser.spec.ts` — Browser Compatibility ❌ Candidate for Removal

| Group | Test | Issue |
|-------|------|-------|
| Core Functionality | page navigation | Legitimate |
| Core Functionality | API requests complete | Legitimate |
| Core Functionality | localStorage operations | Tests JS API, not app |
| Core Functionality | cookies | Tests browser API, not app |
| Form Handling | text input | Same as coops creation test |
| Form Handling | validation displays | Same as coops form validation |
| Form Handling | select/dropdown | Tests settings page language select |
| CSS Features | flexbox layout | Checks `display` property — trivial |
| CSS Features | CSS grid | Checks `display` property — trivial |
| CSS Features | transitions/animations | Checks non-empty `transition` property |
| CSS Features | box-shadow | Checks ≠ "none" — trivial |
| CSS Features | border-radius | Checks ≠ "0px" — trivial |
| JS APIs | Promise/async-await | Tests JS language, not app |
| JS APIs | fetch API | Tests browser API |
| JS APIs | array methods (map/filter/reduce) | Tests JS language |
| JS APIs | Object.entries/values/keys | Tests JS language |
| JS APIs | template literals | Tests JS language |
| Touch Events | click events | Covered by other tests |
| Touch Events | scroll events | Trivial |
| Accessibility | focus management | Covered by accessibility spec |
| Accessibility | ARIA attributes | Covered by accessibility spec |
| Accessibility | keyboard navigation | Covered by accessibility spec |
| Error Handling | JS errors captured | No assertion on count |
| Error Handling | network errors graceful | Slow route (100ms) — not failure |

---

### `crossbrowser/responsive-layout.crossbrowser.spec.ts` — Responsive Layout

Runs for 6 breakpoints (320, 375, 480, 768, 1024, 1920).

| Group | Tests per breakpoint | What it verifies |
|-------|---------------------|-----------------|
| Dashboard Page (3×6=18) | grid layout, bottom nav, FAB position | No overflow; nav spans full width; FAB in bottom-right |
| Coops Page (2×6=12) | list layout, card touch targets | No overflow; cards fit viewport; menu buttons ≥44px |
| Modal Dialogs (1×6=6) | create coop modal | Full-screen on mobile (<600px), centered on desktop |
| Typography Scaling (1×6=6) | h1 font size, body text | H1 ≥20px mobile/28px desktop; body ≥12px |
| Navigation Behavior (1×6=6) | bottom nav interaction | Click coops → `/coops`; click settings → `/settings` |
| Scroll Behavior (1×6=6) | vertical scroll | scrollY changes; bottom nav stays fixed |
| Device-Specific (4) | iPhone SE/14, iPad, Galaxy A52 | Full 8-step journey: dashboard → coops → modal → settings |

---

## Coverage Summary by Feature

| Feature | Has Functional Tests | Has Validation Tests | Has Mobile Tests | Has a11y Tests | Has Perf Tests |
|---------|---------------------|---------------------|-----------------|----------------|----------------|
| Coops | ✅ | ✅ | ✅ | ❌ | ❌ |
| Flocks | ✅ | ✅ | ✅ | ❌ | ✅ |
| Flock i18n | ✅ | ✅ | ❌ | ✅ (ARIA) | ❌ |
| Purchases | ✅ | ✅ | ✅ | ⚠️ (partial) | ❌ |
| Daily Records | ✅ | ✅ | ✅ | ✅ | ✅ |
| Dashboard FAB | ✅ | ❌ | ✅ | ✅ (aria-label) | ❌ |

---

## Candidates for Removal / Consolidation

### ❌ Remove — Low Value

| File | Reason |
|------|--------|
| `purchases.spec.ts` | Tests raw HTTP API, not the UI. Coverage already provided by `purchases-crud.spec.ts` via browser requests. Backend integration tests belong in backend test projects. |
| `purchases-list.spec.ts` | Every meaningful test is conditional on pre-existing data (`if (count > 0)`) → silently passes when empty. Scenarios fully covered by `purchases-crud.spec.ts`. |
| `purchases-page.spec.ts` | Navigation, FAB, create/edit modal scenarios are all duplicated in `purchases-crud.spec.ts`. The auth-protection test (`context.clearCookies`) is useful but could be a single test in a dedicated auth spec. |
| `purchases-form.spec.ts` | Every scenario (form fill, autocomplete, validation, a11y, mobile) is a strict subset of `purchases-crud.spec.ts` + `purchases-autocomplete.spec.ts`. |
| `daily-records-edit.spec.ts` | Every test uses `if (await editButton.isVisible())` without creating test data — tests are effectively no-ops in a clean environment. The edit flow is thoroughly covered in `daily-records-full-workflow.spec.ts`. |
| `crossbrowser/browser-compatibility.crossbrowser.spec.ts` | ~12 tests test JavaScript language features (Promises, array methods, template literals, Object.keys), not the application. CSS property tests (flexbox display, border-radius, box-shadow) add no regression value. Accessibility and navigation scenarios are already covered by dedicated specs. |

### ⚠️ Consolidate / Reduce

| File | Issue | Recommendation |
|------|-------|---------------|
| `daily-records-quick-add-performance.spec.ts` | Runs 3 identical Fast 3G performance tests (Run 1/3, 2/3, 3/3). The runs create separate coops/flocks, triple setup cost. | Keep 1 run + the detailed breakdown test. Delete Run 2/3 and Run 3/3. |
| `daily-records-mobile-ui.spec.ts` | 32 device-variant tests that all silently skip if FAB is not present. FAB positioning is covered by `daily-records-full-workflow.spec.ts` and `dashboard-fab.spec.ts`. | Keep the 4 bottom-nav touch target tests (they always have data). Remove FAB-conditional tests or merge with `daily-records-quick-add.spec.ts`. |
| `dashboard-fab.spec.ts` | Each of 7 tests creates a full coop+flock just to verify FAB CSS/positioning properties. The functional test (opens modal) is in `daily-records-quick-add.spec.ts`. | Keep "opens modal" and "accessibility label" tests. Remove positioning/fixed CSS tests — these belong in visual regression. |
| `flocks-performance.spec.ts` | Creates 50 flocks in `beforeAll` (slow setup). Tests 1s budget is too strict for E2E vs local backend. `max ≤ 2× min` rapid navigation assertion is fragile. | Keep FCP and CLS tests. Remove wall-clock render and rapid-navigation tests; those metrics are better measured with Lighthouse CI. |

### ✅ Keep As-Is (High Value)

- `coops.spec.ts` — Core CRUD + API sanity checks
- `flocks.spec.ts` — Core CRUD + filter + API validation
- `flocks-i18n.spec.ts` — Critical i18n coverage
- `purchases-crud.spec.ts` — Comprehensive purchase lifecycle
- `purchases-autocomplete.spec.ts` — Targeted regression test
- `daily-records-quick-add.spec.ts` — Core quick-add workflow
- `daily-records-full-workflow.spec.ts` — Full CRUD across viewports
- `daily-records-list.spec.ts` — Filter UI coverage
- `daily-records-accessibility.spec.ts` — Axe scans + keyboard navigation
- `crossbrowser/visual-regression.crossbrowser.spec.ts` — Snapshot regression
- `crossbrowser/responsive-layout.crossbrowser.spec.ts` — Layout across breakpoints

---

## Cleanup Summary (Completed 2026-02-18)

| Action | Files | Tests removed |
|--------|-------|--------------|
| Deleted | `purchases.spec.ts` | 11 |
| Deleted | `purchases-list.spec.ts` | 13 |
| Deleted | `purchases-page.spec.ts` | 15 |
| Deleted | `purchases-form.spec.ts` | 11 |
| Deleted | `daily-records-edit.spec.ts` | 11 |
| Deleted | `crossbrowser/browser-compatibility.crossbrowser.spec.ts` | 26 |
| Reduced | `daily-records-quick-add-performance.spec.ts` | −2 (removed Run 2/3 and Run 3/3) |
| Reduced | `daily-records-mobile-ui.spec.ts` | −22 (kept only bottom-nav touch target tests) |
| Reduced | `dashboard-fab.spec.ts` | −5 (kept a11y label + opens modal) |
| Reduced | `flocks-performance.spec.ts` | −5 (kept FCP, API response time, CLS) |

**Net result**: ~282 → ~188 tests across 15 files (removed ~33% of test suite while preserving all meaningful regression coverage)
