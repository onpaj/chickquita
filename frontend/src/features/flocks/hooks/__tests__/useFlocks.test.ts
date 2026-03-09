import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useCreateFlock, useUpdateFlock, useArchiveFlock } from '../useFlocks';

// Mock flocksApi
const mockCreate = vi.fn();
const mockUpdate = vi.fn();
const mockArchive = vi.fn();

vi.mock('../../api/flocksApi', () => ({
  flocksApi: {
    create: (...args: unknown[]) => mockCreate(...args),
    update: (...args: unknown[]) => mockUpdate(...args),
    archive: (...args: unknown[]) => mockArchive(...args),
    getAll: vi.fn().mockResolvedValue([]),
    getById: vi.fn().mockResolvedValue(null),
    matureChicks: vi.fn().mockResolvedValue(null),
  },
}));

// Mock useToast
vi.mock('../../../../hooks/useToast', () => ({
  useToast: () => ({
    showSuccess: vi.fn(),
    showError: vi.fn(),
  }),
}));

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

function createWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: React.ReactNode }) {
    return React.createElement(QueryClientProvider, { client: queryClient }, children);
  };
}

describe('useCreateFlock', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
  });

  it('invalidates ["flocks", "all"] after successful creation', async () => {
    const mockFlock = {
      id: 'flock-1',
      tenantId: 't1',
      coopId: 'coop-1',
      identifier: 'F1',
      hatchDate: '2024-01-01',
      currentHens: 10,
      currentRoosters: 1,
      currentChicks: 0,
      isActive: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      history: [],
    };
    mockCreate.mockResolvedValue(mockFlock);

    // Seed the ['flocks', 'all'] cache with some data
    queryClient.setQueryData(['flocks', 'all'], [{ id: 'old', identifier: 'Old', coopName: 'Coop' }]);

    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useCreateFlock(), {
      wrapper: createWrapper(queryClient),
    });

    await act(async () => {
      result.current.mutate({
        coopId: 'coop-1',
        identifier: 'F1',
        hatchDate: '2024-01-01',
        initialHens: 10,
        initialRoosters: 1,
        initialChicks: 0,
      });
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    const invalidatedKeys = invalidateSpy.mock.calls.map((call) => call[0]);
    const invalidatedFlat = invalidatedKeys.map((c) => JSON.stringify(c));
    expect(invalidatedFlat).toContain(JSON.stringify({ queryKey: ['flocks', 'all'] }));
  });

  it('invalidates scoped coop key after successful creation', async () => {
    mockCreate.mockResolvedValue({ id: 'flock-1', coopId: 'coop-1', identifier: 'F1', hatchDate: '2024-01-01', currentHens: 10, currentRoosters: 1, currentChicks: 0, isActive: true, createdAt: '', updatedAt: '', history: [], tenantId: 't1' });

    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useCreateFlock(), {
      wrapper: createWrapper(queryClient),
    });

    await act(async () => {
      result.current.mutate({ coopId: 'coop-1', identifier: 'F1', hatchDate: '2024-01-01', initialHens: 10, initialRoosters: 1, initialChicks: 0 });
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    const invalidatedKeys = invalidateSpy.mock.calls.map((call) => JSON.stringify(call[0]));
    expect(invalidatedKeys).toContain(JSON.stringify({ queryKey: ['flocks', 'coop-1'] }));
  });
});

describe('useUpdateFlock', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
  });

  it('invalidates ["flocks", "all"] after successful update', async () => {
    mockUpdate.mockResolvedValue({ id: 'flock-1' });

    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useUpdateFlock(), {
      wrapper: createWrapper(queryClient),
    });

    await act(async () => {
      result.current.mutate({
        coopId: 'coop-1',
        data: {
          flockId: 'flock-1',
          identifier: 'Updated',
          hatchDate: '2024-01-01',
          currentHens: 15,
          currentRoosters: 2,
          currentChicks: 0,
        },
      });
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    const invalidatedKeys = invalidateSpy.mock.calls.map((call) => JSON.stringify(call[0]));
    expect(invalidatedKeys).toContain(JSON.stringify({ queryKey: ['flocks', 'all'] }));
  });

  it('invalidates coop-scoped and flock-specific keys after successful update', async () => {
    mockUpdate.mockResolvedValue({ id: 'flock-1' });

    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useUpdateFlock(), {
      wrapper: createWrapper(queryClient),
    });

    await act(async () => {
      result.current.mutate({
        coopId: 'coop-1',
        data: {
          flockId: 'flock-1',
          identifier: 'Updated',
          hatchDate: '2024-01-01',
          currentHens: 15,
          currentRoosters: 2,
          currentChicks: 0,
        },
      });
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    const invalidatedKeys = invalidateSpy.mock.calls.map((call) => JSON.stringify(call[0]));
    expect(invalidatedKeys).toContain(JSON.stringify({ queryKey: ['flocks', 'coop-1'] }));
    expect(invalidatedKeys).toContain(JSON.stringify({ queryKey: ['flocks', 'coop-1', 'flock-1'] }));
  });
});

describe('useArchiveFlock', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
  });

  it('invalidates ["flocks", "all"] after successful archive', async () => {
    mockArchive.mockResolvedValue(undefined);

    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useArchiveFlock(), {
      wrapper: createWrapper(queryClient),
    });

    await act(async () => {
      result.current.mutate({ coopId: 'coop-1', flockId: 'flock-1' });
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    const invalidatedKeys = invalidateSpy.mock.calls.map((call) => JSON.stringify(call[0]));
    expect(invalidatedKeys).toContain(JSON.stringify({ queryKey: ['flocks', 'all'] }));
  });

  it('invalidates coop-scoped and flock-specific keys after successful archive', async () => {
    mockArchive.mockResolvedValue(undefined);

    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useArchiveFlock(), {
      wrapper: createWrapper(queryClient),
    });

    await act(async () => {
      result.current.mutate({ coopId: 'coop-1', flockId: 'flock-1' });
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    const invalidatedKeys = invalidateSpy.mock.calls.map((call) => JSON.stringify(call[0]));
    expect(invalidatedKeys).toContain(JSON.stringify({ queryKey: ['flocks', 'coop-1'] }));
    expect(invalidatedKeys).toContain(JSON.stringify({ queryKey: ['flocks', 'coop-1', 'flock-1'] }));
  });
});
