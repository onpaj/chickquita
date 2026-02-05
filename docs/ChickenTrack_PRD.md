# Product Requirements Document: ChickenTrack PWA

**Version:** 1.0  
**Datum:** 4. února 2026  
**Autor:** Ondřej (Ondra)  
**Status:** Draft for Review

---

## Executive Summary

ChickenTrack je PWA aplikace pro sledování finanční rentability chovu slepic s multi-tenant architekturou. Aplikace je primárně navržena jako **mobile-first** řešení, které umožňuje chovatelům efektivně evidovat náklady, produkci a vypočítat ekonomickou efektivitu chovu přímo u kurníků s podporou offline režimu.

### Klíčové hodnoty
- 📱 **Mobile-first PWA** - optimalizováno pro použití na mobilu venku
- 💰 **Finanční transparentnost** - přesný výpočet ceny vejce
- 📊 **Datově řízená rozhodnutí** - statistiky a trendy
- 🔒 **Multi-tenant** - izolace dat mezi chovateli
- 📴 **Offline-first** - funguje i bez připojení

---

## 1. Cíle Produktu

### Primární cíle
1. **Mobile-first PWA přístup** - aplikace primárně navržena pro použití na mobilu venku u kurníků
2. Umožnit chovateli přesně sledovat náklady na chov
3. Vypočítat skutečnou cenu jednoho vejce včetně všech nákladů
4. Evidovat historický vývoj hejna a produkce
5. Poskytovat data pro rozhodování o ekonomické udržitelnosti chovu

### Metriky úspěchu
- Denní logování produkce vajec > 90% dnů
- Přesná evidence všech nákladů
- User retention rate 30+ dní
- Offline usage > 40% všech interakcí
- Lighthouse score > 90 (všechny kategorie)

### Use Case Scenario (typické použití)
```
07:00 - Chovatel jde ke kurníkům
      → Otevře PWA na mobilu (instant load z cache)
      → Quick action: "Přidat denní záznam"
      → Vybere hejno z dropdownu
      → Zadá počet vajec
      → Save (funguje i offline)
      → Data se synchronizují když se vrátí domů
```

---

## 2. Uživatelské Persony

### Persona 1: Hobby chovatel (Primární)
**Profil:**
- Věk: 35-55 let
- Má 1-2 kurníky, celkem 5-20 slepic
- Chov jako koníček, částečná soběstačnost
- Technicky zdatný (smartphone každý den)

**Motivace:**
- Chce vědět, jestli se mu chov vyplací
- Zajímá ho ekonomická stránka
- Potřebuje jednoduchou evidenci bez zbytečné komplexity

**Pain Points:**
- Neví přesně, kolik ho stojí jedno vejce
- Zapomíná zaznamenávat produkci
- Neví, kdy obměnit hejno

**Goals:**
- Rychle zadat denní produkci (< 30 sekund)
- Vidět ekonomiku na první pohled
- Minimální čas strávený administrací

### Persona 2: Semi-profesionální chovatel (Sekundární)
**Profil:**
- Věk: 40-65 let
- Více kurníků, větší hejna (50+ slepic)
- Chov jako vedlejší příjem
- Aktivně optimalizuje náklady

**Motivace:**
- Potřebuje detailní statistiky a trendy
- Porovnává produktivitu hejen
- Plánuje expanzi nebo redukci

**Pain Points:**
- Excel tabulky jsou nepřehledné
- Ztráta dat při havárii počítače
- Složité sdílení s rodinou

**Goals:**
- Vidět produktivitu po hejnech
- Srovnávat různá krmiva
- Export dat pro účetnictví

---

## 3. Funkční Požadavky

### 3.1 Autentizace & Multi-tenancy

#### Registrace
- **Email + heslo**
  - Validace email formátu
  - Heslo: min 8 znaků, 1 velké písmeno, 1 číslo
- **Vytvoření tenantu** - automaticky pro každého uživatele
- **Welcome email** (volitelné - Phase 2)

#### Přihlášení
- Email + heslo
- **Session persistence: 30 dní** (refresh token)
- "Zapomenutí hesla" flow:
  - Reset link na email
  - Platnost 24 hodin
  - Nové heslo

#### Bezpečnost
- JWT token-based authentication
- Refresh token rotation
- Izolace dat mezi tenanty (partition key: TenantId)
- Rate limiting na login endpoint
- HTTPS only

### 3.2 Agenda: Správa Slepic (Hierarchie)

#### 3.2.1 Kurník (Coop)

**Atributy:**
- Název (povinný, max 100 znaků)
- Lokace (text, volitelný, max 200 znaků)
- Datum vytvoření (automaticky)
- Status: Aktivní / Archivovaný

**Operace:**
- Vytvořit kurník
- Upravit kurník (název, lokace)
- Archivovat kurník (soft delete)
- Obnovit archivovaný kurník
- Smazat kurník (pouze pokud nemá hejna)

**Business pravidla:**
- Každý tenant může mít neomezený počet kurníků
- Název musí být unikátní v rámci tenantu
- Archivovaný kurník se nezobrazuje v seznamech

#### 3.2.2 Hejno (Flock)

**Atributy:**
- Referenční ID/označení (povinný, max 50 znaků, např. "Hnědé 2024")
- Datum líhnutí (povinný)
- Kurník (vazba, povinný)
- **Počáteční složení:**
  - Počet slepic (povinný, >= 0)
  - Počet kohoutů (povinný, >= 0)
  - **Počet kuřat (povinný, >= 0)**
- Aktuální složení (vypočítáno z historie)
- Datum vytvoření
- Status: Aktivní / Archivované

**Operace:**
- Vytvořit hejno
- Upravit základní údaje (název, datum líhnutí)
- Archivovat hejno
- Zobrazit historii změn
- **Převést kuřata na dospělé**
- Manuální úprava složení

**Business pravidla:**
- Hejno musí patřit k aktivnímu kurníku
- Označení musí být unikátní v rámci kurníku
- Při archivaci hejna se archivují i denní záznamy
- Minimálně jedna kategorie musí být > 0

#### 3.2.3 Historie složení hejna (FlockHistory)

**Atributy:**
- Datum změny (povinný)
- Počet slepic (povinný)
- Počet kohoutů (povinný)
- **Počet kuřat (povinný)**
- Typ změny (enum):
  - `adjustment` - ruční úprava (úhyn, prodej, nákup)
  - `maturation` - převod kuřat na dospělé
- Poznámka (volitelný, max 500 znaků)

**Operace:**
- Vytvořit záznam historie (automaticky při změnách)
- Zobrazit timeline změn
- Editovat poznámku (pouze)
- Smazat záznam (pouze poslední)

**Business pravidla:**
- Historie je immutable (kromě poznámky)
- První záznam = počáteční stav hejna
- Záznamy seřazeny chronologicky

#### 3.2.4 Akce: Převod kuřat (Chick Maturation)

**Vstupní parametry:**
- Hejno (vazba)
- Datum převodu (povinný)
- Počet kuřat k převodu (povinný, > 0)
- Výsledné rozdělení:
  - Počet nových slepic (povinný, >= 0)
  - Počet nových kohoutů (povinný, >= 0)
- Poznámka (volitelný, max 500 znaků)

