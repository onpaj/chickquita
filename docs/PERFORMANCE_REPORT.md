# Performance Testing & Optimization Report

> **📊 CURRENT PERFORMANCE BASELINE REPORT**
> **Report Date:** 2026-02-09 (Updated)
> **Status:** ✅ Current (Includes Purchases feature, M1-M8 implementation complete)
>
> **Note:** Bundle analysis reflects codebase state including Purchases feature (US-033 to US-043) and all features through Milestone 8 (Chick Maturation).
> **Last Generated:** 2026-02-09

**Story:** TESTING-003 - Performance Testing & Optimization
**Environment:** Production Build (Vite 7.3.1)

## Executive Summary

This report documents the performance testing and optimization efforts for the Chickquita PWA. The application underwent bundle size analysis and code-splitting optimizations to improve caching and load performance.

### Performance Status

| Metric | Target | Current Status | Result |
|--------|--------|----------------|--------|
| Bundle Size (gzipped) | < 200 KB | ~324 KB | ⚠️ **EXCEEDS TARGET** |
| First Contentful Paint | < 1.5s | Unable to measure* | ⏸️ **BLOCKED** |
| Time to Interactive | < 3.5s | Unable to measure* | ⏸️ **BLOCKED** |
| Largest Contentful Paint | < 2.5s | Unable to measure* | ⏸️ **BLOCKED** |
| Cumulative Layout Shift | < 0.1 | 0 (dev test) | ✅ **PASS** |
| Lighthouse Score | > 90 | Unable to measure* | ⏸️ **BLOCKED** |

*Runtime metrics could not be measured due to Clerk authentication redirect preventing Lighthouse from capturing FCP. Further testing requires authenticated test user setup or auth bypass for performance testing.

---

## Bundle Size Analysis

### Before Optimization
**Single Bundle Approach:**
- `index.js`: 900.63 KB (271.83 KB gzipped)
- ⚠️ **Issue:** All dependencies bundled together, poor caching strategy

### After Optimization (Updated 2026-02-09)
**Code-Splitting Strategy Implemented:**

| Chunk | Raw Size | Gzipped | Description |
|-------|----------|---------|-------------|
| react-vendor | 47.17 KB | 16.72 KB | React, ReactDOM, React Router |
| mui-vendor | 326.09 KB | 101.62 KB | Material-UI components |
| clerk-vendor | 80.07 KB | 20.86 KB | Clerk authentication |
| query-vendor | 35.82 KB | 10.71 KB | TanStack Query |
| i18n-vendor | 54.49 KB | 17.71 KB | i18next localization |
| form-vendor | 91.33 KB | 27.20 KB | React Hook Form + Zod |
| charts-vendor | 0.09 KB | 0.10 KB | Recharts |
| index (app code) | 444.45 KB | 128.62 KB | Application code |
| index.css | 0.91 KB | 0.49 KB | Styles |
| **TOTAL** | **1,080 KB** | **323.54 KB** | (excluding HTML) |

### Bundle Size Increase
- Baseline (Feb 7, before Purchases): ~273.73 KB gzipped
- Current (Feb 9, with Purchases & M1-M8): ~323.54 KB gzipped (CSS + JS chunks)
- **Increase: 49.81 KB (18.2%)** ⚠️ **SIGNIFICANT INCREASE** from Purchases feature implementation

---

## Optimizations Implemented

### 1. Manual Code-Splitting (vite.config.ts)

Implemented strategic chunk splitting to improve browser caching:

```typescript
rollupOptions: {
  output: {
    manualChunks: {
      'react-vendor': ['react', 'react-dom', 'react-router-dom'],
      'mui-vendor': ['@mui/material', '@mui/icons-material', '@emotion/react', '@emotion/styled'],
      'clerk-vendor': ['@clerk/clerk-react'],
      'query-vendor': ['@tanstack/react-query'],
      'form-vendor': ['react-hook-form', '@hookform/resolvers', 'zod'],
      'charts-vendor': ['recharts'],
      'i18n-vendor': ['i18next', 'react-i18next', 'i18next-browser-languagedetector'],
    },
  },
}
```

