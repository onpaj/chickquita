# Product Requirements Document: Chickquita PWA

**Version:** 2.0
**Date:** February 5, 2026
**Author:** Ondřej (Ondra)
**Status:** Approved

---

## Executive Summary

Chickquita is a PWA application for tracking the financial profitability of chicken farming with multi-tenant architecture. The application is designed with a **mobile-first** approach, enabling farmers to efficiently record costs, production, and calculate economic efficiency directly at the chicken coops with offline mode support.

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

Chickquita uses **Clerk** for authentication management, providing:
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
const db = new Dexie('ChickquitaDB');
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
  "name": "Chickquita - Evidence chovu slepic",
  "short_name": "Chickquita",
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

## 8. Development Milestones

**Approach:** Value-driven incremental delivery. Each milestone represents a single complete user action that can be deployed to production and provides immediate value. Milestones build progressively toward the full application.

### Milestone Dependency Map

```
Foundation:
M1 (Auth) → M2 (Coops) → M3 (Flocks)
                             ↓
Core Value Loop:            M4 (Daily Records) ──→ M6 (Egg Cost Dashboard)
                             ↓                       ↑
                            M5 (Purchases) ─────────┘

Advanced Flock Management:
M3 (Flocks) → M7 (Edit Composition) → M8 (Mature Chicks) → M9 (History View)

Enhanced UX:
M4 (Daily Records) → M10 (Offline Mode)
M6 (Dashboard) → M11 (Statistics)
M10 (Offline) → M12 (PWA Install)
```

---

### M1: User Authentication

**Value Delivered:** Users can create accounts and securely access the application

**User Story:** As a farmer, I want to sign up and sign in securely, so that my farm data is private and accessible only to me.

**Acceptance Criteria:**
- ✅ User can sign up with email/password via Clerk hosted UI
- ✅ User receives verification email and can verify account
- ✅ User can sign in with verified credentials
- ✅ Backend creates tenant record via Clerk webhook
- ✅ Protected routes redirect unauthenticated users to sign-in
- ✅ User can sign out
- ✅ JWT tokens work for API authentication
- ✅ App is deployable to Azure with Clerk configured

**Technical Scope:**
- **Frontend:**
  - @clerk/clerk-react integration
  - Sign-in/sign-up pages (Clerk hosted UI)
  - Protected route middleware with React Router
  - Basic empty dashboard shell (landing page after login)
  - Clerk UserButton component in header
- **Backend:**
  - Clerk JWT validation middleware
  - Webhook endpoint: POST /api/webhooks/clerk
  - Tenant creation logic (ClerkUserId → TenantId mapping)
  - RLS context setting per request
- **Infrastructure:**
  - Clerk account setup (free tier)
  - Environment variables in Azure (Clerk keys)
  - Database: `tenants` table with RLS policies
  - CI/CD: GitHub Actions deployment pipeline
  - Docker multi-stage build (frontend + backend)

**Dependencies:** None (first milestone)

**Out of Scope:**
- Social logins (Google, Facebook)
- MFA (requires Clerk Pro)
- Password reset UI customization
- User profile editing

---

### M2: Coop Management

**Value Delivered:** Users can organize their chicken coops

**User Story:** As a farmer, I want to create and manage coops, so that I can organize my flocks by location.

**Acceptance Criteria:**
- ✅ User can create a coop (name, optional location)
- ✅ User sees list of their coops on Coops page
- ✅ User can edit coop details (name, location)
- ✅ User can archive a coop (soft delete)
- ✅ User can delete empty coops (validation: no active flocks)
- ✅ Tenant isolation verified (users only see their own coops)
- ✅ Mobile-responsive forms and list view
- ✅ Form validation (name required, max length)

**Technical Scope:**
- **Frontend:**
  - Coops list page with bottom navigation
  - Create/edit form modal (Material-UI Dialog)
  - React Hook Form + Zod validation
  - Empty state UI ("No coops yet - Add your first coop")
  - TanStack Query for data fetching and caching
- **Backend:**
  - CQRS handlers: CreateCoopCommand, UpdateCoopCommand, DeleteCoopCommand, GetCoopsQuery
  - EF Core repository with RLS
  - FluentValidation for request validation
  - Endpoint: GET /api/coops, POST /api/coops, PUT /api/coops/{id}, DELETE /api/coops/{id}
