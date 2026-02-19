import { db } from './db';
import { apiClient } from './apiClient';

/**
 * Maximum number of retry attempts before giving up.
 */
const MAX_RETRIES = 5;

/**
 * Maximum age of a request before it's considered stale (24 hours).
 */
const MAX_REQUEST_AGE_MS = 24 * 60 * 60 * 1000;

/**
 * Calculates exponential backoff delay in milliseconds.
 * Formula: 2^retryCount * 1000ms (1s, 2s, 4s, 8s, 16s, 32s)
 */
function getBackoffDelay(retryCount: number): number {
  return Math.pow(2, retryCount) * 1000;
}

/**
 * Checks if a request is too old and should be discarded.
 */
function isRequestStale(timestamp: string): boolean {
  const requestTime = new Date(timestamp).getTime();
  const now = Date.now();
  return (now - requestTime) > MAX_REQUEST_AGE_MS;
}

/**
 * Processes all pending requests in the offline queue.
 * Uses exponential backoff for retries and removes stale requests.
 *
 * @returns Object with success/failure counts and any errors encountered
 */
export async function syncPendingRequests(): Promise<{
  synced: number;
  failed: number;
  discarded: number;
  errors: string[];
}> {
  const result = {
    synced: 0,
    failed: 0,
    discarded: 0,
    errors: [] as string[]
  };

  try {
    // Update sync status to 'syncing'
    const pendingRequests = await db.getPendingRequests();
    await db.updateSyncStatus({
      status: 'syncing',
      pendingCount: pendingRequests.length
    });

    for (const request of pendingRequests) {
      // Discard stale requests (older than 24 hours)
      if (isRequestStale(request.timestamp)) {
        await db.removeRequest(request.id!);
        result.discarded++;
        console.warn(`Discarded stale request: ${request.method} ${request.url}`);
        continue;
      }

      // Skip if max retries exceeded
      if (request.retryCount >= MAX_RETRIES) {
        await db.removeRequest(request.id!);
        result.failed++;
        result.errors.push(`Max retries exceeded for ${request.method} ${request.url}`);
        console.error(`Giving up on request after ${MAX_RETRIES} retries: ${request.method} ${request.url}`);
        continue;
      }

      try {
        // Attempt to sync the request
        await apiClient({
          method: request.method,
          url: request.url,
          data: request.body,
          headers: request.headers
        });

        // Success: Remove from queue
        await db.removeRequest(request.id!);
        result.synced++;
        console.log(`✓ Synced: ${request.method} ${request.url}`);

      } catch (error: unknown) {
        // Failure: Update retry count
        const errorMessage = error instanceof Error ? error.message : 'Unknown error';
        await db.updateRequestRetry(request.id!, errorMessage);
        result.failed++;
        result.errors.push(errorMessage);

        console.error(`✗ Failed to sync (retry ${request.retryCount + 1}/${MAX_RETRIES}): ${request.method} ${request.url}`, error);

        // Schedule retry with exponential backoff
        const delay = getBackoffDelay(request.retryCount + 1);
        setTimeout(() => {
          syncPendingRequests(); // Retry this specific request later
        }, delay);
      }
    }

    // Update final sync status
    const remainingCount = await db.pendingRequests.count();
    await db.updateSyncStatus({
      status: remainingCount > 0 ? 'error' : 'success',
      pendingCount: remainingCount,
      lastSync: new Date().toISOString(),
      errorMessage: result.errors.length > 0 ? result.errors[0] : undefined
    });

  } catch (error: unknown) {
    // Critical error during sync process
    console.error('Critical error during sync:', error);
    const errorMessage = error instanceof Error ? error.message : 'Unknown sync error';
    await db.updateSyncStatus({
      status: 'error',
      errorMessage
    });
    result.errors.push(errorMessage);
  }

  return result;
}

/**
 * Starts automatic sync when the browser comes online.
 * Registers event listeners for online/offline events.
 */
export function startAutoSync(): void {
  // Sync immediately if online
  if (navigator.onLine) {
    syncPendingRequests();
  }

  // Listen for online event
  window.addEventListener('online', () => {
    console.log('🌐 Connection restored - starting sync...');
    syncPendingRequests();
  });

  // Listen for offline event
  window.addEventListener('offline', () => {
    console.log('📡 Connection lost - requests will be queued');
  });

  // Periodic sync every 5 minutes (only if online)
  setInterval(() => {
    if (navigator.onLine) {
      syncPendingRequests();
    }
  }, 5 * 60 * 1000);
}

/**
 * Manually triggers a sync operation.
 * Useful for user-initiated sync via a button.
 */
export async function manualSync(): Promise<boolean> {
  if (!navigator.onLine) {
    console.warn('Cannot sync while offline');
    return false;
  }

  const result = await syncPendingRequests();
  return result.failed === 0;
}