**Validace:**
- Součet slepic + kohoutů = počet kuřat k převodu
- Počet kuřat k převodu <= aktuální počet kuřat v hejnu
- Datum převodu >= datum líhnutí

**Výstup:**
- Nový záznam v historii s typem `maturation`
- Aktualizace aktuálního složení hejna:
  - Kuřata: -X
  - Slepice: +Y
  - Kohouti: +Z

**Příklad:**
```
Před převodem:
- Kuřata: 20
- Slepice: 10
- Kohouti: 2

Akce: Převést 15 kuřat → 12 slepic + 3 kohouti

Po převodu:
- Kuřata: 5 (20 - 15)
- Slepice: 22 (10 + 12)
- Kohouti: 5 (2 + 3)
```

#### 3.2.5 Jednotlivé slepice (volitelné - Phase 3)

**Atributy:**
- Identifikátor (jméno/číslo, max 50 znaků)
- Vazba na hejno
- Datum přidání
- Datum odchodu (volitelné)
- Poznámka (volitelný, max 500 znaků)

**Operace:**
- Přidat slepici do hejna
- Označit jako odešlou (úhyn, prodej)
- Zobrazit detail slepice

### 3.3 Agenda: Evidence Krmiva & Nákladů

#### 3.3.1 Nákup položky (Purchase)

**Atributy:**
- Typ položky (povinný, dropdown/tagy):
  - Krmivo
  - Vitamíny a doplňky
  - Stelivo
  - Hračky a vybavení
  - Veterinární péče
  - Jiné
- Název položky (povinný, max 100 znaků, autocomplete z historie)
- Datum nákupu (povinný)
- Cena (povinný, decimální, >= 0)
- Množství (povinný, decimální, > 0)
- Jednotka (povinný, dropdown):
  - kg
  - ks
  - l
  - balení
  - jiné
- Datum spotřeby (volitelný) - pro výpočet délky spotřeby
- Poznámka (volitelný, max 500 znaků)
- Vazba na hejno/kurník (volitelné) - pokud je nákup specifický

**Operace:**
- Vytvořit nákup
- Upravit nákup
- Smazat nákup
- Filtrovat nákupy (typ, datum, hejno)
- Zobrazit historii nákupů

**Business pravidla:**
- Datum spotřeby >= datum nákupu
- Autocomplete názvů z předchozích nákupů
- Výpočet: cena za jednotku = cena / množství

### 3.4 Agenda: Denní Evidence Provozu

#### 3.4.1 Denní záznam (DailyRecord)

**Atributy:**
- Datum (povinný)
- Vazba na hejno (povinný)
- Počet snesených vajec (povinný, celé číslo, >= 0)
- Poznámka k hejnu (volitelný, max 1000 znaků)

**Operace:**
- Vytvořit denní záznam (offline-capable)
- Upravit denní záznam (pouze tentýž den)
- Smazat denní záznam
- Zobrazit historii záznamů
- Quick add z dashboard (modal)

**Business pravidla:**
- Jeden záznam na hejno na den
- Nelze vytvořit záznam pro budoucí datum
- Při offline režimu: queue do background sync
- Po úpravě složení hejna: upozornění na změnu kontextu

#### 3.4.2 Výstražné události (volitelné - Phase 2)

**Typy událostí:**
- Nemoc
- Úhyn
- Snížená aktivita
- Agresivní chování
- Jiné

**Atributy:**
- Datum
- Typ události
- Vazba na hejno / jednotlivou slepici
- Popis (max 1000 znaků)
- Foto (volitelné)

### 3.5 Agenda: Statistiky & Reporting

#### 3.5.1 Dashboard (Přehled)

**Widgety:**
1. **Dnes:**
   - Počet snesených vajec (celkem)
   - Aktuální počet slepic (celkem)

2. **Tento týden:**
   - Celková produkce vajec
   - Průměrná produkce/den
   - Trend (↑↓)

3. **Ekonomika:**
   - Aktuální cena vejce (celkové náklady / celková produkce)
   - Trend ceny (↑↓ oproti minulému měsíci)

4. **Stav hejna:**
   - Celkový počet: slepice / kohouti / kuřata
   - Počet aktivních hejen

**Quick actions:**
- FAB: Přidat denní záznam
- Přidat nákup
- Převést kuřata

#### 3.5.2 Detail: Cena vejce

**Výpočet:**
```
Celkové náklady = Vstupní náklady + Provozní náklady
Vstupní náklady = Cena pořízení hejna
Provozní náklady = SUM(všechny nákupy)

Celková produkce vajec = SUM(denní záznamy)

Cena vejce = Celkové náklady / Celková produkce vajec
```

**Zobrazení:**
- Hlavní metrika: **X Kč / vejce**
- Graf: Vývoj ceny v čase (line chart)
- Breakdown nákladů (pie chart):
  - Krmivo: X %
  - Vitamíny: X %
  - Stelivo: X %
  - Veterinární péče: X %
  - Jiné: X %
- Filtry:
  - Časové období (posledních 7 dní, 30 dní, 3 měsíce, rok, custom)
  - Hejno (všechna / specifické)

**Business pravidla:**
- Kuřata se počítají do nákladů (spotřeba krmiva)
- Kuřata se **nepočítají** do produkce (nenesou vejce)
- Pouze slepice přispívají k produkci

#### 3.5.3 Detail: Vývoj hejna

**Hierarchický přehled:**
```
Kurník 1 - Velký kurník
├─ Hejno A - Hnědé 2024
│  └─ Aktuálně: 15 slepic, 2 kohouti, 3 kuřata
│  └─ Produktivita: 12.5 vajec/den (0.83 vejce/slepice/den)
└─ Hejno B - Bílé 2023
   └─ Aktuálně: 8 slepic, 1 kohout
   └─ Produktivita: 6 vajec/den (0.75 vejce/slepice/den)

Kurník 2 - Malý kurník
└─ Hejno C - Mix 2024
   └─ Aktuálně: 5 slepic, 1 kohout, 10 kuřat
   └─ Produktivita: 4 vajec/den (0.80 vejce/slepice/den)
```

**Detail hejna:**
- **Timeline změn** (vertikální osa času):
  ```
  04.02.2024 - Převod kuřat
    Kuřata: 20 → 5 (-15)
    Slepice: 10 → 22 (+12)
    Kohouti: 2 → 5 (+3)
    Poznámka: První převod z líhně

  28.01.2024 - Úhyn
    Kuřata: 22 → 20 (-2)
    Poznámka: Nemoc

  15.01.2024 - Založení hejna
    Kuřata: 22
    Poznámka: Líhnutí
  ```

- **Grafy:**
  - Velikost hejna v čase (area chart, 3 series: slepice, kohouti, kuřata)
  - Produktivita (vejce/slepice/den) v čase (line chart)

**Produktivita:**
```
Produktivita = Počet vajec / Počet slepic / Počet dní

Příklad:
- Za týden: 84 vajec
- Průměrný počet slepic: 12
- 7 dní
→ Produktivita = 84 / 12 / 7 = 1.0 vejce/slepice/den
```

#### 3.5.4 Exporty (nice-to-have - Phase 3)

**Formáty:**
- CSV export (všechny agendy)
- PDF report (dashboard snapshot)

