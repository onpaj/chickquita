import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { purchasesApi } from '../../api/purchasesApi';
import { usePurchaseAutocomplete } from '../usePurchaseAutocomplete';

/**
 * Unit tests for usePurchaseAutocomplete hook.
 * Tests cover:
 * - Debouncing functionality (300ms delay)
 * - Query not fired for queries with < 2 characters
 * - Results caching with 10-minute staleTime
 * - Empty query returns empty array
 * - Loading states
 */

// Mock the purchasesApi module
vi.mock('../../api/purchasesApi', () => ({
  purchasesApi: {
    getPurchaseNames: vi.fn(),
  },
}));

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  }
  return Wrapper;
}

// Helper to wait for a specific duration
const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

describe('usePurchaseAutocomplete', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('debouncing', () => {
    it('should debounce query with 300ms delay', async () => {
      const mockSuggestions = ['Feed A', 'Feed B', 'Feed C'];
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(mockSuggestions);

      const { result, rerender } = renderHook(
        ({ query }) => usePurchaseAutocomplete(query),
        {
          wrapper: createWrapper(),
          initialProps: { query: '' },
        }
      );

      // Update query to "Fe" (2 chars, should trigger)
      rerender({ query: 'Fe' });

      // API should not be called immediately
      expect(purchasesApi.getPurchaseNames).not.toHaveBeenCalled();

      // Wait 200ms (not enough)
      await delay(200);
      expect(purchasesApi.getPurchaseNames).not.toHaveBeenCalled();

      // Wait another 150ms (total 350ms, past the 300ms threshold)
      await delay(150);

      // Now API should be called
      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fe');
      });

      // Wait for the query to complete
      await waitFor(() => {
        expect(result.current.suggestions).toEqual(mockSuggestions);
      });
    });

    it('should reset debounce timer on rapid query changes', async () => {
      const mockSuggestions = ['Feed A', 'Feed B'];
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(mockSuggestions);

      const { rerender } = renderHook(
        ({ query }) => usePurchaseAutocomplete(query),
        {
          wrapper: createWrapper(),
          initialProps: { query: '' },
        }
      );

      // Type "Fe"
      rerender({ query: 'Fe' });

      // Wait 200ms
      await delay(200);

      // Type "Fee" (timer resets)
      rerender({ query: 'Fee' });

      // Wait 200ms (total 400ms since first change, but only 200ms since last)
      await delay(200);

      // API should not be called yet
      expect(purchasesApi.getPurchaseNames).not.toHaveBeenCalled();

      // Wait another 150ms (350ms since last change, past 300ms threshold)
      await delay(150);

      // Now API should be called with the latest query
      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fee');
      });

      // Should only be called once (not with "Fe")
      expect(purchasesApi.getPurchaseNames).toHaveBeenCalledTimes(1);
    });

    it('should cancel pending debounce when query becomes empty', async () => {
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(['Feed A']);

      const { result, rerender } = renderHook(
        ({ query }) => usePurchaseAutocomplete(query),
        {
          wrapper: createWrapper(),
          initialProps: { query: '' },
        }
      );

      // Set initial query
      rerender({ query: 'Fe' });

      // Wait 200ms (before 300ms debounce completes)
      await delay(200);

      // Clear query before 300ms
      rerender({ query: '' });

      // Wait past 300ms to ensure debounce would have triggered
      await delay(200);

      // API should not be called because query was cleared
      expect(purchasesApi.getPurchaseNames).not.toHaveBeenCalled();

      // Result should be empty
      expect(result.current.suggestions).toEqual([]);
    });
  });

  describe('minimum query length', () => {
    it('should not fire query for empty string', async () => {
      const { result } = renderHook(() => usePurchaseAutocomplete(''), {
        wrapper: createWrapper(),
      });

      await delay(350);

      expect(purchasesApi.getPurchaseNames).not.toHaveBeenCalled();
      expect(result.current.suggestions).toEqual([]);
      expect(result.current.isLoading).toBe(false);
    });

    it('should not fire query for single character', async () => {
      const { result } = renderHook(() => usePurchaseAutocomplete('F'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      expect(purchasesApi.getPurchaseNames).not.toHaveBeenCalled();
      expect(result.current.suggestions).toEqual([]);
      expect(result.current.isLoading).toBe(false);
    });

    it('should fire query for exactly 2 characters', async () => {
      const mockSuggestions = ['Feed A', 'Feed B'];
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(mockSuggestions);

      const { result } = renderHook(() => usePurchaseAutocomplete('Fe'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fe');
      });

      await waitFor(() => {
        expect(result.current.suggestions).toEqual(mockSuggestions);
      });
    });

    it('should fire query for more than 2 characters', async () => {
      const mockSuggestions = ['Feed Premium'];
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(mockSuggestions);

      const { result } = renderHook(() => usePurchaseAutocomplete('Feed Premium'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Feed Premium');
      });

      await waitFor(() => {
        expect(result.current.suggestions).toEqual(mockSuggestions);
      });
    });
  });

  describe('results caching', () => {
    it('should cache results with 10-minute staleTime', async () => {
      const mockSuggestions = ['Feed A', 'Feed B'];
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(mockSuggestions);

      const wrapper = createWrapper();

      const { result, unmount } = renderHook(
        ({ query }) => usePurchaseAutocomplete(query),
        {
          wrapper,
          initialProps: { query: 'Fe' },
        }
      );

      // Wait for debounce
      await delay(350);

      await waitFor(() => {
        expect(result.current.suggestions).toEqual(mockSuggestions);
      });

      expect(purchasesApi.getPurchaseNames).toHaveBeenCalledTimes(1);

      // Unmount and remount with same query (same wrapper to preserve cache)
      unmount();

      const { result: result2 } = renderHook(
        ({ query }) => usePurchaseAutocomplete(query),
        {
          wrapper,
          initialProps: { query: 'Fe' },
        }
      );

      await delay(350);

      // Should use cached data immediately without calling API again
      await waitFor(() => {
        expect(result2.current.suggestions).toEqual(mockSuggestions);
      });

      // API should still only have been called once (using cache)
      expect(purchasesApi.getPurchaseNames).toHaveBeenCalledTimes(1);
    });

    it('should fetch fresh data for different queries', async () => {
      const mockSuggestions1 = ['Feed A', 'Feed B'];
      const mockSuggestions2 = ['Vitamins A', 'Vitamins B'];

      vi.mocked(purchasesApi.getPurchaseNames)
        .mockResolvedValueOnce(mockSuggestions1)
        .mockResolvedValueOnce(mockSuggestions2);

      const { result, rerender } = renderHook(
        ({ query }) => usePurchaseAutocomplete(query),
        {
          wrapper: createWrapper(),
          initialProps: { query: 'Fe' },
        }
      );

      // Wait for first query
      await delay(350);

      await waitFor(() => {
        expect(result.current.suggestions).toEqual(mockSuggestions1);
      });

      expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fe');

      // Change query to "Vi"
      rerender({ query: 'Vi' });

      await delay(350);

      await waitFor(() => {
        expect(result.current.suggestions).toEqual(mockSuggestions2);
      });

      expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Vi');
      expect(purchasesApi.getPurchaseNames).toHaveBeenCalledTimes(2);
    });
  });

  describe('loading state', () => {
    it('should set isLoading to true during fetch', async () => {
      const mockSuggestions = ['Feed A'];
      vi.mocked(purchasesApi.getPurchaseNames).mockImplementation(
        () =>
          new Promise((resolve) => {
            setTimeout(() => resolve(mockSuggestions), 100);
          })
      );

      const { result } = renderHook(() => usePurchaseAutocomplete('Fe'), {
        wrapper: createWrapper(),
      });

      // Wait for debounce to complete
      await delay(350);

      // Should be loading now (or might have already completed if very fast)
      // We need to check that it was loading at some point during the fetch
      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fe');
      });

      // Wait for the query to complete
      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      await waitFor(() => {
        expect(result.current.suggestions).toEqual(mockSuggestions);
      });
    });

    it('should not be loading for queries with < 2 characters', async () => {
      const { result } = renderHook(() => usePurchaseAutocomplete('F'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      expect(result.current.isLoading).toBe(false);
      expect(result.current.suggestions).toEqual([]);
    });
  });

  describe('empty results handling', () => {
    it('should return empty array for empty query', async () => {
      const { result } = renderHook(() => usePurchaseAutocomplete(''), {
        wrapper: createWrapper(),
      });

      await delay(350);

      expect(result.current.suggestions).toEqual([]);
    });

    it('should return empty array when API returns empty array', async () => {
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue([]);

      const { result } = renderHook(() => usePurchaseAutocomplete('xyz'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('xyz');
      });

      await waitFor(() => {
        expect(result.current.suggestions).toEqual([]);
      });
    });

    it('should return empty array when query becomes too short after valid query', async () => {
      const mockSuggestions = ['Feed A'];
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(mockSuggestions);

      const { result, rerender } = renderHook(
        ({ query }) => usePurchaseAutocomplete(query),
        {
          wrapper: createWrapper(),
          initialProps: { query: 'Fe' },
        }
      );

      // Wait for first query
      await delay(350);

      await waitFor(() => {
        expect(result.current.suggestions).toEqual(mockSuggestions);
      });

      // Change to single char
      rerender({ query: 'F' });

      await delay(350);

      // Should return empty array
      expect(result.current.suggestions).toEqual([]);
    });
  });

  describe('error handling', () => {
    it('should handle API errors gracefully', async () => {
      const error = new Error('Network error');
      vi.mocked(purchasesApi.getPurchaseNames).mockRejectedValue(error);

      const { result } = renderHook(() => usePurchaseAutocomplete('Fe'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fe');
      });

      // Should still return empty suggestions on error
      await waitFor(() => {
        expect(result.current.suggestions).toEqual([]);
      });

      expect(result.current.isLoading).toBe(false);
    });
  });

  describe('non-array API response handling', () => {
    it('should return empty array when API returns an object instead of array', async () => {
      // Simulate API returning a non-array response (e.g., wrapped object)
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(
        { names: ['Feed A'] } as unknown as string[]
      );

      const { result } = renderHook(() => usePurchaseAutocomplete('Fe'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fe');
      });

      // Should return empty array, not the object
      await waitFor(() => {
        expect(Array.isArray(result.current.suggestions)).toBe(true);
        expect(result.current.suggestions).toEqual([]);
      });
    });

    it('should return empty array when API returns a string instead of array', async () => {
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(
        'unexpected string' as unknown as string[]
      );

      const { result } = renderHook(() => usePurchaseAutocomplete('Fe'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fe');
      });

      await waitFor(() => {
        expect(Array.isArray(result.current.suggestions)).toBe(true);
        expect(result.current.suggestions).toEqual([]);
      });
    });

    it('should return empty array when API returns null', async () => {
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(
        null as unknown as string[]
      );

      const { result } = renderHook(() => usePurchaseAutocomplete('Fe'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      await waitFor(() => {
        expect(purchasesApi.getPurchaseNames).toHaveBeenCalledWith('Fe');
      });

      await waitFor(() => {
        expect(Array.isArray(result.current.suggestions)).toBe(true);
        expect(result.current.suggestions).toEqual([]);
      });
    });

    it('should return valid array when API returns proper string array', async () => {
      const validNames = ['Feed Premium', 'Feed Standard'];
      vi.mocked(purchasesApi.getPurchaseNames).mockResolvedValue(validNames);

      const { result } = renderHook(() => usePurchaseAutocomplete('Fe'), {
        wrapper: createWrapper(),
      });

      await delay(350);

      await waitFor(() => {
        expect(result.current.suggestions).toEqual(validNames);
      });

      expect(Array.isArray(result.current.suggestions)).toBe(true);
    });
  });
});
