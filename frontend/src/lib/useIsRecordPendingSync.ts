import { useState, useEffect } from 'react';
import { db } from './db';

/**
 * Hook that checks whether a given daily record ID has any pending offline sync requests.
 * Returns true if the record has pending PUT or DELETE requests queued for sync.
 *
 * @param recordId - The UUID of the daily record to check
 * @returns true if the record has pending sync requests, false otherwise
 */
export function useIsRecordPendingSync(recordId: string): boolean {
  const [isPending, setIsPending] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function checkPending() {
      try {
        const count = await db.pendingRequests
          .where('recordId')
          .equals(recordId)
          .count();
        if (!cancelled) {
          setIsPending(count > 0);
        }
      } catch {
        if (!cancelled) {
          setIsPending(false);
        }
      }
    }

    checkPending();

    return () => {
      cancelled = true;
    };
  }, [recordId]);

  return isPending;
}
