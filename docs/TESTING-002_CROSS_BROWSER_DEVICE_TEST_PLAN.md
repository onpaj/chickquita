# TESTING-002: Cross-Browser & Device Testing Plan

**Version:** 1.0
**Date:** February 7, 2026
**Status:** Implementation Ready

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Test Requirements](#test-requirements)
3. [Test Strategy](#test-strategy)
4. [Browser & Device Matrix](#browser--device-matrix)
5. [Breakpoint Testing](#breakpoint-testing)
6. [Critical UI Components](#critical-ui-components)
7. [Critical User Flows](#critical-user-flows)
8. [Implementation Details](#implementation-details)
9. [Screenshot & Reporting](#screenshot--reporting)
10. [CI/CD Integration](#cicd-integration)
11. [Execution Plan](#execution-plan)

---

## Executive Summary

This document outlines the comprehensive test strategy for cross-browser and device testing of the Chickquita application. The testing covers:

- **6 browser configurations** (Chrome, Firefox, Safari - latest 2 versions each)
- **4 mobile devices** (iPhone SE, iPhone 14, iPad, Samsung Galaxy A52)
- **5 responsive breakpoints** (320px, 480px, 768px, 1024px, 1920px)
- **Full visual regression testing** with screenshot comparison
- **Automated HTML/JSON reporting** for CI integration

---

## Test Requirements

### Browser Support Requirements

| Browser | Versions | Platform | Priority |
|---------|----------|----------|----------|
| Chrome | Latest 2 | Desktop | P0 |
| Safari | Latest 2 | Desktop | P0 |
| Firefox | Latest 2 | Desktop | P1 |
| Mobile Safari | iOS 15+ | iPhone/iPad | P0 |
| Mobile Chrome | Android 10+ | Android | P0 |

### Device Support Requirements

| Device | Viewport | Platform | Priority |
|--------|----------|----------|----------|
| iPhone SE | 320x568 | iOS | P0 |
| iPhone 14 | 390x844 | iOS | P0 |
| iPad | 768x1024 | iOS | P1 |
| Samsung Galaxy A52 | 412x915 | Android | P1 |

### Breakpoint Requirements

| Breakpoint | Width | Use Case |
|------------|-------|----------|
| Mobile XS | 320px | Smallest mobile devices |
| Mobile Landscape | 480px | Mobile landscape orientation |
| Tablet | 768px | Tablet portrait |
| Desktop | 1024px | Small laptops/tablets landscape |
| Desktop HD | 1920px | Full HD monitors |

---

## Test Strategy

### Testing Pyramid for Cross-Browser

```
           /\
          /  \
         / VR \        Visual Regression (screenshots)
        /------\
       /  Func  \      Functional Tests (browser-specific)
      /----------\
     / Responsive \    Responsive Layout Tests
    /--------------\
   /   Compat.      \  Browser Compatibility Tests
  /__________________\
```

### Test Categories

1. **Browser Compatibility Tests**
   - JavaScript API compatibility
   - CSS feature support
   - Form handling
   - Local storage/cookies
   - Network request handling

2. **Responsive Layout Tests**
   - Grid/flexbox behavior
   - Typography scaling
   - Touch target sizes
   - Navigation behavior
   - Modal dialog sizing

3. **Visual Regression Tests**
   - Page layout screenshots
   - Component state screenshots
   - Breakpoint comparison
   - i18n layout stability

4. **Functional Tests**
   - User flow completion
   - Form submission
   - Navigation
   - Error handling

---

## Browser & Device Matrix

### Desktop Browsers

| Test ID | Browser | Version | Viewport | Notes |
|---------|---------|---------|----------|-------|
| CB-D01 | Chrome | Latest | 1920x1080 | Primary desktop |
| CB-D02 | Chrome | Latest-1 | 1024x768 | Fallback |
| CB-D03 | Firefox | Latest | 1920x1080 | Secondary |
| CB-D04 | Firefox | Latest-1 | 1024x768 | Fallback |
| CB-D05 | Safari | Latest | 1920x1080 | macOS primary |
| CB-D06 | Safari | Latest-1 | 1024x768 | Fallback |

### Mobile Browsers

| Test ID | Device | Browser | Viewport | Notes |
|---------|--------|---------|----------|-------|
| CB-M01 | iPhone SE | Safari | 320x568 | Smallest iOS |
| CB-M02 | iPhone 14 | Safari | 390x844 | Standard iOS |
| CB-M03 | iPad | Safari | 768x1024 | Tablet iOS |
| CB-M04 | Pixel 5 | Chrome | 393x851 | Android ref |
| CB-M05 | Galaxy A52 | Chrome | 412x915 | Android mid-range |

---

## Breakpoint Testing

### Breakpoint Test Matrix

| Breakpoint | Dashboard | Coops | Flocks | Settings | Modals |
|------------|-----------|-------|--------|----------|--------|
| 320px | X | X | X | X | X |
| 480px | X | X | X | X | X |
| 768px | X | X | X | X | X |
| 1024px | X | X | X | X | X |
| 1920px | X | X | X | X | X |

### Breakpoint-Specific Behaviors

#### 320px (Mobile XS)
- Single column layout
- Full-width cards
- Stacked navigation
- Full-screen modals
- Compressed typography

#### 480px (Mobile Landscape)
- Single column with larger margins
- Horizontal scroll prevention
- Landscape modal adjustments

#### 768px (Tablet)
- Two-column grid for cards
- Side margins appear
- Modal width constraints begin

#### 1024px (Desktop)
- Multi-column dashboard grid
- Standard modal sizing
- Desktop navigation enhancement

#### 1920px (Desktop HD)
- Maximum container width
- Optimal reading width
- Full feature visibility

---

## Critical UI Components

### Components to Test

| Component | File | Test Priority | Visual Regression |
|-----------|------|---------------|-------------------|
| BottomNavigation | `src/components/BottomNavigation.tsx` | P0 | Yes |
| CoopCard | `src/features/coops/components/CoopCard.tsx` | P0 | Yes |
| FlockCard | `src/features/flocks/components/FlockCard.tsx` | P0 | Yes |
| CreateCoopModal | `src/features/coops/components/CreateCoopModal.tsx` | P0 | Yes |
| EditCoopModal | `src/features/coops/components/EditCoopModal.tsx` | P1 | Yes |
| CreateFlockModal | `src/features/flocks/components/CreateFlockModal.tsx` | P0 | Yes |
| ConfirmationDialog | `src/shared/components/ConfirmationDialog.tsx` | P0 | Yes |
| DashboardWidgets | `src/features/dashboard/components/*.tsx` | P1 | Yes |
| EmptyStates | Various | P1 | Yes |
| LoadingSkeletons | Various | P2 | Yes |

### Touch Target Requirements

Per Material Design and iOS HIG guidelines:

| Element | Minimum Size | Recommended |
|---------|-------------|-------------|
| FAB Button | 56x56px | 56x56px |
| Icon Buttons | 44x44px | 48x48px |
| Nav Items | 44px height | 56px height |
| Menu Items | 48px height | 48px height |
| Form Inputs | 44px height | 56px height |

---

## Critical User Flows

### Flow 1: Dashboard to Coops Navigation (P0)

```
1. Land on Dashboard
2. View statistics widgets (responsive grid)
3. Click "Coops" in bottom navigation
4. Verify Coops page loads
5. Verify cards display correctly
```

**Test across:** All breakpoints, all browsers

### Flow 2: Create Coop (P0)

```
1. Navigate to Coops page
2. Click FAB button (touch target verification)
3. Modal opens (fullscreen on mobile)
4. Fill form (keyboard interaction)
5. Submit and verify creation
6. Modal closes, card appears
```

**Test across:** All devices, all browsers

### Flow 3: Manage Flock (P0)

```
1. Navigate to Coop detail
2. Click "Flocks" section
3. Create new flock via modal
4. Edit flock (pre-filled form)
5. Archive flock (confirmation dialog)
6. Filter flocks (toggle buttons)
```

**Test across:** Mobile devices, all breakpoints

### Flow 4: Language Switch (P1)

```
1. Navigate to Settings
2. Open language dropdown
3. Select English/Czech
4. Verify UI updates
5. Navigate to other pages
6. Confirm language persists
```

**Test across:** All browsers (localStorage test)

### Flow 5: Offline Indicator (P2)

```
1. Load application online
2. Simulate offline mode
3. Verify offline indicator appears
4. Attempt actions (queue behavior)
5. Go back online
6. Verify sync behavior
```

**Test across:** Chrome, Safari (PWA-focused)

---

## Implementation Details

### Playwright Configuration

**File:** `frontend/playwright.crossbrowser.config.ts`

Key configuration:
- 16 browser/device projects
- Screenshot capture for all tests
- HTML + JSON + JUnit reporters
- Trace on failure
- Video on failure

### Test Files Structure

```
frontend/e2e/
├── crossbrowser/
│   ├── visual-regression.crossbrowser.spec.ts    # Screenshot tests
│   ├── responsive-layout.crossbrowser.spec.ts    # Layout tests
│   └── browser-compatibility.crossbrowser.spec.ts # Compat tests
├── utils/
│   └── screenshot-reporter.ts                     # Reporting utilities
└── pages/                                         # Page Object Models
```

### Running Tests

```bash
# Run all cross-browser tests
npm run test:crossbrowser

# Run specific browser
npm run test:crossbrowser -- --project=chrome-desktop

# Run specific breakpoint
npm run test:crossbrowser -- --project=breakpoint-320

# Run with UI mode for debugging
npm run test:crossbrowser:ui

# Update visual snapshots
npm run test:crossbrowser:update
```

---

## Screenshot & Reporting

### Screenshot Organization

```
test-results/
├── crossbrowser/
│   ├── results.json                    # Test results
│   └── junit.xml                       # CI integration
├── screenshots/
│   ├── baseline/                       # Reference screenshots
│   ├── current/                        # Latest run screenshots
│   ├── diff/                           # Visual differences
│   └── reports/
│       ├── report.html                 # Visual report
│       ├── report.json                 # Machine-readable
│       └── ci-summary.json             # CI status
└── playwright-report/
    └── crossbrowser/                   # Playwright HTML report
```

### Report Contents

The HTML report includes:
- Total screenshot count
- Browser distribution
- Device distribution
- Breakpoint coverage
- Filterable screenshot gallery
- Visual diff viewer

### Baseline Management

1. **Initial baseline:** Run tests with `--update-snapshots`
2. **CI comparison:** Baseline stored in git
3. **Update process:** Review diffs, approve changes
4. **Failure handling:** Generate diff images for review

---

## CI/CD Integration

### GitHub Actions Workflow

```yaml
name: Cross-Browser Tests

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
  schedule:
    - cron: '0 6 * * 1'  # Weekly Monday 6 AM

jobs:
  crossbrowser-tests:
    runs-on: ubuntu-latest
    strategy:
      fail-fast: false
      matrix:
        browser: [chromium, firefox, webkit]

    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci
        working-directory: frontend

      - name: Install Playwright browsers
        run: npx playwright install --with-deps ${{ matrix.browser }}
        working-directory: frontend

      - name: Run cross-browser tests
        run: npm run test:crossbrowser -- --project="${{ matrix.browser }}*"
        working-directory: frontend

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: crossbrowser-results-${{ matrix.browser }}
          path: |
            frontend/playwright-report/
            frontend/test-results/
            frontend/screenshots/
          retention-days: 30

      - name: Upload screenshots
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: failure-screenshots-${{ matrix.browser }}
          path: frontend/test-results/crossbrowser/
          retention-days: 7
```

### Quality Gates

| Metric | Threshold | Action on Failure |
|--------|-----------|-------------------|
| Test Pass Rate | 100% | Block merge |
| Visual Diff | <0.2% | Review required |
| Screenshot Count | All captured | Warning |
| Report Generation | Success | Block merge |

---

## Execution Plan

### Phase 1: Setup (Day 1)

1. [x] Create Playwright cross-browser config
2. [x] Implement screenshot reporter utility
3. [x] Create visual regression tests
4. [x] Create responsive layout tests
5. [x] Create browser compatibility tests
6. [ ] Add npm scripts to package.json
7. [ ] Generate initial baseline screenshots

### Phase 2: Baseline (Day 2)

1. [ ] Run tests on all browsers
2. [ ] Review baseline screenshots
3. [ ] Fix any layout issues found
4. [ ] Commit baseline to repository
5. [ ] Document known differences

### Phase 3: CI Integration (Day 3)

1. [ ] Create GitHub Actions workflow
2. [ ] Configure artifact upload
3. [ ] Set up quality gates
4. [ ] Test PR workflow
5. [ ] Document CI process

### Phase 4: Maintenance (Ongoing)

1. [ ] Weekly scheduled runs
2. [ ] Baseline updates on UI changes
3. [ ] New browser version validation
4. [ ] Report review process

---

## Appendix A: Test Execution Commands

```bash
# Install Playwright browsers
cd frontend
npx playwright install --with-deps

# Run all cross-browser tests
npm run test:crossbrowser

# Run with specific browser
npx playwright test --config=playwright.crossbrowser.config.ts --project=chrome-desktop

# Run only visual regression tests
npx playwright test --config=playwright.crossbrowser.config.ts visual-regression

# Run only responsive tests
npx playwright test --config=playwright.crossbrowser.config.ts responsive-layout

# Run breakpoint-specific tests
npx playwright test --config=playwright.crossbrowser.config.ts --project=breakpoint-320

# Update snapshots after intentional UI changes
npx playwright test --config=playwright.crossbrowser.config.ts --update-snapshots

# Debug mode with UI
npx playwright test --config=playwright.crossbrowser.config.ts --ui

# Generate report
npx playwright show-report playwright-report/crossbrowser
```

---

## Appendix B: Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Flaky screenshots | Animations | Use `animations: 'disabled'` |
| Font differences | System fonts | Use web fonts |
| Timeout on mobile | Slow emulation | Increase timeout |
| Missing elements | Race conditions | Add proper waits |
| Color differences | Color profiles | Use consistent profile |

### Debug Checklist

1. Check test artifacts in `test-results/`
2. Review Playwright trace files
3. Compare baseline vs current screenshots
4. Check browser console for errors
5. Verify authentication state
6. Test manually on target device

---

## Appendix C: Browser-Specific Notes

### Chrome
- Uses Chromium engine bundled with Playwright
- Best cross-platform support
- DevTools protocol for debugging

### Firefox
- Uses Firefox engine bundled with Playwright
- May have minor rendering differences
- Good for accessibility testing

### Safari/WebKit
- Uses WebKit engine bundled with Playwright
- Closest to actual Safari behavior
- Required for iOS testing

### Mobile Safari (iOS)
- Emulated via WebKit
- Touch events supported
- Viewport meta tag honored

### Mobile Chrome (Android)
- Emulated via Chromium
- Touch events supported
- Android-specific viewport handling

---

**Document Prepared By:** Claude Assistant
**Review Status:** Ready for Implementation
**Next Review:** After Phase 2 Completion
