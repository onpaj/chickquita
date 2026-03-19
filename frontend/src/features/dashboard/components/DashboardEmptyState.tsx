import { Box, Typography, Button, Paper } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import AddIcon from '@mui/icons-material/Add';
import { EmptyDashboardIllustration } from '../../../assets/illustrations';
import { useUserSettingsContext } from '@/features/settings';
import { useCoops } from '@/features/coops/hooks/useCoops';

/**
 * Empty state component for dashboard when user has no data.
 * In single-coop mode the user already has a coop, so we direct them
 * to create their first flock instead.
 */
export function DashboardEmptyState() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { singleCoopMode } = useUserSettingsContext();
  const { data: coops } = useCoops();

  const handleCta = () => {
    if (singleCoopMode && coops && coops.length > 0) {
      navigate(`/coops/${coops[0].id}/flocks`);
    } else {
      navigate('/coops');
    }
  };

  const ctaLabel = singleCoopMode && coops && coops.length > 0
    ? t('dashboard.emptyState.createFirstFlock')
    : t('dashboard.emptyState.createFirstCoop');

  const ctaDesc = singleCoopMode && coops && coops.length > 0
    ? t('dashboard.emptyState.createFirstFlockDesc')
    : t('dashboard.emptyState.createFirstCoopDesc');

  return (
    <Paper
      elevation={0}
      sx={{
        p: 4,
        textAlign: 'center',
        backgroundColor: 'background.default',
        border: '2px dashed',
        borderColor: 'divider',
        borderRadius: 2,
      }}
    >
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: 3,
        }}
      >
        {/* Illustration */}
        <Box>
          <EmptyDashboardIllustration aria-label={t('dashboard.emptyState.title')} />
        </Box>

        {/* Text content */}
        <Box sx={{ maxWidth: 400 }}>
          <Typography variant="h5" gutterBottom fontWeight="bold">
            {t('dashboard.emptyState.title')}
          </Typography>
          <Typography variant="body1" color="text.secondary" paragraph>
            {t('dashboard.emptyState.message')}
          </Typography>
        </Box>

        {/* Call to action */}
        <Button
          variant="contained"
          size="large"
          onClick={handleCta}
          startIcon={<AddIcon />}
          sx={{
            borderRadius: 2,
            px: 4,
            py: 1.5,
          }}
        >
          {ctaLabel}
        </Button>

        <Typography variant="caption" color="text.secondary">
          {ctaDesc}
        </Typography>
      </Box>
    </Paper>
  );
}
