# Product Requirements Document: ChickenTrack PWA

**Version:** 2.0
**Date:** February 5, 2026
**Author:** Ondřej (Ondra)
**Status:** Approved

---

## Executive Summary

ChickenTrack is a PWA application for tracking the financial profitability of chicken farming with multi-tenant architecture. The application is designed with a **mobile-first** approach, enabling farmers to efficiently record costs, production, and calculate economic efficiency directly at the chicken coops with offline mode support.

### Key Values
- 📱 **Mobile-first PWA** - optimized for mobile use outdoors
- 💰 **Financial transparency** - precise egg cost calculation
- 📊 **Data-driven decisions** - statistics and trends
- 🔒 **Multi-tenant** - data isolation between farmers
- 📴 **Offline-first** - works without connection

---

## 1. Product Goals

### Primary Goals
1. **Mobile-first PWA approach** - application primarily designed for mobile use outdoors at chicken coops
2. Enable farmers to accurately track breeding costs
3. Calculate the true cost of one egg including all expenses
4. Record historical flock development and production
5. Provide data for economic sustainability decisions

### Success Metrics
- Daily egg production logging > 90% of days
- Accurate recording of all costs
- User retention rate 30+ days
- Offline usage > 40% of all interactions
- Lighthouse score > 90 (all categories)

### Use Case Scenario (typical usage)
```
07:00 - Farmer goes to chicken coops
      → Opens PWA on mobile (instant load from cache)
      → Quick action: "Add daily record"
      → Selects flock from dropdown
      → Enters egg count
      → Save (works offline)
      → Data syncs when returning home
```

---

## 2. User Personas

### Persona 1: Hobby Breeder (Primary)
**Profile:**
- Age: 35-55 years
- Has 1-2 coops, total 5-20 chickens
- Breeding as hobby, partial self-sufficiency
- Technically proficient (smartphone daily)

**Motivation:**
- Wants to know if breeding is profitable
- Interested in economic aspects
- Needs simple tracking without unnecessary complexity

**Pain Points:**
- Doesn't know exactly how much one egg costs
- Forgets to record production
- Doesn't know when to replace flock

**Goals:**
- Quickly enter daily production (< 30 seconds)
- See economics at a glance
- Minimal time on administration

### Persona 2: Semi-professional Breeder (Secondary)
**Profile:**
- Age: 40-65 years
- Multiple coops, larger flocks (50+ chickens)
- Breeding as side income
- Actively optimizes costs

**Motivation:**
- Needs detailed statistics and trends
- Compares flock productivity
- Plans expansion or reduction

**Pain Points:**
- Excel spreadsheets are messy
- Data loss during computer failure
- Difficult sharing with family

**Goals:**
- See productivity by flock
- Compare different feeds
- Export data for accounting

---

## 3. Functional Requirements

### 3.1 Authentication & Multi-tenancy

#### Technology: Clerk.com

ChickenTrack uses **Clerk** for authentication management, providing:
- Secure, battle-tested authentication
- Pre-built UI components
- Email + password authentication (MVP)
- Future: Social logins (Google, Facebook)
- Session management (7 days default)
- Email verification
- Password reset flows

#### Registration
- **Clerk hosted sign-up UI**
  - Email validation
  - Password: min 8 characters, 1 uppercase, 1 number
- **Automatic tenant creation** - each user gets their own tenant
- Welcome email (optional - Phase 2)

#### Sign-in
- Clerk hosted sign-in UI
- **Session persistence: 7 days** (Clerk default, configurable)
- "Forgot password" flow:
  - Reset link via email
  - Valid for 24 hours
  - New password

#### Security
- JWT token-based authentication (managed by Clerk)
- Automatic token refresh (Clerk SDK handles)
- Data isolation between tenants (Postgres RLS)
- Rate limiting on auth endpoints (Clerk managed)
- HTTPS only

#### Backend Integration
- Clerk JWT validation in .NET API
- Webhook for user sync (user.created event)
- Tenant record creation when user signs up
- `ClerkUserId` → `TenantId` mapping in database

---

### 3.2 Chicken Management (Hierarchy)

#### 3.2.1 Coop

**Attributes:**
- Name (required, max 100 characters)
- Location (text, optional, max 200 characters)
- Created date (automatic)
- Status: Active / Archived

**Operations:**
- Create coop
- Edit coop (name, location)
- Archive coop (soft delete)
- Restore archived coop
- Delete coop (only if no flocks)

**Business Rules:**
- Each tenant can have unlimited coops
- Name must be unique within tenant
- Archived coop doesn't show in lists

#### 3.2.2 Flock

**Attributes:**
- Reference ID/identifier (required, max 50 characters, e.g., "Browns 2024")
- Hatch date (required)
- Coop (reference, required)
- **Initial composition:**
  - Hen count (required, >= 0)
  - Rooster count (required, >= 0)
  - **Chick count (required, >= 0)**
- Current composition (calculated from history)
- Created date
- Status: Active / Archived

**Operations:**
- Create flock
- Edit basic info (name, hatch date)
- Archive flock
- View change history
- **Convert chicks to adults**
- Manual composition adjustment

**Business Rules:**
- Flock must belong to active coop
- Identifier must be unique within coop
- Archiving flock also archives daily records
- At least one category must be > 0

#### 3.2.3 Flock History

**Attributes:**
- Change date (required)
- Hen count (required)
- Rooster count (required)
- **Chick count (required)**
- Change type (enum):
  - `adjustment` - manual adjustment (death, sale, purchase)
  - `maturation` - chick conversion to adults
- Notes (optional, max 500 characters)

**Operations:**
- Create history record (automatic on changes)
- View timeline of changes
- Edit notes (only)
- Delete record (only latest)

**Business Rules:**
- History is immutable (except notes)
- First record = initial flock state
- Records sorted chronologically

#### 3.2.4 Action: Chick Maturation

**Input Parameters:**
- Flock (reference)
- Maturation date (required)
- Chick count to convert (required, > 0)
- Resulting split:
  - New hen count (required, >= 0)
  - New rooster count (required, >= 0)
- Notes (optional, max 500 characters)

**Validation:**
- Sum of hens + roosters = chick count to convert
- Chick count to convert <= current chicks in flock
- Maturation date >= hatch date

**Output:**
- New history entry with type `maturation`
- Updated current flock composition:
  - Chicks: -X
  - Hens: +Y
  - Roosters: +Z

**Example:**
```
Before conversion:
- Chicks: 20
- Hens: 10
- Roosters: 2

Action: Convert 15 chicks → 12 hens + 3 roosters

After conversion:
- Chicks: 5 (20 - 15)
- Hens: 22 (10 + 12)
- Roosters: 5 (2 + 3)
```