**Benefits:**
- ✅ Vendor libraries cached separately from app code
- ✅ React ecosystem (16.72 KB) loads first, can be cached long-term
- ✅ Material-UI (101.62 KB) can be cached independently
- ✅ Application updates only invalidate 128.62 KB main chunk

### 2. Chunk Size Warning Threshold
- Increased `chunkSizeWarningLimit` to 600 KB to suppress false warnings for legitimate vendor chunks

---

## Lighthouse Testing Results

### Development Server Test (localhost:3100)
**Performance Score:** 25/100 ❌

This test ran against the Vite dev server with development mode enabled, which explains the extremely poor metrics:

| Metric | Value | Status |
|--------|-------|--------|
| First Contentful Paint | 45.5s | ❌ FAIL |
| Largest Contentful Paint | 90.8s | ❌ FAIL |
| Time to Interactive | 90.8s | ❌ FAIL |
| Total Blocking Time | 4.3s | ❌ FAIL |
| Cumulative Layout Shift | 0 | ✅ PASS |

**Root Cause:** Development mode with React DevTools, HMR overhead, and unminified bundles.

### Production Server Test (localhost:4173)
**Status:** ⏸️ **BLOCKED** - NO_FCP Error

The Lighthouse test against the production preview server failed with `NO_FCP` (No First Contentful Paint) error.

**Root Cause Analysis:**
- Application redirects to Clerk authentication immediately
- Lighthouse cannot capture metrics on auth redirect
- No content painted before redirect occurs

**Recommendation:** Set up authenticated Lighthouse CI with saved auth state or create a performance testing route that bypasses authentication.

---

## Identified Performance Issues

### 1. Bundle Size Exceeds Target (⚠️ High Priority)

**Issue:** Total gzipped bundle is 323.54 KB, exceeding the 200 KB target by 62%

**Impact:**
- Slower initial load on slow 3G connections
- Higher data costs for mobile users in rural areas

**Potential Optimizations:**
1. **Tree-shaking analysis** - Identify unused MUI components
2. **Lazy loading** - Code-split routes with React.lazy()
3. **MUI optimization** - Use individual component imports instead of barrel imports
4. **Remove unused dependencies** - Audit package.json for unused packages
5. **Font optimization** - Use system fonts or subset custom fonts

**Estimated Savings:** 80-120 KB gzipped (bringing total to ~200-240 KB)

**Bundle Growth Analysis:**
- MUI vendor chunk: +9.39 KB (from 92.23 KB → 101.62 KB) - More MUI components used
- Form vendor chunk: +27.10 KB (from 0.10 KB → 27.20 KB) - Significant form handling in Purchases feature
- App code (index): +13.32 KB (from 115.30 KB → 128.62 KB) - Purchases feature pages, components, API client
- **Total Growth:** +49.81 KB from Purchases feature implementation (M5)

### 2. No Runtime Performance Metrics (🔒 Blocked)

**Issue:** Cannot measure FCP, LCP, TTI, or Lighthouse score due to auth redirect

**Impact:**
- Unknown actual user experience metrics
- Cannot verify performance targets
- No baseline for future optimization

**Solutions:**
1. **Lighthouse CI with Auth** - Save Clerk session cookies and pass to Lighthouse
2. **Performance Test Route** - Create `/perf-test` route that bypasses auth
3. **Real User Monitoring (RUM)** - Implement Application Insights or similar
4. **Synthetic Monitoring** - Set up Playwright tests that measure Core Web Vitals

**Recommendation:** Implement RUM in production to capture real user metrics.

### 3. Unused JavaScript in Vendor Bundles

