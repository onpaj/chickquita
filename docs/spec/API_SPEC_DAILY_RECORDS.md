# Daily Records API Specification

## Overview

The Daily Records API provides endpoints for managing daily egg production records. All endpoints require authentication via JWT Bearer token (managed by Clerk). All data is automatically filtered by the authenticated user's tenant ID through Row-Level Security (RLS).

**Base URL**: `/api/daily-records` or `/api/flocks/{flockId}/daily-records`

**Authentication**: Required for all endpoints
**Authorization**: `Bearer <jwt_token>`

---

## Endpoints

### 1. GET /api/daily-records

Retrieves all daily records for the authenticated tenant with optional filtering by flock and date range.

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `flockId` | GUID | No | `null` | Filter records by specific flock ID |
| `startDate` | DateTime | No | `null` | Filter records from this date (inclusive) |
| `endDate` | DateTime | No | `null` | Filter records until this date (inclusive) |

#### Request Example

```http
GET /api/daily-records?flockId=3fa85f64-5717-4562-b3fc-2c963f66afa6&startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
[
  {
    "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
    "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
    "flockId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "recordDate": "2024-01-15T00:00:00Z",
    "eggCount": 8,
    "notes": "All eggs were large today",
    "createdAt": "2024-01-15T18:30:00Z",
    "updatedAt": "2024-01-15T18:30:00Z"
  },
  {
    "id": "b2c3d4e5-6789-01bc-def2-234567890abc",
    "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
    "flockId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "recordDate": "2024-01-16T00:00:00Z",
    "eggCount": 7,
    "notes": null,
    "createdAt": "2024-01-16T19:15:00Z",
    "updatedAt": "2024-01-16T19:15:00Z"
  }
]
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Flock with specified ID not found for this tenant | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 2. GET /api/flocks/{flockId}/daily-records

Retrieves all daily records for a specific flock with optional date range filtering.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `flockId` | GUID | Yes | The unique identifier of the flock |

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `startDate` | DateTime | No | `null` | Filter records from this date (inclusive) |
| `endDate` | DateTime | No | `null` | Filter records until this date (inclusive) |

#### Request Example

```http
GET /api/flocks/3fa85f64-5717-4562-b3fc-2c963f66afa6/daily-records?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
[
  {
    "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
    "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
    "flockId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "recordDate": "2024-01-15T00:00:00Z",
    "eggCount": 8,
    "notes": "All eggs were large today",
    "createdAt": "2024-01-15T18:30:00Z",
    "updatedAt": "2024-01-15T18:30:00Z"
  }
]
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Flock with specified ID not found for this tenant | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 3. POST /api/flocks/{flockId}/daily-records

Creates a new daily record for egg production for a specific flock.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `flockId` | GUID | Yes | The unique identifier of the flock |

#### Request Body

```json
{
  "recordDate": "2024-01-15T00:00:00Z",
  "eggCount": 8,
  "notes": "All eggs were large today"
}
```

#### Request Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `recordDate` | DateTime | Yes | Cannot be in the future | The date of the egg collection (time is ignored, only date is used) |
| `eggCount` | integer | Yes | >= 0 | The number of eggs collected on this date |
| `notes` | string | No | Max 500 characters | Optional notes about the daily collection |

#### Request Example

```http
POST /api/flocks/3fa85f64-5717-4562-b3fc-2c963f66afa6/daily-records
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "recordDate": "2024-01-15T00:00:00Z",
  "eggCount": 8,
  "notes": "All eggs were large today"
}
```

#### Success Response (201 Created)

```json
{
  "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
  "flockId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "recordDate": "2024-01-15T00:00:00Z",
  "eggCount": 8,
  "notes": "All eggs were large today",
  "createdAt": "2024-01-15T18:30:00Z",
  "updatedAt": "2024-01-15T18:30:00Z"
}
```