#### 3.2.5 Individual Chickens (optional - Phase 3)

**Attributes:**
- Identifier (name/number, max 50 characters)
- Flock reference
- Date added
- Date departed (optional)
- Notes (optional, max 500 characters)

**Operations:**
- Add chicken to flock
- Mark as departed (death, sale)
- View chicken details

---

### 3.3 Feed & Costs Management

#### 3.3.1 Purchase

**Attributes:**
- Item type (required, dropdown/tags):
  - Feed
  - Vitamins and supplements
  - Bedding
  - Toys and equipment
  - Veterinary care
  - Other
- Item name (required, max 100 characters, autocomplete from history)
- Purchase date (required)
- Price (required, decimal, >= 0)
- Quantity (required, decimal, > 0)
- Unit (required, dropdown):
  - kg
  - pcs
  - l
  - package
  - other
- Consumption date (optional) - for calculating consumption duration
- Notes (optional, max 500 characters)
- Flock/coop reference (optional) - if purchase is specific

**Operations:**
- Create purchase
- Edit purchase
- Delete purchase
- Filter purchases (type, date, flock)
- View purchase history

**Business Rules:**
- Consumption date >= purchase date
- Autocomplete names from previous purchases
- Calculation: price per unit = price / quantity

---

### 3.4 Daily Operations Management

#### 3.4.1 Daily Record

**Attributes:**
- Date (required)
- Flock reference (required)
- Egg count (required, integer, >= 0)
- Flock notes (optional, max 1000 characters)

**Operations:**
- Create daily record (offline-capable)
- Edit daily record (same day only)
- Delete daily record
- View record history
- Quick add from dashboard (modal)

**Business Rules:**
- One record per flock per day
- Cannot create record for future date
- In offline mode: queue to background sync
- After flock composition change: warning about context change

#### 3.4.2 Alert Events (optional - Phase 2)

**Event Types:**
- Illness
- Death
- Decreased activity
- Aggressive behavior
- Other

**Attributes:**
- Date
- Event type
- Flock / individual chicken reference
- Description (max 1000 characters)
- Photo (optional)

---

### 3.5 Statistics & Reporting

#### 3.5.1 Dashboard (Overview)

**Widgets:**
1. **Today:**
   - Total eggs laid
   - Current hen count (total)

2. **This Week:**
   - Total egg production
   - Average production/day
   - Trend (↑↓)

3. **Economics:**
   - Current egg price (total costs / total production)
   - Price trend (↑↓ vs last month)

4. **Flock Status:**
   - Total count: hens / roosters / chicks
   - Active flock count

**Quick Actions:**
- FAB: Add daily record
- Add purchase
- Convert chicks

#### 3.5.2 Detail: Egg Price

**Calculation:**
```
Total costs = Initial costs + Operating costs
Initial costs = Flock purchase price
Operating costs = SUM(all purchases)

Total egg production = SUM(daily records)

Egg price = Total costs / Total egg production
```

**Display:**
- Main metric: **X CZK / egg**
- Chart: Price development over time (line chart)
- Cost breakdown (pie chart):
  - Feed: X %
  - Vitamins: X %
  - Bedding: X %
  - Veterinary care: X %
  - Other: X %
- Filters:
  - Time period (last 7 days, 30 days, 3 months, year, custom)
  - Flock (all / specific)

