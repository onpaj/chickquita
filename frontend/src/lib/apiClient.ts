import axios from 'axios';
import type { AxiosInstance, InternalAxiosRequestConfig, AxiosError } from 'axios';
import { processApiError } from './errors';
import { db } from './db';
import type { PendingRequest } from './db';

interface OfflineQueuedError extends Error {
  isOfflineQueued: boolean;
  requestId: number | undefined;
}

/**
 * API Client Configuration
 *
 * This file configures Axios with JWT token interceptors for authenticated API requests.
 * The Clerk JWT token is automatically attached to all requests and refreshed when needed.
 */

// Get API base URL from environment variables
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

if (!API_BASE_URL) {
  throw new Error('Missing VITE_API_BASE_URL environment variable');
}

/**
 * Axios instance with base configuration
 */
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000, // 30 seconds timeout
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Token getter function
 * This will be set by the React app using setTokenGetter()
 */
let getToken: (() => Promise<string | null>) | null = null;

/**
 * Set the token getter function
 * This should be called once when the app initializes with Clerk's getToken function
 *
 * @param tokenGetter - Function that returns a promise resolving to the JWT token
 */
export const setTokenGetter = (tokenGetter: () => Promise<string | null>) => {
  getToken = tokenGetter;
};

/**
 * Request interceptor to attach JWT token from Clerk
 *
 * This interceptor:
 * 1. Retrieves the current JWT token from Clerk via the token getter
 * 2. Adds it to the Authorization header
 * 3. Handles token refresh automatically (Clerk SDK manages this)
 */
apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    try {
      // Get the token using the configured token getter
      if (getToken) {
        const token = await getToken();

        if (token) {
          // Attach token to Authorization header
          config.headers.Authorization = `Bearer ${token}`;
        }
      }
    } catch (error) {
      // Log error but don't block the request
      // Some endpoints might not require authentication
      console.error('Error getting Clerk token:', error);
    }

    return config;
  },
  (error) => {
    // Handle request configuration errors
    return Promise.reject(error);
  }
);

/**
 * Response interceptor for error handling and offline queue management
 *
 * This interceptor:
 * 1. Handles common HTTP errors (401, 403, 500, etc.)
 * 2. Provides consistent error messages
 * 3. Logs errors for debugging
 * 4. Processes errors into a standardized format
 * 5. Queues failed POST/PUT/PATCH/DELETE requests when offline
 */
apiClient.interceptors.response.use(
  (response) => {
    // Pass through successful responses
    return response;
  },
  async (error: AxiosError) => {
    const config = error.config;

    // Check if this is a network error (offline) and should be queued
    const isNetworkError = !error.response && error.code === 'ERR_NETWORK';
    const isModifyingRequest = config && ['POST', 'PUT', 'PATCH', 'DELETE'].includes(config.method?.toUpperCase() || '');

    if (isNetworkError && isModifyingRequest && config) {
      try {
        // Extract record ID from URL patterns like /daily-records/{uuid}
        const url = config.url || '';
        const recordIdMatch = url.match(/\/daily-records\/([0-9a-f-]{36})/i);
        const recordId = recordIdMatch ? recordIdMatch[1] : undefined;

        // Queue the request for later sync
        const requestId = await db.queueRequest({
          method: config.method?.toUpperCase() as PendingRequest['method'],
          url,
          body: config.data,
          headers: config.headers as Record<string, string>,
          recordId
        });

        console.log(`📥 Queued offline request #${requestId}: ${config.method} ${config.url}`);

        // Update sync status
        const pendingCount = await db.pendingRequests.count();
        await db.updateSyncStatus({
          status: 'idle',
          pendingCount
        });

        // Return a special offline error that components can detect
        const offlineError = new Error('Request queued for offline sync') as OfflineQueuedError;
        offlineError.isOfflineQueued = true;
        offlineError.requestId = requestId;
        return Promise.reject(offlineError);
      } catch (queueError) {
        console.error('Failed to queue offline request:', queueError);
        // Fall through to normal error handling
      }
    }

    // Process the error into a standardized format
    const processedError = processApiError(error);

    // Log the processed error for debugging
    console.error('API Error:', {
      type: processedError.type,
      message: processedError.message,
      translationKey: processedError.translationKey,
      canRetry: processedError.canRetry,
      fieldErrors: processedError.fieldErrors,
    });

    // Reject with the original error (components will handle the error display)
    return Promise.reject(error);
  }
);

/**
 * Export the configured Axios instance
 */
export default apiClient;

/**
 * Export named instance for explicit imports
 */
export { apiClient };
