import { SignUp } from '@clerk/clerk-react';
import { Box, Container } from '@mui/material';

/**
 * Sign-Up Page Component
 *
 * Displays the Clerk-hosted sign-up UI for user registration.
 * Configured to be mobile-responsive and match the app's design.
 */
export default function SignUpPage() {
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
        <SignUp
          path="/sign-up"
          routing="path"
          signInUrl="/sign-in"
          redirectUrl="/dashboard"
          afterSignUpUrl="/dashboard"
        />
      </Box>
    </Container>
  );
}