- **Database:**
  - `coops` table with RLS policy
  - Indexes on tenant_id

**Dependencies:** M1 (requires authentication)

**Out of Scope:**
- Coop photos
- Location map integration
- Sharing coops with other users
- Coop statistics (egg production per coop)

---

### M3: Basic Flock Creation

**Value Delivered:** Users can create flocks with initial composition

**User Story:** As a farmer, I want to create a flock with its initial composition (hens, roosters, chicks), so that I can start tracking my chickens.

**Acceptance Criteria:**
- ✅ User can create a flock (identifier, hatch date, coop, initial counts)
- ✅ User sees list of flocks within a coop
- ✅ User can view flock details (current composition)
- ✅ User can edit basic flock info (identifier, hatch date)
- ✅ User can archive a flock
- ✅ Initial flock history record created automatically
- ✅ Validation: At least one animal type > 0
- ✅ Validation: Identifier unique within coop

**Technical Scope:**
- **Frontend:**
  - Flock list view (within coop detail page)
  - Create flock form (identifier, hatch date, coop selector, initial counts)
  - Flock detail page showing current composition
  - Number input components with +/- buttons
  - Material-UI cards for flock display
- **Backend:**
  - CQRS handlers: CreateFlockCommand, UpdateFlockCommand, GetFlocksQuery, GetFlockByIdQuery
  - Automatic creation of first flock_history record (type: "initial")
  - Endpoints: GET /api/flocks?coopId={id}, POST /api/flocks, PUT /api/flocks/{id}, GET /api/flocks/{id}
- **Database:**
  - `flocks` table with RLS
  - `flock_history` table with RLS
  - Indexes on tenant_id, coop_id, flock_id

**Dependencies:** M2 (requires coops to exist)

**Out of Scope:**
- Editing flock composition (M7)
- Chick maturation (M8)
- History timeline view (M9)
- Breed/color tracking

---

### M4: Daily Egg Records

**Value Delivered:** Users can record daily egg production

**User Story:** As a farmer, I want to quickly record how many eggs each flock produced today, so that I can track production over time.

**Acceptance Criteria:**
- ✅ User can create a daily record (flock, date, egg count)
- ✅ User can access quick-add via FAB button on dashboard
- ✅ User can view daily records list (filtered by flock/date range)
- ✅ User can edit daily record (same day only)
- ✅ User can delete daily record
- ✅ Validation: One record per flock per day
- ✅ Validation: Cannot create future-dated records
- ✅ Validation: Egg count >= 0
- ✅ Mobile-optimized quick-add flow (< 30 seconds)

**Technical Scope:**
- **Frontend:**
  - FAB button on dashboard (Material-UI Fab)
  - Quick-add modal (flock dropdown, date picker, egg count input)
  - Daily records list page
  - Date shortcuts (Today, Yesterday)
  - Large touch-friendly number input (80x60px buttons)
- **Backend:**
  - CQRS handlers: CreateDailyRecordCommand, UpdateDailyRecordCommand, DeleteDailyRecordCommand, GetDailyRecordsQuery
  - Validation: Unique constraint on (flock_id, record_date)
  - Endpoints: POST /api/daily-records, GET /api/daily-records, PUT /api/daily-records/{id}, DELETE /api/daily-records/{id}
- **Database:**
  - `daily_records` table with RLS
  - Unique index on (flock_id, record_date)
  - Index on record_date for range queries

**Dependencies:** M3 (requires flocks to record against)

**Out of Scope:**
- Offline mode (M10)
- Notes field
- Photo attachments
- Bulk record creation

---

### M5: Purchase Tracking

**Value Delivered:** Users can track all chicken-related expenses

**User Story:** As a farmer, I want to record my purchases (feed, bedding, vitamins), so that I can calculate my total costs.

**Acceptance Criteria:**
- ✅ User can create a purchase (type, name, date, price, quantity, unit)
- ✅ User sees list of purchases (with filters: type, date range)
- ✅ User can edit purchase details
- ✅ User can delete a purchase
- ✅ Purchase name autocomplete from history
- ✅ Purchase types: Feed, Vitamins, Bedding, Toys, Veterinary, Other
- ✅ Units: kg, pcs, l, package, other
- ✅ Optional flock/coop assignment

