import { describe, it, expect } from 'vitest';
import {
  determineGranularity,
  aggregateProductionData,
} from '../aggregateProductionData';
import type { ProductionTrendItem } from '../../types';

/**
 * Builds an array of daily ProductionTrendItem entries starting from startDate.
 */
function buildDailyData(startDate: string, days: number, eggsPerDay = 5): ProductionTrendItem[] {
  const items: ProductionTrendItem[] = [];
  for (let i = 0; i < days; i++) {
    const date = new Date(startDate);
    date.setDate(date.getDate() + i);
    items.push({ date: date.toISOString().split('T')[0], eggs: eggsPerDay });
  }
  return items;
}

describe('determineGranularity', () => {
  it('returns daily for empty data', () => {
    expect(determineGranularity([])).toBe('daily');
  });

  it('returns daily for single item', () => {
    expect(determineGranularity([{ date: '2024-01-01', eggs: 5 }])).toBe('daily');
  });

  it('returns daily for <= 30 day span', () => {
    const data = buildDailyData('2024-01-01', 30);
    expect(determineGranularity(data)).toBe('daily');
  });

  it('returns daily for exactly 30 days span', () => {
    const data = [
      { date: '2024-01-01', eggs: 5 },
      { date: '2024-01-31', eggs: 5 },
    ];
    expect(determineGranularity(data)).toBe('daily');
  });

  it('returns weekly for 31-112 day span', () => {
    const data = buildDailyData('2024-01-01', 60);
    expect(determineGranularity(data)).toBe('weekly');
  });

  it('returns weekly for exactly 112 day span', () => {
    const data = [
      { date: '2024-01-01', eggs: 5 },
      { date: '2024-04-22', eggs: 5 }, // 112 days later
    ];
    expect(determineGranularity(data)).toBe('weekly');
  });

  it('returns monthly for > 112 day span', () => {
    const data = buildDailyData('2024-01-01', 120);
    expect(determineGranularity(data)).toBe('monthly');
  });
});

describe('aggregateProductionData', () => {
  it('returns empty array for empty input', () => {
    expect(aggregateProductionData([])).toEqual([]);
  });

  it('returns daily items unchanged', () => {
    const data = buildDailyData('2024-01-01', 7);
    const result = aggregateProductionData(data);
    expect(result).toHaveLength(7);
    expect(result[0].granularity).toBe('daily');
    expect(result[0].eggs).toBe(5);
    expect(result[0].date).toBe('2024-01-01');
  });

  it('aggregates by week for 31+ day range', () => {
    // 60 daily records — should become weekly buckets
    const data = buildDailyData('2024-01-01', 60, 7);
    const result = aggregateProductionData(data);

    expect(result.length).toBeLessThan(60);
    expect(result.every((r) => r.granularity === 'weekly')).toBe(true);

    // Total eggs should be preserved
    const totalInput = data.reduce((sum, d) => sum + d.eggs, 0);
    const totalOutput = result.reduce((sum, r) => sum + r.eggs, 0);
    expect(totalOutput).toBe(totalInput);
  });

  it('aggregates by month for > 112 day range', () => {
    const data = buildDailyData('2024-01-01', 180, 3);
    const result = aggregateProductionData(data);

    expect(result.every((r) => r.granularity === 'monthly')).toBe(true);

    // Should have roughly 6 months
    expect(result.length).toBeGreaterThanOrEqual(5);
    expect(result.length).toBeLessThanOrEqual(7);

    // Total eggs should be preserved
    const totalInput = data.reduce((sum, d) => sum + d.eggs, 0);
    const totalOutput = result.reduce((sum, r) => sum + r.eggs, 0);
    expect(totalOutput).toBe(totalInput);
  });

  it('weekly buckets use start-of-week as date key', () => {
    // Two days in the same week should be in one bucket
    const data = [
      { date: '2024-01-08', eggs: 4 }, // Monday
      { date: '2024-01-09', eggs: 6 }, // Tuesday — same week
      { date: '2024-01-22', eggs: 3 }, // 14 days later — trigger weekly by having 32+ day span
      { date: '2024-02-10', eggs: 2 }, // >30 days from first
    ];
    const result = aggregateProductionData(data);
    expect(result.every((r) => r.granularity === 'weekly')).toBe(true);

    // The Monday+Tuesday bucket should sum to 10
    const firstBucket = result[0];
    expect(firstBucket.eggs).toBe(10);
  });

  it('monthly buckets sum eggs for the same month', () => {
    const data = buildDailyData('2024-01-01', 150, 2);
    const result = aggregateProductionData(data);

    // January has 31 days × 2 eggs = 62 eggs
    const janBucket = result.find((r) => r.date.startsWith('2024-01'));
    expect(janBucket?.eggs).toBe(62);
  });
});