From Lighthouse development test analysis:
- MUI bundle contains ~53% unused code (Autocomplete, Tabs, Slider, Tooltip)
- React Router contains ~88% unused code in development bundle

**Optimization:** Switch to named imports and verify tree-shaking:

```typescript
// Bad
import { Button } from '@mui/material';

// Good
import Button from '@mui/material/Button';
```

---

## Core Web Vitals Baseline

### Measured Metrics (Development Server)
These metrics are from the development server and **not representative** of production performance:

- **FCP:** 45.5s (Target: < 1.5s) ❌
- **LCP:** 90.8s (Target: < 2.5s) ❌
- **TTI:** 90.8s (Target: < 3.5s) ❌
- **CLS:** 0 (Target: < 0.1) ✅
- **TBT:** 4.3s ❌

### Estimated Production Metrics
Based on bundle size and typical PWA performance:

- **FCP:** ~1.2-1.8s (Mobile 4G) ⚠️ *Borderline*
- **LCP:** ~2.0-3.0s (Mobile 4G) ⚠️ *May exceed target*
- **TTI:** ~2.5-4.0s (Mobile 4G) ⚠️ *May exceed target*
- **CLS:** 0 ✅ *Expected to pass*

**Note:** These are estimates. Real metrics require authenticated performance testing or RUM implementation.

---

## Recommendations

### Immediate Actions (Required for MVP)

1. **✅ COMPLETED - Code Splitting**
   - Implemented manual chunk splitting for better caching
   - Vendor libraries separated from application code

2. **🔜 Implement RUM (Real User Monitoring)**
   - Use Application Insights or similar to track actual user metrics
   - Monitor FCP, LCP, TTI, CLS in production
   - Set up alerts for performance regression

3. **🔜 Set Up Authenticated Lighthouse CI**
   - Configure Lighthouse to run with saved Clerk session
   - Add to GitHub Actions pipeline
   - Track performance metrics over time

### Medium-Term Optimizations (Phase 2)

4. **Route-Based Code Splitting**
   - Implement React.lazy() for major routes
   - Estimated savings: 30-50 KB initial bundle

5. **MUI Tree-Shaking Verification**
   - Audit and convert to direct component imports
   - Remove unused MUI components
   - Estimated savings: 20-30 KB

6. **Image Optimization**
   - Implement lazy loading for images
   - Use modern formats (WebP, AVIF)
   - Responsive images with srcset

7. **Font Optimization**
   - Subset Roboto font to Czech + English characters only
   - Use font-display: swap
   - Estimated savings: 5-10 KB

### Long-Term Enhancements (Phase 3)

8. **Service Worker Caching Strategy**
   - Implement Workbox with proper caching rules
   - Cache-first for vendor chunks
   - Network-first for app code

9. **Performance Budget Enforcement**
   - Add bundle size checks to CI/CD
   - Fail builds that exceed 200 KB gzipped
   - Monitor with bundlesize.io or similar

10. **Progressive Enhancement**
    - Implement skeleton screens for perceived performance
    - Optimize TTI with code streaming
    - Defer non-critical JavaScript

---

## Technical Debt & Blockers

### 1. Authentication Blocks Performance Testing
**Severity:** High
**Impact:** Cannot measure or verify performance targets
**Recommended Solution:** Implement dedicated performance testing route or Lighthouse CI with authentication

### 2. No Real User Monitoring
**Severity:** Medium
**Impact:** No visibility into actual user experience
**Recommended Solution:** Implement Application Insights or similar RUM solution

### 3. Bundle Size Exceeds Target
**Severity:** Medium
**Impact:** Slower load times on slow connections
**Recommended Solution:** Implement route-based code splitting and MUI optimization

---

## Conclusion

### Summary of Changes
- ✅ Implemented manual code-splitting for vendor libraries
- ✅ Optimized caching strategy with separate vendor chunks
- ⚠️ Bundle size increased 18.2% from Purchases feature (49.81 KB)
- ⚠️ Total bundle size exceeds 200 KB target (323.54 KB, 62% over target)
- ⏸️ Runtime metrics blocked by authentication redirect

