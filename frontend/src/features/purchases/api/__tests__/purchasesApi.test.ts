import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { purchasesApi } from '../purchasesApi';
import { apiClient } from '../../../../lib/apiClient';
import { PurchaseType, QuantityUnit } from '../../types/purchase.types';
import type { PurchaseDto } from '../../types/purchase.types';
import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios';

/**
 * Unit tests for purchasesApi.
 * Tests cover all CRUD operations with proper error handling.
 * Uses Vitest mocking for consistency with existing tests.
 */
vi.mock('../../../../lib/apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('purchasesApi', () => {
  const mockPurchase: PurchaseDto = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    tenantId: '123e4567-e89b-12d3-a456-426614174001',
    coopId: '123e4567-e89b-12d3-a456-426614174002',
    name: 'Premium Chicken Feed',
    type: PurchaseType.Feed,
    amount: 450.50,
    quantity: 25,
    unit: QuantityUnit.Kg,
    purchaseDate: '2024-01-15T00:00:00Z',
    consumedDate: null,
    notes: 'Organic feed for winter',
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-01-15T10:00:00Z',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('getPurchases', () => {
    it('should fetch all purchases without filters', async () => {
      const mockResponse: AxiosResponse<PurchaseDto[]> = {
        data: [mockPurchase],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchases();

      expect(apiClient.get).toHaveBeenCalledWith('/purchases');
      expect(result).toEqual([mockPurchase]);
    });

    it('should fetch purchases with date range filters', async () => {
      const mockResponse: AxiosResponse<PurchaseDto[]> = {
        data: [mockPurchase],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const fromDate = '2024-01-01T00:00:00Z';
      const toDate = '2024-01-31T23:59:59Z';

      const result = await purchasesApi.getPurchases({ fromDate, toDate });

      expect(apiClient.get).toHaveBeenCalledWith(
        `/purchases?fromDate=${encodeURIComponent(fromDate)}&toDate=${encodeURIComponent(toDate)}`
      );
      expect(result).toEqual([mockPurchase]);
    });

    it('should fetch purchases with type filter', async () => {
      const mockResponse: AxiosResponse<PurchaseDto[]> = {
        data: [mockPurchase],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchases({ type: PurchaseType.Feed });

      expect(apiClient.get).toHaveBeenCalledWith(`/purchases?type=${PurchaseType.Feed}`);
      expect(result).toEqual([mockPurchase]);
    });

    it('should fetch purchases with flockId filter', async () => {
      const mockResponse: AxiosResponse<PurchaseDto[]> = {
        data: [mockPurchase],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const flockId = '123e4567-e89b-12d3-a456-426614174003';

      const result = await purchasesApi.getPurchases({ flockId });

      expect(apiClient.get).toHaveBeenCalledWith(`/purchases?flockId=${flockId}`);
      expect(result).toEqual([mockPurchase]);
    });

    it('should fetch purchases with all filters', async () => {
      const mockResponse: AxiosResponse<PurchaseDto[]> = {
        data: [mockPurchase],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const params = {
        fromDate: '2024-01-01T00:00:00Z',
        toDate: '2024-01-31T23:59:59Z',
        type: PurchaseType.Feed,
        flockId: '123e4567-e89b-12d3-a456-426614174003',
      };

      const result = await purchasesApi.getPurchases(params);

      expect(apiClient.get).toHaveBeenCalledWith(
        `/purchases?fromDate=${encodeURIComponent(params.fromDate)}&toDate=${encodeURIComponent(params.toDate)}&type=${params.type}&flockId=${params.flockId}`
      );
      expect(result).toEqual([mockPurchase]);
    });

    it('should handle type filter with value 0 (Feed)', async () => {
      const mockResponse: AxiosResponse<PurchaseDto[]> = {
        data: [mockPurchase],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchases({ type: 0 });

      expect(apiClient.get).toHaveBeenCalledWith(`/purchases?type=0`);
      expect(result).toEqual([mockPurchase]);
    });

    it('should handle API errors properly', async () => {
      const error = new Error('Network error');
      vi.mocked(apiClient.get).mockRejectedValue(error);

      await expect(purchasesApi.getPurchases()).rejects.toThrow('Network error');
      expect(apiClient.get).toHaveBeenCalledWith('/purchases');
    });

    it('should return empty array when no purchases found', async () => {
      const mockResponse: AxiosResponse<PurchaseDto[]> = {
        data: [],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchases();

      expect(result).toEqual([]);
    });
  });

  describe('getPurchaseById', () => {
    const purchaseId = '123e4567-e89b-12d3-a456-426614174000';

    it('should fetch a specific purchase by ID', async () => {
      const mockResponse: AxiosResponse<PurchaseDto> = {
        data: mockPurchase,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchaseById(purchaseId);

      expect(apiClient.get).toHaveBeenCalledWith(`/purchases/${purchaseId}`);
      expect(result).toEqual(mockPurchase);
    });

    it('should handle not found errors', async () => {
      const notFoundError = {
        response: {
          status: 404,
          data: {
            error: {
              code: 'NOT_FOUND',
              message: 'Purchase not found',
            },
          },
        },
      };

      vi.mocked(apiClient.get).mockRejectedValue(notFoundError);

      await expect(purchasesApi.getPurchaseById(purchaseId)).rejects.toEqual(notFoundError);
      expect(apiClient.get).toHaveBeenCalledWith(`/purchases/${purchaseId}`);
    });

    it('should handle network errors', async () => {
      const networkError = new Error('Network timeout');
      vi.mocked(apiClient.get).mockRejectedValue(networkError);

      await expect(purchasesApi.getPurchaseById(purchaseId)).rejects.toThrow('Network timeout');
    });
  });

  describe('getPurchaseNames', () => {
    it('should fetch purchase names without query filter', async () => {
      const mockNames = ['Premium Chicken Feed', 'Organic Bedding', 'Vitamin Supplements'];
      const mockResponse: AxiosResponse<string[]> = {
        data: mockNames,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchaseNames();

      expect(apiClient.get).toHaveBeenCalledWith('/purchases/names?limit=20');
      expect(result).toEqual(mockNames);
    });

    it('should fetch purchase names with query filter', async () => {
      const mockNames = ['Premium Chicken Feed'];
      const mockResponse: AxiosResponse<string[]> = {
        data: mockNames,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchaseNames('feed');

      expect(apiClient.get).toHaveBeenCalledWith('/purchases/names?query=feed&limit=20');
      expect(result).toEqual(mockNames);
    });

    it('should fetch purchase names with custom limit', async () => {
      const mockNames = ['Premium Chicken Feed'];
      const mockResponse: AxiosResponse<string[]> = {
        data: mockNames,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchaseNames('feed', 10);

      expect(apiClient.get).toHaveBeenCalledWith('/purchases/names?query=feed&limit=10');
      expect(result).toEqual(mockNames);
    });

    it('should return empty array when no names match', async () => {
      const mockResponse: AxiosResponse<string[]> = {
        data: [],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await purchasesApi.getPurchaseNames('nonexistent');

      expect(result).toEqual([]);
    });

    it('should handle API errors properly', async () => {
      const error = new Error('Bad request');
      vi.mocked(apiClient.get).mockRejectedValue(error);

      await expect(purchasesApi.getPurchaseNames()).rejects.toThrow('Bad request');
    });
  });

  describe('createPurchase', () => {
    const createData = {
      coopId: '123e4567-e89b-12d3-a456-426614174002',
      name: 'Premium Chicken Feed',
      type: PurchaseType.Feed,
      amount: 450.50,
      quantity: 25,
      unit: QuantityUnit.Kg,
      purchaseDate: '2024-01-15T00:00:00Z',
      consumedDate: null,
      notes: 'Organic feed for winter',
    };

    it('should create a new purchase with all fields', async () => {
      const mockResponse: AxiosResponse<PurchaseDto> = {
        data: mockPurchase,
        status: 201,
        statusText: 'Created',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

      const result = await purchasesApi.createPurchase(createData);

      expect(apiClient.post).toHaveBeenCalledWith('/purchases', createData);
      expect(result).toEqual(mockPurchase);
    });

    it('should create a purchase without optional fields', async () => {
      const minimalData = {
        name: 'Basic Feed',
        type: PurchaseType.Feed,
        amount: 100,
        quantity: 10,
        unit: QuantityUnit.Kg,
        purchaseDate: '2024-01-15T00:00:00Z',
      };

      const minimalPurchase = {
        ...mockPurchase,
        coopId: null,
        consumedDate: null,
        notes: null,
        name: 'Basic Feed',
        amount: 100,
        quantity: 10,
      };

      const mockResponse: AxiosResponse<PurchaseDto> = {
        data: minimalPurchase,
        status: 201,
        statusText: 'Created',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

      const result = await purchasesApi.createPurchase(minimalData);

      expect(apiClient.post).toHaveBeenCalledWith('/purchases', minimalData);
      expect(result).toEqual(minimalPurchase);
    });

    it('should create a purchase with consumedDate', async () => {
      const dataWithConsumedDate = {
        ...createData,
        consumedDate: '2024-02-15T00:00:00Z',
      };

      const purchaseWithConsumedDate = {
        ...mockPurchase,
        consumedDate: '2024-02-15T00:00:00Z',
      };

      const mockResponse: AxiosResponse<PurchaseDto> = {
        data: purchaseWithConsumedDate,
        status: 201,
        statusText: 'Created',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

      const result = await purchasesApi.createPurchase(dataWithConsumedDate);

      expect(apiClient.post).toHaveBeenCalledWith('/purchases', dataWithConsumedDate);
      expect(result.consumedDate).toBe('2024-02-15T00:00:00Z');
    });

    it('should handle validation errors', async () => {
      const validationError = {
        response: {
          status: 400,
          data: {
            error: {
              code: 'VALIDATION_ERROR',
              message: 'Amount must be positive',
            },
          },
        },
      };

      vi.mocked(apiClient.post).mockRejectedValue(validationError);

      await expect(purchasesApi.createPurchase(createData)).rejects.toEqual(validationError);
      expect(apiClient.post).toHaveBeenCalledWith('/purchases', createData);
    });

    it('should handle coop not found errors', async () => {
      const notFoundError = {
        response: {
          status: 404,
          data: {
            error: {
              code: 'NOT_FOUND',
              message: 'Coop not found',
            },
          },
        },
      };

      vi.mocked(apiClient.post).mockRejectedValue(notFoundError);

      await expect(purchasesApi.createPurchase(createData)).rejects.toEqual(notFoundError);
    });
  });

  describe('updatePurchase', () => {
    const updateData = {
      id: '123e4567-e89b-12d3-a456-426614174000',
      coopId: '123e4567-e89b-12d3-a456-426614174002',
      name: 'Updated Feed Name',
      type: PurchaseType.Feed,
      amount: 500,
      quantity: 30,
      unit: QuantityUnit.Kg,
      purchaseDate: '2024-01-15T00:00:00Z',
      consumedDate: '2024-02-01T00:00:00Z',
      notes: 'Updated notes',
    };

    it('should update an existing purchase', async () => {
      const updatedPurchase = { ...mockPurchase, ...updateData };

      const mockResponse: AxiosResponse<PurchaseDto> = {
        data: updatedPurchase,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.put).mockResolvedValue(mockResponse);

      const result = await purchasesApi.updatePurchase(updateData);

      expect(apiClient.put).toHaveBeenCalledWith(`/purchases/${updateData.id}`, updateData);
      expect(result).toEqual(updatedPurchase);
      expect(result.name).toBe('Updated Feed Name');
      expect(result.amount).toBe(500);
    });

    it('should update purchase with null consumedDate', async () => {
      const dataWithNullConsumedDate = {
        ...updateData,
        consumedDate: null,
      };

      const updatedPurchase = { ...mockPurchase, ...dataWithNullConsumedDate };

      const mockResponse: AxiosResponse<PurchaseDto> = {
        data: updatedPurchase,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.put).mockResolvedValue(mockResponse);

      const result = await purchasesApi.updatePurchase(dataWithNullConsumedDate);

      expect(apiClient.put).toHaveBeenCalledWith(
        `/purchases/${updateData.id}`,
        dataWithNullConsumedDate
      );
      expect(result.consumedDate).toBeNull();
    });

    it('should handle not found errors', async () => {
      const notFoundError = {
        response: {
          status: 404,
          data: {
            error: {
              code: 'NOT_FOUND',
              message: 'Purchase not found',
            },
          },
        },
      };

      vi.mocked(apiClient.put).mockRejectedValue(notFoundError);

      await expect(purchasesApi.updatePurchase(updateData)).rejects.toEqual(notFoundError);
      expect(apiClient.put).toHaveBeenCalledWith(`/purchases/${updateData.id}`, updateData);
    });

    it('should handle validation errors', async () => {
      const validationError = {
        response: {
          status: 400,
          data: {
            error: {
              code: 'VALIDATION_ERROR',
              message: 'Quantity must be positive',
            },
          },
        },
      };

      vi.mocked(apiClient.put).mockRejectedValue(validationError);

      await expect(purchasesApi.updatePurchase(updateData)).rejects.toEqual(validationError);
    });

    it('should handle forbidden errors', async () => {
      const forbiddenError = {
        response: {
          status: 403,
          data: {
            error: {
              code: 'FORBIDDEN',
              message: 'You do not have permission to update this purchase',
            },
          },
        },
      };

      vi.mocked(apiClient.put).mockRejectedValue(forbiddenError);

      await expect(purchasesApi.updatePurchase(updateData)).rejects.toEqual(forbiddenError);
    });
  });

  describe('deletePurchase', () => {
    const purchaseId = '123e4567-e89b-12d3-a456-426614174000';

    it('should delete a purchase successfully', async () => {
      const mockResponse: AxiosResponse<void> = {
        data: undefined,
        status: 204,
        statusText: 'No Content',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.delete).mockResolvedValue(mockResponse);

      const result = await purchasesApi.deletePurchase(purchaseId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/purchases/${purchaseId}`);
      expect(result).toBe(true);
    });

    it('should handle not found errors', async () => {
      const notFoundError = {
        response: {
          status: 404,
          data: {
            error: {
              code: 'NOT_FOUND',
              message: 'Purchase not found',
            },
          },
        },
      };

      vi.mocked(apiClient.delete).mockRejectedValue(notFoundError);

      await expect(purchasesApi.deletePurchase(purchaseId)).rejects.toEqual(notFoundError);
      expect(apiClient.delete).toHaveBeenCalledWith(`/purchases/${purchaseId}`);
    });

    it('should handle forbidden errors', async () => {
      const forbiddenError = {
        response: {
          status: 403,
          data: {
            error: {
              code: 'FORBIDDEN',
              message: 'You do not have permission to delete this purchase',
            },
          },
        },
      };

      vi.mocked(apiClient.delete).mockRejectedValue(forbiddenError);

      await expect(purchasesApi.deletePurchase(purchaseId)).rejects.toEqual(forbiddenError);
    });

    it('should handle network errors', async () => {
      const networkError = new Error('Connection timeout');
      vi.mocked(apiClient.delete).mockRejectedValue(networkError);

      await expect(purchasesApi.deletePurchase(purchaseId)).rejects.toThrow('Connection timeout');
    });
  });
});
