# Purchases API Specification

## Overview

The Purchases API provides endpoints for tracking and managing expenses related to chicken farming, such as feed, vitamins, bedding, veterinary care, and other costs. All endpoints require authentication via JWT Bearer token (managed by Clerk). All data is automatically filtered by the authenticated user's tenant ID through Row-Level Security (RLS).

**Base URL**: `/api/v1/purchases`

**Authentication**: Required for all endpoints
**Authorization**: `Bearer <jwt_token>`

---

## Endpoints

### 1. GET /api/v1/purchases

Retrieves all purchases for the authenticated tenant with optional filtering.

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `fromDate` | DateTime | No | - | Filter purchases from this date (inclusive) |
| `toDate` | DateTime | No | - | Filter purchases until this date (inclusive) |
| `type` | PurchaseType | No | - | Filter by purchase type (Feed, Vitamins, Bedding, Toys, Veterinary, Other) |
| `flockId` | GUID | No | - | Filter purchases by associated flock |

#### Request Example

```http
GET /api/v1/purchases?fromDate=2024-01-01&toDate=2024-12-31&type=Feed
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
    "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
    "name": "Organic Chicken Feed 25kg",
    "type": 0,
    "amount": 450.00,
    "quantity": 25.0,
    "unit": 0,
    "purchaseDate": "2024-01-15T00:00:00Z",
    "consumedDate": "2024-02-10T00:00:00Z",
    "notes": "Bought from local supplier, organic certified",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": "9d41f5e2-6b3c-4d7e-a8f9-0c1d2e3f4a5b",
    "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
    "coopId": null,
    "name": "Vitamin Supplements",
    "type": 1,
    "amount": 120.00,
    "quantity": 2.0,
    "unit": 3,
    "purchaseDate": "2024-01-20T00:00:00Z",
    "consumedDate": null,
    "notes": null,
    "createdAt": "2024-01-20T14:15:00Z",
    "updatedAt": "2024-01-20T14:15:00Z"
  }
]
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Invalid query parameters | See [Error Format](#error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Flock not found (when filtering by flockId) | See [Error Format](#error-format) |

---

### 2. GET /api/v1/purchases/{id}

Retrieves a specific purchase by its ID.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the purchase |

#### Request Example

```http
GET /api/v1/purchases/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
  "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
  "name": "Organic Chicken Feed 25kg",
  "type": 0,
  "amount": 450.00,
  "quantity": 25.0,
  "unit": 0,
  "purchaseDate": "2024-01-15T00:00:00Z",
  "consumedDate": "2024-02-10T00:00:00Z",
  "notes": "Bought from local supplier, organic certified",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Purchase with specified ID not found for this tenant | See [Error Format](#error-format) |

---

### 3. GET /api/v1/purchases/names

Retrieves a list of distinct purchase names for autocomplete functionality. Useful for suggesting previously used purchase names when creating new purchases.

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `query` | string | No | - | Search query to filter purchase names |
| `limit` | integer | No | 20 | Maximum number of results to return |

#### Request Example

```http
GET /api/v1/purchases/names?query=feed&limit=10
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
[
  "Organic Chicken Feed 25kg",
  "Premium Layer Feed",
  "Starter Feed for Chicks",
  "Winter Feed Mix"
]
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Invalid query parameters (e.g., limit out of range) | See [Error Format](#error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |

---

### 4. POST /api/v1/purchases

Creates a new purchase record for the authenticated tenant.

#### Request Body

```json
{
  "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
  "name": "Organic Chicken Feed 25kg",
  "type": 0,
  "amount": 450.00,
  "quantity": 25.0,
  "unit": 0,
  "purchaseDate": "2024-01-15T00:00:00Z",
  "consumedDate": "2024-02-10T00:00:00Z",
  "notes": "Bought from local supplier, organic certified"
}
```

#### Request Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `coopId` | GUID | No | Must belong to tenant | ID of the coop this purchase is associated with (null for general purchases) |
| `name` | string | Yes | Max 100 characters | Name or description of the purchased item |
| `type` | PurchaseType | Yes | Valid enum value (0-5) | Type of purchase (Feed=0, Vitamins=1, Bedding=2, Toys=3, Veterinary=4, Other=5) |
| `amount` | decimal | Yes | >= 0 | Amount paid for the purchase |
| `quantity` | decimal | Yes | > 0 | Quantity purchased |
| `unit` | QuantityUnit | Yes | Valid enum value (0-4) | Unit of quantity (Kg=0, Pcs=1, L=2, Package=3, Other=4) |
| `purchaseDate` | DateTime | Yes | Not in future | Date when the purchase was made |
| `consumedDate` | DateTime | No | >= purchaseDate | Date when the item was consumed or used |
| `notes` | string | No | Max 500 characters | Optional notes about the purchase |

#### Request Example

```http
POST /api/v1/purchases
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
  "name": "Organic Chicken Feed 25kg",
  "type": 0,
  "amount": 450.00,
  "quantity": 25.0,
  "unit": 0,
  "purchaseDate": "2024-01-15T00:00:00Z",
  "consumedDate": null,
  "notes": "Bought from local supplier"
}
```

#### Success Response (201 Created)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
  "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
  "name": "Organic Chicken Feed 25kg",
  "type": 0,
  "amount": 450.00,
  "quantity": 25.0,
  "unit": 0,
  "purchaseDate": "2024-01-15T00:00:00Z",
  "consumedDate": null,
  "notes": "Bought from local supplier",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

**Location Header**: `/api/v1/purchases/3fa85f64-5717-4562-b3fc-2c963f66afa6`

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Validation failed (e.g., name is empty, amount is negative, quantity is zero) | See [Validation Error Format](#validation-error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Coop with specified ID not found for this tenant | See [Error Format](#error-format) |

---

### 5. PUT /api/v1/purchases/{id}

Updates an existing purchase record.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the purchase to update |

#### Request Body

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
  "name": "Updated Feed Name",
  "type": 0,
  "amount": 500.00,
  "quantity": 30.0,
  "unit": 0,
  "purchaseDate": "2024-01-15T00:00:00Z",
  "consumedDate": "2024-02-15T00:00:00Z",
  "notes": "Updated notes"
}
```

