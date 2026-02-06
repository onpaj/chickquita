import { Container, Typography, Button, Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';

/**
 * 404 Not Found Page
 *
 * Displays when a user navigates to a non-existent route
 */
export default function NotFoundPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <Container
      maxWidth="sm"
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        textAlign: 'center',
        py: 4,
      }}
    >
      <Box sx={{ mb: 3 }}>
        <ErrorOutlineIcon sx={{ fontSize: 80, color: 'error.main' }} />
      </Box>

      <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
        {t('errors.notFoundTitle')}
      </Typography>

      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        {t('errors.notFoundDescription')}
      </Typography>

      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', justifyContent: 'center' }}>
        <Button
          variant="contained"
          onClick={() => navigate('/dashboard')}
          size="large"
        >
          {t('errors.backToDashboard')}
        </Button>

        <Button
          variant="outlined"
          onClick={() => navigate(-1)}
          size="large"
        >
          {t('common.back')}
        </Button>
      </Box>
    </Container>
  );
}