**CSV obsahuje:**
- Denní záznamy (datum, hejno, počet vajec, poznámka)
- Nákupy (datum, typ, název, cena, množství, jednotka)
- Historie hejna (datum, změny, důvod)

---

## 4. Technické Požadavky

### 4.1 Architecture

#### Frontend - Mobile First PWA

**Technologie:**
- **React 18+** s TypeScript
- **Vite** - build tool (fast refresh)
- **React Router** - routing
- **Zustand / Redux Toolkit** - state management + persistence
- **TanStack Query (React Query)** - server state & caching
- **Axios** - HTTP client s interceptory

**PWA Stack:**
- **Workbox** - service worker management
- **manifest.json** - app manifest
- **IndexedDB** - offline storage (via Dexie.js)
- **Background Sync API** - queue pro offline requests

**UI Framework:**
- **Material-UI (MUI)** nebo **Chakra UI**
  - Proven mobile support
  - Touch-optimized components
  - Theming capabilities
  - Accessibility built-in

**Charting:**
- **Recharts** nebo **Chart.js**
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

**Technologie:**
- **.NET 8** Web API
- **ASP.NET Core** Minimal APIs nebo Controllers
- **Entity Framework Core** (Code First)
- **AutoMapper** - DTO mapping
- **FluentValidation** - request validation
- **Serilog** - structured logging
- **MediatR** - CQRS pattern (volitelné)

**Architecture Pattern:**
- Clean Architecture / Onion Architecture
- Dependency Injection
- Repository + Unit of Work (volitelné)

**Authentication:**
- **JWT Bearer tokens**
- **Refresh tokens** (sliding expiration 30 dní)
- **IdentityUser** + custom tenant claims
- Password hashing: bcrypt nebo ASP.NET Core Identity

**API Design:**
- RESTful principles
- Versioning: URL-based (`/api/v1/...`)
- Consistent error responses
- CORS enabled pro PWA origin

#### Database

**Primární volba: Azure Table Storage**

**Proč Table Storage:**
- ✅ Cost-friendly (0.045 USD/GB/měsíc)
- ✅ Scalable (auto-scaling)
- ✅ Partition key = TenantId (perfektní izolace)
- ✅ Schema flexibility
- ✅ High availability
- ⚠️ Eventual consistency (OK pro use case)
- ⚠️ Omezené query možnosti (workaround: materialized views)

**Alternativa: Azure SQL Database (Basic tier)**
- Pro komplexnější reporting
- Relační integrita
- LINQ queries
- ~5 EUR/měsíc

**Data Model - Table Storage:**
```
Table: Tenants
PartitionKey: "TENANT"
RowKey: TenantId
Columns: Email, PasswordHash, CreatedAt

Table: Coops
PartitionKey: TenantId
RowKey: CoopId
Columns: Name, Location, IsActive, CreatedAt

Table: Flocks
PartitionKey: TenantId
RowKey: FlockId
Columns: CoopId, Identifier, HatchDate, CurrentHens, CurrentRoosters, CurrentChicks, IsActive

Table: FlockHistory
PartitionKey: TenantId_{FlockId}
RowKey: Timestamp_Reverse (pro chronologické třídění)
Columns: Hens, Roosters, Chicks, ChangeType, Notes

Table: Purchases
PartitionKey: TenantId
RowKey: PurchaseId
Columns: Type, Name, Date, Amount, Quantity, Unit, ConsumedDate, Notes, FlockId

Table: DailyRecords
PartitionKey: TenantId_{FlockId}
RowKey: Date_Reverse
Columns: EggCount, Notes
```

**Indexing strategie:**
- Partition Key optimalizace (TenantId)
- Row Key pro časové dotazy (reverse timestamp)
- Point queries > range queries

#### Hosting

**Azure Container Apps** (doporučeno)
- ✅ Managed Kubernetes
- ✅ Auto-scaling (0-N replicas)
- ✅ HTTPS out of the box
- ✅ Custom domains
- ✅ Cost-effective (pay-per-use)
- ~10-30 EUR/měsíc

**Alternativa: Azure Web App for Containers**
- Jednodušší setup
- Basic tier: ~13 EUR/měsíc

**Docker Setup:**
- Multi-stage build (build + runtime)
- Base image: `mcr.microsoft.com/dotnet/aspnet:8.0`
- Build image: `mcr.microsoft.com/dotnet/sdk:8.0`
- Node image pro React build: `node:20-alpine`

**CI/CD:**
- Azure DevOps Pipelines nebo GitHub Actions
- Automatic deployment on main branch
- Preview environments pro PR (volitelné)

**CDN (volitelné - Phase 2):**
- Azure CDN pro static assets
- Caching strategy
- Global distribution

### 4.2 Offline Strategy

