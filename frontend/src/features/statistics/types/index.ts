/**
 * Statistics Feature Types
 *
 * Type definitions for statistics and analytics data.
 */

/**
 * Cost breakdown by purchase type
 */
export interface CostBreakdownItem {
  type: string;
  amount: number;
  percentage: number;
}

/**
 * Production trend data point
 */
export interface ProductionTrendItem {
  date: string;
  eggs: number;
}

/**
 * Cost per egg trend data point
 */
export interface CostPerEggTrendItem {
  date: string;
  costPerEgg: number;
}

/**
 * Flock productivity data
 */
export interface FlockProductivityItem {
  flockName: string;
  eggsPerHenPerDay: number;
  totalEggs: number;
  henCount: number;
}

/**
 * Statistics API response
 */
export interface StatisticsDto {
  costBreakdown: CostBreakdownItem[];
  productionTrend: ProductionTrendItem[];
  costPerEggTrend: CostPerEggTrendItem[];
  flockProductivity: FlockProductivityItem[];
  summary: {
    totalEggs: number;
    totalCost: number;
    avgCostPerEgg: number;
    avgEggsPerDay: number;
  };
}
