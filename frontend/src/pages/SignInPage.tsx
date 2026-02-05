import { SignIn } from '@clerk/clerk-react';
import { Box, Container } from '@mui/material';

/**
 * Sign-In Page Component
 *
 * Displays the Clerk-hosted sign-in UI for user authentication.
 * Configured to be mobile-responsive and match the app's design.
 */
export default function SignInPage() {
  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '100vh',
          py: 3,
        }}
      >
        <SignIn
          path="/sign-in"
          routing="path"
          signUpUrl="/sign-up"
          redirectUrl="/dashboard"
          afterSignInUrl="/dashboard"
        />
      </Box>
    </Container>
  );
}
