import { describe, it, expect } from 'vitest';
import { formatDate, formatDateTime } from '../dateFormat';

describe('formatDate', () => {
  it('formats a date string as D. M. YYYY', () => {
    expect(formatDate('2026-03-08')).toBe('8. 3. 2026');
  });

  it('does not zero-pad day or month', () => {
    expect(formatDate('2024-02-07')).toBe('7. 2. 2024');
  });

  it('handles end of year', () => {
    expect(formatDate('2024-12-31')).toBe('31. 12. 2024');
  });

  it('handles leap year date', () => {
    expect(formatDate('2024-02-29')).toBe('29. 2. 2024');
  });
});

describe('formatDateTime', () => {
  it('formats a datetime string as D. M. YYYY, HH:mm', () => {
    expect(formatDateTime('2026-03-08T14:30:00.000Z')).toMatch(/8\. 3\. 2026, \d{2}:\d{2}/);
  });

  it('includes time in 24-hour format', () => {
    // The exact time depends on the local timezone in the test environment,
    // so we just verify the date part and the time format
    const result = formatDateTime('2026-01-01T00:00:00.000Z');
    expect(result).toMatch(/^\d+\. \d+\. 2026, \d{2}:\d{2}$/);
  });

  it('does not zero-pad day or month', () => {
    const result = formatDateTime('2024-02-07T09:05:00.000Z');
    expect(result).toMatch(/^7\. 2\. 2024, /);
  });
});