#### Request Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `id` | GUID | Yes | Must match path parameter | The ID of the purchase to update |
| `coopId` | GUID | No | Must belong to tenant | ID of the coop this purchase is associated with |
| `name` | string | Yes | Max 100 characters | Name or description of the purchased item |
| `type` | PurchaseType | Yes | Valid enum value (0-5) | Type of purchase |
| `amount` | decimal | Yes | >= 0 | Amount paid for the purchase |
| `quantity` | decimal | Yes | > 0 | Quantity purchased |
| `unit` | QuantityUnit | Yes | Valid enum value (0-4) | Unit of quantity |
| `purchaseDate` | DateTime | Yes | Not in future | Date when the purchase was made |
| `consumedDate` | DateTime | No | >= purchaseDate | Date when the item was consumed |
| `notes` | string | No | Max 500 characters | Optional notes |

#### Request Example

```http
PUT /api/v1/purchases/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
  "name": "Updated Feed Name",
  "type": 0,
  "amount": 500.00,
  "quantity": 30.0,
  "unit": 0,
  "purchaseDate": "2024-01-15T00:00:00Z",
  "consumedDate": "2024-02-15T00:00:00Z",
  "notes": "Updated notes"
}
```

#### Success Response (200 OK)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
  "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
  "name": "Updated Feed Name",
  "type": 0,
  "amount": 500.00,
  "quantity": 30.0,
  "unit": 0,
  "purchaseDate": "2024-01-15T00:00:00Z",
  "consumedDate": "2024-02-15T00:00:00Z",
  "notes": "Updated notes",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T16:45:00Z"
}
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Validation failed or route ID doesn't match body ID | See [Validation Error Format](#validation-error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 403 | `Error.Forbidden` | Purchase belongs to a different tenant | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Purchase with specified ID not found | See [Error Format](#error-format) |

---

### 6. DELETE /api/v1/purchases/{id}

Permanently deletes a purchase record.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the purchase to delete |

#### Request Example

