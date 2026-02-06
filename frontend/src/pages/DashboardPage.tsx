import { useEffect, useState } from 'react';
import { useAuth, useUser } from '@clerk/clerk-react';
import {
  Container,
  Box,
  Typography,
  Paper,
  CircularProgress,
  Alert,
  Button,
} from '@mui/material';
import { useTranslation } from 'react-i18next';

interface UserData {
  id: string;
  clerkUserId: string;
  email: string;
  createdAt: string;
}

/**
 * Dashboard Page Component
 *
 * Main landing page after authentication.
 * Displays user information and makes a test API call to verify backend connectivity.
 */
export default function DashboardPage() {
  const { t } = useTranslation();
  const { getToken, signOut } = useAuth();
  const { user } = useUser();
  const [userData, setUserData] = useState<UserData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchUserData = async () => {
      try {
        setLoading(true);
        setError(null);

        const token = await getToken();
        console.log('Token obtained:', token ? 'Yes' : 'No');

        const response = await fetch('http://localhost:5000/api/users/me', {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        });

        console.log('Response status:', response.status);

        if (!response.ok) {
          const errorText = await response.text();
          console.error('API Error:', errorText);
          throw new Error(`API request failed: ${response.status} - ${errorText}`);
        }

        const data = await response.json();
        console.log('User data received:', data);
        setUserData(data);
      } catch (err) {
        console.error('Error fetching user data:', err);
        setError(err instanceof Error ? err.message : 'Unknown error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchUserData();
  }, [getToken]);

  const handleSignOut = async () => {
    await signOut();
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Typography variant="h3" component="h1" gutterBottom>
          {t('dashboard.title', 'Dashboard')}
        </Typography>

        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h5" gutterBottom>
            {t('dashboard.clerkInfo', 'Clerk Authentication Info')}
          </Typography>
          <Typography variant="body1" sx={{ mb: 1 }}>
            <strong>Email:</strong> {user?.primaryEmailAddress?.emailAddress}
          </Typography>
          <Typography variant="body1" sx={{ mb: 1 }}>
            <strong>Clerk User ID:</strong> {user?.id}
          </Typography>
          <Typography variant="body1" sx={{ mb: 2 }}>
            <strong>Username:</strong> {user?.username || 'Not set'}
          </Typography>
          <Button variant="outlined" onClick={handleSignOut}>
            {t('dashboard.signOut', 'Sign Out')}
          </Button>
        </Paper>

        <Paper sx={{ p: 3 }}>
          <Typography variant="h5" gutterBottom>
            {t('dashboard.backendInfo', 'Backend API Response')}
          </Typography>

          {loading && (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
              <CircularProgress />
            </Box>
          )}

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              <strong>Error:</strong> {error}
            </Alert>
          )}

          {userData && (
            <>
              <Alert severity="success" sx={{ mb: 2 }}>
                Successfully connected to backend!
              </Alert>
              <Typography variant="body1" sx={{ mb: 1 }}>
                <strong>User ID (DB):</strong> {userData.id}
              </Typography>
              <Typography variant="body1" sx={{ mb: 1 }}>
                <strong>Clerk User ID:</strong> {userData.clerkUserId}
              </Typography>
              <Typography variant="body1" sx={{ mb: 1 }}>
                <strong>Email:</strong> {userData.email}
              </Typography>
              <Typography variant="body1">
                <strong>Created At:</strong>{' '}
                {new Date(userData.createdAt).toLocaleString()}
              </Typography>
            </>
          )}
        </Paper>
      </Box>
    </Container>
  );
}
