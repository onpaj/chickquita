import { useEffect } from 'react';
import { useAuth } from '@clerk/clerk-react';
import { setTokenGetter } from './apiClient';

/**
 * Hook to initialize the API client with Clerk authentication
 *
 * This hook:
 * 1. Gets the getToken function from Clerk's useAuth hook
 * 2. Sets it as the token getter for the API client
 * 3. Ensures all API requests include the current JWT token
 *
 * Usage:
 * Call this hook once at the app root level (in App.tsx or similar)
 *
 * @example
 * ```tsx
 * function App() {
 *   useApiClient();
 *
 *   return <div>...</div>;
 * }
 * ```
 */
export function useApiClient() {
  const { getToken } = useAuth();

  useEffect(() => {
    // Set up the token getter for the API client
    // Clerk's getToken automatically handles token refresh
    setTokenGetter(() => getToken());
  }, [getToken]);
}
