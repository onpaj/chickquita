import { useState, useCallback } from 'react';
import {
  Box,
  Typography,
  Fab,
  Card,
  CardContent,
  Skeleton,
  Alert,
  Container,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useTranslation } from 'react-i18next';
import { useCoops } from '../features/coops/hooks/useCoops';
import { CreateCoopModal } from '../features/coops/components/CreateCoopModal';
import { CoopCard } from '../features/coops/components/CoopCard';

export default function CoopsPage() {
  const { t } = useTranslation();
  const { data: coops, isLoading, error, refetch } = useCoops();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Pull-to-refresh handler
  const handleRefresh = useCallback(async () => {
    setIsRefreshing(true);
    await refetch();
    setIsRefreshing(false);
  }, [refetch]);

  // Sort coops by created date (newest first)
  const sortedCoops = coops
    ? [...coops].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    : [];

  if (error) {
    return (
      <Container sx={{ mt: 2 }}>
        <Alert severity="error">{t('errors.generic')}</Alert>
      </Container>
    );
  }

  return (
    <Container
      sx={{ pb: 10 }}
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
      <Box sx={{ py: 3 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          {t('coops.title')}
        </Typography>

        {isLoading ? (
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            {[1, 2, 3].map((index) => (
              <Card key={index} elevation={2}>
                <CardContent>
                  <Skeleton variant="text" width="60%" height={32} />
                  <Skeleton variant="text" width="40%" sx={{ mt: 1 }} />
                  <Skeleton variant="text" width="30%" sx={{ mt: 1 }} />
                </CardContent>
              </Card>
            ))}
          </Box>
        ) : sortedCoops.length === 0 ? (
          <Box sx={{ textAlign: 'center', mt: 8 }}>
            <Typography variant="body1" color="text.secondary" gutterBottom>
              {t('coops.noCoops')}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {t('coops.addFirstCoop')}
            </Typography>
          </Box>
        ) : (
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            {sortedCoops.map((coop) => (
              <CoopCard key={coop.id} coop={coop} />
            ))}
          </Box>
        )}

        {isRefreshing && (
          <Box sx={{ textAlign: 'center', mt: 2 }}>
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
          bottom: 80,
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
