import { useAuth } from '@clerk/clerk-react';
import { Navigate, Outlet } from 'react-router-dom';

/**
 * ProtectedRoute Component
 *
 * A wrapper component that protects routes from unauthenticated access.
 * Redirects unauthenticated users to the sign-in page.
 *
 * @example
 * ```tsx
 * <Route element={<ProtectedRoute />}>
 *   <Route path="/dashboard" element={<Dashboard />} />
 * </Route>
 * ```
 */
export function ProtectedRoute() {
  const { isSignedIn, isLoaded } = useAuth();

  // Wait for Clerk to load before making routing decisions
  if (!isLoaded) {
    return null; // Or a loading spinner if preferred
  }

  // Redirect to sign-in if not authenticated
  if (!isSignedIn) {
    return <Navigate to="/sign-in" replace />;
  }

  // Render child routes if authenticated
  return <Outlet />;
}

export default ProtectedRoute;
