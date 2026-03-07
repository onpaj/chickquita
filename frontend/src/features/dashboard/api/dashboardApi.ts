import apiClient from '@/lib/apiClient';

/**
 * Dashboard statistics types matching the backend DashboardStatsDto.
 */
export interface DashboardStats {
  totalCoops: number;
  activeFlocks: number;
  totalHens: number;
  totalRoosters: number;
  totalChicks: number;
  totalAnimals: number;
  todayEggs: number;
  thisWeekEggs: number;
  avgEggsPerDay: number;
  costPerEgg: number | null;
}

/**
 * Get dashboard statistics from the backend /statistics/dashboard endpoint.
 */
export const dashboardApi = {
  getStats: async (): Promise<DashboardStats> => {
    const response = await apiClient.get<DashboardStats>('/statistics/dashboard');
    return response.data;
  },
};
