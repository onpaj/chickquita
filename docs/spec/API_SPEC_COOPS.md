# Coops API Specification

## Overview

The Coops API provides endpoints for managing chicken coops. All endpoints require authentication via JWT Bearer token (managed by Clerk). All data is automatically filtered by the authenticated user's tenant ID through Row-Level Security (RLS).

**Base URL**: `/api/coops`

**Authentication**: Required for all endpoints
**Authorization**: `Bearer <jwt_token>`

---

## Endpoints

### 1. GET /api/coops

Retrieves all coops for the authenticated tenant.

#### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `includeArchived` | boolean | No | `false` | Include archived (soft-deleted) coops in the results |

#### Request Example

```http
GET /api/coops?includeArchived=false
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
    "name": "Main Coop",
    "location": "Behind the barn, north side",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-20T14:45:00Z",
    "isActive": true,
    "flocksCount": 3
  },
  {
    "id": "8c21d7f3-4a2e-4e6b-9c1d-5f8e2a3b4c5d",
    "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
    "name": "Secondary Coop",
    "location": null,
    "createdAt": "2024-02-01T08:00:00Z",
    "updatedAt": "2024-02-01T08:00:00Z",
    "isActive": true,
    "flocksCount": 1
  }
]
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 2. GET /api/coops/{id}

Retrieves a specific coop by its ID.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the coop |

#### Request Example

```http
GET /api/coops/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
  "name": "Main Coop",
  "location": "Behind the barn, north side",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T14:45:00Z",
  "isActive": true,
  "flocksCount": 3
}
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Coop with specified ID not found for this tenant | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 3. POST /api/coops

Creates a new coop for the authenticated tenant.

#### Request Body

```json
{
  "name": "Main Coop",
  "location": "Behind the barn, north side"
}
```

#### Request Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `name` | string | Yes | Max 100 characters | The name of the coop |
| `location` | string | No | Max 200 characters | Optional location description |

#### Request Example

```http
POST /api/coops
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "Main Coop",
  "location": "Behind the barn, north side"
}
```

#### Success Response (201 Created)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
  "name": "Main Coop",
  "location": "Behind the barn, north side",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "isActive": true,
  "flocksCount": 0
}
```

**Location Header**: `/api/coops/3fa85f64-5717-4562-b3fc-2c963f66afa6`

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Validation failed (e.g., name is empty or too long) | See [Validation Error Format](#validation-error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 409 | `Error.Conflict` | A coop with the same name already exists for this tenant | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 4. PUT /api/coops/{id}

Updates an existing coop.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the coop to update |

#### Request Body

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Updated Main Coop",
  "location": "New location description"
}
```

#### Request Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `id` | GUID | Yes | Must match path parameter | The ID of the coop to update |
| `name` | string | Yes | Max 100 characters | The name of the coop |
| `location` | string | No | Max 200 characters | Optional location description |

#### Request Example

```http
PUT /api/coops/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Updated Main Coop",
  "location": "New location description"
}
```

#### Success Response (200 OK)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "7b12c8e4-9a3d-4f21-b5e6-1c8d9e0f2a3b",
  "name": "Updated Main Coop",
  "location": "New location description",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T14:45:00Z",
  "isActive": true,
  "flocksCount": 3
}
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Validation failed or route ID doesn't match body ID | See [Validation Error Format](#validation-error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Coop with specified ID not found for this tenant | See [Error Format](#error-format) |
| 409 | `Error.Conflict` | A different coop with the same name already exists | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 5. DELETE /api/coops/{id}

Permanently deletes a coop (hard delete). The coop must not have any associated flocks.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the coop to delete |

#### Request Example

```http
DELETE /api/coops/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
true
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `HAS_FLOCKS` | Cannot delete coop because it has associated flocks | See [Flock Validation Error](#flock-validation-error) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Coop with specified ID not found for this tenant | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

### 6. PATCH /api/coops/{id}/archive

Archives a coop (soft delete). Archived coops are excluded from normal queries unless `includeArchived=true` is specified.

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The unique identifier of the coop to archive |

#### Request Example

```http
PATCH /api/coops/3fa85f64-5717-4562-b3fc-2c963f66afa6/archive
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)

```json
true
```

#### Error Responses

