# Technical Specifications - Chickquita PWA

## 1. System Architecture

### 1.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Client (PWA)                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  React 18+   │  │ Service      │  │  IndexedDB   │      │
│  │  TypeScript  │  │ Worker       │  │  (Dexie.js)  │      │
│  │  Material-UI │  │ (Workbox)    │  │              │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            ↕ HTTPS (JWT Bearer Token)
┌─────────────────────────────────────────────────────────────┐
│                 Authentication Layer                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Clerk.com (Hosted Auth)                             │   │
│  │  - Sign-up/Sign-in UI                                │   │
│  │  - JWT Token Management                              │   │
│  │  - Webhooks (user.created)                           │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↕ JWT Validation
┌─────────────────────────────────────────────────────────────┐
│                   Backend (.NET 8 API)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Minimal API │  │  MediatR     │  │  EF Core     │      │
│  │  Endpoints   │  │  (CQRS)      │  │  (Code First)│      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            ↕ Parameterized Queries
┌─────────────────────────────────────────────────────────────┐
│              Database (Neon Postgres)                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Row-Level Security (RLS) Policies                   │   │
│  │  - Tenant Isolation                                  │   │
│  │  - SET app.current_tenant_id = <tenant_id>          │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Multi-Tenant Architecture

**Tenant Isolation Strategy:**
1. **Database Level (Primary):** Row-Level Security (RLS) policies on all tables
2. **Application Level (Secondary):** EF Core global query filters
3. **Request Level:** Middleware sets RLS context per request

**Data Partitioning:**
- All tables include `tenant_id UUID NOT NULL` column
- Foreign key: `tenant_id REFERENCES tenants(id) ON DELETE CASCADE`
- RLS Policy Example:
  ```sql
  CREATE POLICY tenant_isolation ON coops
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
  ```

**Request Flow:**
```
1. Client sends request with Clerk JWT
2. Backend validates JWT → extracts ClerkUserId
3. Backend queries tenants table → retrieves TenantId
4. Backend sets RLS context: SET app.current_tenant_id = <tenant_id>
5. All subsequent queries automatically filtered by tenant_id
6. Response returned to client
```

---

## 2. Core Business Rules

### 2.1 Egg Cost Calculation
```
Total costs = SUM(all purchases.amount)
Total eggs = SUM(daily_records.egg_count WHERE flock has hens > 0)

Egg cost per unit = Total costs / Total eggs

IMPORTANT:
- Chicks count in costs (feed consumption)
- Chicks do NOT count in production (only hens lay eggs)
```

### 2.2 Flock Composition Changes
1. **Initial Composition:** First flock_history record (type: `initial`)
2. **Manual Adjustment:** User-initiated changes (type: `adjustment`)
3. **Chick Maturation:** Converting chicks to adults (type: `maturation`)
   - Validation: `resultingHens + resultingRoosters = chicksCount`
   - Validation: `chicksCount <= flock.current_chicks`
   - Updates flock: `chicks -= X, hens += Y, roosters += Z`

### 2.3 Daily Records
- One record per flock per day (unique constraint)
- Cannot create future-dated records
- Same-day edit allowed, historical edit restricted
- Offline-capable with background sync queue

---

## 3. API Specifications

### 3.1 Authentication Endpoints

#### POST /api/webhooks/clerk
**Description:** Clerk webhook handler for user sync (user.created event)