### Acceptance Criteria Status

| Criteria | Status | Notes |
|----------|--------|-------|
| Bundle size increase < 10% | ❌ FAIL | 18.2% increase from Purchases feature |
| Bundle size < 200 KB | ❌ FAIL | 323.54 KB (62% over target) |
| First Contentful Paint < 1.5s | ⏸️ BLOCKED | Unable to measure (auth redirect) |
| Time to Interactive < 3.5s | ⏸️ BLOCKED | Unable to measure (auth redirect) |
| Largest Contentful Paint < 2.5s | ⏸️ BLOCKED | Unable to measure (auth redirect) |
| Cumulative Layout Shift < 0.1 | ✅ PASS | CLS = 0 |
| Lighthouse Score > 90 | ⏸️ BLOCKED | Unable to measure (auth redirect) |
| Performance metrics report | ✅ COMPLETE | This document |

### Next Steps

1. **Implement RUM** to capture real production metrics
2. **Set up authenticated Lighthouse CI** for automated testing
3. **Implement route-based code splitting** to reduce initial bundle size
4. **Monitor performance** in production and iterate based on real user data

### Risk Assessment

**Low Risk:**
- Code-splitting implementation is stable and provides better caching
- No breaking changes to application functionality
- Cumulative Layout Shift remains at 0

**Medium Risk:**
- Total bundle size may impact users on slow connections
- Unknown runtime performance without measurement
- Estimated metrics may not reflect real-world usage

**Mitigation:**
- Implement RUM immediately after deployment
- Set up performance alerts in Application Insights
- Plan optimization sprint if metrics fall below targets

---

## Appendix

### Test Environment
- **Node Version:** 24.10.1
- **npm Version:** 11.2.0
- **Vite Version:** 7.3.1
- **Build Tool:** Rollup (via Vite)
- **Browser:** Chrome Headless (Lighthouse)

### Build Output (Updated 2026-02-09)
```
dist/index.html                          0.95 kB │ gzip:   0.40 kB
dist/assets/index-DQ3P1g1z.css           0.91 kB │ gzip:   0.49 kB
dist/assets/charts-vendor-B-FRIktE.js    0.09 kB │ gzip:   0.10 kB
dist/assets/query-vendor-BVAGMksN.js    35.82 kB │ gzip:  10.71 kB
dist/assets/react-vendor-fzd2TgEV.js    47.17 kB │ gzip:  16.72 kB
dist/assets/i18n-vendor-DWwMIbEQ.js     54.49 kB │ gzip:  17.71 kB
dist/assets/clerk-vendor-BddxYGWf.js    80.07 kB │ gzip:  20.86 kB
dist/assets/form-vendor-B49rqemV.js     91.33 kB │ gzip:  27.20 kB
dist/assets/mui-vendor-DSCFRh-2.js     326.09 kB │ gzip: 101.62 kB
dist/assets/index-BYthgvG5.js          444.45 kB │ gzip: 128.62 kB
```

**Notable Changes Since Feb 7:**
- form-vendor: 0.09 KB → 91.33 KB (+91.24 KB raw, +27.10 KB gzipped)
- mui-vendor: 298.25 KB → 326.09 KB (+27.84 KB raw, +9.39 KB gzipped)
- index (app): 384.49 KB → 444.45 KB (+59.96 KB raw, +13.32 KB gzipped)

### References
- [Vite Build Optimization Guide](https://vitejs.dev/guide/build.html)
- [Web Vitals](https://web.dev/vitals/)
- [Lighthouse CI Documentation](https://github.com/GoogleChrome/lighthouse-ci)
- [Material-UI Tree Shaking](https://mui.com/material-ui/guides/minimizing-bundle-size/)
