import { QueryClient } from '@tanstack/react-query';

/**
 * React Query (TanStack Query) configuration for Chickquita PWA
 *
 * Configured for offline-first strategy:
 * - Network-first with cache fallback for GET requests
 * - 5 minute stale time for typical data freshness
 * - 10 minute cache time for offline availability
 * - No automatic refetch to save mobile data
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // 5 minutes before data is considered stale
      staleTime: 5 * 60 * 1000,

      // 10 minutes before inactive queries are garbage collected
      gcTime: 10 * 60 * 1000,

      // Retry failed requests (useful for flaky mobile networks)
      retry: 2,

      // Don't refetch on window focus (saves mobile data)
      refetchOnWindowFocus: false,

      // Don't refetch on reconnect (manual refresh is preferred)
      refetchOnReconnect: false,

      // Don't refetch when component mounts if data is still fresh
      refetchOnMount: false,
    },
    mutations: {
      // Retry mutations once (for network errors)
      retry: 1,
    },
  },
});
