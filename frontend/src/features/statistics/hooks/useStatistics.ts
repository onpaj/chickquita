/**
 * useStatistics Hook
 *
 * React Query hook for fetching statistics data.
 */

import { useQuery } from '@tanstack/react-query';
import { statisticsApi } from '../api/statisticsApi';
import type { StatisticsDto } from '../types';

/**
 * Fetch statistics for a given date range
 */
export function useStatistics(startDate: string, endDate: string) {
  return useQuery<StatisticsDto>({
    queryKey: ['statistics', startDate, endDate],
    queryFn: () => statisticsApi.getStatistics(startDate, endDate),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });
}