#### Service Worker Strategie

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
        maxAgeSeconds: 30 * 24 * 60 * 60, // 30 dní
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
        maxAgeSeconds: 5 * 60, // 5 minut
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
    maxRetentionTime: 24 * 60, // 24 hodin
    onSync: async ({queue}) => {
      // Retry logika
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

**Strategie: Last-Write-Wins (Simple)**
```
Scenario: Offline úprava hejna + online úprava hejna
1. Offline: Změna počtu slepic na 10
2. Online (jiné zařízení): Změna počtu slepic na 12
3. Sync: Porovnej timestamp
   → Novější záznam vyhrává
4. UI: Toast notifikace "Data byla synchronizována"
```

**Phase 2: Conflict Detection**
- Server vrací `ETag` nebo `LastModified`
- Client kontroluje před zápisem
- Při konfliktu: UI s volbou (Keep mine / Take theirs / Merge)

### 4.3 Security

**Authentication Flow:**
```
1. Login → Access Token (15 min) + Refresh Token (30 dní)
2. API calls → Bearer Access Token in header
3. Token expired? → Auto-refresh via Refresh Token
4. Refresh Token expired? → Re-login required
```

**Token Storage:**
- Access Token: Memory (Zustand store)
- Refresh Token: HttpOnly cookie (secure) nebo localStorage (s šifrováním)

**API Security:**
- HTTPS only (enforced)
- CORS: whitelist PWA origins
- Rate limiting:
  - Login: 5 attempts / 15 min / IP
  - API: 100 requests / min / user
- Input validation (FluentValidation)
- SQL Injection protection (parameterized queries)
- XSS protection (sanitize inputs)

**Password Requirements:**
- Minimálně 8 znaků
- Alespoň 1 velké písmeno
- Alespoň 1 číslo
- Alespoň 1 speciální znak (volitelné)

### 4.4 Monitoring & Logging

**Application Insights** (Azure)
- Request tracking
- Exception logging
- Performance metrics
- Custom events (business metrics)

**Frontend Monitoring:**
- Error boundary (React)
- Sentry nebo Azure App Insights JS SDK
- Performance API (Web Vitals)

**Backend Logging (Serilog):**
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

## 5. UI/UX Požadavky

### 5.1 Mobile-First Design Principles

#### Layout Strategie

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
│  [Logo] [Bell] [Menu]   │
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
🐔 Kurníky
📝 Denní záznamy
📊 Statistiky
⋮  Menu (Nákupy, Nastavení)
```

**Floating Action Button (FAB):**
- Primární akce: "Přidat denní záznam"
- Pozice: Bottom-right (60dp margin)
- Velikost: 56x56dp
- Skok na tap (elevation animation)

#### Touch Optimization

**Touch Target Size:**
- Minimum: 44x44px (iOS standard)
- Preferováno: 48x48px (Material Design)
- Spacing mezi targets: 8px minimum

**Gestures:**
- **Swipe to refresh** (pull-to-refresh) - dashboard, seznamy
- **Swipe to delete** - volitelně u seznamů (s undo)
- **Long press** - kontextové menu (volitelné)
- **Pinch to zoom** - grafy (nice-to-have)

**Input Components:**

1. **Number Inputs** (kritické pro rychlost)
   ```
   Počet vajec:
   ┌─────────────────────────┐
   │  [-]    [24]     [+]    │
   └─────────────────────────┘
   Velká tlačítka (80x60px)
   ```

2. **Date Pickers**
   - Native date picker (mobile optimalizovaný)
   - Quick shortcuts: Dnes, Včera, Před týdnem

3. **Dropdowns / Selects**
   - Large touch area
   - Search/filter pro dlouhé seznamy
   - Recent items na vrcholu

4. **Text Areas**
   - Auto-expand při psaní
   - Character counter (volitelný)
   - Voice input button (Phase 2)

#### Forms Best Practices

**Formulářové principy:**
- **Max 5 polí na obrazovku** (scroll pro více)
- **Auto-focus** na první editovatelné pole
- **Tab/Enter navigation** mezi poli
- **Inline validation** (real-time feedback)
- **Sticky submit button** (vždy viditelný)
- **Clear error messages** (co udělat pro nápravu)

**Příklad: Quick Add Denní záznam**
```
┌─────────────────────────────┐
│ Denní záznam                │ ← Header
├─────────────────────────────┤
│ Hejno: [Hnědé 2024    ▾]   │ ← Auto-selected (last used)
│                             │
│ Datum: [Dnes           📅]  │ ← Default today
│                             │
│ Počet vajec:                │
│    [-]     [24]      [+]    │ ← Focus here
│                             │
│ Poznámka (volitelné):       │
│ ┌─────────────────────────┐ │
│ │                         │ │
│ └─────────────────────────┘ │
│                             │
│ [Zrušit]        [Uložit ✓] │ ← Sticky bottom
└─────────────────────────────┘
```

**Validation:**
- ✅ Real-time (při změně pole)
- ✅ Error message pod polem (červená)
- ✅ Success indication (zelená check)
- ✅ Disable submit dokud není validní

### 5.2 PWA Features - Detailní Specifikace

#### 5.2.1 Installation

**Install Prompt Strategie:**
```javascript
// Trigger po 2. návštěvě nebo po 5 minutách použití
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
- Detekce iOS Safari
- Step-by-step guide s obrázky:
  1. Tap Share button (📤)
  2. Scroll & tap "Add to Home Screen"
  3. Tap "Add"

#### 5.2.2 Offline Mode

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

**Sync Indikátor:**
```
Bottom bar:
[3 neuložené záznamy] [Synchronizovat]
```

**Offline Capabilities:**
- ✅ Zobrazení všech cached dat
- ✅ Vytváření denních záznamů
- ✅ Vytváření nákupů
- ✅ Úprava hejna (s conflict warning)
- ❌ Registrace / Login (vyžaduje síť)
- ❌ Statistiky (pokud nejsou v cache)

#### 5.2.3 Background Sync

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
- Immediate retry při obnovení sítě
- Exponential backoff: 1s, 2s, 4s, 8s, 16s, 30s
- Max 5 pokusů
- Po 5 pokusech: manual retry tlačítko

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

#### 5.2.4 Manifest & Icons

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

### 5.3 Klíčové Obrazovky

#### 5.3.1 Dashboard (Home Screen)

```
┌─────────────────────────────┐
│ ChickenTrack   🔔  ⋮        │ ← Header
├─────────────────────────────┤
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ 📅 Dnes                ┃ │
│ ┃ ─────────────────────  ┃ │
│ ┃ 🥚 Vajec: 24           ┃ │
│ ┃ 🐔 Slepic: 32          ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ 📊 Tento týden         ┃ │
│ ┃ ─────────────────────  ┃ │
│ ┃ Vejce: 156 (↑ +12)    ┃ │
│ ┃ Cena/vejce: 4.20 Kč    ┃ │
│ ┃ Produktivita: 0.82     ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ 📈 Quick Stats         ┃ │
│ ┃ ─────────────────────  ┃ │
│ ┃ [Mini graf - týdenní   ┃ │
│ ┃  produkce]             ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│                      [+] ◄── FAB
├─────────────────────────────┤
│ [🏠] [🐔] [📝] [📊] [⋮]     │ ← Bottom Nav
└─────────────────────────────┘
```

**Interactions:**
- Tap Card → Detail view
- FAB (+) → Quick Add denní záznam (modal)
- Pull-to-refresh → Update stats
- Bell icon → Notifications (Phase 2)

#### 5.3.2 Quick Add Modal (Bottom Sheet)

```
┌─────────────────────────────┐
│ Denní záznam           [×]  │
├─────────────────────────────┤
│                             │
│ Hejno:                      │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ Hnědé 2024          ▾  ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ Datum:                      │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ Dnes                📅 ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ Počet vajec:                │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃   [-]   [24]   [+]     ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ Poznámka (volitelně):       │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃                        ┃ │
│ ┃                        ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃        Uložit ✓        ┃ │ ← Sticky
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
└─────────────────────────────┘
```

**UX:**
- Animace: slide-up z bottom
- Backdrop: semi-transparent černá
- Tap mimo modal → Close
- Hejno: auto-select poslední použité
- Datum: default dnes
- Focus: počet vajec (number input)
- Submit: fade-out + success toast

#### 5.3.3 Seznam Kurníků a Hejen

```
┌─────────────────────────────┐
│ ← Kurníky            [+]    │
├─────────────────────────────┤
│ 🔍 Hledat...                │
├─────────────────────────────┤
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ 🏠 Kurník 1 - Velký    ┃ │
│ ┃                        ┃ │
│ ┃ Hnědé 2024             ┃ │
│ ┃ 🐔 15s │ 2k │ 3k      ┃ │ ← slepice│kohouti│kuřata
│ ┃ 🥚 Dnes: 12            ┃ │
│ ┃                    [→] ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ 🏠 Kurník 2 - Malý     ┃ │
│ ┃                        ┃ │
│ ┃ Mix 2024               ┃ │
│ ┃ 🐔 5s │ 1k │ 10k      ┃ │
│ ┃ 🥚 Dnes: 4             ┃ │
│ ┃                    [→] ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
├─────────────────────────────┤
│ [🏠] [🐔] [📝] [📊] [⋮]     │
└─────────────────────────────┘
```

**Interactions:**
- Tap Card → Detail hejna
- Swipe card left → Quick actions (Upravit, Převést kuřata)
- [+] button → Přidat kurník / hejno
- Pull-to-refresh

#### 5.3.4 Detail Hejna

```
┌─────────────────────────────┐
│ ← Hnědé 2024          [⋮]   │
├─────────────────────────────┤
│                             │
│ 📋 Základní info            │
│ ─────────────────────────── │
│ Kurník: Kurník 1 - Velký    │
│ Datum líhnutí: 15.01.2024   │
│ Stáří: 20 dní               │
│                             │
│ 🐔 Aktuální složení         │
│ ─────────────────────────── │
│ Slepice:   15               │
│ Kohouti:    2               │
│ Kuřata:     3               │
│ ─────────────────────────── │
│ Celkem:    20               │
│                             │
│ 📊 Produktivita             │
│ ─────────────────────────── │
│ Dnes:           12 vajec    │
│ Tento týden:    84 vajec    │
│ Na slepici/den: 0.83        │
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ Převést kuřata         ┃ │ ← Primary action
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ 📜 Historie změn            │
│ ─────────────────────────── │
│ ⬤ 04.02.2024 - Převod      │
│   Kuřata: 20 → 5 (-15)     │
│   Slepice: 10 → 22 (+12)   │
│   [...detail]              │
│                             │
│ ⬤ 28.01.2024 - Úhyn        │
│   Kuřata: 22 → 20 (-2)     │
│   [...detail]              │
│                             │
├─────────────────────────────┤
│ [🏠] [🐔] [📝] [📊] [⋮]     │
└─────────────────────────────┘
```

**Menu (⋮):**
- Upravit základní údaje
- Upravit aktuální složení
- Archivovat hejno
- Smazat hejno

#### 5.3.5 Převod Kuřat

```
┌─────────────────────────────┐
│ ← Převod kuřat         [×]  │
├─────────────────────────────┤
│                             │
│ Hejno: Hnědé 2024           │
│ Aktuálně kuřat: 20          │
│                             │
│ Datum převodu:              │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ Dnes                📅 ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ Počet kuřat k převodu:      │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃   [-]   [15]   [+]     ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ Rozdělení:                  │
│ ─────────────────────────── │
│ Slepice:                    │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃   [-]   [12]   [+]     ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ Kohouti:                    │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃   [-]    [3]   [+]     ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ ✓ Součet = 15 kuřat         │ ← Live validation
│                             │
│ Poznámka (volitelně):       │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃ První převod z líhně   ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃      Převést ✓         ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
└─────────────────────────────┘
```

**Validace:**
- Real-time součet: slepice + kohouti = kuřata
- Error: "Součet musí být 15"
- Success: "✓ Součet = 15 kuřat" (zelená)

#### 5.3.6 Statistiky - Cena Vejce

```
┌─────────────────────────────┐
│ ← Statistiky                │
├─────────────────────────────┤
│                             │
│ 💰 Cena vejce               │
│                             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃      4.20 Kč / vejce   ┃ │ ← Large
│ ┃      ↓ -0.15 Kč        ┃ │ ← Trend
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ 📅 Období:                  │
│ [7 dní] [30 dní] [Rok] [...│
│                             │
│ 📈 Vývoj v čase             │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃     Line chart         ┃ │
│ ┃     (cena/vejce)       ┃ │
│ ┃                        ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ 💸 Breakdown nákladů        │
│ ┏━━━━━━━━━━━━━━━━━━━━━━━┓ │
│ ┃   Pie chart            ┃ │
│ ┃   Krmivo: 65%          ┃ │
│ ┃   Stelivo: 20%         ┃ │
│ ┃   Vitamíny: 10%        ┃ │
│ ┃   Jiné: 5%             ┃ │
│ ┗━━━━━━━━━━━━━━━━━━━━━━━┛ │
│                             │
│ 📊 Detail                   │
│ ─────────────────────────── │
│ Celkové náklady: 2,520 Kč   │
│ Celková produkce: 600 vajec │
│ Průměr/den: 24 vajec        │
│                             │
├─────────────────────────────┤
│ [🏠] [🐔] [📝] [📊] [⋮]     │
└─────────────────────────────┘
```

**Interactions:**
- Tap okres → Change filter
- Tap chart → Tooltip with detail
- Scroll down → More details
- Export button (Phase 3)

### 5.4 Accessibility (A11Y)

**WCAG 2.1 Level AA Compliance:**
- ✅ Keyboard navigation (Tab, Enter, Space)
- ✅ Screen reader support (ARIA labels)
- ✅ Color contrast ratio ≥ 4.5:1 (text)
- ✅ Color contrast ratio ≥ 3:1 (UI components)
- ✅ Focus indicators (visible)
- ✅ Touch targets ≥ 44x44px
- ✅ No motion for critical functions (respect prefers-reduced-motion)

**Screen Reader Optimizations:**
- Semantic HTML (header, nav, main, footer, article)
- ARIA landmarks
- Alt text for icons/images
- Form labels properly associated
- Error messages announced

### 5.5 Performance Optimization

**Images:**
- WebP format with fallback
- Lazy loading (loading="lazy")
- Responsive images (srcset)
- Compression: < 100KB per image

**Code Splitting:**
```javascript
const Dashboard = lazy(() => import('./pages/Dashboard'));
const Statistics = lazy(() => import('./pages/Statistics'));
```

**Bundle Size:**
- Main bundle: < 150KB
- Vendor bundle: < 200KB
- Total: < 350KB (gzipped)

**Critical CSS:**
- Inline critical CSS
- Defer non-critical CSS
- Remove unused CSS (PurgeCSS)

---

## 6. Non-Functional Requirements

### 6.1 Performance

**Metrics (Target):**
- **Lighthouse Score:** > 90 (všechny kategorie)
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
- Support pro **1,000+ tenants** (Year 1)
- **10,000 daily records/day** aggregate
- **100 concurrent users**
- **1M API requests/day**

**Database Optimization:**
- Partition key strategy (TenantId)
- Indexing (Row key jako timestamp)
- Query optimization (avoid full table scans)
- Data archival strategy (> 2 roky → archive storage)

**Backend Scaling:**
- Horizontal scaling (Azure Container Apps auto-scale)
- Stateless API (no in-memory sessions)
- CDN for static assets
- Database connection pooling

### 6.3 Reliability & Availability

**Uptime Target:** 99.5% (SLA)
- Scheduled maintenance: < 4 hours/měsíc
- Downtime notifications: Email + in-app banner

**Backup Strategy:**
- Automated daily backups (Azure Table Storage snapshots)
- Retention: 30 dní
- Point-in-time recovery: < 24 hodin

**Disaster Recovery:**
- RTO (Recovery Time Objective): 4 hodiny
- RPO (Recovery Point Objective): 24 hodin
- Failover region: West Europe → North Europe

### 6.4 Security

**Compliance:**
- GDPR compliant (EU users)
- Data encryption at rest (Azure default)
- Data encryption in transit (HTTPS/TLS 1.3)
- Regular security audits (quarterly)

**Authentication:**
- Password hashing: bcrypt (cost factor 12)
- JWT expiration: 15 min (access), 30 dní (refresh)
- Rate limiting:
  - Login: 5 attempts / 15 min / IP
  - API: 100 req / min / user
  - Password reset: 3 attempts / 1 hour / email

**Data Privacy:**
- Tenant data isolation (partition key)
- No cross-tenant data access
- Personal data export (GDPR right)
- Account deletion (GDPR right to be forgotten)

**Input Validation:**
- Frontend: React Hook Form + Zod
- Backend: FluentValidation
- SQL Injection: Parameterized queries only
- XSS: Sanitize user inputs (DOMPurify)

### 6.5 Monitoring & Logging

**Application Insights (Azure):**
- Request tracking (all API calls)
- Exception logging (errors, warnings)
- Custom events:
  - User registration
  - Daily record created
  - Chick maturation
  - Offline sync completed
- Performance counters (CPU, memory, response time)

**Alerting:**
- Error rate > 5%: Slack notification
- API response time > 1s (p95): Email alert
- Downtime: SMS + Email + Slack

**Log Retention:**
- Application logs: 90 dní
- Access logs: 180 dní
- Audit logs: 2 roky

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

#### POST /api/auth/register
**Request:**
```json
{
  "email": "ondra@example.com",
  "password": "SecurePass123"
}
```
**Response (201):**
```json
{
  "userId": "tenant_123",
  "email": "ondra@example.com",
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 900
}
```

#### POST /api/auth/login
**Request:**
```json
{
  "email": "ondra@example.com",
  "password": "SecurePass123"
}
```
**Response (200):**
```json
{
  "userId": "tenant_123",
  "email": "ondra@example.com",
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 900
}
```

#### POST /api/auth/refresh
**Request:**
```json
{
  "refreshToken": "eyJhbGc..."
}
```
**Response (200):**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 900
}
```

#### POST /api/auth/forgot-password
**Request:**
```json
{
  "email": "ondra@example.com"
}
```
**Response (200):**
```json
{
  "message": "Reset link sent to email"
}
```

#### POST /api/auth/reset-password
**Request:**
```json
{
  "token": "reset_token_123",
  "newPassword": "NewSecurePass123"
}
```
**Response (200):**
```json
{
  "message": "Password reset successful"
}
```

### 7.2 Coop Endpoints

#### GET /api/coops
**Response (200):**
```json
[
  {
    "id": "coop_1",
    "name": "Kurník 1 - Velký",
    "location": "Za domem",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z",
    "flocksCount": 2
  }
]
```

#### POST /api/coops
**Request:**
```json
{
  "name": "Kurník 2 - Malý",
  "location": "Před domem"
}
```
**Response (201):**
```json
{
  "id": "coop_2",
  "name": "Kurník 2 - Malý",
  "location": "Před domem",
  "isActive": true,
  "createdAt": "2024-02-04T08:00:00Z"
}
```

#### PUT /api/coops/{id}
**Request:**
```json
{
  "name": "Kurník 2 - Upravený",
  "location": "Vedle garáže"
}
```

#### DELETE /api/coops/{id}
**Response (204):** No content

### 7.3 Flock Endpoints

#### GET /api/flocks?coopId={id}
**Response (200):**
```json
[
  {
    "id": "flock_1",
    "coopId": "coop_1",
    "identifier": "Hnědé 2024",
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
**Request:**
```json
{
  "coopId": "coop_1",
  "identifier": "Bílé 2024",
  "hatchDate": "2024-02-01",
  "initialHens": 0,
  "initialRoosters": 0,
  "initialChicks": 30
}
```

#### PUT /api/flocks/{id}
**Request:**
```json
{
  "identifier": "Bílé 2024 - Upravené",
  "hatchDate": "2024-02-01"
}
```

#### POST /api/flocks/{id}/history
**Manuální úprava složení**
**Request:**
```json
{
  "date": "2024-02-04",
  "hens": 16,
  "roosters": 2,
  "chicks": 2,
  "changeType": "adjustment",
  "notes": "Úhyn 1 kuřete"
}
```

#### POST /api/flocks/{id}/mature-chicks
**Převod kuřat**
**Request:**
```json
{
  "date": "2024-02-04",
  "chicksCount": 15,
  "resultingHens": 12,
  "resultingRoosters": 3,
  "notes": "První převod z líhně"
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
    "notes": "První převod z líhně",
    "createdAt": "2024-02-04T07:30:00Z"
  },
  {
    "id": "history_122",
    "date": "2024-01-28",
    "hens": 10,
    "roosters": 2,
    "chicks": 20,
    "changeType": "adjustment",
    "notes": "Úhyn 2 kuřat - nemoc",
    "createdAt": "2024-01-28T09:00:00Z"
  }
]
```

### 7.4 Purchase Endpoints

#### GET /api/purchases?from={date}&to={date}&type={type}
**Response (200):**
```json
[
  {
    "id": "purchase_1",
    "type": "Krmivo",
    "name": "Krmná směs A",
    "date": "2024-02-01",
    "amount": 250.00,
    "quantity": 25,
    "unit": "kg",
    "consumedDate": "2024-02-15",
    "notes": "Balení 25 kg",
    "flockId": null,
    "createdAt": "2024-02-01T10:00:00Z"
  }
]
```

#### POST /api/purchases
**Request:**
```json
{
  "type": "Vitamíny",
  "name": "Multivitamín",
  "date": "2024-02-04",
  "amount": 120.00,
  "quantity": 1,
  "unit": "balení",
  "consumedDate": null,
  "notes": "Pro celý chov",
  "flockId": null
}
```

#### PUT /api/purchases/{id}
**Request:**
```json
{
  "consumedDate": "2024-02-20",
  "notes": "Spotřebováno dříve než plánováno"
}
```

#### DELETE /api/purchases/{id}
**Response (204):** No content

### 7.5 Daily Record Endpoints

#### GET /api/daily-records?flockId={id}&from={date}&to={date}
**Response (200):**
```json
[
  {
    "id": "record_1",
    "flockId": "flock_1",
    "date": "2024-02-04",
    "eggCount": 12,
    "notes": "Standardní produkce",
    "createdAt": "2024-02-04T07:15:00Z"
  }
]
```

#### POST /api/daily-records
**Request:**
```json
{
  "flockId": "flock_1",
  "date": "2024-02-04",
  "eggCount": 12,
  "notes": ""
}
```
**Response (201):**
```json
{
  "id": "record_1",
  "flockId": "flock_1",
  "date": "2024-02-04",
  "eggCount": 12,
  "notes": "",
  "createdAt": "2024-02-04T07:15:00Z"
}
```

#### PUT /api/daily-records/{id}
**Request:**
```json
{
  "eggCount": 13,
  "notes": "Oprava - přepočítáno"
}
```

#### DELETE /api/daily-records/{id}
**Response (204):** No content

### 7.6 Statistics Endpoints

#### GET /api/statistics/dashboard
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
**Response (200):**
```json
{
  "eggCost": 4.20,
  "totalCosts": 2520.00,
  "totalEggs": 600,
  "costBreakdown": [
    {
      "category": "Krmivo",
      "amount": 1638.00,
      "percentage": 65
    },
    {
      "category": "Stelivo",
      "amount": 504.00,
      "percentage": 20
    },
    {
      "category": "Vitamíny",
      "amount": 252.00,
      "percentage": 10
    },
    {
      "category": "Jiné",
      "amount": 126.00,
      "percentage": 5
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

#### GET /api/statistics/flock-evolution/{flockId}?from={date}&to={date}
**Response (200):**
```json
{
  "flockId": "flock_1",
  "identifier": "Hnědé 2024",
  "timeline": [
    {
      "date": "2024-01-15",
      "hens": 0,
      "roosters": 0,
      "chicks": 22,
      "changeType": "adjustment",
      "notes": "Založení hejna"
    },
    {
      "date": "2024-02-04",
      "hens": 22,
      "roosters": 5,
      "chicks": 5,
      "changeType": "maturation",
      "notes": "První převod"
    }
  ],
  "productivity": [
    {
      "date": "2024-02-04",
      "eggsPerHenPerDay": 0.83
    }
  ]
}
```

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
- `UNAUTHORIZED` (401)
- `FORBIDDEN` (403)
- `NOT_FOUND` (404)
- `CONFLICT` (409) - např. duplicate email
- `RATE_LIMIT_EXCEEDED` (429)
- `INTERNAL_SERVER_ERROR` (500)

---

## 8. Fáze Vývoje (Roadmap)

### 8.1 MVP - Phase 1 (3-4 měsíce)

**Týden 1-2: Setup & Infrastructure**
- ✅ Azure účet + resource groups
- ✅ Docker setup (multi-stage build)
- ✅ CI/CD pipeline (Azure DevOps / GitHub Actions)
- ✅ .NET 8 Web API skeleton
- ✅ React + Vite + PWA setup
- ✅ Azure Table Storage connection

**Týden 3-4: Authentication**
- ✅ Registrace endpoint
- ✅ Login endpoint (JWT + refresh tokens)
- ✅ Zapomenutí hesla flow
- ✅ Frontend: Login/Register screens
- ✅ Token management (interceptors)
- ✅ Protected routes

**Týden 5-6: Kurníky & Hejna (CRUD)**
- ✅ Backend: Coops API
- ✅ Backend: Flocks API (s kuřaty)
- ✅ Frontend: Seznam kurníků
- ✅ Frontend: Detail hejna
- ✅ Frontend: Formuláře (create/edit)

**Týden 7-8: Akce Převodu Kuřat**
- ✅ Backend: Mature chicks endpoint
- ✅ Backend: Flock history tracking
- ✅ Frontend: Převod kuřat modal
- ✅ Frontend: Historie změn hejna

**Týden 9-10: Evidence Nákupů**
- ✅ Backend: Purchases API
- ✅ Frontend: Seznam nákupů
- ✅ Frontend: Přidat nákup (form)
- ✅ Frontend: Autocomplete názvů

**Týden 11-12: Denní Záznamy (Offline-First)**
- ✅ Backend: Daily records API
- ✅ Frontend: Quick Add modal
- ✅ Service Worker setup
- ✅ IndexedDB integration
- ✅ Background Sync queue

**Týden 13-14: Dashboard & Statistiky**
- ✅ Backend: Dashboard stats endpoint
- ✅ Backend: Egg cost calculation
- ✅ Frontend: Dashboard widgets
- ✅ Frontend: Statistika cena vejce
- ✅ Základní grafy (Recharts)

**Týden 15-16: PWA Features & Testing**
- ✅ Manifest.json + icons
- ✅ Install prompt
- ✅ Offline banner
- ✅ Mobile testing (real devices)
- ✅ Performance optimization
- ✅ Lighthouse audit (score > 90)

**Deliverables MVP:**
- Funkční PWA s offline režimem
- Multi-tenant autentizace (30 dní session)
- CRUD Kurníky + Hejna (s kuřaty)
- Akce převodu kuřat
- Evidence nákupů
- Denní záznamy (offline-capable)
- Dashboard s přehledem
- Statistika: cena vejce
- Lighthouse score > 90

### 8.2 Phase 2 (2-3 měsíce)

**Detailní Statistiky**
- Historie změn hejna (timeline view)
- Vývoj velikosti hejna (grafy)
- Produktivita (vejce/slepice/den) v čase
- Srovnání hejen (side-by-side)

**Push Notifikace**
- Denní připomínka (19:00): "Nezapomeň zaznamenat vejce"
- Sync completed: "3 záznamy uloženy"
- Kuřata ready to mature: "Kuřata jsou 6 týdnů stará"

**UX Improvements**
- Install prompt optimization (personalizace)
- Onboarding tutorial (first-time user)
- Dark mode (volitelné)
- Swipe gestures (delete, archive)

**Performance**
- Advanced caching strategies
- Prefetching (anticipate user actions)
- Image optimization (WebP + lazy loading)

### 8.3 Phase 3 (2-3 měsíce)

**Jednotlivé Slepice (Volitelné)**
- CRUD jednotlivé slepice
- Vazba na hejno
- Detail slepice (poznámky, foto)

**Exporty**
- CSV export (všechny agendy)
- PDF report (dashboard snapshot)
- Email reports (týdenní/měsíční)

**Advanced Features**
- Voice input pro poznámky (Web Speech API)
- Photo upload (s compression)
- Calendar view (denní záznamy)
- Multi-language support (EN, DE)

**Offline Enhancements**
- Advanced conflict resolution (merge strategies)
- Offline conflict UI (choose version)
- Offline analytics (track offline usage)

### 8.4 Future Ideas (Backlog)

**Integrace**
- Export do účetního SW (Money S3, Pohoda)
- Integrace s e-shopy (prodej vajec)
- API pro externí aplikace

**Pokročilé Analytiky**
- Machine Learning: predikce produkce
- Anomaly detection (neobvyklý pokles produkce)
- Optimalizace krmiva (cost/benefit analýza)

**Communita**
- Sdílení statistik (anonymizované)
- Benchmark s ostatními chovateli
- Diskuzní fórum

---

## 9. Otevřené Otázky k Diskusi

### 9.1 Uživatelské Role
**Otázka:** Bude jen jeden uživatel = jeden tenant, nebo plánuješ sdílení farmy mezi více uživateli (např. rodina)?

**Možnosti:**
- **A) Single-user:** Jednodušší, každý má svůj účet
- **B) Multi-user:** Jeden účet, více členů rodiny (role: owner, editor, viewer)
- **C) Hybrid:** Možnost pozvat další uživatele (optional feature)

**Dopad:**
- Multi-user vyžaduje:
  - Role-based access control (RBAC)
  - Invite system (email invites)
  - Permission management
  - Audit log (kdo co změnil)

### 9.2 Měna
**Otázka:** CZK? Nebo multi-currency?

**Možnosti:**
- **A) CZK only:** Jednodušší, cílíme na český trh
- **B) Multi-currency:** EUR, USD, CZK (select při registraci)

**Dopad:**
- Multi-currency vyžaduje:
  - Currency field v DB
  - Formátování čísel podle locale
  - Kurzové přepočty (volitelné)

**Doporučení:** Start s CZK only, Phase 2 multi-currency

### 9.3 Jednotky Množství
**Otázka:** Máš preferované jednotky pro krmivo (kg, ks, litry)? Nebo volitelné?

**Možnosti:**
- **A) Predefined:** kg, ks, l, balení (dropdown)
- **B) Custom:** Uživatel může přidat vlastní jednotky
- **C) Hybrid:** Predefined + možnost custom

**Doporučení:** Start s predefined (A), Phase 2 custom

### 9.4 Offline Capabilities
**Otázka:** Jak důležité je plně funkční offline používání? Nebo stačí cache pro zobrazení dat?

**Možnosti:**
- **A) Read-only offline:** Cache pro zobrazení, zápis vyžaduje síť
- **B) Full offline:** Všechny CRUD operace offline + background sync (MVP)
- **C) Selective offline:** Jen denní záznamy offline, zbytek online

**Aktuální návrh:** Full offline pro denní záznamy (B), read-only pro zbytek

### 9.5 Fotografie
**Otázka:** Chtěl bys možnost přidávat foto slepic/kurníků/hejen?

**Možnosti:**
- **A) No photos:** Jen text data (jednodušší, menší storage)
- **B) Phase 2:** Přidat foto support později
- **C) MVP:** Základní foto upload (compress + Azure Blob Storage)

**Dopad:**
- Photos vyžadují:
  - Azure Blob Storage (cost)
  - Image compression (client-side)
  - Thumbnail generation
  - Offline sync complexity

**Doporučení:** Phase 2 nebo 3

### 9.6 Plemeno/Barva
**Otázka:** Evidovat plemeno nebo barvu slepic?

**Možnosti:**
- **A) No breeding info:** Jen počty
- **B) Breed field:** Dropdown s plemeny (Leghorn, Rhode Island Red, atd.)
- **C) Custom tags:** Volné tagy (barva, plemeno, atd.)

**Doporučení:** Phase 2, custom tags v poznámkách hejna (interim)

### 9.7 Prodej Vajec (ROI)
**Otázka:** Plánuješ evidovat i příjmy z prodeje vajec pro ROI výpočet?

**Možnosti:**
- **A) Costs only:** Jen náklady → cena vejce
- **B) Income tracking:** Příjmy z prodeje → ROI, zisk/ztráta
- **C) Hybrid:** Start costs only, Phase 2 income

**Dopad:**
- Income tracking vyžaduje:
  - Sales agenda (datum, počet, cena, kupující)
  - Profit/loss calculations
  - ROI dashboard
  - Tax reporting (volitelné)

**Doporučení:** Start costs only (A), Phase 2 income (B)

### 9.8 Notifikace
**Otázka:** Push notifikace na připomenutí denního záznamu?

**Možnosti:**
- **A) No notifications:** Uživatel musí pamatovat
- **B) Phase 2:** Push notifikace (19:00 denně)
- **C) MVP:** Email reminders (jednodušší)

**Doporučení:** Phase 2 push notifikace

### 9.9 Datový Model - Vstupní Náklady
**Otázka:** Jak evidovat vstupní náklady (nákup slepic/kuřat)?

**Možnosti:**
- **A) Ignorovat:** Předpokládat líhnutí, vstupní náklad = 0
- **B) Purchase type:** Typ "Nákup zvířat" v Purchase agenda
- **C) Flock initial cost:** Pole "Vstupní náklad" při zakládání hejna

**Doporučení:** B - Purchase type "Nákup zvířat"

### 9.10 Historická Data
**Otázka:** Potřebuješ import historických dat (migrace z Excelu)?

**Možnosti:**
- **A) No import:** Start from scratch
- **B) CSV import:** Jednoduchý import z CSV
- **C) Excel import:** Automatický parsing Excel souborů

**Doporučení:** Phase 2 - CSV import

---

## 10. Rizika & Mitigation

### 10.1 Technická Rizika

**1. Offline Sync Konflikty**
- **Riziko:** Ztráta dat při konfliktním merge
- **Mitigation:**
  - Last-write-wins pro MVP (simple)
  - Toast notifikace po syncu
  - Phase 2: Conflict detection UI

**2. Azure Table Storage Limitace**
- **Riziko:** Omezené query možnosti, eventual consistency
- **Mitigation:**
  - Partition key design (TenantId)
  - Materialized views pro complex queries
  - Fallback na Azure SQL pokud potřeba

**3. PWA Install Rate**
- **Riziko:** Uživatelé nenainstalují PWA (50%+ bounce)
- **Mitigation:**
  - Aggressive install prompts
  - Education (benefits highlight)
  - Fallback: Funguje i v browseru

**4. Battery Drain (Offline Sync)**
- **Riziko:** Background sync drains battery
- **Mitigation:**
  - Throttle sync attempts
  - Use Workbox exponential backoff
  - Respect battery saver mode

### 10.2 Business Rizika

**1. Low User Adoption**
- **Riziko:** Cílová skupina nepřijme aplikaci
- **Mitigation:**
  - MVP validace s beta testery (5-10 chovatelů)
  - Iterativní development based on feedback
  - Freemium model (zdarma, later paid pro)

**2. Konkurence**
- **Riziko:** Existující řešení (Excel, jiné apps)
- **Mitigation:**
  - USP: Offline-first, mobile-optimized, ROI focus
  - Differentiace: Czech market, specific use case
  - Community building

**3. Scaling Costs**
- **Riziko:** Azure náklady rostou s users
- **Mitigation:**
  - Cost-effective storage (Table Storage)
  - Auto-scaling s limity
  - Pricing model: Freemium → Paid tiers

### 10.3 UX Rizika

**1. Complexity Creep**
- **Riziko:** Feature bloat → ztráta simplicity
- **Mitigation:**
  - Strict scope per phase
  - User testing každé nové featury
  - "One main action per screen" rule

**2. Offline Confusion**
- **Riziko:** Uživatelé nerozumí offline režimu
- **Mitigation:**
  - Clear UI indikátory
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

## 12. Závěr & Next Steps

### 12.1 Shrnutí

ChickenTrack je **mobile-first PWA** aplikace pro sledování finanční rentability chovu slepic s důrazem na:
- ✅ **Offline-first přístup** (důležité pro použití u kurníků)
- ✅ **Multi-tenant architektura** (izolace dat)
- ✅ **Rychlé logování** (denní záznamy < 30 sekund)
- ✅ **Ekonomická transparentnost** (přesný výpočet ceny vejce)
- ✅ **Cost-effective hosting** (Azure Table Storage + Container Apps)

### 12.2 Next Steps

**1. Upřesnění otevřených otázek (Priorita: HIGH)**
- Odpověz na 10 otázek v sekci 9
- Upřesnění scope MVP

**2. Design Mockupy (Priorita: MEDIUM)**
- Wireframes klíčových obrazovek (Figma)
- User flow diagrams
- Design system (colors, typography, spacing)

**3. Backend API Contract (Priorita: HIGH)**
- Finalizace API endpointů
- OpenAPI/Swagger spec
- Request/Response examples

**4. Database Schema (Priorita: HIGH)**
- Finální Table Storage schema
- Partition key/Row key strategie
- Indexing plan

**5. Development Kickoff (Priorita: HIGH)**
- Setup Azure resource groups
- Init Git repositories (frontend + backend)
- CI/CD pipeline setup
- Sprint planning (2-week sprints)

---

## 13. Kontakt & Revize

**Dokument:** ChickenTrack PRD v1.0  
**Autor:** Ondřej (Ondra)  
**Datum:** 4. února 2026  
**Status:** Draft for Review

**Next Review:** Po zodpovězení otevřených otázek  
**Approvers:** Ondřej (Product Owner + Developer)

---

**Změnový Log:**
- v1.0 (2024-02-04): Initial draft
  - Executive summary
  - Funkční požadavky (včetně kuřat + akce převodu)
  - Technické požadavky (mobile-first PWA)
  - UI/UX specifikace
  - API endpoints
  - Roadmap (3 fáze)
  - Otevřené otázky (10)

---

**Přílohy:**
- [TBD] Wireframes (Figma link)
- [TBD] User Flows
- [TBD] OpenAPI Spec
- [TBD] Database Schema Diagram