**Business Rules:**
- Chicks count in costs (feed consumption)
- Chicks **do not count** in production (don't lay eggs)
- Only hens contribute to production

#### 3.5.3 Detail: Flock Development

**Hierarchical Overview:**
```
Coop 1 - Large Coop
├─ Flock A - Browns 2024
│  └─ Currently: 15 hens, 2 roosters, 3 chicks
│  └─ Productivity: 12.5 eggs/day (0.83 eggs/hen/day)
└─ Flock B - Whites 2023
   └─ Currently: 8 hens, 1 rooster
   └─ Productivity: 6 eggs/day (0.75 eggs/hen/day)

Coop 2 - Small Coop
└─ Flock C - Mix 2024
   └─ Currently: 5 hens, 1 rooster, 10 chicks
   └─ Productivity: 4 eggs/day (0.80 eggs/hen/day)
```

**Flock Detail:**
- **Timeline of changes** (vertical time axis):
  ```
  04.02.2024 - Chick conversion
    Chicks: 20 → 5 (-15)
    Hens: 10 → 22 (+12)
    Roosters: 2 → 5 (+3)
    Notes: First batch from hatchery

  28.01.2024 - Death
    Chicks: 22 → 20 (-2)
    Notes: Illness

  15.01.2024 - Flock established
    Chicks: 22
    Notes: Hatched
  ```

- **Charts:**
  - Flock size over time (area chart, 3 series: hens, roosters, chicks)
  - Productivity (eggs/hen/day) over time (line chart)

**Productivity:**
```
Productivity = Egg count / Hen count / Day count

Example:
- Week total: 84 eggs
- Average hen count: 12
- 7 days
→ Productivity = 84 / 12 / 7 = 1.0 eggs/hen/day
```

#### 3.5.4 Exports (nice-to-have - Phase 3)

**Formats:**
- CSV export (all agendas)
- PDF report (dashboard snapshot)

**CSV Contains:**
- Daily records (date, flock, egg count, notes)
- Purchases (date, type, name, price, quantity, unit)
- Flock history (date, changes, reason)

---

## 4. Technical Requirements

### 4.1 Architecture

#### Frontend - Mobile First PWA

**Technologies:**
- **React 18+** with TypeScript
- **Vite** - build tool (fast refresh)
- **React Router** - routing
- **Zustand** - state management + persistence
- **TanStack Query (React Query)** - server state & caching
- **Axios** - HTTP client with interceptors

**PWA Stack:**
- **Workbox** - service worker management
- **manifest.json** - app manifest
- **IndexedDB** - offline storage (via Dexie.js)
- **Background Sync API** - queue for offline requests

**UI Framework:**
- **Material-UI (MUI)**
  - Proven mobile support
  - Touch-optimized components
  - Theming capabilities
  - Accessibility built-in

**Charting:**
- **Recharts**
  - Lightweight
  - Responsive
  - Touch-friendly

**Forms:**
- **React Hook Form** - performance & validation
- **Zod** - schema validation

**Performance:**
- Code splitting (lazy loading routes)
- Image optimization (WebP, lazy loading)
- Bundle analysis (webpack-bundle-analyzer)
- Performance budget: < 200kb gzipped

**Performance Budget:**
- First Contentful Paint < 1.5s
- Time to Interactive < 3.5s
- Largest Contentful Paint < 2.5s
- Cumulative Layout Shift < 0.1
- Bundle size < 200kb (gzipped)

#### Backend

**Technologies:**
- **.NET 8** Web API
- **ASP.NET Core** Minimal APIs
- **Entity Framework Core** (Code First)
- **AutoMapper** - DTO mapping
- **FluentValidation** - request validation
- **Microsoft.Extensions.Logging** - structured logging
- **MediatR** - CQRS pattern

**Architecture Pattern:**
- Clean Architecture / Onion Architecture
- Dependency Injection
- Vertical Slice Architecture with CQRS

**Authentication:**
- **Clerk.com** - Authentication provider
- **JWT Bearer tokens** - Validated by .NET API
- **Automatic token refresh** - Handled by Clerk SDK
- Clerk webhooks for user sync

**API Design:**
- RESTful principles
- Versioning: URL-based (`/api/v1/...`)
- Consistent error responses
- CORS enabled for PWA origin

#### Database

**Primary Choice: Neon Postgres (Serverless)**

**Why Neon:**
- ✅ Cost-friendly (Free tier: 0.5GB storage)
- ✅ Serverless (auto-scaling, auto-pause)
- ✅ Branching for dev/staging environments
- ✅ Connection pooling built-in
- ✅ PostgreSQL 16 features
- ✅ High availability
- ✅ Fast queries with proper indexing
- ✅ Row-Level Security (RLS) for tenant isolation

**Data Model - Postgres:**
```sql
-- Tenants (linked to Clerk users)
CREATE TABLE tenants (
    id UUID PRIMARY KEY,
    clerk_user_id TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

-- Coops
CREATE TABLE coops (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    name TEXT NOT NULL,
    location TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

-- Flocks
CREATE TABLE flocks (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    coop_id UUID NOT NULL REFERENCES coops(id),
    identifier TEXT NOT NULL,
    hatch_date DATE NOT NULL,
    current_hens INTEGER NOT NULL,
    current_roosters INTEGER NOT NULL,
    current_chicks INTEGER NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

-- Flock History
CREATE TABLE flock_history (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    flock_id UUID NOT NULL REFERENCES flocks(id),
    change_date DATE NOT NULL,
    hens INTEGER NOT NULL,
    roosters INTEGER NOT NULL,
    chicks INTEGER NOT NULL,
    change_type change_type NOT NULL, -- ENUM: adjustment, maturation
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL
);

-- Purchases
CREATE TABLE purchases (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    flock_id UUID REFERENCES flocks(id),
    type purchase_type NOT NULL,
    name TEXT NOT NULL,
    purchase_date DATE NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    quantity DECIMAL(10, 2) NOT NULL,
    unit quantity_unit NOT NULL,
    consumed_date DATE,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

-- Daily Records
CREATE TABLE daily_records (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    flock_id UUID NOT NULL REFERENCES flocks(id),
    record_date DATE NOT NULL,
    egg_count INTEGER NOT NULL,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    UNIQUE (flock_id, record_date)
);

-- Row-Level Security (RLS)
ALTER TABLE coops ENABLE ROW LEVEL SECURITY;
ALTER TABLE flocks ENABLE ROW LEVEL SECURITY;
ALTER TABLE flock_history ENABLE ROW LEVEL SECURITY;
ALTER TABLE purchases ENABLE ROW LEVEL SECURITY;
ALTER TABLE daily_records ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation ON coops
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
-- ... similar policies for other tables
```

**Multi-tenancy Strategy:**
- **Row-Level Security (RLS)** enforced at database level
- Every table has `tenant_id` column
- Postgres policies prevent cross-tenant access
- EF Core sets RLS context per request
- Global query filters as additional safety layer

#### Hosting

**Azure Container Apps** (recommended)
- ✅ Managed Kubernetes
- ✅ Auto-scaling (0-N replicas)
- ✅ HTTPS out of the box
- ✅ Custom domains
- ✅ Cost-effective (pay-per-use)
- ~10-30 EUR/month

**Docker Setup:**
- Multi-stage build (build + runtime)
- Base image: `mcr.microsoft.com/dotnet/aspnet:8.0`
- Build image: `mcr.microsoft.com/dotnet/sdk:8.0`
- Node image for React build: `node:20-alpine`

**CI/CD:**
- GitHub Actions
- Automatic deployment on main branch
- Preview environments for PR (optional)

**CDN (optional - Phase 2):**
- Azure CDN for static assets
- Caching strategy
- Global distribution

---

### 4.2 Offline Strategy

#### Service Worker Strategies

**Static Assets: Cache-First**
```javascript
// HTML, CSS, JS, fonts
workbox.routing.registerRoute(
  ({request}) => request.destination === 'script' ||
                  request.destination === 'style',
  new workbox.strategies.CacheFirst({
    cacheName: 'static-resources',
    plugins: [
      new workbox.expiration.ExpirationPlugin({
        maxAgeSeconds: 30 * 24 * 60 * 60, // 30 days
      }),
    ],
  })
);
```

**API GET Requests: Network-First with Cache Fallback**
```javascript
workbox.routing.registerRoute(
  ({url}) => url.pathname.startsWith('/api/'),
  new workbox.strategies.NetworkFirst({
    cacheName: 'api-cache',
    networkTimeoutSeconds: 3,
    plugins: [
      new workbox.expiration.ExpirationPlugin({
        maxEntries: 50,
        maxAgeSeconds: 5 * 60, // 5 minutes
      }),
    ],
  }),
  'GET'
);
```

**API POST/PUT/DELETE: Background Sync Queue**
```javascript
const bgSyncPlugin = new workbox.backgroundSync.BackgroundSyncPlugin(
  'apiQueue',
  {
    maxRetentionTime: 24 * 60, // 24 hours
    onSync: async ({queue}) => {
      // Retry logic
    }
  }
);

workbox.routing.registerRoute(
  ({url}) => url.pathname.startsWith('/api/'),
  new workbox.strategies.NetworkOnly({
    plugins: [bgSyncPlugin],
  }),
  'POST'
);
```

#### IndexedDB Schema

```javascript
// Dexie.js schema
const db = new Dexie('ChickenTrackDB');
db.version(1).stores({
  // Offline queue
  pendingRequests: '++id, method, url, timestamp',

  // Cached data
  coops: 'id, tenantId',
  flocks: 'id, coopId, tenantId',
  purchases: 'id, tenantId, date',
  dailyRecords: '[flockId+date], tenantId',

  // Metadata
  syncStatus: 'key'
});
```

#### Conflict Resolution

**Strategy: Last-Write-Wins (Simple)**
```
Scenario: Offline flock edit + online flock edit
1. Offline: Change hen count to 10
2. Online (different device): Change hen count to 12
3. Sync: Compare timestamp
   → Newer record wins
4. UI: Toast notification "Data was synchronized"
```

**Phase 2: Conflict Detection**
- Server returns `ETag` or `LastModified`
- Client checks before write
- On conflict: UI with choice (Keep mine / Take theirs / Merge)

---

### 4.3 Security

**Authentication Flow:**
```
1. User clicks Sign In → Clerk hosted UI
2. User enters credentials → Clerk validates
3. Clerk issues JWT session token
4. Frontend gets token via Clerk SDK
5. API calls include Bearer token in Authorization header
6. .NET API validates Clerk JWT
7. API extracts userId, looks up tenant_id
8. API sets RLS context for request
9. All queries automatically filtered by tenant
```

**Token Storage:**
- Clerk manages token storage (secure)
- Frontend uses Clerk hooks to get fresh tokens
- No manual token refresh needed

**API Security:**
- HTTPS only (enforced)
- CORS: whitelist PWA origins
- Rate limiting:
  - Clerk manages auth endpoint limits
  - API: 100 requests / min / user
- Input validation (FluentValidation)
- SQL Injection protection (EF Core parameterized queries)
- XSS protection (sanitize inputs)

**Password Requirements:**
- Managed by Clerk
- Default: min 8 characters
- Configurable in Clerk dashboard

---

### 4.4 Monitoring & Logging

**Application Insights** (Azure)
- Request tracking
- Exception logging
- Performance metrics
- Custom events (business metrics)

**Frontend Monitoring:**
- Error boundary (React)
- Azure App Insights JS SDK
- Performance API (Web Vitals)

**Backend Logging:**
```csharp
Log.Information("User {UserId} created flock {FlockId}", userId, flockId);
Log.Error(ex, "Failed to sync daily record {RecordId}", recordId);
```

**Metrics:**
- API response times (p50, p95, p99)
- Error rate
- Active users (DAU, MAU)
- Offline sync success rate
- PWA install rate

---

## 5. UI/UX Requirements

### 5.1 Mobile-First Design Principles

#### Layout Strategy

**Breakpoints:**
```css
/* Mobile First */
@media (min-width: 320px)  { /* Mobile portrait */ }
@media (min-width: 480px)  { /* Mobile landscape */ }
@media (min-width: 768px)  { /* Tablet */ }
@media (min-width: 1024px) { /* Desktop */ }
```

**Grid System:**
- Mobile: 1 column
- Tablet: 2 columns
- Desktop: 3-4 columns (optional)

**Navigation Pattern:**
```
┌─────────────────────────┐
│  Header                 │
│  [Logo] [Bell] [User]   │  ← Clerk UserButton component
├─────────────────────────┤
│                         │
│  Content Area           │
│  (Scrollable)           │
│                         │
│                         │
├─────────────────────────┤
│ Bottom Navigation       │
│ [🏠] [🐔] [📝] [📊] [⋮] │
└─────────────────────────┘

Bottom Nav Items:
🏠 Dashboard
🐔 Coops
📝 Daily Records
📊 Statistics
⋮  Menu (Purchases, Settings)
```

**Floating Action Button (FAB):**
- Primary action: "Add daily record"
- Position: Bottom-right (60dp margin)
- Size: 56x56dp
- Bounce on tap (elevation animation)

#### Touch Optimization

**Touch Target Size:**
- Minimum: 44x44px (iOS standard)
- Preferred: 48x48px (Material Design)
- Spacing between targets: 8px minimum

**Gestures:**
- **Swipe to refresh** (pull-to-refresh) - dashboard, lists
- **Swipe to delete** - optional in lists (with undo)
- **Long press** - context menu (optional)
- **Pinch to zoom** - charts (nice-to-have)

**Input Components:**

1. **Number Inputs** (critical for speed)
   ```
   Egg count:
   ┌─────────────────────────┐
   │  [-]    [24]     [+]    │
   └─────────────────────────┘
   Large buttons (80x60px)
   ```

2. **Date Pickers**
   - Native date picker (mobile optimized)
   - Quick shortcuts: Today, Yesterday, Week ago

3. **Dropdowns / Selects**
   - Large touch area
   - Search/filter for long lists
   - Recent items at top

4. **Text Areas**
   - Auto-expand while typing
   - Character counter (optional)
   - Voice input button (Phase 2)

---

### 5.2 Internationalization (i18n)

**Language Support:**
- **Primary language: Czech (cs-CZ)**
- **Secondary language: English (en-US)**
- User can switch language in settings
- Language preference stored in local storage
- Fallback to browser language on first visit

**Implementation:**
- **react-i18next** for frontend translations
- Translation files: `locales/cs/translation.json`, `locales/en/translation.json`
- Date/time formatting with `date-fns` locale support
- Currency formatting (CZK primary, EUR for international)

**Translation Structure:**
```json
// locales/cs/translation.json
{
  "dashboard": {
    "title": "Přehled",
    "today": "Dnes",
    "eggs": "Vajíčka",
    "hens": "Slepice"
  },
  "coops": {
    "title": "Kurníky",
    "add": "Přidat kurník"
  }
}

// locales/en/translation.json
{
  "dashboard": {
    "title": "Dashboard",
    "today": "Today",
    "eggs": "Eggs",
    "hens": "Hens"
  },
  "coops": {
    "title": "Coops",
    "add": "Add coop"
  }
}
```

---

### 5.3 PWA Features - Detailed Specification

#### 5.3.1 Installation

**Install Prompt Strategy:**
```javascript
// Trigger after 2nd visit or 5 minutes of usage
if (visitCount >= 2 || timeSpentMinutes >= 5) {
  showInstallPrompt();
}
```

**Custom Install Banner:**
```
┌─────────────────────────────┐
│ ⚡ Přidat na plochu          │
│                             │
│ Rychlý přístup k evidenci   │
│ vajec i bez internetu!      │
│                             │
│ [Možná později] [Přidat]    │
└─────────────────────────────┘
```

**iOS Add to Home Screen Instructions:**
- Detect iOS Safari
- Step-by-step guide with images:
  1. Tap Share button (📤)
  2. Scroll & tap "Add to Home Screen"
  3. Tap "Add"

#### 5.3.2 Offline Mode

**Offline Detection:**
```javascript
window.addEventListener('online', () => {
  // Trigger background sync
  showToast('Připojeno - synchronizuji data...');
});

window.addEventListener('offline', () => {
  showBanner('Offline režim - data se uloží lokálně');
});
```

**Offline Banner (persistent):**
```
┌─────────────────────────────┐
│ 📴 Jste offline             │
│ Data se uloží lokálně       │
└─────────────────────────────┘
```

**Sync Indicator:**
```
Bottom bar:
[3 neuložené záznamy] [Synchronizovat]
```

**Offline Capabilities:**
- ✅ View all cached data
- ✅ Create daily records
- ✅ Create purchases
- ✅ Edit flock (with conflict warning)
- ❌ Registration / Login (requires network - Clerk)
- ❌ Statistics (if not in cache)

#### 5.3.3 Background Sync

**Sync Queue Management:**
```javascript
// IndexedDB queue
{
  id: 1,
  method: 'POST',
  url: '/api/daily-records',
  body: {...},
  timestamp: '2024-02-04T07:15:00Z',
  retryCount: 0
}
```

**Retry Logic:**
- Immediate retry when network restored
- Exponential backoff: 1s, 2s, 4s, 8s, 16s, 30s
- Max 5 attempts
- After 5 attempts: manual retry button

**Success Notifications:**
```
Toast (2s):
✓ 3 záznamy úspěšně uloženy
```

**Error Handling:**
```
Persistent banner:
⚠️ Nepodařilo se uložit 2 záznamy
[Detail] [Zkusit znovu]

Detail:
- Denní záznam 04.02.2024: Server error (500)
- Nákup krmiva: Network timeout
```

#### 5.3.4 Manifest & Icons

**manifest.json:**
```json
{
  "name": "ChickenTrack - Evidence chovu slepic",
  "short_name": "ChickenTrack",
  "description": "Sledování rentability chovu slepic",
  "start_url": "/",
  "display": "standalone",
  "theme_color": "#FF6B35",
  "background_color": "#FFFFFF",
  "orientation": "portrait",
  "lang": "cs-CZ",
  "icons": [
    {
      "src": "/icons/icon-72x72.png",
      "sizes": "72x72",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "/icons/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "/icons/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any maskable"
    }
  ],
  "screenshots": [
    {
      "src": "/screenshots/dashboard.png",
      "sizes": "540x720",
      "type": "image/png"
    }
  ]
}
```

**Splash Screen:**
- Background: theme_color
- Logo: centered
- App name: below logo
- Fade-in animation (300ms)

---

## 6. Non-Functional Requirements

### 6.1 Performance

**Metrics (Target):**
- **Lighthouse Score:** > 90 (all categories)
- **First Contentful Paint:** < 1.5s (3G connection)
- **Time to Interactive:** < 3.5s
- **Largest Contentful Paint:** < 2.5s
- **Cumulative Layout Shift:** < 0.1
- **API Response Time:** < 500ms (p95)

**Mobile Constraints:**
- Battery efficient (minimize background tasks, WebSockets)
- Data usage (compress API responses via gzip)
- Storage awareness (IndexedDB ~50MB limit, cleanup old data)
- Memory efficient (optimize for 2GB RAM devices)

### 6.2 Scalability

**Capacity Planning:**
- Support for **1,000+ tenants** (Year 1)
- **10,000 daily records/day** aggregate
- **100 concurrent users**
- **1M API requests/day**

**Database Optimization:**
- Row-Level Security for tenant isolation
- Indexing on foreign keys and date columns
- Connection pooling (Neon built-in)
- Query optimization (EF Core query analysis)

**Backend Scaling:**
- Horizontal scaling (Azure Container Apps auto-scale)
- Stateless API (no in-memory sessions)
- CDN for static assets
- Database connection pooling

### 6.3 Reliability & Availability

**Uptime Target:** 99.5% (SLA)
- Scheduled maintenance: < 4 hours/month
- Downtime notifications: Email + in-app banner

**Backup Strategy:**
- Automated daily backups (Neon automatic backups)
- Point-in-time recovery (Neon feature)
- Retention: 7 days (free tier), 30 days (paid)

**Disaster Recovery:**
- RTO (Recovery Time Objective): 4 hours
- RPO (Recovery Point Objective): 24 hours
- Neon multi-region availability

### 6.4 Security

**Compliance:**
- GDPR compliant (EU users)
- Data encryption at rest (Neon default)
- Data encryption in transit (HTTPS/TLS 1.3)
- Regular security audits (quarterly)

**Authentication:**
- Managed by Clerk (SOC 2 Type II certified)
- JWT expiration: Clerk default (7 days)
- Rate limiting: Clerk managed
- MFA support (Phase 2 - Clerk Pro tier)

**Data Privacy:**
- Tenant data isolation (RLS policies)
- No cross-tenant data access
- Personal data export (GDPR right)
- Account deletion (GDPR right to be forgotten)

**Input Validation:**
- Frontend: React Hook Form + Zod
- Backend: FluentValidation
- SQL Injection: EF Core parameterized queries
- XSS: Sanitize user inputs (DOMPurify)

### 6.5 Monitoring & Logging

**Application Insights (Azure):**
- Request tracking (all API calls)
- Exception logging (errors, warnings)
- Custom events:
  - User registration (via Clerk webhook)
  - Daily record created
  - Chick maturation
  - Offline sync completed
- Performance counters (CPU, memory, response time)

**Alerting:**
- Error rate > 5%: Slack notification
- API response time > 1s (p95): Email alert
- Downtime: SMS + Email + Slack

**Log Retention:**
- Application logs: 90 days
- Access logs: 180 days
- Audit logs: 2 years

### 6.6 Browser & Device Support

**Desktop Browsers:**
- Chrome/Edge: latest 2 versions ✅
- Firefox: latest 2 versions ✅
- Safari: latest 2 versions ✅
- **NO IE11** ❌

**Mobile Browsers:**
- iOS Safari: 15+ ✅
- Android Chrome: 90+ ✅
- Samsung Internet: latest ✅

**Device Testing:**
- iPhone SE (2020) - small screen
- iPhone 14 Pro - modern iOS
- Samsung Galaxy A52 - mid-range Android
- Google Pixel 6 - flagship Android

**Screen Sizes:**
- 320px (iPhone SE portrait) ✅
- 768px (iPad portrait) ✅
- 1024px (iPad landscape) ✅
- 1920px (Desktop) ✅

---

## 7. API Specification

### 7.1 Authentication Endpoints

**Note:** Most authentication is handled by Clerk. Backend only needs webhook and sync.

#### POST /api/webhooks/clerk
**Clerk Webhook (user.created event)**

**Request Headers:**
```
svix-signature: v1,signature
svix-timestamp: timestamp
svix-id: webhook-id
```

**Request Body:**
```json
{
  "type": "user.created",
  "data": {
    "id": "user_2abc123",
    "email_addresses": [
      {
        "email_address": "ondra@example.com"
      }
    ]
  }
}
```

**Response (200):**
```json
{
  "message": "User synced successfully"
}
```

#### POST /api/users/sync
**Manual user sync (called from frontend after sign-up)**

**Headers:**
```
Authorization: Bearer <clerk-jwt-token>
```

**Response (200):**
```json
{
  "tenantId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "ondra@example.com",
  "createdAt": "2024-02-04T10:00:00Z"
}
```

---

### 7.2 Coop Endpoints

#### GET /api/coops
**Get all coops for authenticated user**

**Headers:**
```
Authorization: Bearer <clerk-jwt-token>
```

**Response (200):**
```json
[
  {
    "id": "coop_1",
    "name": "Coop 1 - Large",
    "location": "Behind the house",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z",
    "flocksCount": 2
  }
]
```

#### POST /api/coops
**Create new coop**

**Request:**
```json
{
  "name": "Coop 2 - Small",
  "location": "In front of house"
}
```

**Response (201):**
```json
{
  "id": "coop_2",
  "name": "Coop 2 - Small",
  "location": "In front of house",
  "isActive": true,
  "createdAt": "2024-02-04T08:00:00Z"
}
```

#### PUT /api/coops/{id}
**Update coop**

**Request:**
```json
{
  "name": "Coop 2 - Updated",
  "location": "Next to garage"
}
```

#### DELETE /api/coops/{id}
**Delete coop (only if no active flocks)**

**Response (204):** No content

---

### 7.3 Flock Endpoints

#### GET /api/flocks?coopId={id}
**Get flocks for coop**

**Response (200):**
```json
[
  {
    "id": "flock_1",
    "coopId": "coop_1",
    "identifier": "Browns 2024",
    "hatchDate": "2024-01-15",
    "currentHens": 15,
    "currentRoosters": 2,
    "currentChicks": 3,
    "isActive": true,
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```

#### POST /api/flocks
**Create new flock**

**Request:**
```json
{
  "coopId": "coop_1",
  "identifier": "Whites 2024",
  "hatchDate": "2024-02-01",
  "initialHens": 0,
  "initialRoosters": 0,
  "initialChicks": 30
}
```

#### POST /api/flocks/{id}/mature-chicks
**Convert chicks to adult chickens**

**Request:**
```json
{
  "date": "2024-02-04",
  "chicksCount": 15,
  "resultingHens": 12,
  "resultingRoosters": 3,
  "notes": "First batch from hatchery"
}
```

**Response (200):**
```json
{
  "flockId": "flock_1",
  "historyId": "history_123",
  "updatedFlock": {
    "currentHens": 22,
    "currentRoosters": 5,
    "currentChicks": 5
  }
}
```

#### GET /api/flocks/{id}/history
**Get flock change history**

**Response (200):**
```json
[
  {
    "id": "history_123",
    "date": "2024-02-04",
    "hens": 22,
    "roosters": 5,
    "chicks": 5,
    "changeType": "maturation",
    "notes": "First batch from hatchery",
    "createdAt": "2024-02-04T07:30:00Z"
  }
]
```

---

### 7.4 Purchase Endpoints

#### GET /api/purchases?from={date}&to={date}&type={type}
**Get purchases with optional filters**

**Response (200):**
```json
[
  {
    "id": "purchase_1",
    "type": "Feed",
    "name": "Feed Mix A",
    "date": "2024-02-01",
    "amount": 250.00,
    "quantity": 25,
    "unit": "kg",
    "consumedDate": "2024-02-15",
    "notes": "25 kg package",
    "flockId": null,
    "createdAt": "2024-02-01T10:00:00Z"
  }
]
```

#### POST /api/purchases
**Create purchase**

**Request:**
```json
{
  "type": "Vitamins",
  "name": "Multivitamin",
  "date": "2024-02-04",
  "amount": 120.00,
  "quantity": 1,
  "unit": "package",
  "consumedDate": null,
  "notes": "For entire farm",
  "flockId": null
}
```

---

### 7.5 Daily Record Endpoints

#### GET /api/daily-records?flockId={id}&from={date}&to={date}
**Get daily records**

**Response (200):**
```json
[
  {
    "id": "record_1",
    "flockId": "flock_1",
    "date": "2024-02-04",
    "eggCount": 12,
    "notes": "Standard production",
    "createdAt": "2024-02-04T07:15:00Z"
  }
]
```

#### POST /api/daily-records
**Create daily record (offline-capable)**

**Request:**
```json
{
  "flockId": "flock_1",
  "date": "2024-02-04",
  "eggCount": 12,
  "notes": ""
}
```

---

### 7.6 Statistics Endpoints

#### GET /api/statistics/dashboard
**Get dashboard statistics**

**Response (200):**
```json
{
  "today": {
    "totalEggs": 24,
    "totalHens": 32,
    "totalRoosters": 5,
    "totalChicks": 8
  },
  "thisWeek": {
    "totalEggs": 156,
    "avgEggsPerDay": 22.3,
    "trend": "up",
    "trendValue": 12
  },
  "economics": {
    "currentEggCost": 4.20,
    "trend": "down",
    "trendValue": -0.15
  },
  "flockStatus": {
    "activeFlocks": 3,
    "totalAnimals": 45
  }
}
```

#### GET /api/statistics/egg-cost?from={date}&to={date}&flockId={id}
**Get egg cost statistics**

**Response (200):**
```json
{
  "eggCost": 4.20,
  "totalCosts": 2520.00,
  "totalEggs": 600,
  "costBreakdown": [
    {
      "category": "Feed",
      "amount": 1638.00,
      "percentage": 65
    },
    {
      "category": "Bedding",
      "amount": 504.00,
      "percentage": 20
    }
  ],
  "timeline": [
    {
      "date": "2024-01-29",
      "cost": 4.35
    },
    {
      "date": "2024-02-05",
      "cost": 4.20
    }
  ]
}
```

---

### 7.7 Error Responses

**Standard Error Format:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input data",
    "details": [
      {
        "field": "email",
        "message": "Invalid email format"
      }
    ]
  }
}
```

**Error Codes:**
- `VALIDATION_ERROR` (400)
- `UNAUTHORIZED` (401) - Clerk JWT invalid or missing
- `FORBIDDEN` (403) - Tenant isolation violation
- `NOT_FOUND` (404)
- `CONFLICT` (409) - e.g., duplicate name
- `RATE_LIMIT_EXCEEDED` (429)
- `INTERNAL_SERVER_ERROR` (500)

---

## 8. Development Phases (Roadmap)

### 8.1 MVP - Phase 1 (3-4 months)

**Week 1-2: Setup & Infrastructure**
- ✅ Azure account + resource groups
- ✅ Neon Postgres setup
- ✅ Clerk setup (free tier)
- ✅ Docker setup (multi-stage build)
- ✅ CI/CD pipeline (GitHub Actions)
- ✅ .NET 8 Web API skeleton with EF Core
- ✅ React + Vite + PWA setup

**Week 3-4: Authentication (Clerk Integration)**
- ✅ Clerk frontend integration (@clerk/clerk-react)
- ✅ Clerk backend JWT validation
- ✅ User sync webhook
- ✅ Frontend: Protected routes
- ✅ Tenant creation on sign-up

**Week 5-6: Coops & Flocks (CRUD)**
- ✅ Backend: Coops API with EF Core
- ✅ Backend: Flocks API (with chicks)
- ✅ Frontend: Coop list
- ✅ Frontend: Flock detail
- ✅ Frontend: Forms (create/edit)

**Week 7-8: Chick Maturation Action**
- ✅ Backend: Mature chicks endpoint
- ✅ Backend: Flock history tracking
- ✅ Frontend: Chick maturation modal
- ✅ Frontend: Change history timeline

**Week 9-10: Purchase Management**
- ✅ Backend: Purchases API
- ✅ Frontend: Purchase list
- ✅ Frontend: Add purchase (form)
- ✅ Frontend: Name autocomplete

**Week 11-12: Daily Records (Offline-First)**
- ✅ Backend: Daily records API
- ✅ Frontend: Quick Add modal
- ✅ Service Worker setup
- ✅ IndexedDB integration
- ✅ Background Sync queue

**Week 13-14: Dashboard & Statistics**
- ✅ Backend: Dashboard stats endpoint
- ✅ Backend: Egg cost calculation
- ✅ Frontend: Dashboard widgets
- ✅ Frontend: Egg cost statistics
- ✅ Basic charts (Recharts)

**Week 15-16: PWA Features & Testing**
- ✅ Manifest.json + icons
- ✅ Install prompt
- ✅ Offline banner
- ✅ Mobile testing (real devices)
- ✅ Performance optimization
- ✅ Lighthouse audit (score > 90)

**Deliverables MVP:**
- Functional PWA with offline mode
- Clerk authentication (email + password)
- CRUD Coops + Flocks (with chicks)
- Chick maturation action
- Purchase management
- Daily records (offline-capable)
- Dashboard with overview
- Statistics: egg cost
- Lighthouse score > 90

---

### 8.2 Phase 2 (2-3 months)

**Detailed Statistics**
- Flock change history (timeline view)
- Flock size development (charts)
- Productivity (eggs/hen/day) over time
- Flock comparison (side-by-side)

**Push Notifications**
- Daily reminder (19:00): "Don't forget to log eggs"
- Sync completed: "3 records saved"
- Chicks ready to mature: "Chicks are 6 weeks old"

**UX Improvements**
- Install prompt optimization (personalization)
- Onboarding tutorial (first-time user)
- Dark mode (optional)
- Swipe gestures (delete, archive)

**Clerk Upgrades (Optional)**
- Social logins (Google, Facebook) - still free tier
- Multi-factor authentication (MFA) - requires Pro tier ($25/month)
- Organizations (family sharing) - requires Pro tier

**Performance**
- Advanced caching strategies
- Prefetching (anticipate user actions)
- Image optimization (WebP + lazy loading)

---

### 8.3 Phase 3 (2-3 months)

**Individual Chickens (Optional)**
- CRUD individual chickens
- Link to flock
- Chicken detail (notes, photo)

**Exports**
- CSV export (all agendas)
- PDF report (dashboard snapshot)
- Email reports (weekly/monthly)

**Advanced Features**
- Voice input for notes (Web Speech API)
- Photo upload (with compression)
- Calendar view (daily records)
- Multi-language support (full i18n)

**Offline Enhancements**
- Advanced conflict resolution (merge strategies)
- Offline conflict UI (choose version)
- Offline analytics (track offline usage)

---

### 8.4 Future Ideas (Backlog)

**Integrations**
- Export to accounting software (Money S3, Pohoda)
- E-commerce integration (egg sales)
- API for external applications

**Advanced Analytics**
- Machine Learning: production prediction
- Anomaly detection (unusual production decrease)
- Feed optimization (cost/benefit analysis)

**Community**
- Share statistics (anonymized)
- Benchmark with other farmers
- Discussion forum

---

## 9. Open Questions for Discussion

### 9.1 Currency
**Question:** CZK only? Or multi-currency?

**Options:**
- **A) CZK only:** Simpler, targeting Czech market
- **B) Multi-currency:** EUR, USD, CZK (select on registration)

**Recommendation:** Start with CZK only, Phase 2 multi-currency

### 9.2 Quantity Units
**Question:** Preferred units for feed (kg, pcs, liters)? Or optional?

**Options:**
- **A) Predefined:** kg, pcs, l, package (dropdown)
- **B) Custom:** User can add own units
- **C) Hybrid:** Predefined + custom option

**Recommendation:** Start with predefined (A), Phase 2 custom

### 9.3 Photos
**Question:** Want to add photos of chickens/coops/flocks?

**Options:**
- **A) No photos:** Text data only (simpler, less storage)
- **B) Phase 2:** Add photo support later
- **C) MVP:** Basic photo upload (compress + Azure Blob Storage)

**Impact:**
- Photos require:
  - Azure Blob Storage (cost)
  - Image compression (client-side)
  - Thumbnail generation
  - Offline sync complexity

**Recommendation:** Phase 2 or 3

### 9.4 Breed/Color
**Question:** Record breed or chicken color?

**Options:**
- **A) No breeding info:** Just counts
- **B) Breed field:** Dropdown with breeds (Leghorn, Rhode Island Red, etc.)
- **C) Custom tags:** Free tags (color, breed, etc.)

**Recommendation:** Phase 2, custom tags in flock notes (interim)

### 9.5 Egg Sales (ROI)
**Question:** Plan to track income from egg sales for ROI calculation?

**Options:**
- **A) Costs only:** Just costs → egg price
- **B) Income tracking:** Sales income → ROI, profit/loss
- **C) Hybrid:** Start costs only, Phase 2 income

**Impact:**
- Income tracking requires:
  - Sales agenda (date, count, price, buyer)
  - Profit/loss calculations
  - ROI dashboard
  - Tax reporting (optional)

**Recommendation:** Start costs only (A), Phase 2 income (B)

### 9.6 Notifications
**Question:** Push notifications for daily record reminder?

**Options:**
- **A) No notifications:** User must remember
- **B) Phase 2:** Push notifications (19:00 daily)
- **C) MVP:** Email reminders (simpler)

**Recommendation:** Phase 2 push notifications

### 9.7 Initial Costs Data Model
**Question:** How to track initial costs (purchasing chickens/chicks)?

**Options:**
- **A) Ignore:** Assume hatching, initial cost = 0
- **B) Purchase type:** Type "Animal purchase" in Purchase agenda
- **C) Flock initial cost:** "Initial cost" field when creating flock

**Recommendation:** B - Purchase type "Animal purchase"

### 9.8 Historical Data
**Question:** Need to import historical data (migration from Excel)?

**Options:**
- **A) No import:** Start from scratch
- **B) CSV import:** Simple import from CSV
- **C) Excel import:** Automatic Excel file parsing

**Recommendation:** Phase 2 - CSV import

---

## 10. Risks & Mitigation

### 10.1 Technical Risks

**1. Offline Sync Conflicts**
- **Risk:** Data loss during conflict merge
- **Mitigation:**
  - Last-write-wins for MVP (simple)
  - Toast notification after sync
  - Phase 2: Conflict detection UI

**2. Clerk Vendor Lock-in**
- **Risk:** Dependent on Clerk service
- **Mitigation:**
  - Clerk has good SLA (99.99% uptime)
  - Can migrate to another provider (OpenID Connect standard)
  - User data synced to own database

**3. Neon Free Tier Limits**
- **Risk:** 0.5GB storage limit, 1 project
- **Mitigation:**
  - Monitor storage usage
  - Upgrade to paid tier when needed (~$20/month for 10GB)
  - Optimize queries to reduce storage

**4. PWA Install Rate**
- **Risk:** Users don't install PWA (50%+ bounce)
- **Mitigation:**
  - Aggressive install prompts
  - Education (benefits highlight)
  - Fallback: Works in browser too

**5. Battery Drain (Offline Sync)**
- **Risk:** Background sync drains battery
- **Mitigation:**
  - Throttle sync attempts
  - Use Workbox exponential backoff
  - Respect battery saver mode

---

### 10.2 Business Risks

**1. Low User Adoption**
- **Risk:** Target audience doesn't adopt app
- **Mitigation:**
  - MVP validation with beta testers (5-10 farmers)
  - Iterative development based on feedback
  - Freemium model (free, later paid pro)

**2. Competition**
- **Risk:** Existing solutions (Excel, other apps)
- **Mitigation:**
  - USP: Offline-first, mobile-optimized, ROI focus
  - Differentiation: Czech market, specific use case
  - Community building

**3. Scaling Costs**
- **Risk:** Azure costs grow with users
- **Mitigation:**
  - Cost-effective storage (Neon free tier)
  - Auto-scaling with limits
  - Pricing model: Freemium → Paid tiers

---

### 10.3 UX Risks

**1. Complexity Creep**
- **Risk:** Feature bloat → loss of simplicity
- **Mitigation:**
  - Strict scope per phase
  - User testing for each new feature
  - "One main action per screen" rule

**2. Offline Confusion**
- **Risk:** Users don't understand offline mode
- **Mitigation:**
  - Clear UI indicators
  - Onboarding tutorial (first use)
  - Help/FAQ section

---

## 11. Success Metrics (KPIs)

### 11.1 Adoption Metrics
- **Registrations:** 100+ users (Year 1)
- **Active Users (MAU):** 50+ (Month 3 after launch)
- **PWA Install Rate:** 40%+ of users
- **Retention (30 days):** 60%+

### 11.2 Engagement Metrics
- **Daily Records Created:** 80%+ days (active users)
- **Avg Session Duration:** 3+ minutes
- **Sessions per Week:** 5+ (daily loggers)
- **Feature Usage:**
  - Daily records: 90%
  - Statistics: 60%
  - Purchases: 40%
  - Chick maturation: 20%

### 11.3 Performance Metrics
- **Lighthouse Score:** 90+ (all categories)
- **API Response Time:** < 500ms (p95)
- **Offline Sync Success Rate:** 98%+
- **Error Rate:** < 1%

### 11.4 Business Metrics
- **Cost per User:** < 0.50 EUR/month (hosting + storage)
- **User Satisfaction (NPS):** 40+ (promoters - detractors)
- **Support Tickets:** < 5/month (per 100 users)

---

## 12. Conclusion & Next Steps

### 12.1 Summary

ChickenTrack is a **mobile-first PWA** application for tracking chicken farming financial profitability with focus on:
- ✅ **Offline-first approach** (important for use at coops)
- ✅ **Multi-tenant architecture** (data isolation)
- ✅ **Fast logging** (daily records < 30 seconds)
- ✅ **Economic transparency** (precise egg cost calculation)
- ✅ **Modern auth** (Clerk - battle-tested, secure)
- ✅ **Cost-effective hosting** (Neon Postgres + Azure Container Apps)

### 12.2 Technology Stack Summary

**Frontend:**
- React 18 + TypeScript + Vite
- Material-UI (MUI)
- TanStack Query + Zustand
- Clerk React SDK
- Workbox PWA

**Backend:**
- .NET 8 Web API
- Entity Framework Core
- Clerk JWT validation
- Clean Architecture + CQRS

**Database:**
- Neon Postgres (serverless)
- Row-Level Security (RLS)
- Automatic backups

**Hosting:**
- Azure Container Apps
- Docker multi-stage build
- GitHub Actions CI/CD

### 12.3 Next Steps

**1. Finalize Open Questions (Priority: HIGH)**
- Answer 8 questions in Section 9
- Clarify MVP scope

**2. Design Mockups (Priority: MEDIUM)**
- Wireframes for key screens (Figma)
- User flow diagrams
- Design system (colors, typography, spacing)

**3. Setup Development Environment (Priority: HIGH)**
- Setup Neon Postgres
- Setup Clerk account
- Setup Azure resource groups
- Init Git repositories (monorepo)
- CI/CD pipeline setup

**4. Sprint Planning (Priority: HIGH)**
- 2-week sprints
- First sprint: Infrastructure + Clerk integration
- Iterative development based on roadmap

---

## 13. Contact & Revisions

**Document:** ChickenTrack PRD v2.0
**Author:** Ondřej (Ondra)
**Date:** February 5, 2026
**Status:** Approved

**Next Review:** After Phase 1 MVP completion
**Approvers:** Ondřej (Product Owner + Developer)

---

**Change Log:**
- v2.0 (2026-02-05): Major update
  - Translated to English
  - Updated authentication: Clerk.com instead of custom JWT
  - Updated database: Neon Postgres instead of Azure Table Storage
  - Added Row-Level Security (RLS) for multi-tenancy
  - Updated all API endpoints
  - Added internationalization (i18n) section
  - Maintained offline-first PWA architecture
- v1.0 (2024-02-04): Initial draft (Czech)
  - Executive summary
  - Functional requirements (including chicks + maturation action)
  - Technical requirements (mobile-first PWA)
  - UI/UX specification
  - API endpoints
  - Roadmap (3 phases)
  - Open questions (10)

---

**Attachments:**
- [TBD] Wireframes (Figma link)
- [TBD] User Flows
- [TBD] OpenAPI Spec
- [TBD] Database Schema Diagram
