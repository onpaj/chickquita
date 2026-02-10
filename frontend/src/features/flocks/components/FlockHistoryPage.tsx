import { useParams, useNavigate } from 'react-router-dom';
import { Box, Container, IconButton, Typography } from '@mui/material';
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useFlockHistory } from '../hooks/useFlockHistory';
import { FlockHistoryTimeline } from './FlockHistoryTimeline';

/**
 * Full page view for flock history timeline.
 * Accessible via route: /coops/:coopId/flocks/:flockId/history
 */
export function FlockHistoryPage() {
  const { t } = useTranslation();
  const { coopId, flockId } = useParams<{ coopId: string; flockId: string }>();
  const navigate = useNavigate();

  const { data: history, isLoading, error } = useFlockHistory(flockId || '');

  const handleBack = () => {
    navigate(`/coops/${coopId}/flocks/${flockId}`);
  };

  return (
    <Container maxWidth="md" sx={{ py: 3 }}>
      {/* Header */}
      <Box display="flex" alignItems="center" mb={3}>
        <IconButton
          edge="start"
          onClick={handleBack}
          aria-label={t('common.back', 'Back')}
          sx={{ mr: 2 }}
        >
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h5" component="h1" fontWeight="bold">
          {t('flockHistory.title', 'Flock History')}
        </Typography>
      </Box>

      {/* Timeline */}
      <FlockHistoryTimeline history={history || []} loading={isLoading} error={error} />
    </Container>
  );
}
