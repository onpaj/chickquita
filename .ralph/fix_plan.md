# Ralph Fix Plan

## High Priority - Core MVP Features (M9-M12)

### M9: Flock History View ✅ COMPLETED
- [x] Create FlockHistory timeline component (Material-UI Timeline)
  - [x] Vertical timeline layout
  - [x] Change type icons (initial, adjustment, maturation)
  - [x] Delta displays with +/- color coding
  - [x] Expandable notes section (inline editing)
- [x] Add API endpoint: GET /api/flocks/{id}/history
  - [x] Query handler: GetFlockHistoryQuery
  - [x] Return all flock_history records sorted by change_date DESC
- [x] Add flock history page route
  - [x] Accessible from flock detail page (View History button)
  - [x] Route: /coops/:coopId/flocks/:flockId/history
- [x] Implement edit notes functionality
  - [x] Inline edit component in timeline
  - [x] API endpoint: PATCH /api/flock-history/{id}/notes
  - [x] Optimistic updates with TanStack Query
- [ ] Add tests for flock history (optional - defer to later)
  - [ ] API integration tests
  - [ ] E2E tests for timeline rendering

### M10: Offline Egg Recording ✅ COMPLETED
- [x] Configure Workbox service worker
  - [x] Cache-first strategy for static assets (30 days)
  - [x] Network-first for API GET requests (5 min cache)
  - [x] Background sync queue for POST requests (via apiClient)
- [x] Implement IndexedDB schema (Dexie.js)
  - [x] pendingRequests table (offline queue with retry tracking)
  - [x] syncStatus metadata table
  - [ ] Cached data tables (coops, flocks, dailyRecords) - deferred (not required for MVP)
- [x] Add offline detection UI
  - [x] Persistent offline banner component (OfflineBanner)
  - [x] Sync status indicator (pending count)
  - [x] Manual sync button
- [x] Implement background sync logic
  - [x] Retry logic with exponential backoff (2^n * 1000ms, max 5 retries)
  - [x] Auto-sync on online event + periodic sync (5 min)
  - [x] 24-hour staleness check for requests
- [ ] Add offline mode tests (optional - defer to later)
  - [ ] Service worker registration
  - [ ] IndexedDB CRUD operations
  - [ ] Background sync queue processing

### M11: Statistics Dashboard
- [ ] Create statistics page with chart grid
  - [ ] Date range filters (7/30/90 days, custom)
  - [ ] Flock filter (all / specific flock)
- [ ] Implement egg cost breakdown chart
  - [ ] Pie chart by purchase type (Recharts)
  - [ ] Percentage labels and legends
- [ ] Implement production trend chart
  - [ ] Line chart for last 30 days
  - [ ] Tooltips with daily totals
- [ ] Implement cost trend chart
  - [ ] Line chart: cost per egg over time
  - [ ] Trend indicators (↑↓)
- [ ] Implement flock productivity comparison
  - [ ] Bar chart: eggs/hen/day per flock
  - [ ] Sort by productivity
- [ ] Add API endpoint: GET /api/statistics/egg-cost
  - [ ] Cost breakdown calculation (group by purchase type)
  - [ ] Timeline aggregation (by week/month)
  - [ ] Flock productivity calculation
- [ ] Mobile optimization for charts
  - [ ] Touch-friendly tooltips
  - [ ] Responsive chart sizing
  - [ ] Loading skeletons
- [ ] Add statistics tests
  - [ ] API calculation accuracy
  - [ ] Chart rendering tests

