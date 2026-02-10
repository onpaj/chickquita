/**
 * Statistics API Client
 *
 * Handles API communication for statistics and analytics data.
 */

import apiClient from '@/lib/apiClient';
import { StatisticsDto } from '../types';

/**
 * Fetch statistics for a given date range
 */
export const statisticsApi = {
  getStatistics: async (startDate: string, endDate: string): Promise<StatisticsDto> => {
    const response = await apiClient.get<StatisticsDto>('/statistics', {
      params: { startDate, endDate },
    });
    return response.data;
  },
};