**Technical Scope:**
- **Frontend:**
  - Purchase list page with filters
  - Create/edit purchase form (type dropdown, name autocomplete, date, price, quantity, unit)
  - Type-based icons and color coding
  - Purchase summary card (total spent this month)
  - Material-UI Autocomplete for name field
- **Backend:**
  - CQRS handlers: CreatePurchaseCommand, UpdatePurchaseCommand, DeletePurchaseCommand, GetPurchasesQuery
  - Autocomplete endpoint: GET /api/purchases/names?query={text}
  - Endpoints: GET /api/purchases, POST /api/purchases, PUT /api/purchases/{id}, DELETE /api/purchases/{id}
- **Database:**
  - `purchases` table with RLS
  - Enums: purchase_type, quantity_unit
  - Indexes on tenant_id, purchase_date, type

**Dependencies:** M3 (optional flock reference)

**Out of Scope:**
- Receipt photo uploads
- Recurring purchases
- Budget tracking
- Supplier management

---

### M6: Egg Cost Calculation Dashboard

**Value Delivered:** Users can see the true cost per egg

**User Story:** As a farmer, I want to see how much one egg costs me (including all expenses), so that I can make informed decisions about profitability.

**Acceptance Criteria:**
- ✅ Dashboard shows current egg cost (CZK/egg)
- ✅ Dashboard shows today's totals (eggs, hens)
- ✅ Dashboard shows weekly production summary
- ✅ Dashboard shows flock status (total animals, active flocks)
- ✅ Calculation: Total costs / Total eggs produced
- ✅ Trend indicators (↑↓ vs previous period)
- ✅ Chicks count in costs but NOT in production
- ✅ Empty state UI (no data yet)

**Technical Scope:**
- **Frontend:**
  - Dashboard widgets (Material-UI Grid)
  - Egg cost card (large metric display)
  - Today's summary card
  - Weekly summary card
  - Flock status card
  - Trend arrows with color coding
- **Backend:**
  - Statistics endpoint: GET /api/statistics/dashboard
  - Egg cost calculation query:
    - Total costs = SUM(purchases.amount)
    - Total eggs = SUM(daily_records.egg_count)
    - Cost per egg = Total costs / Total eggs
  - Performance: Optimize with database aggregation
- **Database:**
  - No new tables, uses existing data
  - Consider materialized view for performance (future)

**Dependencies:** M4 (daily records) + M5 (purchases)

**Out of Scope:**
- Detailed cost breakdown charts (M11)
- Historical cost trends (M11)
- Flock comparison (M11)
- Export reports

---

### M7: Flock Composition Editing

**Value Delivered:** Users can manually adjust flock composition

**User Story:** As a farmer, I want to record when chickens die, are sold, or are purchased, so that my flock counts stay accurate.

**Acceptance Criteria:**
- ✅ User can adjust flock composition (hens, roosters, chicks)
- ✅ User provides reason for change (notes)
- ✅ New flock history record created (type: "adjustment")
- ✅ Validation: Cannot reduce counts below zero
- ✅ UI shows before/after comparison
- ✅ Confirmation dialog for destructive changes

**Technical Scope:**
- **Frontend:**
  - "Edit Composition" button on flock detail page
  - Adjustment form (current counts → new counts)
  - Delta display (+/- indicators)
  - Notes field (required for adjustments)
  - Confirmation dialog with summary
- **Backend:**
  - CQRS handler: AdjustFlockCompositionCommand
  - Creates flock_history record (type: "adjustment")
  - Updates flock current counts
  - Endpoint: POST /api/flocks/{id}/adjust
- **Database:**
  - Uses existing tables (flocks, flock_history)

**Dependencies:** M3 (requires flocks)

**Out of Scope:**
- Batch adjustments
- Undo functionality
- Reasons dropdown (free-text only)

---

### M8: Chick Maturation

**Value Delivered:** Users can convert chicks to adult chickens

**User Story:** As a farmer, I want to record when my chicks mature into adult hens and roosters, so that my production calculations are accurate.

**Acceptance Criteria:**
- ✅ User can mature chicks (select count, split into hens/roosters)
- ✅ Validation: hens + roosters = chicks to mature
- ✅ Validation: chicks to mature <= current chicks in flock
- ✅ Validation: maturation date >= hatch date
- ✅ New flock history record created (type: "maturation")
- ✅ Flock counts updated automatically
- ✅ UI shows calculation preview

