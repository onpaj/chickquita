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
import EggIcon from '@mui/icons-material/Egg';
import { useTranslation } from 'react-i18next';
import { Navigate, useNavigate } from 'react-router-dom';
import { useCoops, useEnsureDefaultCoop } from '../features/coops/hooks/useCoops';
import { CreateCoopModal } from '../features/coops/components/CreateCoopModal';
import { CoopCard } from '../features/coops/components/CoopCard';
import { CoopsEmptyState } from '../features/coops/components/CoopsEmptyState';
import { CoopCardSkeleton } from '../shared/components/CoopCardSkeleton';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { processApiError } from '../lib/errors';
import { useUserSettingsContext } from '../features/settings';

export default function CoopsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data: coops, isLoading, error, refetch } = useCoops();
  const { handleError } = useErrorHandler();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const { singleCoopMode } = useUserSettingsContext();
  const { mutate: ensureDefaultCoop, isPending: isEnsuringCoop } = useEnsureDefaultCoop();

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

  if (singleCoopMode && coops && coops.length > 0) {
    return <Navigate to={`/coops/${coops[0].id}/flocks`} replace />;
  }

  const handleAddFirstFlock = () => {
    ensureDefaultCoop(undefined, {
      onSuccess: (coop) => {
        navigate(`/coops/${coop.id}/flocks?addFlock=true`);
      },
      onError: (err) => {
        handleError(err, handleAddFirstFlock);
      },
    });
  };

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
          py: 3,
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
      sx={{ py: 3 }}
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
      <Box>
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
          singleCoopMode ? (
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                minHeight: '40vh',
                textAlign: 'center',
                gap: 2,
              }}
            >
              <Typography variant="h6" color="text.secondary">
                {t('flocks.noFlocks')}
              </Typography>
              <Button
                variant="contained"
                size="large"
                startIcon={isEnsuringCoop ? <CircularProgress size={18} color="inherit" /> : <EggIcon />}
                onClick={handleAddFirstFlock}
                disabled={isEnsuringCoop}
                sx={{ mt: 1 }}
              >
                {t('flocks.addFirstFlock')}
              </Button>
            </Box>
          ) : (
            <CoopsEmptyState onAddClick={() => setIsModalOpen(true)} />
          )
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
          bottom: { xs: 'calc(env(safe-area-inset-bottom) + 80px)', sm: 24 },
          right: 16,
          zIndex: 1000,
        }}
        onClick={() => setIsModalOpen(true)}
      >
        <AddIcon />
      </Fab>

      <CreateCoopModal open={isModalOpen} onClose={() => setIsModalOpen(false)} />
    </Container>
  );
}