**Request Headers:**
```
svix-signature: v1,<signature>
svix-timestamp: <timestamp>
svix-id: <webhook-id>
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

---

### 3.2 Flock History Endpoints (M9)

#### GET /api/flocks/{id}/history
**Description:** Get full flock change history timeline

**Authorization:** Clerk JWT required

**Response (200):**
```json
[
  {
    "id": "770e8400-e29b-41d4-a716-446655440003",
    "flockId": "550e8400-e29b-41d4-a716-446655440002",
    "changeDate": "2024-02-04",
    "hens": 22,
    "roosters": 5,
    "chicks": 5,
    "changeType": "maturation",
    "notes": "First batch from hatchery",
    "createdAt": "2024-02-04T07:30:00Z"
  },
  {
    "id": "770e8400-e29b-41d4-a716-446655440002",
    "flockId": "550e8400-e29b-41d4-a716-446655440002",
    "changeDate": "2024-01-28",
    "hens": 10,
    "roosters": 2,
    "chicks": 20,
    "changeType": "adjustment",
    "notes": "Death - illness",
    "createdAt": "2024-01-28T10:00:00Z"
  },
  {
    "id": "770e8400-e29b-41d4-a716-446655440001",
    "flockId": "550e8400-e29b-41d4-a716-446655440002",
    "changeDate": "2024-01-15",
    "hens": 10,
    "roosters": 2,
    "chicks": 22,
    "changeType": "initial",
    "notes": "Hatched",
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```

#### PATCH /api/flock-history/{id}/notes
**Description:** Update notes on history record

**Request Body:**
```json
{
  "notes": "Updated note text"
}
```

**Response (200):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440003",
  "notes": "Updated note text"
}
```

---

### 3.3 Statistics Endpoints (M11)

#### GET /api/statistics/egg-cost
**Description:** Egg cost statistics with breakdown

**Query Parameters:**
- `from`: ISO date (optional)
- `to`: ISO date (optional)
- `flockId`: UUID (optional)

**Response (200):**
```json
{
  "eggCost": 4.20,
  "totalCosts": 2520.00,
  "totalEggs": 600,
  "costBreakdown": [
    {
      "category": "feed",
      "amount": 1638.00,
      "percentage": 65
    },
    {
      "category": "bedding",
      "amount": 504.00,
      "percentage": 20
    },
    {
      "category": "vitamins",
      "amount": 252.00,
      "percentage": 10
    },
    {
      "category": "veterinary",
      "amount": 126.00,
      "percentage": 5
    }
  ],
  "timeline": [
    {
      "date": "2024-01-29",
      "cost": 4.35,
      "eggs": 140,
      "costs": 609.00
    },
    {
      "date": "2024-02-05",
      "cost": 4.20,
      "eggs": 156,
      "costs": 655.20
    }
  ],
  "flockProductivity": [
    {
      "flockId": "flock_1",
      "flockName": "Browns 2024",
      "eggsPerHenPerDay": 0.83,
      "totalEggs": 350,
      "averageHens": 15
    },
    {
      "flockId": "flock_2",
      "flockName": "Whites 2023",
      "eggsPerHenPerDay": 0.75,
      "totalEggs": 250,
      "averageHens": 12
    }
  ]
}
```

**Calculation Logic:**
```sql
-- Egg cost
WITH total_costs AS (
    SELECT SUM(amount) AS total
    FROM purchases
    WHERE tenant_id = current_setting('app.current_tenant_id')::UUID
      AND purchase_date >= $from
      AND purchase_date <= $to
),
total_eggs AS (
    SELECT SUM(egg_count) AS total
    FROM daily_records
    WHERE tenant_id = current_setting('app.current_tenant_id')::UUID
      AND record_date >= $from
      AND record_date <= $to
)
SELECT
    tc.total / NULLIF(te.total, 0) AS egg_cost,
    tc.total AS total_costs,
    te.total AS total_eggs
FROM total_costs tc, total_eggs te;

-- Cost breakdown by type
SELECT
    type AS category,
    SUM(amount) AS amount,
    (SUM(amount) / (SELECT SUM(amount) FROM purchases WHERE ...)) * 100 AS percentage
FROM purchases
WHERE tenant_id = current_setting('app.current_tenant_id')::UUID
  AND purchase_date >= $from
  AND purchase_date <= $to
GROUP BY type;

-- Flock productivity (eggs per hen per day)
SELECT
    f.id AS flock_id,
    f.identifier AS flock_name,
    SUM(dr.egg_count) / AVG(f.current_hens) / COUNT(DISTINCT dr.record_date) AS eggs_per_hen_per_day,
    SUM(dr.egg_count) AS total_eggs,
    AVG(f.current_hens) AS average_hens
FROM flocks f
JOIN daily_records dr ON f.id = dr.flock_id
WHERE f.tenant_id = current_setting('app.current_tenant_id')::UUID
  AND dr.record_date >= $from
  AND dr.record_date <= $to
GROUP BY f.id, f.identifier;
```

---

## 4. Frontend Architecture

### 4.1 Offline Mode (M10)

#### Service Worker Configuration (Workbox)
```javascript
// workbox-config.js
module.exports = {
  globDirectory: 'dist/',
  globPatterns: [
    '**/*.{html,js,css,png,jpg,webp,svg,woff2}'
  ],
  swDest: 'dist/sw.js',

  runtimeCaching: [
    // Static assets: Cache-first (30 days)
    {
      urlPattern: /\.(?:png|jpg|jpeg|svg|gif|webp|woff2)$/,
      handler: 'CacheFirst',
      options: {
        cacheName: 'static-resources',
        expiration: {
          maxEntries: 60,
          maxAgeSeconds: 30 * 24 * 60 * 60, // 30 days
        },
      },
    },

    // API GET: Network-first (5 min cache)
    {
      urlPattern: ({url, request}) =>
        url.pathname.startsWith('/api/') && request.method === 'GET',
      handler: 'NetworkFirst',
      options: {
        cacheName: 'api-cache',
        networkTimeoutSeconds: 3,
        expiration: {
          maxEntries: 50,
          maxAgeSeconds: 5 * 60, // 5 minutes
        },
      },
    },

    // API POST/PUT/DELETE: Background sync
    {
      urlPattern: ({url, request}) =>
        url.pathname.startsWith('/api/') &&
        ['POST', 'PUT', 'DELETE'].includes(request.method),
      handler: 'NetworkOnly',
      options: {
        backgroundSync: {
          name: 'apiQueue',
          options: {
            maxRetentionTime: 24 * 60, // 24 hours
          },
        },
      },
    },
  ],
};
```

#### IndexedDB Schema (Dexie.js)
```typescript
// db.ts
import Dexie, { Table } from 'dexie';

interface PendingRequest {
  id?: number;
  method: string;
  url: string;
  body: any;
  timestamp: string;
  retryCount: number;
}

interface SyncStatus {
  key: string;
  lastSync: string;
  status: 'syncing' | 'success' | 'error';
}

class ChickquitaDB extends Dexie {
  pendingRequests!: Table<PendingRequest, number>;
  syncStatus!: Table<SyncStatus, string>;

  constructor() {
    super('ChickquitaDB');

    this.version(1).stores({
      pendingRequests: '++id, method, url, timestamp',
      syncStatus: 'key',
    });
  }
}

export const db = new ChickquitaDB();
```

#### Background Sync Logic
```typescript
// syncManager.ts
import { db } from './db';
import apiClient from '@/lib/apiClient';

export async function syncPendingRequests() {
  const pending = await db.pendingRequests.toArray();

  for (const request of pending) {
    try {
      const response = await apiClient({
        method: request.method,
        url: request.url,
        data: request.body,
      });

      // Success: Remove from queue
      await db.pendingRequests.delete(request.id!);
      showToast('✓ Záznam úspěšně uložen');

    } catch (error) {
      // Retry with exponential backoff
      const newRetryCount = request.retryCount + 1;

      if (newRetryCount >= 5) {
        showErrorToast('⚠️ Nepodařilo se uložit záznam');
        continue;
      }

      await db.pendingRequests.update(request.id!, {
        retryCount: newRetryCount,
      });

      // Schedule retry
      const delay = Math.pow(2, newRetryCount) * 1000; // 2s, 4s, 8s, 16s, 32s
      setTimeout(() => syncPendingRequests(), delay);
    }
  }
}

// Listen for online event
window.addEventListener('online', () => {
  syncPendingRequests();
});
```

---

## 5. PWA Specifications (M12)

### 5.1 Manifest Configuration
```json
// public/manifest.json
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

### 5.2 Install Prompt Handler
```typescript
// hooks/useInstallPrompt.ts
import { useState, useEffect } from 'react';

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

export function useInstallPrompt() {
  const [deferredPrompt, setDeferredPrompt] = useState<BeforeInstallPromptEvent | null>(null);
  const [isInstallable, setIsInstallable] = useState(false);

  useEffect(() => {
    const handler = (e: Event) => {
      e.preventDefault();
      setDeferredPrompt(e as BeforeInstallPromptEvent);
      setIsInstallable(true);
    };

    window.addEventListener('beforeinstallprompt', handler);

    return () => window.removeEventListener('beforeinstallprompt', handler);
  }, []);

  const handleInstallClick = async () => {
    if (!deferredPrompt) return;

    await deferredPrompt.prompt();
    const { outcome } = await deferredPrompt.userChoice;

    if (outcome === 'accepted') {
      console.log('User accepted install prompt');
    }

    setDeferredPrompt(null);
    setIsInstallable(false);
  };

  return { isInstallable, handleInstallClick };
}
```

---

## 6. Performance Requirements

### 6.1 Metrics & Targets

| Metric | Target | Measurement Tool |
|--------|--------|------------------|
| Lighthouse Performance | >90 | Chrome DevTools |
| Lighthouse Accessibility | >90 | Chrome DevTools |
| Lighthouse Best Practices | >90 | Chrome DevTools |
| Lighthouse SEO | >90 | Chrome DevTools |
| Lighthouse PWA | >90 | Chrome DevTools |
| First Contentful Paint | <1.5s | Web Vitals |
| Time to Interactive | <3.5s | Web Vitals |
| Largest Contentful Paint | <2.5s | Web Vitals |
| Cumulative Layout Shift | <0.1 | Web Vitals |
| Bundle Size (gzipped) | <200kb | webpack-bundle-analyzer |
| API Response Time (p95) | <500ms | Application Insights |

---

## 7. Security Requirements

### 7.1 Input Validation

**Frontend (Zod):**
```typescript
const matureChicksSchema = z.object({
  date: z.string().refine((val) => new Date(val) <= new Date(), {
    message: 'Cannot mature chicks in the future'
  }),
  chicksCount: z.number().int().positive(),
  resultingHens: z.number().int().min(0),
  resultingRoosters: z.number().int().min(0),
  notes: z.string().max(500).optional(),
}).refine(
  (data) => data.resultingHens + data.resultingRoosters === data.chicksCount,
  {
    message: 'Sum of hens and roosters must equal chicks count',
    path: ['resultingHens'],
  }
);
```

**Backend (FluentValidation):**
```csharp
public class MatureChicksCommandValidator : AbstractValidator<MatureChicksCommand>
{
    public MatureChicksCommandValidator()
    {
        RuleFor(x => x.ChicksCount)
            .GreaterThan(0).WithMessage("Chicks count must be positive");

        RuleFor(x => x.ResultingHens)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ResultingRoosters)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.ResultingHens + x.ResultingRoosters == x.ChicksCount)
            .WithMessage("Sum of hens and roosters must equal chicks count");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Cannot mature chicks in the future");
    }
}
```

---

## 8. Testing Requirements

### 8.1 E2E Tests (Playwright)

**Critical Flows:**
1. **Flock History Timeline (M9):**
   - View flock history
   - See change type icons
   - Edit notes inline
   - Verify timeline order (newest first)

2. **Offline Mode (M10):**
   - Create daily record offline
   - See offline banner
   - Go online
   - Verify background sync
   - Check success toast

3. **Statistics Dashboard (M11):**
   - View egg cost breakdown chart
   - Filter by date range
   - Filter by flock
   - Verify calculations accuracy

4. **PWA Installation (M12):**
   - Trigger install prompt
   - Install app
   - Launch from home screen
   - Verify standalone mode

---

This specification provides the technical foundation for implementing the remaining Chickquita MVP milestones (M9-M12). All implementation should reference this document alongside the PRD and CLAUDE.md for architectural guidance.