**Technical Scope:**
- **Frontend:**
  - "Mature Chicks" button on flock detail page
  - Maturation form (chicks count, split into hens/roosters, date, notes)
  - Real-time validation with error messages
  - Preview of resulting composition
- **Backend:**
  - CQRS handler: MatureChicksCommand
  - Business logic:
    - Validate sum (hens + roosters = chicks)
    - Validate availability (chicks <= current)
    - Update flock: chicks -= X, hens += Y, roosters += Z
    - Create history record (type: "maturation")
  - Endpoint: POST /api/flocks/{id}/mature-chicks
- **Database:**
  - Uses existing tables (flocks, flock_history)

**Dependencies:** M3 (requires flocks with chicks)

**Out of Scope:**
- Automatic maturation reminders
- Age-based suggestions
- Mortality rate tracking during maturation

---

### M9: Flock History View

**Value Delivered:** Users can see complete flock composition history

**User Story:** As a farmer, I want to see the full timeline of my flock's changes (deaths, maturations, purchases), so that I understand how my flock evolved.

**Acceptance Criteria:**
- ✅ User can view flock history timeline
- ✅ Timeline shows all composition changes chronologically
- ✅ Each entry shows: date, change type, counts, delta, notes
- ✅ Visual indicators for change types (icons, colors)
- ✅ User can edit notes on history entries
- ✅ Entries sorted newest first

**Technical Scope:**
- **Frontend:**
  - Flock history page (accessible from flock detail)
  - Vertical timeline component (Material-UI Timeline)
  - Change type icons (initial, adjustment, maturation)
  - Delta displays (+/- with color coding)
  - Expandable notes section
  - Edit notes inline
- **Backend:**
  - Query: GET /api/flocks/{id}/history
  - Returns all flock_history records sorted by change_date DESC
  - Update endpoint: PATCH /api/flock-history/{id}/notes
- **Database:**
  - Uses existing flock_history table
  - Index on (flock_id, change_date)

**Dependencies:** M3 (initial history) + M7 (adjustments) + M8 (maturations)

**Out of Scope:**
- Charts of flock size over time (M11)
- Comparison between flocks (M11)
- Export history as CSV

---

### M10: Offline Egg Recording

**Value Delivered:** Users can record eggs without internet connection

**User Story:** As a farmer, I want to record egg production even when I'm outdoors without internet, so that I never miss a day of logging.

**Acceptance Criteria:**
- ✅ Service worker caches app assets (HTML, CSS, JS)
- ✅ User can create daily records while offline
- ✅ Records queued in IndexedDB
- ✅ Background sync triggers when online
- ✅ Offline indicator banner shown
- ✅ Sync status indicator (pending count)
- ✅ Manual sync button
- ✅ Toast notification on successful sync

**Technical Scope:**
- **Frontend:**
  - Workbox service worker configuration
  - Cache-first strategy for static assets (30 days)
  - Network-first for API GET requests (5 min cache)
  - IndexedDB integration (Dexie.js)
  - Background Sync API for POST requests
  - Offline banner component (persistent)
  - Sync queue UI (bottom bar with pending count)
- **Backend:**
  - No changes required (existing endpoints handle sync)
  - Idempotency considerations for daily records
- **Infrastructure:**
  - Service worker registration in index.html
  - Workbox webpack plugin configuration

**Dependencies:** M4 (daily records to sync)

**Out of Scope:**
- Offline mode for purchases (POST)
- Offline mode for flock edits (complex conflict resolution)
- Advanced conflict resolution (last-write-wins only)

---

### M11: Statistics Dashboard

**Value Delivered:** Users can analyze trends and productivity

**User Story:** As a farmer, I want to see charts and trends of my production and costs, so that I can optimize my operation.

**Acceptance Criteria:**
- ✅ Egg cost breakdown chart (pie chart by purchase type)
- ✅ Production trend chart (line chart, last 30 days)
- ✅ Cost trend chart (line chart, cost per egg over time)
- ✅ Flock productivity comparison (bar chart, eggs/hen/day)
- ✅ Date range filters (7 days, 30 days, 3 months, custom)
- ✅ Flock filter (all / specific flock)
- ✅ Mobile-optimized charts (touch-friendly, responsive)

