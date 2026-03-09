/**
 * Statistics API Client
 *
 * Handles API communication for statistics and analytics data.
 */

import apiClient from '@/lib/apiClient';
import type { StatisticsDto } from '../types';

/**
 * Fetch statistics for a given date range
 */
export const statisticsApi = {
  getStatistics: async (startDate: string, endDate: string, coopId?: string, flockId?: string): Promise<StatisticsDto> => {
    const response = await apiClient.get<StatisticsDto>('/statistics', {
      params: { startDate, endDate, ...(coopId && { coopId }), ...(flockId && { flockId }) },
    });
    return response.data;
  },
};