| Status Code | Error Code | Description | Example |
|-------------|------------|-------------|---------|
| 400 | `Error.Validation` | Invalid coop ID format | See [Error Format](#error-format) |
| 401 | `Error.Unauthorized` | User is not authenticated or token is invalid | See [Error Format](#error-format) |
| 404 | `Error.NotFound` | Coop with specified ID not found for this tenant | See [Error Format](#error-format) |
| 500 | `Error.Failure` | Internal server error | See [Error Format](#error-format) |

---

## Error Response Formats

### Error Format

Standard error response structure:

```json
{
  "error": {
    "code": "Error.NotFound",
    "message": "Coop not found"
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
        "message": "Coop name is required."
      },
      {
        "field": "Name",
        "message": "Coop name must not exceed 100 characters."
      }
    ]
  }
}
```

### Flock Validation Error

Special validation error when attempting to delete a coop with flocks:

```json
{
  "error": {
    "code": "HAS_FLOCKS",
    "message": "Cannot delete coop with existing flocks. Please delete or move all flocks first."
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
| 400 | `Error.Validation` | Request validation failed (invalid input data) | POST, PUT, DELETE, PATCH |
| 400 | `HAS_FLOCKS` | Coop cannot be deleted because it has associated flocks | DELETE |
| 401 | `Error.Unauthorized` | Authentication failed or user not authenticated | All endpoints |
| 404 | `Error.NotFound` | Resource not found for this tenant | GET by ID, PUT, DELETE, PATCH |
| 409 | `Error.Conflict` | Resource conflict (e.g., duplicate name) | POST, PUT |
| 500 | `Error.Failure` | Internal server error | All endpoints |

---

## Data Model

### CoopDto

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `id` | GUID | No | Unique identifier for the coop |
| `tenantId` | GUID | No | The tenant that owns this coop (automatically set) |
| `name` | string | No | Name of the coop (max 100 characters) |
| `location` | string | Yes | Optional location description (max 200 characters) |
| `createdAt` | DateTime | No | Timestamp when the coop was created (ISO 8601 format) |
| `updatedAt` | DateTime | No | Timestamp when the coop was last updated (ISO 8601 format) |
| `isActive` | boolean | No | Indicates whether the coop is active (false if archived) |
| `flocksCount` | integer | No | Number of flocks associated with this coop (includes active and archived) |

---

## Business Rules

1. **Tenant Isolation**: Users can only access coops belonging to their tenant. This is enforced automatically via Row-Level Security (RLS) and EF Core global query filters.

2. **Name Uniqueness**: Coop names must be unique per tenant (enforced at database level).

3. **Soft Delete (Archive)**: Archiving a coop sets `isActive = false`. Archived coops are excluded from `GET /api/coops` unless `includeArchived=true` is specified.

4. **Hard Delete Restriction**: A coop can only be permanently deleted if it has no associated flocks. Use the archive endpoint for coops with flocks.

5. **Automatic Timestamps**: `createdAt` and `updatedAt` are automatically managed by the system.

6. **Flock Count**: The `flocksCount` field includes both active and archived flocks.

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
Base URL includes version: `/api/coops`

Future versions will use explicit versioning: `/api/v2/coops`

---

## Examples

### Example 1: Create a new coop

```bash
curl -X POST https://api.chickquita.com/api/coops \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Garden Coop",
    "location": "Next to the vegetable garden"
  }'
```

### Example 2: Get all coops including archived

```bash
curl -X GET "https://api.chickquita.com/api/coops?includeArchived=true" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Example 3: Update a coop

```bash
curl -X PUT https://api.chickquita.com/api/coops/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Updated Garden Coop",
    "location": "Relocated to east side"
  }'
```

### Example 4: Archive a coop

```bash
curl -X PATCH https://api.chickquita.com/api/coops/3fa85f64-5717-4562-b3fc-2c963f66afa6/archive \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Example 5: Delete a coop (no flocks)

```bash
curl -X DELETE https://api.chickquita.com/api/coops/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
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

---

## Related Documentation

- [CLAUDE.md](../CLAUDE.md) - Project overview and conventions
- [Technology Stack](./technology-stack.md) - Full tech stack documentation
- [Filesystem Structure](./filesystem-structure.md) - Project structure