**Technical Scope:**
- **Frontend:**
  - Statistics page with chart grid
  - Recharts integration (pie, line, bar charts)
  - Filter controls (date range picker, flock selector)
  - Chart tooltips and legends
  - Loading skeletons
  - Empty state handling
- **Backend:**
  - Statistics endpoint: GET /api/statistics/egg-cost
  - Cost breakdown calculation (group by purchase type)
  - Timeline calculation (aggregate by week/month)
  - Flock productivity calculation (eggs per hen per day)
- **Database:**
  - Complex aggregation queries
  - Consider database indexes for performance

**Dependencies:** M6 (egg cost calculation)

**Out of Scope:**
- Predictive analytics
- Goal setting
- Alerts/notifications for trends
- Export charts as images

---

### M12: PWA Installation

**Value Delivered:** Users can install app on home screen like a native app

**User Story:** As a farmer, I want to install Chickquita on my phone's home screen, so that I can access it quickly like a native app.

**Acceptance Criteria:**
- ✅ Web app manifest configured (name, icons, theme)
- ✅ App icons in all required sizes (72x72, 192x192, 512x512)
- ✅ Install prompt shown after 2nd visit or 5 minutes usage
- ✅ Custom install banner (Czech + English)
- ✅ iOS "Add to Home Screen" instructions
- ✅ Standalone display mode (no browser chrome)
- ✅ Splash screen with branding
- ✅ Lighthouse PWA score > 90

**Technical Scope:**
- **Frontend:**
  - manifest.json configuration
  - App icons (maskable + any purpose)
  - Install prompt handler (beforeinstallprompt event)
  - Custom install banner component
  - iOS detection and instructions modal
  - Theme color and background color
- **Infrastructure:**
  - Icon generation (multiple sizes)
  - Screenshot generation for app stores
  - manifest.json served from root

**Dependencies:** M10 (service worker for PWA)

**Out of Scope:**
- App store submission (Google Play, Apple App Store)
- Push notifications
- Advanced PWA features (Web Share API, etc.)

---

## 9. Future Enhancements

Features beyond the 12 core milestones, prioritized for future development.

### 11.1 High Priority (Next Wave)

**Push Notifications**
- Daily reminders (19:00): "Don't forget to log eggs"
- Sync completed: "3 records saved"
- Chicks ready to mature: "Chicks are 6 weeks old"
- **Complexity:** Medium (Web Push API + backend notification service)

**Advanced Flock Statistics**
- Flock size over time (area chart)
- Productivity trends (eggs/hen/day)
- Flock comparison (side-by-side)
- Historical composition chart
- **Complexity:** Medium (aggregation queries + chart components)

**Dark Mode**
- Theme toggle in settings
- Persist preference
- Material-UI theme switching
- **Complexity:** Small

**Swipe Gestures**
- Swipe to delete (with undo)
- Swipe to archive
- Pull to refresh
- **Complexity:** Small

---

### 11.2 Medium Priority

**Individual Chicken Tracking**
- CRUD individual chickens
- Link to flock
- Notes and photo per chicken
- Death/sale tracking
- **Complexity:** Large (new entities, complex UI)
- **Why not in core:** Not essential for profitability tracking

**Photo Uploads**
- Chicken photos
- Coop photos
- Receipt photos for purchases
- Client-side compression
- Azure Blob Storage integration
- **Complexity:** Large (storage costs, offline sync complexity)
- **Why not in core:** Not essential for MVP, adds infrastructure costs

**CSV/PDF Exports**
- Export daily records as CSV
- Export purchases as CSV
- Generate PDF reports (dashboard snapshot)
- Email reports (weekly/monthly)
- **Complexity:** Medium (report generation library)

**Breed/Color Tags**
- Free-text tags per flock
- Tag-based filtering
- Popular tags autocomplete
- **Complexity:** Small

**Multi-language Expansion**
- Additional languages beyond Czech/English
- Community translations
- **Complexity:** Medium (translation effort)

---

### 11.3 Low Priority (Nice to Have)

**Voice Input**
- Voice-to-text for notes
- Web Speech API
- **Complexity:** Small
- **Why not in core:** Limited browser support, not critical

