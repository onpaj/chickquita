import { Container, Typography, Button, Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import SearchOffIcon from '@mui/icons-material/SearchOff';

/**
 * Resource Not Found Component
 *
 * Displays when a specific resource (like a coop) cannot be found
 */
interface ResourceNotFoundProps {
  resourceType: string;
  translationKey: string;
  backPath: string;
  backButtonTranslationKey: string;
}

export function ResourceNotFound({
  resourceType,
  translationKey,
  backPath,
  backButtonTranslationKey,
}: ResourceNotFoundProps) {
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
        minHeight: '60vh',
        textAlign: 'center',
        py: 4,
      }}
    >
      <Box sx={{ mb: 3 }}>
        <SearchOffIcon sx={{ fontSize: 64, color: 'warning.main' }} />
      </Box>

      <Typography variant="h5" component="h1" gutterBottom fontWeight="bold">
        {t(translationKey)}
      </Typography>

      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        {t('errors.resourceNotFoundDescription', { resource: resourceType })}
      </Typography>

      <Button
        variant="contained"
        onClick={() => navigate(backPath)}
        size="large"
      >
        {t(backButtonTranslationKey)}
      </Button>
    </Container>
  );
}