```http
DELETE /api/v1/purchases/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (204 No Content)

No response body.

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Invalid purchase ID format | See [Error Format](#error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 403 | `Error.Forbidden` | Purchase belongs to a different tenant | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Purchase with specified ID not found | See [Error Format](#error-format) |

---

## Error Response Formats

### Error Format

Standard error response structure:

```json
{
  "error": {
    "code": "Error.NotFound",
    "message": "Purchase not found"
  }
}
```

### Validation Error Format

Validation errors include detailed field-level messages:

```json
{
  "error": {
    "code": "Error.Validation",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "Name",
        "message": "Purchase name is required."
      },
      {
        "field": "Amount",
        "message": "Amount must be greater than or equal to zero."
      },
      {
        "field": "Quantity",
        "message": "Quantity must be greater than zero."
      }
    ]
  }
}
```

### Route ID Mismatch Error

When the ID in the URL path doesn't match the ID in the request body:

```json
{
  "error": {
    "message": "Route ID and command ID do not match"
  }
}
```

---

## Common Error Codes

| Status Code | Error Code | Description | Applies To |
|-------------|------------|-------------|------------|
| 400 | `Error.Validation` | Request validation failed (invalid input data) | POST, PUT, DELETE, GET (query params) |
| 401 | `Error.Unauthorized` | Authentication failed or user not authenticated | All endpoints |
| 403 | `Error.Forbidden` | User does not have permission to modify this resource | PUT, DELETE |
| 404 | `Error.NotFound` | Resource not found for this tenant | GET by ID, PUT, DELETE, GET (filtering) |
| 500 | `Error.Failure` | Internal server error | All endpoints |

---

## Data Model

### PurchaseDto

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | GUID | No | Unique identifier for the purchase |
| `tenantId` | GUID | No | The tenant that owns this purchase (automatically set) |
| `coopId` | GUID | Yes | The coop this purchase is associated with (null for general purchases) |
| `name` | string | No | Name or description of the purchased item (max 100 characters) |
| `type` | PurchaseType | No | Type of purchase (enum: 0-5) |
| `amount` | decimal | No | Amount paid for the purchase (>= 0) |
| `quantity` | decimal | No | Quantity purchased (> 0) |
| `unit` | QuantityUnit | No | Unit of quantity (enum: 0-4) |
| `purchaseDate` | DateTime | No | Date when the purchase was made (ISO 8601 format, UTC) |
| `consumedDate` | DateTime | Yes | Date when the item was consumed or used (ISO 8601 format, UTC) |
| `notes` | string | Yes | Optional notes about the purchase (max 500 characters) |
| `createdAt` | DateTime | No | Timestamp when the purchase was created (ISO 8601 format) |
| `updatedAt` | DateTime | No | Timestamp when the purchase was last updated (ISO 8601 format) |

### PurchaseType Enum

| Value | Name | Description |
|-------|------|-------------|
| 0 | Feed | Chicken feed purchase |
| 1 | Vitamins | Vitamins and supplements purchase |
| 2 | Bedding | Bedding material purchase |
| 3 | Toys | Toys and enrichment items purchase |
| 4 | Veterinary | Veterinary care and medication purchase |
| 5 | Other | Other miscellaneous purchases |

### QuantityUnit Enum

| Value | Name | Description |
|-------|------|-------------|
| 0 | Kg | Kilograms |
| 1 | Pcs | Pieces |
| 2 | L | Liters |
| 3 | Package | Package (unspecified unit) |
| 4 | Other | Other unit not listed |

---

## Business Rules

1. **Tenant Isolation**: Users can only access purchases belonging to their tenant. This is enforced automatically via Row-Level Security (RLS) and EF Core global query filters.

2. **Coop Association**: Purchases can be associated with a specific coop or be general (coopId = null). If a coopId is provided, it must belong to the authenticated tenant.

3. **Date Validation**:
   - `purchaseDate` cannot be in the future
   - `consumedDate` (if provided) must be greater than or equal to `purchaseDate`
   - All dates are normalized to UTC midnight (date-only, no time component)

4. **Quantity Rules**:
   - `quantity` must be greater than zero
   - `amount` must be greater than or equal to zero (free items allowed)

5. **Name Autocomplete**: The `/names` endpoint returns distinct purchase names from the tenant's previous purchases, sorted by usage frequency (most recent first).

6. **Automatic Timestamps**: `createdAt` and `updatedAt` are automatically managed by the system.

7. **Soft Delete**: Purchases do not support soft delete. Use the DELETE endpoint for permanent deletion.

---

## Authentication Notes

- All endpoints require a valid JWT Bearer token issued by Clerk.
- Tokens are validated on every request.
- The `tenantId` is automatically extracted from the authenticated user context.
- Tenant isolation is enforced at both the application and database levels (RLS).

---

## Rate Limiting

- **Standard limit**: 100 requests per minute per user
- **Burst limit**: 200 requests per minute

Exceeding rate limits will result in a `429 Too Many Requests` response.

---

## CORS Configuration

CORS is enabled for the PWA origin. Cross-origin requests must include credentials (cookies) for authentication.

---

## Versioning

Current API version: **v1**
Base URL includes version: `/api/v1/purchases`

Future versions will use explicit versioning: `/api/v2/purchases`

---

## Examples

### Example 1: Create a feed purchase

```bash
curl -X POST https://api.chickquita.com/api/v1/purchases \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
    "name": "Organic Layer Feed 25kg",
    "type": 0,
    "amount": 450.00,
    "quantity": 25.0,
    "unit": 0,
    "purchaseDate": "2024-01-15T00:00:00Z",
    "notes": "Premium organic feed for layers"
  }'