**Location Header**: `/api/daily-records/a1b2c3d4-5678-90ab-cdef-1234567890ab`

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Validation failed (e.g., future date, negative egg count, notes too long) | See [Validation Error Format](#validation-error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Flock with specified ID not found for this tenant | See [Error Format](#error-format) |
| 409 | `Error.Conflict` | A daily record already exists for this flock on the specified date | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 4. PUT /api/daily-records/{id}

Updates an existing daily record. Only the egg count and notes can be updated; the record date and flock ID cannot be changed.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the daily record to update |

#### Request Body

```json
{
  "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "eggCount": 9,
  "notes": "Updated: Found one more egg in the nesting box"
}
```

#### Request Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `id` | GUID | Yes | Must match path parameter | The ID of the daily record to update |
| `eggCount` | integer | Yes | >= 0 | The updated number of eggs collected |
| `notes` | string | No | Max 500 characters | Optional notes about the daily collection |

#### Request Example

```http
PUT /api/daily-records/a1b2c3d4-5678-90ab-cdef-1234567890ab
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "eggCount": 9,
  "notes": "Updated: Found one more egg in the nesting box"
}
```

#### Success Response (200 OK)

```json
{
  "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
  "flockId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "recordDate": "2024-01-15T00:00:00Z",
  "eggCount": 9,
  "notes": "Updated: Found one more egg in the nesting box",
  "createdAt": "2024-01-15T18:30:00Z",
  "updatedAt": "2024-01-15T20:45:00Z"
}
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Validation failed or route ID doesn't match body ID | See [Validation Error Format](#validation-error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Daily record with specified ID not found for this tenant | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 5. DELETE /api/daily-records/{id}

Permanently deletes a daily record.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the daily record to delete |

#### Request Example

```http
DELETE /api/daily-records/a1b2c3d4-5678-90ab-cdef-1234567890ab
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (204 No Content)

No response body.

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Daily record with specified ID not found for this tenant | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

## Error Response Formats

### Error Format

Standard error response structure:

```json
{
  "error": {
    "code": "Error.NotFound",
    "message": "Daily record not found"
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
        "field": "RecordDate",
        "message": "Record date cannot be in the future."
      },
      {
        "field": "EggCount",
        "message": "Egg count cannot be negative."
      },
      {
        "field": "Notes",
        "message": "Notes must not exceed 500 characters."
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
| 400 | `Error.Validation` | Request validation failed (invalid input data) | POST, PUT |
| 401 | `Error.Unauthorized` | Authentication failed or user not authenticated | All endpoints |
| 404 | `Error.NotFound` | Resource not found for this tenant | GET by flock, POST, PUT, DELETE |
| 409 | `Error.Conflict` | Resource conflict (e.g., duplicate record for flock and date) | POST |
| 500 | `Error.Failure` | Internal server error | All endpoints |

---

## Data Model

### DailyRecordDto

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | GUID | No | Unique identifier for the daily record |
| `tenantId` | GUID | No | The tenant that owns this daily record (automatically set) |
| `flockId` | GUID | No | The flock this record belongs to |
| `recordDate` | DateTime | No | The date of the egg collection (ISO 8601 format, time component is ignored) |
| `eggCount` | integer | No | Number of eggs collected on this date (>= 0) |
| `notes` | string | Yes | Optional notes about the daily collection (max 500 characters) |
| `createdAt` | DateTime | No | Timestamp when the record was created (ISO 8601 format) |
| `updatedAt` | DateTime | No | Timestamp when the record was last updated (ISO 8601 format) |

---

## Business Rules

1. **Tenant Isolation**: Users can only access daily records belonging to their tenant. This is enforced automatically via Row-Level Security (RLS) and EF Core global query filters.

2. **Date Validation**: The `recordDate` cannot be in the future. Only current or past dates are allowed.

3. **Egg Count**: Must be a non-negative integer (0 or greater). Zero is valid for days when no eggs were collected.

4. **Notes Length**: Notes are optional but must not exceed 500 characters when provided.

5. **Flock Association**: Daily records must be associated with an existing flock belonging to the same tenant.

6. **Duplicate Prevention**: Only one daily record can exist per flock per date. Attempting to create a duplicate will result in a `409 Conflict` error.

7. **Automatic Timestamps**: `createdAt` and `updatedAt` are automatically managed by the system.

8. **Date-Only Storage**: While `recordDate` is a DateTime type, only the date portion is used. Time is ignored for record identification.

9. **Offline Support**: Daily records are designed to be created offline and synced later, supporting the PWA offline-first architecture.

---

## Offline-First Considerations

Daily records are a critical part of the PWA offline-first strategy:

- **Background Sync**: POST requests for creating daily records are queued when offline and synchronized when the connection is restored.
- **Conflict Resolution**: Uses last-write-wins strategy (MVP). Future phases will implement UI-based conflict resolution.
- **IndexedDB Storage**: Local copies are stored in IndexedDB for offline access.
- **Sync Queue**: Offline changes are retained in the sync queue for up to 24 hours.

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
Base URL includes implicit version: `/api/daily-records`

Future versions will use explicit versioning: `/api/v2/daily-records`

---

## Examples

### Example 1: Create a daily record for today

```bash
curl -X POST https://api.chickquita.com/api/flocks/3fa85f64-5717-4562-b3fc-2c963f66afa6/daily-records \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "recordDate": "2024-01-15T00:00:00Z",
    "eggCount": 8,
    "notes": "All eggs were large today"
  }'
```

### Example 2: Get all daily records for a flock in January 2024

```bash
curl -X GET "https://api.chickquita.com/api/flocks/3fa85f64-5717-4562-b3fc-2c963f66afa6/daily-records?startDate=2024-01-01&endDate=2024-01-31" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Example 3: Get all daily records across all flocks for a date range

```bash
curl -X GET "https://api.chickquita.com/api/daily-records?startDate=2024-01-01&endDate=2024-01-31" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Example 4: Update a daily record

```bash
curl -X PUT https://api.chickquita.com/api/daily-records/a1b2c3d4-5678-90ab-cdef-1234567890ab \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "id": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
    "eggCount": 9,
    "notes": "Updated: Found one more egg in the nesting box"
  }'
```

### Example 5: Delete a daily record

```bash
curl -X DELETE https://api.chickquita.com/api/daily-records/a1b2c3d4-5678-90ab-cdef-1234567890ab \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Example 6: Get records for a specific flock without date filtering

```bash
curl -X GET "https://api.chickquita.com/api/flocks/3fa85f64-5717-4562-b3fc-2c963f66afa6/daily-records" \
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
- **Offline Support**: Workbox for service worker management, IndexedDB for local storage

---

## Related Documentation

- [CLAUDE.md](../CLAUDE.md) - Project overview and conventions
- [API_SPEC_COOPS.md](./API_SPEC_COOPS.md) - Coops API specification
- [Technology Stack](./technology-stack.md) - Full tech stack documentation
- [Filesystem Structure](./filesystem-structure.md) - Project structure
- [ChickenTrack_PRD.md](./ChickenTrack_PRD.md) - Product requirements document
