import axios from 'axios';
import type { AxiosInstance, InternalAxiosRequestConfig, AxiosError } from 'axios';

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
 * Response interceptor for error handling
 *
 * This interceptor:
 * 1. Handles common HTTP errors (401, 403, 500, etc.)
 * 2. Provides consistent error messages
 * 3. Logs errors for debugging
 */
apiClient.interceptors.response.use(
  (response) => {
    // Pass through successful responses
    return response;
  },
  async (error: AxiosError) => {
    // Handle different error scenarios
    if (error.response) {
      // Server responded with error status
      const status = error.response.status;

      switch (status) {
        case 401:
          // Unauthorized - token might be expired or invalid
          console.error('Unauthorized: Invalid or expired token');

          // Clerk automatically handles token refresh
          // If we get a 401, the token is truly invalid and user should be redirected
          // The Clerk React components will handle this automatically
          break;

        case 403:
          // Forbidden - user doesn't have permission
          console.error('Forbidden: Insufficient permissions');
          break;

        case 404:
          // Not found
          console.error('Not found:', error.config?.url);
          break;

        case 422:
          // Validation error
          console.error('Validation error:', error.response.data);
          break;

        case 500:
          // Server error
          console.error('Server error:', error.response.data);
          break;

        default:
          console.error('API error:', status, error.response.data);
      }
    } else if (error.request) {
      // Request was made but no response received
      console.error('Network error: No response received');
    } else {
      // Error in request configuration
      console.error('Request configuration error:', error.message);
    }

    // Reject with the original error
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
