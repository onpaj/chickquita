import apiClient from '@/lib/apiClient';

/**
 * Dashboard statistics types
 * Note: Currently aggregated from coops and flocks data on the frontend
 * TODO: Move aggregation to backend API endpoints when daily records and purchases are implemented
 */

export interface DashboardStats {
  flockStats: {
    totalHens: number;
    totalRoosters: number;
    totalChicks: number;
    activeFlocks: number;
  };
  // Future stats - when daily records and purchases are implemented
  productionStats?: {
    eggsToday: number;
    eggsThisWeek: number;
  };
  costStats?: {
    totalCosts: number;
    totalEggs: number;
    costPerEgg: number;
  };
}

export interface FlockSummary {
  id: string;
  coopId: string;
  identifier: string;
  currentHens: number;
  currentRoosters: number;
  currentChicks: number;
  isActive: boolean;
}

/**
 * Get dashboard statistics
 * Currently aggregates data from existing endpoints (coops and flocks)
 */
export const dashboardApi = {
  /**
   * Get aggregated dashboard statistics
   * Fetches all active flocks and calculates totals
   */
  getStats: async (): Promise<DashboardStats> => {
    try {
      // Fetch all flocks (only active ones)
      const flocksResponse = await apiClient.get<FlockSummary[]>('/flocks', {
        params: { includeInactive: false },
      });

      const flocks = flocksResponse.data;

      // Aggregate flock statistics
      const flockStats = flocks.reduce(
        (acc, flock) => ({
          totalHens: acc.totalHens + flock.currentHens,
          totalRoosters: acc.totalRoosters + flock.currentRoosters,
          totalChicks: acc.totalChicks + flock.currentChicks,
          activeFlocks: acc.activeFlocks + 1,
        }),
        {
          totalHens: 0,
          totalRoosters: 0,
          totalChicks: 0,
          activeFlocks: 0,
        }
      );

      return {
        flockStats,
        // Production and cost stats will be undefined until those features are implemented
      };
    } catch (error) {
      console.error('Failed to fetch dashboard stats:', error);
      throw error;
    }
  },
};
