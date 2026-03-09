import { useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useFlockHistory } from '../hooks/useFlockHistory';
import { FlockHistoryTimeline } from './FlockHistoryTimeline';
import { useAppBar } from '../../../context/AppBarContext';

/**
 * Full page view for flock history timeline.
 * Accessible via route: /coops/:coopId/flocks/:flockId/history
 */
export function FlockHistoryPage() {
  const { t } = useTranslation();
  const { coopId, flockId } = useParams<{ coopId: string; flockId: string }>();
  const navigate = useNavigate();
  const { setAppBar, resetAppBar } = useAppBar();

  const { data: history, isLoading, error } = useFlockHistory(flockId || '');

  const handleBack = useCallback(() => {
    navigate(`/coops/${coopId}/flocks/${flockId}`);
  }, [navigate, coopId, flockId]);

  useEffect(() => {
    setAppBar({ title: t('flockHistory.title'), onBack: handleBack });
    return () => resetAppBar();
  }, [handleBack, t, setAppBar, resetAppBar]);

  return (
    <Container maxWidth="md" sx={{ py: 3 }}>
      <FlockHistoryTimeline history={history || []} loading={isLoading} error={error} />
    </Container>
  );
}
