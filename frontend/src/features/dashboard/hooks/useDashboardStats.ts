import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../api/dashboardApi';
import type { DashboardStats } from '../api/dashboardApi';

/**
 * React Query hook for fetching dashboard statistics
 * Caches data for 5 minutes with stale time of 1 minute
 */
export function useDashboardStats() {
  return useQuery<DashboardStats, Error>({
    queryKey: ['dashboard', 'stats'],
    queryFn: dashboardApi.getStats,
    staleTime: 1000 * 60, // 1 minute
    gcTime: 1000 * 60 * 5, // 5 minutes (was cacheTime in older versions)
    retry: 2,
    refetchOnWindowFocus: true,
  });
}
