import { useAuth } from '@clerk/clerk-react';
import { Navigate } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';

/**
 * Protected Route Component
 *
 * Wraps routes that require authentication.
 * Redirects to sign-in if user is not authenticated.
 * Shows loading spinner while checking authentication status.
 */
interface ProtectedRouteProps {
  children: React.ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isLoaded, isSignedIn } = useAuth();

  // Still loading authentication state
  if (!isLoaded) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '100vh',
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  // Not authenticated - redirect to sign in
  if (!isSignedIn) {
    return <Navigate to="/sign-in" replace />;
  }

  // Authenticated - render children
  return <>{children}</>;
}
