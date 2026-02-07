import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { dailyRecordsApi, type DailyRecordDto } from '../dailyRecordsApi';
import { apiClient } from '../../../../lib/apiClient';
import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios';

/**
 * Unit tests for dailyRecordsApi.
 * Tests cover all CRUD operations with proper error handling.
 * Uses Vitest mocking instead of MSW for simplicity and consistency with existing tests.
 */
vi.mock('../../../../lib/apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('dailyRecordsApi', () => {
  const mockDailyRecord: DailyRecordDto = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    tenantId: '123e4567-e89b-12d3-a456-426614174001',
    flockId: '123e4567-e89b-12d3-a456-426614174002',
    recordDate: '2024-01-15',
    eggCount: 25,
    notes: 'Good weather, productive day',
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-01-15T10:00:00Z',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('getAll', () => {
    it('should fetch all daily records without filters', async () => {
      const mockResponse: AxiosResponse<DailyRecordDto[]> = {
        data: [mockDailyRecord],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.getAll();

      expect(apiClient.get).toHaveBeenCalledWith('/daily-records');
      expect(result).toEqual([mockDailyRecord]);
    });

    it('should fetch daily records with flockId filter', async () => {
      const mockResponse: AxiosResponse<DailyRecordDto[]> = {
        data: [mockDailyRecord],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.getAll({ flockId: mockDailyRecord.flockId });

      expect(apiClient.get).toHaveBeenCalledWith(
        `/daily-records?flockId=${mockDailyRecord.flockId}`
      );
      expect(result).toEqual([mockDailyRecord]);
    });

    it('should fetch daily records with date range filters', async () => {
      const mockResponse: AxiosResponse<DailyRecordDto[]> = {
        data: [mockDailyRecord],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const startDate = '2024-01-01';
      const endDate = '2024-01-31';

      const result = await dailyRecordsApi.getAll({ startDate, endDate });

      expect(apiClient.get).toHaveBeenCalledWith(
        `/daily-records?startDate=${startDate}&endDate=${endDate}`
      );
      expect(result).toEqual([mockDailyRecord]);
    });

    it('should fetch daily records with all filters', async () => {
      const mockResponse: AxiosResponse<DailyRecordDto[]> = {
        data: [mockDailyRecord],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const params = {
        flockId: mockDailyRecord.flockId,
        startDate: '2024-01-01',
        endDate: '2024-01-31',
      };

      const result = await dailyRecordsApi.getAll(params);

      expect(apiClient.get).toHaveBeenCalledWith(
        `/daily-records?flockId=${params.flockId}&startDate=${params.startDate}&endDate=${params.endDate}`
      );
      expect(result).toEqual([mockDailyRecord]);
    });

    it('should handle API errors properly', async () => {
      const error = new Error('Network error');
      vi.mocked(apiClient.get).mockRejectedValue(error);

      await expect(dailyRecordsApi.getAll()).rejects.toThrow('Network error');
      expect(apiClient.get).toHaveBeenCalledWith('/daily-records');
    });
  });

  describe('getByFlock', () => {
    const flockId = '123e4567-e89b-12d3-a456-426614174002';

    it('should fetch daily records for a specific flock without date filters', async () => {
      const mockResponse: AxiosResponse<DailyRecordDto[]> = {
        data: [mockDailyRecord],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.getByFlock(flockId);

      expect(apiClient.get).toHaveBeenCalledWith(`/flocks/${flockId}/daily-records`);
      expect(result).toEqual([mockDailyRecord]);
    });

    it('should fetch daily records for a specific flock with date range', async () => {
      const mockResponse: AxiosResponse<DailyRecordDto[]> = {
        data: [mockDailyRecord],
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const startDate = '2024-01-01';
      const endDate = '2024-01-31';

      const result = await dailyRecordsApi.getByFlock(flockId, { startDate, endDate });

      expect(apiClient.get).toHaveBeenCalledWith(
        `/flocks/${flockId}/daily-records?startDate=${startDate}&endDate=${endDate}`
      );
      expect(result).toEqual([mockDailyRecord]);
    });

    it('should handle API errors properly', async () => {
      const error = new Error('Flock not found');
      vi.mocked(apiClient.get).mockRejectedValue(error);

      await expect(dailyRecordsApi.getByFlock(flockId)).rejects.toThrow('Flock not found');
      expect(apiClient.get).toHaveBeenCalledWith(`/flocks/${flockId}/daily-records`);
    });
  });

  describe('create', () => {
    const flockId = '123e4567-e89b-12d3-a456-426614174002';
    const createData = {
      recordDate: '2024-01-15',
      eggCount: 25,
      notes: 'Good weather, productive day',
    };

    it('should create a new daily record with all fields', async () => {
      const mockResponse: AxiosResponse<DailyRecordDto> = {
        data: mockDailyRecord,
        status: 201,
        statusText: 'Created',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.create(flockId, createData);

      expect(apiClient.post).toHaveBeenCalledWith(`/flocks/${flockId}/daily-records`, createData);
      expect(result).toEqual(mockDailyRecord);
    });

    it('should create a new daily record without notes', async () => {
      const dataWithoutNotes = {
        recordDate: '2024-01-15',
        eggCount: 25,
      };

      const recordWithoutNotes = { ...mockDailyRecord, notes: undefined };

      const mockResponse: AxiosResponse<DailyRecordDto> = {
        data: recordWithoutNotes,
        status: 201,
        statusText: 'Created',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.create(flockId, dataWithoutNotes);

      expect(apiClient.post).toHaveBeenCalledWith(
        `/flocks/${flockId}/daily-records`,
        dataWithoutNotes
      );
      expect(result).toEqual(recordWithoutNotes);
    });

    it('should create a daily record with zero egg count', async () => {
      const dataWithZeroEggs = {
        recordDate: '2024-01-15',
        eggCount: 0,
        notes: 'No eggs today',
      };

      const recordWithZeroEggs = { ...mockDailyRecord, eggCount: 0, notes: 'No eggs today' };

      const mockResponse: AxiosResponse<DailyRecordDto> = {
        data: recordWithZeroEggs,
        status: 201,
        statusText: 'Created',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.create(flockId, dataWithZeroEggs);

      expect(apiClient.post).toHaveBeenCalledWith(
        `/flocks/${flockId}/daily-records`,
        dataWithZeroEggs
      );
      expect(result.eggCount).toBe(0);
    });

    it('should handle validation errors', async () => {
      const validationError = {
        response: {
          status: 400,
          data: {
            error: {
              code: 'VALIDATION_ERROR',
              message: 'Egg count cannot be negative',
            },
          },
        },
      };

      vi.mocked(apiClient.post).mockRejectedValue(validationError);

      await expect(dailyRecordsApi.create(flockId, createData)).rejects.toEqual(validationError);
      expect(apiClient.post).toHaveBeenCalledWith(`/flocks/${flockId}/daily-records`, createData);
    });

    it('should handle conflict errors (duplicate record)', async () => {
      const conflictError = {
        response: {
          status: 409,
          data: {
            error: {
              code: 'CONFLICT',
              message: 'A daily record already exists for this flock on the specified date',
            },
          },
        },
      };

      vi.mocked(apiClient.post).mockRejectedValue(conflictError);

      await expect(dailyRecordsApi.create(flockId, createData)).rejects.toEqual(conflictError);
    });
  });

  describe('update', () => {
    const updateData = {
      id: '123e4567-e89b-12d3-a456-426614174000',
      eggCount: 30,
      notes: 'Updated notes',
    };

    it('should update an existing daily record', async () => {
      const updatedRecord = { ...mockDailyRecord, ...updateData };

      const mockResponse: AxiosResponse<DailyRecordDto> = {
        data: updatedRecord,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.put).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.update(updateData);

      expect(apiClient.put).toHaveBeenCalledWith(`/daily-records/${updateData.id}`, updateData);
      expect(result).toEqual(updatedRecord);
      expect(result.eggCount).toBe(30);
      expect(result.notes).toBe('Updated notes');
    });

    it('should update daily record with only egg count', async () => {
      const dataWithOnlyEggCount = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 35,
      };

      const updatedRecord = { ...mockDailyRecord, eggCount: 35 };

      const mockResponse: AxiosResponse<DailyRecordDto> = {
        data: updatedRecord,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.put).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.update(dataWithOnlyEggCount);

      expect(apiClient.put).toHaveBeenCalledWith(
        `/daily-records/${dataWithOnlyEggCount.id}`,
        dataWithOnlyEggCount
      );
      expect(result.eggCount).toBe(35);
    });

    it('should handle not found errors', async () => {
      const notFoundError = {
        response: {
          status: 404,
          data: {
            error: {
              code: 'NOT_FOUND',
              message: 'Daily record not found',
            },
          },
        },
      };

      vi.mocked(apiClient.put).mockRejectedValue(notFoundError);

      await expect(dailyRecordsApi.update(updateData)).rejects.toEqual(notFoundError);
      expect(apiClient.put).toHaveBeenCalledWith(`/daily-records/${updateData.id}`, updateData);
    });

    it('should handle same-day edit restriction errors', async () => {
      const forbiddenError = {
        response: {
          status: 403,
          data: {
            error: {
              code: 'FORBIDDEN',
              message: 'Daily records can only be edited on the same day they were created',
            },
          },
        },
      };

      vi.mocked(apiClient.put).mockRejectedValue(forbiddenError);

      await expect(dailyRecordsApi.update(updateData)).rejects.toEqual(forbiddenError);
    });
  });

  describe('delete', () => {
    const recordId = '123e4567-e89b-12d3-a456-426614174000';

    it('should delete a daily record successfully', async () => {
      const mockResponse: AxiosResponse<void> = {
        data: undefined,
        status: 204,
        statusText: 'No Content',
        headers: {},
        config: {} as InternalAxiosRequestConfig,
      };

      vi.mocked(apiClient.delete).mockResolvedValue(mockResponse);

      const result = await dailyRecordsApi.delete(recordId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/daily-records/${recordId}`);
      expect(result).toBe(true);
    });

    it('should handle not found errors', async () => {
      const notFoundError = {
        response: {
          status: 404,
          data: {
            error: {
              code: 'NOT_FOUND',
              message: 'Daily record not found',
            },
          },
        },
      };

      vi.mocked(apiClient.delete).mockRejectedValue(notFoundError);

      await expect(dailyRecordsApi.delete(recordId)).rejects.toEqual(notFoundError);
      expect(apiClient.delete).toHaveBeenCalledWith(`/daily-records/${recordId}`);
    });

    it('should handle network errors', async () => {
      const networkError = new Error('Network error');
      vi.mocked(apiClient.delete).mockRejectedValue(networkError);

      await expect(dailyRecordsApi.delete(recordId)).rejects.toThrow('Network error');
    });
  });
});
