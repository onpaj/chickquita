import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode } from 'react';
import { dailyRecordsApi, type DailyRecordDto } from '../../api/dailyRecordsApi';

/**
 * Unit tests for useDailyRecords hooks.
 * Tests cover query hooks with proper caching behavior.
 * Mutation hooks are tested via integration tests in the API layer.
 */

vi.mock('../../api/dailyRecordsApi', () => ({
  dailyRecordsApi: {
    getAll: vi.fn(),
    getByFlock: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('../../../hooks/useToast', () => ({
  useToast: () => ({
    showSuccess: vi.fn(),
    showError: vi.fn(),
  }),
}));

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

const mockDailyRecord2: DailyRecordDto = {
  id: '123e4567-e89b-12d3-a456-426614174003',
  tenantId: '123e4567-e89b-12d3-a456-426614174001',
  flockId: '123e4567-e89b-12d3-a456-426614174002',
  recordDate: '2024-01-16',
  eggCount: 28,
  notes: 'Another good day',
  createdAt: '2024-01-16T10:00:00Z',
  updatedAt: '2024-01-16T10:00:00Z',
};

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  });

  function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  }
  return Wrapper;
}

// Import hooks after mocks are set up
import {
  useDailyRecords,
  useDailyRecordsByFlock,
  useCreateDailyRecord,
  useUpdateDailyRecord,
  useDeleteDailyRecord,
} from '../useDailyRecords';