**Calendar View**
- Daily records in calendar format
- Color-coded by production level
- Month/week view toggle
- **Complexity:** Medium

**Social Logins**
- Google Sign-In
- Facebook Sign-In
- Still free tier in Clerk
- **Complexity:** Small
- **Why not in core:** Email/password sufficient for MVP

**Multi-Factor Authentication**
- SMS/App-based MFA
- Requires Clerk Pro tier ($25/month)
- **Complexity:** Small (Clerk handles it)
- **Why not in core:** Cost, not essential for initial users

**Onboarding Tutorial**
- First-time user walkthrough
- Interactive tour of features
- Tooltips and hints
- **Complexity:** Medium

---

### 11.4 Future Exploration

**Income Tracking (ROI)**
- Record egg sales (date, quantity, price, buyer)
- Profit/loss calculation (income - costs)
- ROI dashboard
- Break-even analysis
- **Complexity:** Large (new domain, UI, calculations)
- **Why not in core:** Users need cost tracking first before income

**Accounting Software Integration**
- Export to Money S3
- Export to Pohoda
- Export to other Czech accounting tools
- **Complexity:** Large (API integrations, format conversions)

**E-commerce Integration**
- Online store for egg sales
- Inventory management
- Customer orders
- **Complexity:** Very Large (separate product)

**Machine Learning Predictions**
- Production forecasting
- Anomaly detection (unusual drops)
- Feed optimization recommendations
- **Complexity:** Very Large (ML models, training data)

**Community Features**
- Share statistics (anonymized)
- Benchmark against other farmers
- Discussion forum
- Best practices sharing
- **Complexity:** Large (moderation, privacy concerns)

**Advanced Conflict Resolution**
- Detect offline sync conflicts
- UI for choosing versions (Keep mine / Take theirs / Merge)
- Field-level merge strategies
- **Complexity:** Large (complex UI, backend logic)

---

## 10. Open Questions for Discussion

### 11.1 Currency
**Question:** CZK only? Or multi-currency?

**Options:**
- **A) CZK only:** Simpler, targeting Czech market
- **B) Multi-currency:** EUR, USD, CZK (select on registration)

**Recommendation:** Start with CZK only, Phase 2 multi-currency

### 11.2 Quantity Units
**Question:** Preferred units for feed (kg, pcs, liters)? Or optional?

**Options:**
- **A) Predefined:** kg, pcs, l, package (dropdown)
- **B) Custom:** User can add own units
- **C) Hybrid:** Predefined + custom option

**Recommendation:** Start with predefined (A), Phase 2 custom

### 11.3 Photos
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

### 11.4 Breed/Color
**Question:** Record breed or chicken color?

**Options:**
- **A) No breeding info:** Just counts
- **B) Breed field:** Dropdown with breeds (Leghorn, Rhode Island Red, etc.)
- **C) Custom tags:** Free tags (color, breed, etc.)

**Recommendation:** Phase 2, custom tags in flock notes (interim)

### 11.5 Egg Sales (ROI)
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

### 11.6 Notifications
**Question:** Push notifications for daily record reminder?

**Options:**
- **A) No notifications:** User must remember
- **B) Phase 2:** Push notifications (19:00 daily)
- **C) MVP:** Email reminders (simpler)

**Recommendation:** Phase 2 push notifications

### 11.7 Initial Costs Data Model
**Question:** How to track initial costs (purchasing chickens/chicks)?

**Options:**
- **A) Ignore:** Assume hatching, initial cost = 0
- **B) Purchase type:** Type "Animal purchase" in Purchase agenda
- **C) Flock initial cost:** "Initial cost" field when creating flock

**Recommendation:** B - Purchase type "Animal purchase"

### 11.8 Historical Data
**Question:** Need to import historical data (migration from Excel)?

**Options:**
- **A) No import:** Start from scratch
- **B) CSV import:** Simple import from CSV
- **C) Excel import:** Automatic Excel file parsing

**Recommendation:** Phase 2 - CSV import

---

## 11. Risks & Mitigation

### 11.1 Technical Risks

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

### 11.2 Business Risks

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

### 11.3 UX Risks

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

Chickquita is a **mobile-first PWA** application for tracking chicken farming financial profitability with focus on:
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

**Document:** Chickquita PRD v2.0
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
