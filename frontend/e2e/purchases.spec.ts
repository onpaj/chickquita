import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Purchases API endpoints
 *
 * Coverage:
 * - Happy path: Create → Read → Update → Delete
 * - Error path: Invalid data returns 400
 * - API endpoint accessibility
 * - Response structure validation
 */

test.describe('Purchases API Endpoints', () => {
  let createdPurchaseId: string;

  // Verify backend is responding before running any tests
  test.beforeAll(async ({ request }) => {
    // Check backend health
    const healthResponse = await request.get('http://localhost:5100/health');
    if (!healthResponse.ok()) {
      throw new Error(
        'Backend is not responding! Make sure the backend is running:\n' +
        'cd backend && dotnet run --project src/Chickquita.Api'
      );
    }
  });

  test.describe('Happy Path: Create → Read → Update → Delete', () => {
    test('should create a new purchase successfully', async ({ request }) => {
      const purchaseData = {
        name: 'E2E Test Feed',
        type: 0, // PurchaseType.Feed
        amount: 250.50,
        quantity: 25,
        unit: 0, // QuantityUnit.Kg
        purchaseDate: new Date().toISOString().split('T')[0],
        notes: 'E2E test purchase'
      };

      const response = await request.post('http://localhost:5100/api/v1/purchases', {
        data: purchaseData,
        headers: {
          'Content-Type': 'application/json'
        }
      });

      // Verify 201 Created
      expect(response.status()).toBe(201);

      // Verify Location header
      const location = response.headers()['location'];
      expect(location).toBeTruthy();
      expect(location).toContain('/api/v1/purchases/');

      // Verify response structure
      const data = await response.json();
      expect(data).toHaveProperty('id');
      expect(data.name).toBe('E2E Test Feed');
      expect(data.type).toBe(0);
      expect(data.amount).toBe(250.50);
      expect(data.quantity).toBe(25);

      // Store ID for subsequent tests
      createdPurchaseId = data.id;
    });

    test('should get the created purchase by ID', async ({ request }) => {
      test.skip(!createdPurchaseId, 'No purchase ID from create test');

      const response = await request.get(`http://localhost:5100/api/v1/purchases/${createdPurchaseId}`);

      // Verify 200 OK
      expect(response.status()).toBe(200);

      // Verify response structure
      const data = await response.json();
      expect(data.id).toBe(createdPurchaseId);
      expect(data.name).toBe('E2E Test Feed');
      expect(data.amount).toBe(250.50);
    });

    test('should get all purchases including the created one', async ({ request }) => {
      test.skip(!createdPurchaseId, 'No purchase ID from create test');

      const response = await request.get('http://localhost:5100/api/v1/purchases');

      // Verify 200 OK
      expect(response.status()).toBe(200);

      // Verify response is an array
      const data = await response.json();
      expect(Array.isArray(data)).toBe(true);

      // Verify our purchase is in the list
      const ourPurchase = data.find((p: any) => p.id === createdPurchaseId);
      expect(ourPurchase).toBeTruthy();
      expect(ourPurchase.name).toBe('E2E Test Feed');
    });

    test('should update the purchase successfully', async ({ request }) => {
      test.skip(!createdPurchaseId, 'No purchase ID from create test');

      const updateData = {
        id: createdPurchaseId,
        name: 'E2E Updated Feed',
        type: 0, // PurchaseType.Feed
        amount: 300.00,
        quantity: 30,
        unit: 0, // QuantityUnit.Kg
        purchaseDate: new Date().toISOString().split('T')[0],
        notes: 'Updated in E2E test'
      };

      const response = await request.put(`http://localhost:5100/api/v1/purchases/${createdPurchaseId}`, {
        data: updateData,
        headers: {
          'Content-Type': 'application/json'
        }
      });

      // Verify 200 OK
      expect(response.status()).toBe(200);

      // Verify updated values
      const data = await response.json();
      expect(data.id).toBe(createdPurchaseId);
      expect(data.name).toBe('E2E Updated Feed');
      expect(data.amount).toBe(300.00);
      expect(data.quantity).toBe(30);
      expect(data.notes).toBe('Updated in E2E test');
    });

    test('should delete the purchase successfully', async ({ request }) => {
      test.skip(!createdPurchaseId, 'No purchase ID from create test');

      const response = await request.delete(`http://localhost:5100/api/v1/purchases/${createdPurchaseId}`);

      // Verify 204 No Content
      expect(response.status()).toBe(204);
    });

    test('should return 404 when getting deleted purchase', async ({ request }) => {
      test.skip(!createdPurchaseId, 'No purchase ID from create test');

      const response = await request.get(`http://localhost:5100/api/v1/purchases/${createdPurchaseId}`);

      // Verify 404 Not Found
      expect(response.status()).toBe(404);
    });
  });

  test.describe('Error Path: Invalid Data', () => {
    test('should return 400 for invalid purchase data', async ({ request }) => {
      const invalidData = {
        name: '', // Invalid: empty name
        type: 0,
        amount: 250.50,
        quantity: 25,
        unit: 0,
        purchaseDate: new Date().toISOString().split('T')[0]
      };

      const response = await request.post('http://localhost:5100/api/v1/purchases', {
        data: invalidData,
        headers: {
          'Content-Type': 'application/json'
        }
      });

      // Verify 400 Bad Request
      expect(response.status()).toBe(400);

      // Verify error response structure
      const data = await response.json();
      expect(data).toHaveProperty('error');
    });

    test('should return 404 for non-existent purchase ID', async ({ request }) => {
      const nonExistentId = '00000000-0000-0000-0000-000000000000';

      const response = await request.get(`http://localhost:5100/api/v1/purchases/${nonExistentId}`);

      // Verify 404 Not Found
      expect(response.status()).toBe(404);
    });

    test('should return 400 when updating with mismatched IDs', async ({ request }) => {
      const purchaseId = '11111111-1111-1111-1111-111111111111';
      const differentId = '22222222-2222-2222-2222-222222222222';

      const updateData = {
        id: differentId, // Different from URL
        name: 'Test Feed',
        type: 0,
        amount: 300.00,
        quantity: 30,
        unit: 0,
        purchaseDate: new Date().toISOString().split('T')[0]
      };

      const response = await request.put(`http://localhost:5100/api/v1/purchases/${purchaseId}`, {
        data: updateData,
        headers: {
          'Content-Type': 'application/json'
        }
      });

      // Verify 400 Bad Request
      expect(response.status()).toBe(400);
    });
  });

  test.describe('Purchase Names Endpoint', () => {
    test('should get purchase names for autocomplete', async ({ request }) => {
      const response = await request.get('http://localhost:5100/api/v1/purchases/names?limit=10');

      // Verify 200 OK
      expect(response.status()).toBe(200);

      // Verify response is an array of strings
      const data = await response.json();
      expect(Array.isArray(data)).toBe(true);
    });

    test('should filter purchase names by query parameter', async ({ request }) => {
      const response = await request.get('http://localhost:5100/api/v1/purchases/names?query=Feed&limit=10');

      // Verify 200 OK
      expect(response.status()).toBe(200);

      // Verify response structure
      const data = await response.json();
      expect(Array.isArray(data)).toBe(true);
    });
  });
});