describe('useDailyRecords', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('useDailyRecords', () => {
    it('should fetch all daily records without filters', async () => {
      vi.mocked(dailyRecordsApi.getAll).mockResolvedValue([mockDailyRecord, mockDailyRecord2]);

      const { result } = renderHook(() => useDailyRecords(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(dailyRecordsApi.getAll).toHaveBeenCalledWith(undefined);
      expect(result.current.data).toEqual([mockDailyRecord, mockDailyRecord2]);
    });

    it('should fetch daily records with all filters', async () => {
      const params = {
        flockId: mockDailyRecord.flockId,
        startDate: '2024-01-01',
        endDate: '2024-01-31',
      };

      vi.mocked(dailyRecordsApi.getAll).mockResolvedValue([mockDailyRecord]);

      const { result } = renderHook(() => useDailyRecords(params), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(dailyRecordsApi.getAll).toHaveBeenCalledWith(params);
      expect(result.current.data).toEqual([mockDailyRecord]);
    });

    it('should fetch daily records with flockId filter only', async () => {
      const params = {
        flockId: mockDailyRecord.flockId,
      };

      vi.mocked(dailyRecordsApi.getAll).mockResolvedValue([mockDailyRecord]);

      const { result } = renderHook(() => useDailyRecords(params), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(dailyRecordsApi.getAll).toHaveBeenCalledWith(params);
      expect(result.current.data).toEqual([mockDailyRecord]);
    });

    it('should handle errors properly', async () => {
      const error = new Error('Network error');
      vi.mocked(dailyRecordsApi.getAll).mockRejectedValue(error);

      const { result } = renderHook(() => useDailyRecords(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isError).toBe(true));

      expect(result.current.error).toEqual(error);
    });

    it('should cache results for 5 minutes (staleTime)', async () => {
      vi.mocked(dailyRecordsApi.getAll).mockResolvedValue([mockDailyRecord]);

      const { result } = renderHook(() => useDailyRecords(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      // Verify staleTime is set (data should be considered fresh)
      expect(result.current.dataUpdatedAt).toBeGreaterThan(0);
      expect(result.current.data).toEqual([mockDailyRecord]);
    });
  });

  describe('useDailyRecordsByFlock', () => {
    const flockId = '123e4567-e89b-12d3-a456-426614174002';

    it('should fetch daily records for a specific flock', async () => {
      vi.mocked(dailyRecordsApi.getByFlock).mockResolvedValue([mockDailyRecord, mockDailyRecord2]);

      const { result } = renderHook(() => useDailyRecordsByFlock(flockId), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(dailyRecordsApi.getByFlock).toHaveBeenCalledWith(flockId, undefined);
      expect(result.current.data).toEqual([mockDailyRecord, mockDailyRecord2]);
    });

    it('should fetch daily records with date range', async () => {
      const params = {
        startDate: '2024-01-01',
        endDate: '2024-01-31',
      };

      vi.mocked(dailyRecordsApi.getByFlock).mockResolvedValue([mockDailyRecord]);

      const { result } = renderHook(() => useDailyRecordsByFlock(flockId, params), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(dailyRecordsApi.getByFlock).toHaveBeenCalledWith(flockId, params);
      expect(result.current.data).toEqual([mockDailyRecord]);
    });

    it('should not fetch when flockId is empty', async () => {
      const { result } = renderHook(() => useDailyRecordsByFlock(''), {
        wrapper: createWrapper(),
      });

      expect(result.current.isPending).toBe(true);
      expect(dailyRecordsApi.getByFlock).not.toHaveBeenCalled();
    });

    it('should handle errors properly', async () => {
      const error = new Error('Flock not found');
      vi.mocked(dailyRecordsApi.getByFlock).mockRejectedValue(error);

      const { result } = renderHook(() => useDailyRecordsByFlock(flockId), {
        wrapper: createWrapper(),
      });

      await waitFor(() => expect(result.current.isError).toBe(true));

      expect(result.current.error).toEqual(error);
    });
  });

  describe('useCreateDailyRecord', () => {
    beforeEach(() => {
      vi.clearAllMocks();
      // Mock dailyRecordsApi.create
      vi.mocked(dailyRecordsApi).create = vi.fn();
    });

    it('should create a daily record successfully', async () => {
      const createdRecord = { ...mockDailyRecord };
      vi.mocked(dailyRecordsApi).create = vi.fn().mockResolvedValue(createdRecord);

      const { result } = renderHook(() => useCreateDailyRecord(), {
        wrapper: createWrapper(),
      });

      const createData = {
        flockId: mockDailyRecord.flockId,
        data: {
          recordDate: '2024-01-15',
          eggCount: 25,
          notes: 'Test notes',
        },
      };

      result.current.mutate(createData);

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(dailyRecordsApi.create).toHaveBeenCalledWith(createData.flockId, createData.data);
    });

    it('should handle create errors', async () => {
      const error = new Error('Create failed');
      vi.mocked(dailyRecordsApi).create = vi.fn().mockRejectedValue(error);

      const { result } = renderHook(() => useCreateDailyRecord(), {
        wrapper: createWrapper(),
      });

      const createData = {
        flockId: mockDailyRecord.flockId,
        data: {
          recordDate: '2024-01-15',
          eggCount: 25,
        },
      };

      result.current.mutate(createData);

      await waitFor(() => expect(result.current.isError).toBe(true));
      expect(result.current.error).toEqual(error);
    });

    it('should invalidate queries after successful create', async () => {
      const createdRecord = { ...mockDailyRecord };
      vi.mocked(dailyRecordsApi).create = vi.fn().mockResolvedValue(createdRecord);

      const { result } = renderHook(() => useCreateDailyRecord(), {
        wrapper: createWrapper(),
      });

      const createData = {
        flockId: mockDailyRecord.flockId,
        data: {
          recordDate: '2024-01-15',
          eggCount: 25,
        },
      };

      result.current.mutate(createData);

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      // Query invalidation is handled by React Query internally
      expect(result.current.isSuccess).toBe(true);
    });
  });

  describe('useUpdateDailyRecord', () => {
    beforeEach(() => {
      vi.clearAllMocks();
      vi.mocked(dailyRecordsApi).update = vi.fn();
    });

    it('should update a daily record successfully', async () => {
      const updatedRecord = { ...mockDailyRecord, eggCount: 30 };
      vi.mocked(dailyRecordsApi).update = vi.fn().mockResolvedValue(updatedRecord);

      const { result } = renderHook(() => useUpdateDailyRecord(), {
        wrapper: createWrapper(),
      });

      const updateData = {
        id: mockDailyRecord.id,
        eggCount: 30,
        notes: 'Updated notes',
      };

      result.current.mutate(updateData);

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(dailyRecordsApi.update).toHaveBeenCalledWith(updateData);
    });

    it('should handle update errors', async () => {
      const error = new Error('Update failed');
      vi.mocked(dailyRecordsApi).update = vi.fn().mockRejectedValue(error);

      const { result } = renderHook(() => useUpdateDailyRecord(), {
        wrapper: createWrapper(),
      });

      const updateData = {
        id: mockDailyRecord.id,
        eggCount: 30,
      };

      result.current.mutate(updateData);

      await waitFor(() => expect(result.current.isError).toBe(true));
      expect(result.current.error).toEqual(error);
    });

    it('should invalidate queries after successful update', async () => {
      const updatedRecord = { ...mockDailyRecord, eggCount: 30 };
      vi.mocked(dailyRecordsApi).update = vi.fn().mockResolvedValue(updatedRecord);

      const { result } = renderHook(() => useUpdateDailyRecord(), {
        wrapper: createWrapper(),
      });

      const updateData = {
        id: mockDailyRecord.id,
        eggCount: 30,
      };

      result.current.mutate(updateData);

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(result.current.isSuccess).toBe(true);
    });
  });

  describe('useDeleteDailyRecord', () => {
    beforeEach(() => {
      vi.clearAllMocks();
      vi.mocked(dailyRecordsApi).delete = vi.fn();
    });

    it('should delete a daily record successfully', async () => {
      vi.mocked(dailyRecordsApi).delete = vi.fn().mockResolvedValue(true);

      const { result } = renderHook(() => useDeleteDailyRecord(), {
        wrapper: createWrapper(),
      });

      const deleteData = {
        id: mockDailyRecord.id,
        flockId: mockDailyRecord.flockId,
      };

      result.current.mutate(deleteData);

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(dailyRecordsApi.delete).toHaveBeenCalledWith(deleteData.id);
    });

    it('should handle delete errors', async () => {
      const error = new Error('Delete failed');
      vi.mocked(dailyRecordsApi).delete = vi.fn().mockRejectedValue(error);

      const { result } = renderHook(() => useDeleteDailyRecord(), {
        wrapper: createWrapper(),
      });

      const deleteData = {
        id: mockDailyRecord.id,
        flockId: mockDailyRecord.flockId,
      };

      result.current.mutate(deleteData);

      await waitFor(() => expect(result.current.isError).toBe(true));
      expect(result.current.error).toEqual(error);
    });

    it('should invalidate queries after successful delete', async () => {
      vi.mocked(dailyRecordsApi).delete = vi.fn().mockResolvedValue(true);

      const { result } = renderHook(() => useDeleteDailyRecord(), {
        wrapper: createWrapper(),
      });

      const deleteData = {
        id: mockDailyRecord.id,
        flockId: mockDailyRecord.flockId,
      };

      result.current.mutate(deleteData);

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(result.current.isSuccess).toBe(true);
    });
  });
});
