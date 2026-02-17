import Dexie, { type Table } from 'dexie';

/**
 * Represents a queued API request for offline sync.
 * Stores requests that failed due to network unavailability.
 */
export interface PendingRequest {
  id?: number;
  method: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  url: string;
  body?: any;
  headers?: Record<string, string>;
  timestamp: string;
  retryCount: number;
  lastError?: string;
  recordId?: string;
}

/**
 * Tracks the sync status of the offline queue.
 */
export interface SyncStatus {
  key: string;
  lastSync: string;
  status: 'idle' | 'syncing' | 'success' | 'error';
  pendingCount: number;
  errorMessage?: string;
}

/**
 * IndexedDB database for offline-first functionality.
 *
 * Tables:
 * - pendingRequests: Queue of API requests to sync when online
 * - syncStatus: Metadata about sync state and progress
 */
class ChickquitaDB extends Dexie {
  pendingRequests!: Table<PendingRequest, number>;
  syncStatus!: Table<SyncStatus, string>;

  constructor() {
    super('ChickquitaDB');

    // Define database schema
    this.version(1).stores({
      pendingRequests: '++id, method, url, timestamp, retryCount',
      syncStatus: 'key, status, lastSync'
    });
    this.version(2).stores({
      pendingRequests: '++id, method, url, timestamp, retryCount, recordId',
      syncStatus: 'key, status, lastSync'
    });
  }

  /**
   * Adds a request to the offline queue.
   */
  async queueRequest(request: Omit<PendingRequest, 'id' | 'timestamp' | 'retryCount'>): Promise<number> {
    return await this.pendingRequests.add({
      ...request,
      timestamp: new Date().toISOString(),
      retryCount: 0
    });
  }

  /**
   * Gets all pending requests ordered by timestamp (oldest first).
   */
  async getPendingRequests(): Promise<PendingRequest[]> {
    return await this.pendingRequests
      .orderBy('timestamp')
      .toArray();
  }

  /**
   * Removes a successfully synced request from the queue.
   */
  async removeRequest(id: number): Promise<void> {
    await this.pendingRequests.delete(id);
  }

  /**
   * Updates retry count and error message for a failed request.
   */
  async updateRequestRetry(id: number, error: string): Promise<void> {
    const request = await this.pendingRequests.get(id);
    if (request) {
      await this.pendingRequests.update(id, {
        retryCount: request.retryCount + 1,
        lastError: error
      });
    }
  }

  /**
   * Gets the current sync status.
   */
  async getSyncStatus(): Promise<SyncStatus> {
    const status = await this.syncStatus.get('main');
    if (status) {
      return status;
    }

    // Initialize default status
    const defaultStatus: SyncStatus = {
      key: 'main',
      lastSync: new Date().toISOString(),
      status: 'idle',
      pendingCount: 0
    };
    await this.syncStatus.put(defaultStatus);
    return defaultStatus;
  }

  /**
   * Updates the sync status.
   */
  async updateSyncStatus(status: Partial<SyncStatus>): Promise<void> {
    const currentStatus = await this.getSyncStatus();
    await this.syncStatus.put({
      ...currentStatus,
      ...status
    });
  }

  /**
   * Clears all pending requests (useful for testing or manual reset).
   */
  async clearQueue(): Promise<void> {
    await this.pendingRequests.clear();
    await this.updateSyncStatus({
      status: 'idle',
      pendingCount: 0,
      errorMessage: undefined
    });
  }
}

// Export singleton instance
export const db = new ChickquitaDB();
