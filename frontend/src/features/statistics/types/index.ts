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
 * Monthly revenue vs. costs data point
 */
export interface RevenueTrendItem {
  month: string; // YYYY-MM
  revenue: number;
  costs: number;
}

/**
 * Statistics API response
 */
export interface StatisticsDto {
  costBreakdown: CostBreakdownItem[];
  productionTrend: ProductionTrendItem[];
  costPerEggTrend: CostPerEggTrendItem[];
  flockProductivity: FlockProductivityItem[];
  revenueTrend: RevenueTrendItem[];
  summary: {
    totalEggs: number;
    totalCost: number;
    avgCostPerEgg: number;
    avgEggsPerDay: number;
    totalRevenue: number | null;
    profitLoss: number | null;
  };
}
