import { Box, Typography, Button, Paper } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import EggIcon from '@mui/icons-material/Egg';

/**
 * Empty state component for dashboard when user has no data
 * Encourages user to create their first coop
 */
export function DashboardEmptyState() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const handleCreateCoop = () => {
    navigate('/coops');
  };

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
        <Box
          sx={{
            width: 120,
            height: 120,
            borderRadius: '50%',
            backgroundColor: 'primary.light',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            opacity: 0.8,
          }}
        >
          <EggIcon sx={{ fontSize: 64, color: 'primary.main' }} />
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
          onClick={handleCreateCoop}
          startIcon={<EggIcon />}
          sx={{
            borderRadius: 2,
            px: 4,
            py: 1.5,
          }}
        >
          {t('dashboard.emptyState.createFirstCoop')}
        </Button>

        <Typography variant="caption" color="text.secondary">
          {t('dashboard.emptyState.createFirstCoopDesc')}
        </Typography>
      </Box>
    </Paper>
  );
}
