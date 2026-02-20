import { useState, useCallback } from 'react';
import {
  Box,
  Typography,
  Fab,
  Container,
  Button,
  CircularProgress,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import RefreshIcon from '@mui/icons-material/Refresh';
import { useTranslation } from 'react-i18next';
import { useCoops } from '../features/coops/hooks/useCoops';
import { CreateCoopModal } from '../features/coops/components/CreateCoopModal';
import { CoopCard } from '../features/coops/components/CoopCard';
import { CoopsEmptyState } from '../features/coops/components/CoopsEmptyState';
import { CoopCardSkeleton } from '../shared/components/CoopCardSkeleton';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { processApiError } from '../lib/errors';

export default function CoopsPage() {
  const { t } = useTranslation();
  const { data: coops, isLoading, error, refetch } = useCoops();
  const { handleError } = useErrorHandler();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Pull-to-refresh handler
  const handleRefresh = useCallback(async () => {
    setIsRefreshing(true);
    try {
      await refetch();
    } catch (err) {
      handleError(err, handleRefresh);
    } finally {
      setIsRefreshing(false);
    }
  }, [refetch, handleError]);

  // Sort coops by created date (newest first)
  const sortedCoops = coops
    ? [...coops].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    : [];

  if (error) {
    const processedError = processApiError(error);

    return (
      <Container
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
        <Typography variant="h6" color="error" gutterBottom>
          {t(processedError.translationKey)}
        </Typography>

        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          {processedError.message}
        </Typography>

        {processedError.canRetry && (
          <Button
            variant="contained"
            startIcon={<RefreshIcon />}
            onClick={() => refetch()}
          >
            {t('common.retry')}
          </Button>
        )}
      </Container>
    );
  }

  return (
    <Container
      maxWidth="lg"
      onTouchStart={(e) => {
        const touch = e.touches[0];
        const startY = touch.clientY;
        const onTouchMove = (moveEvent: TouchEvent) => {
          const moveTouch = moveEvent.touches[0];
          const deltaY = moveTouch.clientY - startY;
          if (deltaY > 100 && window.scrollY === 0 && !isRefreshing) {
            handleRefresh();
            document.removeEventListener('touchmove', onTouchMove);
          }
        };
        document.addEventListener('touchmove', onTouchMove);
        document.addEventListener('touchend', () => {
          document.removeEventListener('touchmove', onTouchMove);
        }, { once: true });
      }}
    >
      <Box sx={{ py: 3, pb: 10 }}>
        <Typography variant="h4" component="h1" gutterBottom fontWeight="bold" sx={{ mb: 3 }}>
          {t('coops.title')}
        </Typography>

        {isLoading ? (
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            {[1, 2, 3].map((index) => (
              <CoopCardSkeleton key={index} />
            ))}
          </Box>
        ) : sortedCoops.length === 0 ? (
          <CoopsEmptyState onAddClick={() => setIsModalOpen(true)} />
        ) : (
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            {sortedCoops.map((coop) => (
              <CoopCard key={coop.id} coop={coop} />
            ))}
          </Box>
        )}

        {isRefreshing && (
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 1,
              mt: 2,
              py: 2,
            }}
          >
            <CircularProgress size={24} thickness={4} />
            <Typography variant="caption" color="text.secondary">
              {t('common.loading')}
            </Typography>
          </Box>
        )}
      </Box>

      <Fab
        color="primary"
        aria-label={t('coops.addCoop')}
        sx={{
          position: 'fixed',
          bottom: { xs: 80, sm: 16 },
          right: 16,
        }}
        onClick={() => setIsModalOpen(true)}
      >
        <AddIcon />
      </Fab>

      <CreateCoopModal open={isModalOpen} onClose={() => setIsModalOpen(false)} />
    </Container>
  );
}
