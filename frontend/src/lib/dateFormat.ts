import { format } from 'date-fns';

/**
 * Formats a date string as D. M. YYYY (e.g. "8. 3. 2026").
 * Used consistently across all display contexts in the app.
 */
export function formatDate(dateStr: string): string {
  return format(new Date(dateStr), 'd. M. yyyy');
}

/**
 * Formats a date-time string as D. M. YYYY, HH:mm (e.g. "8. 3. 2026, 14:30").
 */
export function formatDateTime(dateStr: string): string {
  return format(new Date(dateStr), 'd. M. yyyy, HH:mm');
}
