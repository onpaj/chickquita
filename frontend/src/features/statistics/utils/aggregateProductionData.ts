/**
 * Production data aggregation utilities
 *
 * Determines appropriate time granularity based on date range and
 * aggregates daily production data accordingly.
 *
 * Thresholds:
 *   <= 30 days  → daily (no aggregation)
 *   <= 112 days → weekly (16 weeks)
 *   >  112 days → monthly
 */

import dayjs from 'dayjs';
import type { ProductionTrendItem } from '../types';

export type Granularity = 'daily' | 'weekly' | 'monthly';

export interface AggregatedProductionItem {
  /** ISO date string representing the start of the period */
  date: string;
  eggs: number;
  granularity: Granularity;
}

/**
 * Determines aggregation granularity from the data's date span.
 */
export function determineGranularity(data: ProductionTrendItem[]): Granularity {
  if (data.length < 2) return 'daily';

  const first = dayjs(data[0].date);
  const last = dayjs(data[data.length - 1].date);
  const daySpan = last.diff(first, 'day');

  if (daySpan <= 30) return 'daily';
  if (daySpan <= 112) return 'weekly';
  return 'monthly';
}

/**
 * Aggregates daily production data into the appropriate granularity.
 * Returns daily items unchanged when granularity is 'daily'.
 */
export function aggregateProductionData(data: ProductionTrendItem[]): AggregatedProductionItem[] {
  if (!data || data.length === 0) return [];

  const granularity = determineGranularity(data);

  if (granularity === 'daily') {
    return data.map((item) => ({ ...item, granularity }));
  }

  const buckets = new Map<string, number>();
  const bucketOrder: string[] = [];

  for (const item of data) {
    const d = dayjs(item.date);
    const key =
      granularity === 'weekly'
        ? d.startOf('week').format('YYYY-MM-DD')
        : d.startOf('month').format('YYYY-MM-DD');

    if (!buckets.has(key)) {
      buckets.set(key, 0);
      bucketOrder.push(key);
    }
    buckets.set(key, (buckets.get(key) ?? 0) + item.eggs);
  }

  return bucketOrder.map((key) => ({
    date: key,
    eggs: buckets.get(key) ?? 0,
    granularity,
  }));
}
