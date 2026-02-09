# Chickquita Database Schema Documentation

**Version:** 1.1.0
**Last Updated:** 2026-02-09
**Database:** Neon Postgres 16
**ORM:** Entity Framework Core (Code First)

---

## Table of Contents

1. [Overview](#overview)
2. [Multi-Tenancy Architecture](#multi-tenancy-architecture)
3. [Entity Relationship Diagram](#entity-relationship-diagram)
4. [Tables](#tables)
5. [Enumerations](#enumerations)
6. [Indexes](#indexes)
7. [Row-Level Security (RLS) Policies](#row-level-security-rls-policies)
8. [Migrations](#migrations)

---

## Overview

The Chickquita database uses a multi-tenant architecture with Row-Level Security (RLS) to ensure data isolation between farmers. All tables include a `tenant_id` column for partitioning data.

**Key Features:**
- Multi-tenant with RLS enforcement
- UUID primary keys for all entities
- Automatic timestamps (created_at, updated_at)
- PostgreSQL 16 features
- Code-first migrations via Entity Framework Core

---

## Multi-Tenancy Architecture

### Tenant Isolation Strategy

Every data table includes:
- `tenant_id` (UUID, NOT NULL) - Foreign key to `tenants` table
- RLS policy: `tenant_id = current_setting('app.current_tenant_id')::UUID`
- EF Core global query filter as secondary safety layer

### Tenant Context

Backend sets tenant context before each query:
```sql
SELECT set_tenant_context('7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b');
```

This ensures all queries automatically filter by the authenticated user's tenant.

---

## Entity Relationship Diagram

```
┌─────────────┐
│   Tenant    │
│ (User)      │
└──────┬──────┘
       │
       │ 1:N
       │
┌──────▼──────┐
│    Coop     │
│ (Chicken    │
│  Coop)      │
└──────┬──────┘
       │
       │ 1:N
       │
┌──────▼──────┐       ┌─────────────────┐
│    Flock    │◄──────┤ FlockHistory    │
│ (Group of   │  1:N  │ (Composition    │
│  Chickens)  │       │  Changes)       │
└──────┬──────┘       └─────────────────┘
       │
       │ 1:N
       │
┌──────▼──────┐
│DailyRecord  │
│ (Daily Egg  │
│  Production)│
└─────────────┘

┌─────────────┐
│  Purchase   │◄─────┐
│ (Expenses)  │      │ Optional
└─────────────┘      │ (0..1:N)
       ▲             │
       │             │
       └─────────────┘
         Coop (optional association)
```

---

## Tables

### 1. tenants

Stores user accounts linked to Clerk authentication.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | UUID | No | gen_random_uuid() | Primary key |
| `clerk_user_id` | VARCHAR(255) | No | - | Clerk user ID (unique) |
| `email` | VARCHAR(255) | No | - | User email (unique) |
| `created_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Creation timestamp |
| `updated_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Last update timestamp |

**Indexes:**
- PRIMARY KEY: `id`
- UNIQUE: `clerk_user_id`
- UNIQUE: `email`

**Notes:**
- Created via Clerk webhook on user registration
- Fallback: Auto-created on first API request if webhook fails

---

### 2. coops

Stores chicken coop information.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | UUID | No | gen_random_uuid() | Primary key |
| `tenant_id` | UUID | No | - | Foreign key to tenants |
| `name` | VARCHAR(100) | No | - | Coop name (unique per tenant) |
| `location` | VARCHAR(200) | Yes | NULL | Optional location description |
| `is_active` | BOOLEAN | No | TRUE | Active status (false = archived) |
| `created_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Creation timestamp |
| `updated_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Last update timestamp |

**Indexes:**
- PRIMARY KEY: `id`
- FOREIGN KEY: `tenant_id` REFERENCES `tenants(id)` ON DELETE CASCADE
- UNIQUE: `tenant_id, name` (per-tenant name uniqueness)
- INDEX: `tenant_id` (for RLS queries)

**Business Rules:**
- Name must be unique per tenant
- Cannot be hard deleted if flocks exist
- Soft delete via `is_active = false` (archive)

---

### 3. flocks

Stores chicken flock composition (hens, roosters, chicks).

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | UUID | No | gen_random_uuid() | Primary key |
| `tenant_id` | UUID | No | - | Foreign key to tenants |
| `coop_id` | UUID | No | - | Foreign key to coops |
| `identifier` | VARCHAR(50) | No | - | Flock identifier (unique per coop) |
| `current_hens` | INTEGER | No | 0 | Number of adult female chickens |
| `current_roosters` | INTEGER | No | 0 | Number of adult male chickens |
| `current_chicks` | INTEGER | No | 0 | Number of young chickens |
| `hatch_date` | TIMESTAMPTZ | No | - | Date when flock was hatched |
| `is_active` | BOOLEAN | No | TRUE | Active status (false = archived) |
| `created_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Creation timestamp |
| `updated_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Last update timestamp |

**Indexes:**
- PRIMARY KEY: `id`
- FOREIGN KEY: `tenant_id` REFERENCES `tenants(id)` ON DELETE CASCADE
- FOREIGN KEY: `coop_id` REFERENCES `coops(id)` ON DELETE CASCADE
- UNIQUE: `coop_id, identifier` (per-coop identifier uniqueness)
- INDEX: `tenant_id` (for RLS queries)
- INDEX: `coop_id` (for coop-based queries)
- INDEX: `hatch_date` (for date-based queries)
- INDEX: `is_active` (for filtering active flocks)

**Business Rules:**
- Current hens, roosters, chicks must be >= 0
- Chicks count in feed costs, not egg production
- Can be matured (chicks → hens/roosters) with history tracking
- Identifier must be unique per coop

---

### 4. flock_history

Immutable history of flock composition snapshots (created when chicks are matured or composition changes).

**Design Rationale:** FlockHistory stores snapshots of flock state at each change, not before/after deltas. Each record represents the complete flock composition at a specific point in time.

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | UUID | No | gen_random_uuid() | Primary key |
| `tenant_id` | UUID | No | - | Foreign key to tenants |
| `flock_id` | UUID | No | - | Foreign key to flocks |
| `reason` | VARCHAR(50) | No | - | Reason for composition change (e.g., "ChickMaturation", "InitialComposition") |
| `hens` | INTEGER | No | 0 | Number of hens at this snapshot |
| `roosters` | INTEGER | No | 0 | Number of roosters at this snapshot |
| `chicks` | INTEGER | No | 0 | Number of chicks at this snapshot |
| `notes` | VARCHAR(500) | Yes | NULL | Optional notes (editable) |
| `change_date` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | When the composition change occurred |
| `created_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Record creation timestamp |
| `updated_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Last update timestamp (for notes field) |

**Indexes:**
- PRIMARY KEY: `id`
- FOREIGN KEY: `tenant_id` REFERENCES `tenants(id)` ON DELETE CASCADE
- FOREIGN KEY: `flock_id` REFERENCES `flocks(id)` ON DELETE CASCADE
- INDEX: `tenant_id` (for RLS queries)
- INDEX: `flock_id` (for flock history queries)
- INDEX: `change_date DESC` (for chronological queries)

**Business Rules:**
- Immutable (except `notes` field which can be edited)
- First entry for each flock: `reason = "InitialComposition"`
- Maturation entries: `reason = "ChickMaturation"`
- Each record is a complete snapshot, not a delta/diff

---

### 5. daily_records

Daily egg production records (offline-capable).

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | UUID | No | gen_random_uuid() | Primary key |
| `tenant_id` | UUID | No | - | Foreign key to tenants |
| `flock_id` | UUID | No | - | Foreign key to flocks |
| `record_date` | DATE | No | - | Date of the record (date-only, no time) |
| `egg_count` | INTEGER | No | 0 | Number of eggs collected |
| `notes` | TEXT | Yes | NULL | Optional notes |
| `created_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Creation timestamp |
| `updated_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Last update timestamp |

**Indexes:**
- PRIMARY KEY: `id`
- FOREIGN KEY: `tenant_id` REFERENCES `tenants(id)` ON DELETE CASCADE
- FOREIGN KEY: `flock_id` REFERENCES `flocks(id)` ON DELETE CASCADE
- UNIQUE: `flock_id, record_date` (one record per flock per day)
- INDEX: `tenant_id` (for RLS queries)
- INDEX: `flock_id` (for flock-based queries)
- INDEX: `record_date DESC` (for date range queries)

**Business Rules:**
- One record per flock per day
- Egg count must be >= 0
- Offline-first: queued via background sync when offline

---

### 6. purchases

Expense tracking for chicken farming (feed, vitamins, bedding, veterinary, etc.).

| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| `id` | UUID | No | gen_random_uuid() | Primary key |
| `tenant_id` | UUID | No | - | Foreign key to tenants |
| `coop_id` | UUID | Yes | NULL | Optional foreign key to coops |
| `name` | VARCHAR(100) | No | - | Purchase name or description |
| `type` | VARCHAR(20) | No | - | Purchase type enum name (string) |
| `amount` | DECIMAL(18, 2) | No | - | Amount paid (>= 0) |
| `quantity` | DECIMAL(18, 2) | No | - | Quantity purchased (> 0) |
| `unit` | VARCHAR(20) | No | - | Quantity unit enum name (string) |
| `purchase_date` | TIMESTAMPTZ | No | - | Date of purchase (stored as TIMESTAMPTZ) |
| `consumed_date` | TIMESTAMPTZ | Yes | NULL | Date when item was consumed (optional) |
| `notes` | VARCHAR(500) | Yes | NULL | Optional notes (max 500 chars) |
| `created_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Creation timestamp |
| `updated_at` | TIMESTAMPTZ | No | CURRENT_TIMESTAMP | Last update timestamp |

**Indexes:**
- PRIMARY KEY: `id`
- FOREIGN KEY: `tenant_id` REFERENCES `tenants(id)` ON DELETE CASCADE
- FOREIGN KEY: `coop_id` REFERENCES `coops(id)` ON DELETE SET NULL
- INDEX: `tenant_id` (for RLS queries)
- INDEX: `coop_id` (for coop-based queries)
- INDEX: `purchase_date DESC` (for date range queries)
- INDEX: `type` (for type filtering)
- INDEX: `tenant_id, name` (for autocomplete queries)

**Business Rules:**
- Amount must be >= 0 (free items allowed)
- Quantity must be > 0
- purchase_date cannot be in the future
- consumed_date (if provided) must be >= purchase_date
- coop_id is optional (general purchases not tied to specific coop)

---

## Enumerations

**Note:** EF Core converts enums to strings via `.HasConversion<string>()` configuration. Enum values are stored as VARCHAR (enum names like "Feed", "Vitamins"), not as INTEGER.

### PurchaseType

Represents the type of purchase.

| C# Enum Value | Name | Database Value | Description |
|---------------|------|----------------|-------------|
| 0 | Feed | "Feed" | Chicken feed purchase |
| 1 | Vitamins | "Vitamins" | Vitamins and supplements purchase |
| 2 | Bedding | "Bedding" | Bedding material purchase |
| 3 | Toys | "Toys" | Toys and enrichment items purchase |
| 4 | Veterinary | "Veterinary" | Veterinary care and medication purchase |
| 5 | Other | "Other" | Other miscellaneous purchases |

**Database Storage:** VARCHAR(20) (enum name as string)

---

### QuantityUnit

Represents the unit of quantity for purchased items.

| C# Enum Value | Name | Database Value | Description |
|---------------|------|----------------|-------------|
| 0 | Kg | "Kg" | Kilograms |
| 1 | Pcs | "Pcs" | Pieces |
| 2 | L | "L" | Liters |
| 3 | Package | "Package" | Package (unspecified unit) |
| 4 | Other | "Other" | Other unit not listed |

**Database Storage:** VARCHAR(20) (enum name as string)

---

## Indexes

### Performance Indexes

#### tenants
```sql
CREATE INDEX idx_tenants_clerk_user_id ON tenants(clerk_user_id);
CREATE INDEX idx_tenants_email ON tenants(email);
```

#### coops
```sql
CREATE INDEX idx_coops_tenant_id ON coops(tenant_id);
CREATE INDEX idx_coops_tenant_name ON coops(tenant_id, name);
CREATE INDEX idx_coops_is_active ON coops(is_active);
```

#### flocks
```sql
CREATE INDEX idx_flocks_tenant_id ON flocks(tenant_id);
CREATE INDEX idx_flocks_coop_id ON flocks(coop_id);
CREATE INDEX idx_flocks_hatch_date ON flocks(hatch_date);
CREATE INDEX idx_flocks_is_active ON flocks(is_active);
CREATE UNIQUE INDEX idx_flocks_coop_id_identifier ON flocks(coop_id, identifier);
```

#### flock_history
```sql
CREATE INDEX idx_flock_history_tenant_id ON flock_history(tenant_id);
CREATE INDEX idx_flock_history_flock_id ON flock_history(flock_id);
CREATE INDEX idx_flock_history_change_date ON flock_history(change_date DESC);
```

#### daily_records
```sql
CREATE INDEX idx_daily_records_tenant_id ON daily_records(tenant_id);
CREATE INDEX idx_daily_records_flock_id ON daily_records(flock_id);
CREATE INDEX idx_daily_records_record_date ON daily_records(record_date DESC);
CREATE UNIQUE INDEX idx_daily_records_flock_id_record_date ON daily_records(flock_id, record_date);
```

#### purchases
```sql
CREATE INDEX idx_purchases_tenant_id ON purchases(tenant_id);
CREATE INDEX idx_purchases_coop_id ON purchases(coop_id);
CREATE INDEX idx_purchases_purchase_date ON purchases(purchase_date DESC);
CREATE INDEX idx_purchases_type ON purchases(type);
CREATE INDEX idx_purchases_tenant_id_purchase_date ON purchases(tenant_id, purchase_date);
```

---

## Row-Level Security (RLS) Policies

### Tenant Isolation Function

```sql
CREATE OR REPLACE FUNCTION set_tenant_context(tenant_uuid UUID)
RETURNS VOID AS $$
BEGIN
  PERFORM set_config('app.current_tenant_id', tenant_uuid::TEXT, false);
END;
$$ LANGUAGE plpgsql;
```

### RLS Policies Per Table

#### tenants
```sql
-- Tenants table doesn't have RLS (users can only see their own tenant via authentication)
-- No policy needed as access is controlled via Clerk authentication
```

#### coops
```sql
ALTER TABLE coops ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_coops ON coops
  USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

#### flocks
```sql
ALTER TABLE flocks ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_flocks ON flocks
  USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

#### flock_history
```sql
ALTER TABLE flock_history ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_flock_history ON flock_history
  USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

#### daily_records
```sql
ALTER TABLE daily_records ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_daily_records ON daily_records
  USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

#### purchases
```sql
ALTER TABLE purchases ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_purchases ON purchases
  USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

---

## Migrations

### Migration Naming Convention

```
YYYYMMDDHHMMSS_DescriptiveName.cs
```

Example: `20260208120000_AddPurchasesTable.cs`

### Current Migrations

1. **Initial Migration**: Create tenants, coops, flocks tables
2. **FlockHistory Migration**: Add flock_history table
3. **DailyRecords Migration**: Add daily_records table
4. **Purchases Migration**: Add purchases table (latest)

### Creating New Migration

```bash
cd backend

# Create migration
dotnet ef migrations add MigrationName \
  --project src/Chickquita.Infrastructure \
  --startup-project src/Chickquita.Api

# Apply migration to database
dotnet ef database update \
  --project src/Chickquita.Infrastructure \
  --startup-project src/Chickquita.Api
```

### Rollback Migration

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName \
  --project src/Chickquita.Infrastructure \
  --startup-project src/Chickquita.Api

# Remove last migration (if not applied)
dotnet ef migrations remove \
  --project src/Chickquita.Infrastructure \
  --startup-project src/Chickquita.Api
```

---

## Data Integrity Rules

### Cascading Deletes

- **tenant deletion** → Deletes all coops, flocks, daily_records, purchases (CASCADE)
- **coop deletion** → Deletes all flocks, sets purchases.coop_id to NULL (CASCADE for flocks, SET NULL for purchases)
- **flock deletion** → Deletes all daily_records, flock_history (CASCADE)

### Constraints

- **NOT NULL**: All tenant_id, primary keys, required fields
- **CHECK**: Positive values for counts (hens, roosters, chicks, egg_count, quantity, amount)
- **UNIQUE**: Per-tenant coop names, per-flock daily records (one per day)
- **FOREIGN KEY**: All relationships enforced with ON DELETE actions

---

## Backup and Recovery

### Neon Automatic Backups

- **Frequency**: Continuous
- **Retention**: 7 days (free tier), 30 days (paid tier)
- **Point-in-time recovery**: Available

### Manual Backup

```bash
# Export database to SQL file
pg_dump -h [neon-endpoint] -U [username] -d chickquita > backup.sql

# Restore from backup
psql -h [neon-endpoint] -U [username] -d chickquita < backup.sql
```

---

## Performance Considerations

### Query Optimization

1. **Always set tenant context** before queries:
   ```sql
   SELECT set_tenant_context('tenant-uuid-here');
   ```

2. **Use indexes** for date range queries:
   ```sql
   SELECT * FROM daily_records
   WHERE record_date BETWEEN '2024-01-01' AND '2024-12-31'
   ORDER BY record_date DESC;
   ```

3. **Limit results** for large datasets:
   ```sql
   SELECT * FROM purchases
   WHERE purchase_date >= CURRENT_DATE - INTERVAL '30 days'
   LIMIT 100;
   ```

### Connection Pooling

Enable Neon connection pooling for production:
```
postgresql://[user]:[pass]@[endpoint]/[db]?sslmode=require&pgbouncer=true
```

---

## Security Best Practices

1. **Row-Level Security**: Enabled on all data tables
2. **SSL/TLS**: Required for all connections (`sslmode=require`)
3. **Prepared Statements**: EF Core uses parameterized queries (prevents SQL injection)
4. **Least Privilege**: Application uses database owner credentials (to be refined)
5. **Password Rotation**: Rotate Neon database password periodically

---

## Related Documentation

- [API_SPEC_PURCHASES.md](./API_SPEC_PURCHASES.md) - Purchases API specification
- [API_SPEC_COOPS.md](./API_SPEC_COOPS.md) - Coops API specification
- [neon-database-setup.md](./neon-database-setup.md) - Database setup guide
- [database-connection-guide.md](./database-connection-guide.md) - Connection configuration
- [CLAUDE.md](../CLAUDE.md) - Project overview and conventions

---

**Maintainers:** Chickquita Development Team
**Last Updated:** 2026-02-09