```

### Example 2: Get all purchases filtered by date range and type

```bash
curl -X GET "https://api.chickquita.com/api/v1/purchases?fromDate=2024-01-01&toDate=2024-12-31&type=0" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Example 3: Get purchase names for autocomplete

```bash
curl -X GET "https://api.chickquita.com/api/v1/purchases/names?query=feed&limit=10" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Example 4: Update a purchase

```bash
curl -X PUT https://api.chickquita.com/api/v1/purchases/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "coopId": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
    "name": "Premium Layer Feed 30kg",
    "type": 0,
    "amount": 550.00,
    "quantity": 30.0,
    "unit": 0,
    "purchaseDate": "2024-01-15T00:00:00Z",
    "consumedDate": "2024-02-20T00:00:00Z",
    "notes": "Upgraded to larger package"
  }'
```

### Example 5: Delete a purchase

```bash
curl -X DELETE https://api.chickquita.com/api/v1/purchases/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Implementation Notes

- **Backend**: Implemented using .NET 8 Minimal APIs with MediatR (CQRS pattern)
- **Validation**: FluentValidation for input validation
- **Database**: Neon Postgres with Entity Framework Core
- **Tenant Resolution**: Automatic via middleware (`TenantResolutionMiddleware`)
- **Logging**: Structured logging with Microsoft.Extensions.Logging
- **Error Handling**: Centralized via `Result<T>` pattern
- **Date Handling**: All dates normalized to UTC midnight (date-only format)

---

## Frontend Integration

### TypeScript Types

```typescript
export enum PurchaseType {
  Feed = 0,
  Vitamins = 1,
  Bedding = 2,
  Toys = 3,
  Veterinary = 4,
  Other = 5,
}

export enum QuantityUnit {
  Kg = 0,
  Pcs = 1,
  L = 2,
  Package = 3,
  Other = 4,
}

export interface PurchaseDto {
  id: string;
  tenantId: string;
  coopId: string | null;
  name: string;
  type: PurchaseType;
  amount: number;
  quantity: number;
  unit: QuantityUnit;
  purchaseDate: string;
  consumedDate: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePurchaseDto {
  coopId?: string | null;
  name: string;
  type: PurchaseType;
  amount: number;
  quantity: number;
  unit: QuantityUnit;
  purchaseDate: string;
  consumedDate?: string | null;
  notes?: string | null;
}

export interface UpdatePurchaseDto extends CreatePurchaseDto {
  id: string;
}
```

### API Client Usage

```typescript
import apiClient from '@/lib/apiClient';

// Get all purchases with filters
const purchases = await apiClient.get('/api/v1/purchases', {
  params: {
    fromDate: '2024-01-01',
    toDate: '2024-12-31',
    type: PurchaseType.Feed,
  },
});

// Get purchase names for autocomplete
const names = await apiClient.get('/api/v1/purchases/names', {
  params: { query: 'feed', limit: 10 },
});

// Create a new purchase
const newPurchase = await apiClient.post('/api/v1/purchases', {
  name: 'Organic Feed',
  type: PurchaseType.Feed,
  amount: 450.0,
  quantity: 25.0,
  unit: QuantityUnit.Kg,
  purchaseDate: '2024-01-15T00:00:00Z',
});

// Update a purchase
await apiClient.put(`/api/v1/purchases/${id}`, {
  id,
  name: 'Updated Feed',
  type: PurchaseType.Feed,
  amount: 500.0,
  quantity: 30.0,
  unit: QuantityUnit.Kg,
  purchaseDate: '2024-01-15T00:00:00Z',
});

// Delete a purchase
await apiClient.delete(`/api/v1/purchases/${id}`);
```

---

## Related Documentation

- [CLAUDE.md](../CLAUDE.md) - Project overview and conventions
- [API_SPEC_COOPS.md](./API_SPEC_COOPS.md) - Coops API specification
- [Technology Stack](./technology-stack.md) - Full tech stack documentation
- [Filesystem Structure](./filesystem-structure.md) - Project structure