### M12: PWA Installation
- [ ] Configure manifest.json
  - [ ] App name (Czech + English)
  - [ ] Icons (72x72, 192x192, 512x512)
  - [ ] Theme color (#FF6B35)
  - [ ] Display mode: standalone
  - [ ] Screenshots for app stores
- [ ] Generate app icons
  - [ ] Multiple sizes (maskable + any purpose)
  - [ ] Favicon variants
- [ ] Implement install prompt handler
  - [ ] Detect beforeinstallprompt event
  - [ ] Show after 2nd visit or 5 min usage
  - [ ] Custom install banner component
- [ ] Add iOS "Add to Home Screen" instructions
  - [ ] Detect iOS Safari
  - [ ] Step-by-step guide modal
  - [ ] Screenshots with arrows
- [ ] Configure splash screen
  - [ ] Background: theme_color
  - [ ] Logo: centered
  - [ ] Fade-in animation (300ms)
- [ ] Lighthouse PWA audit
  - [ ] Target score >90
  - [ ] Fix any PWA-related issues

## Medium Priority - Performance & UX

### Performance Optimization
- [ ] Bundle size analysis
  - [ ] webpack-bundle-analyzer report
  - [ ] Code splitting optimization
  - [ ] Lazy loading for routes
- [ ] Image optimization
  - [ ] Convert to WebP format
  - [ ] Lazy loading for images
  - [ ] Responsive image srcset
- [ ] API response optimization
  - [ ] Database query analysis (EF Core)
  - [ ] Indexing strategy review
  - [ ] Response compression (gzip)
- [ ] Lighthouse audit improvements
  - [ ] Target >90 all categories
  - [ ] Performance budget enforcement

### UX Enhancements
- [ ] Loading states consistency
  - [ ] Skeleton components for all major views
  - [ ] Optimistic updates for mutations
- [ ] Error handling improvements
  - [ ] Global error boundary (React)
  - [ ] User-friendly error messages (i18n)
  - [ ] Retry mechanisms for failed requests
- [ ] Accessibility audit
  - [ ] ARIA labels for interactive elements
  - [ ] Keyboard navigation support
  - [ ] Screen reader testing
- [ ] Touch target audit
  - [ ] Ensure 48x48px minimum size
  - [ ] 8px spacing between targets

## Low Priority - Nice-to-Have

### Documentation
- [ ] API documentation (OpenAPI/Swagger)
- [ ] Component Storybook setup
- [ ] User guide (Czech + English)
- [ ] Developer onboarding guide

### Developer Experience
- [ ] Pre-commit hooks (lint, type-check)
- [ ] Automatic code formatting (Prettier)
- [ ] VSCode workspace settings
- [ ] Debug configurations

### Monitoring & Logging
- [ ] Application Insights integration
- [ ] Custom event tracking
  - [ ] User registration (via webhook)
  - [ ] Daily record created
  - [ ] Chick maturation
  - [ ] Offline sync completed
- [ ] Error rate alerts
- [ ] Performance monitoring dashboard

## Future Enhancements (Post-MVP)

### High Priority Next Wave
- [ ] Push notifications
  - [ ] Daily reminder (19:00): "Log eggs"
  - [ ] Sync completed notification
  - [ ] Chicks ready to mature alert
- [ ] Dark mode
  - [ ] Theme toggle in settings
  - [ ] Persist preference
  - [ ] Material-UI theme switching
- [ ] Swipe gestures
  - [ ] Swipe to delete (with undo)
  - [ ] Swipe to archive
  - [ ] Pull to refresh

### Medium Priority Future
- [ ] Individual chicken tracking
  - [ ] CRUD individual chickens
  - [ ] Link to flock
  - [ ] Notes and photos
- [ ] Photo uploads
  - [ ] Chicken photos
  - [ ] Coop photos
  - [ ] Receipt photos (Azure Blob Storage)
- [ ] CSV/PDF exports
  - [ ] Export daily records
  - [ ] Export purchases
  - [ ] Generate PDF reports

### Low Priority Future
- [ ] Voice input for notes
- [ ] Calendar view for daily records
- [ ] Social logins (Google, Facebook)
- [ ] Multi-factor authentication (Clerk Pro)

## Completed

- [x] M1: User Authentication (Clerk integration, JWT auth, tenant creation)
- [x] M2: Coop Management (CRUD operations, archive, tenant isolation)
- [x] M3: Basic Flock Creation (CRUD, initial composition, history tracking)
- [x] M4: Daily Egg Records (Quick-add flow, validation, same-day edit rule)
- [x] M5: Purchase Tracking (Full CRUD, type filtering, autocomplete)
- [x] M6: Egg Cost Calculation Dashboard (Dashboard widgets, statistics, trends)
- [x] M7: Flock Composition Editing (Adjustment flow, delta display, confirmation)
- [x] M8: Chick Maturation (Maturation form, validation, history records)
- [x] Project initialization (React + .NET setup, Docker, CI/CD)
- [x] Database schema with RLS policies (Neon Postgres)
- [x] Multi-tenant architecture implementation
- [x] Clerk authentication integration (frontend + backend)
- [x] Material-UI component library setup
- [x] TanStack Query + Zustand state management
- [x] react-i18next internationalization (Czech + English)
- [x] Shared component library (NumericStepper, IllustratedEmptyState, etc.)
- [x] Mobile-first responsive design
- [x] Form validation (React Hook Form + Zod)
- [x] E2E test infrastructure (Playwright)

## Notes

### Implementation Progress (as of 2026-02-09)
- **67% MVP Complete:** M1-M8 implemented and tested
- **33% Remaining:** M9-M12 pending (UI enhancements + PWA features)
- **Next Priority:** Focus on M9 (Flock History View) as it provides immediate value for users tracking flock changes

### Key Learnings
1. **Multi-tenancy:** RLS policies at database level + EF Core global filters provide defense-in-depth
2. **Offline-first:** Service worker configuration requires careful cache strategy planning
3. **Mobile-first:** Touch targets must be 48x48px minimum for usability
4. **Performance:** Bundle size monitoring essential - lazy loading routes reduces initial load
5. **Testing:** E2E tests with Playwright require proper Clerk authentication setup
6. **Chick logic:** Remember - chicks count in costs but NOT in egg production calculations

### Technical Debt
- [ ] Database indexing strategy review (performance optimization)
- [ ] API response caching strategy (reduce database load)
- [ ] Frontend bundle size optimization (currently within budget but could improve)
- [ ] Comprehensive E2E test coverage (currently covers happy paths only)
- [ ] Error boundary implementation (catch React rendering errors)

### Security Checklist
- [x] Row-Level Security (RLS) policies enforced
- [x] Clerk JWT validation on all protected endpoints
- [x] Input validation (frontend: Zod, backend: FluentValidation)
- [x] SQL injection prevention (EF Core parameterized queries)
- [ ] XSS protection (sanitize user inputs with DOMPurify)
- [ ] HTTPS enforcement in production
- [ ] CORS whitelist configuration
- [ ] Rate limiting on API endpoints
