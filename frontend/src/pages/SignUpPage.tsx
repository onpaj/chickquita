import { SignUp } from '@clerk/clerk-react';
import { Box, Container, CircularProgress, Typography } from '@mui/material';
import { useAuth } from '@clerk/clerk-react';

/**
 * Sign-Up Page Component
 *
 * Displays the Clerk-hosted sign-up UI for user registration.
 * Features:
 * - Branded container with theme primary color (#FF6B35)
 * - Mobile-first responsive layout
 * - Minimum 48x48px touch targets
 * - 8px base unit spacing
 * - Loading state with circular progress indicator
 */
export default function SignUpPage() {
  const { isLoaded } = useAuth();

  if (!isLoaded) {
    return (
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '100vh',
          gap: 2,
        }}
      >
        <CircularProgress size={48} sx={{ color: '#FF6B35' }} />
        <Typography variant="body2" color="text.secondary">
          Loading...
        </Typography>
      </Box>
    );
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        flexDirection: 'column',
        backgroundColor: '#fafafa',
      }}
    >
      {/* Branded Header */}
      <Box
        sx={{
          backgroundColor: '#FF6B35',
          py: 3,
          px: 2,
          textAlign: 'center',
          boxShadow: '0px 2px 4px rgba(0, 0, 0, 0.1)',
        }}
      >
        <Typography
          variant="h4"
          component="h1"
          sx={{
            color: '#ffffff',
            fontWeight: 600,
            fontSize: { xs: '1.75rem', sm: '2rem' },
          }}
        >
          Chickquita
        </Typography>
        <Typography
          variant="body2"
          sx={{
            color: 'rgba(255, 255, 255, 0.9)',
            mt: 1,
          }}
        >
          Chicken Farming Profitability Tracker
        </Typography>
      </Box>

      {/* Main Content */}
      <Container maxWidth="sm">
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            flex: 1,
            py: { xs: 3, sm: 4 },
            px: { xs: 2, sm: 3 },
          }}
        >
          <SignUp
            path="/sign-up"
            routing="path"
            signInUrl="/sign-in"
            redirectUrl="/dashboard"
            afterSignUpUrl="/dashboard"
          />
        </Box>
      </Container>
    </Box>
  );
}
