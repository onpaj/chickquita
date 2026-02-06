import { useState, useCallback } from 'react';
import { useParams } from 'react-router-dom';
import {
  Box,
  Typography,
  Fab,
  Card,
  CardContent,
  Skeleton,
  Container,
  Button,
  ToggleButtonGroup,
  ToggleButton,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import RefreshIcon from '@mui/icons-material/Refresh';
import { useTranslation } from 'react-i18next';
import { useFlocks, useArchiveFlock } from '../features/flocks/hooks/useFlocks';
import { useCoopDetail } from '../features/coops/hooks/useCoops';
import { CreateFlockModal } from '../features/flocks/components/CreateFlockModal';
import { EditFlockModal } from '../features/flocks/components/EditFlockModal';
import { ArchiveFlockDialog } from '../features/flocks/components/ArchiveFlockDialog';
import { FlockCard } from '../features/flocks/components/FlockCard';
import { FlocksEmptyState } from '../features/flocks/components/FlocksEmptyState';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { useToast } from '../hooks/useToast';
import { processApiError } from '../lib/errors';
import type { Flock } from '../features/flocks/api/flocksApi';

export default function FlocksPage() {
  const { t } = useTranslation();
  const { coopId } = useParams<{ coopId: string }>();
  const [includeInactive, setIncludeInactive] = useState(false);
  const { data: flocks, isLoading, error, refetch } = useFlocks(coopId!, includeInactive);
  const { data: coop } = useCoopDetail(coopId!);
  const { mutate: archiveFlock, isPending: isArchiving } = useArchiveFlock();
  const { handleError } = useErrorHandler();
  const { showSuccess } = useToast();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isArchiveDialogOpen, setIsArchiveDialogOpen] = useState(false);
  const [selectedFlock, setSelectedFlock] = useState<Flock | null>(null);
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

  // Edit flock handler
  const handleEdit = (flock: Flock) => {
    setSelectedFlock(flock);
    setIsEditModalOpen(true);
  };

  const handleCloseEditModal = () => {
    setIsEditModalOpen(false);
    setSelectedFlock(null);
  };

  // Archive flock handlers
  const handleArchive = (flock: Flock) => {
    setSelectedFlock(flock);
    setIsArchiveDialogOpen(true);
  };

  const handleCloseArchiveDialog = () => {
    setIsArchiveDialogOpen(false);
    setSelectedFlock(null);
  };

  const handleConfirmArchive = () => {
    if (!selectedFlock) return;

    archiveFlock(
      { coopId: selectedFlock.coopId, flockId: selectedFlock.id },
      {
        onSuccess: () => {
          showSuccess(t('flocks.archiveSuccess'));
          setIsArchiveDialogOpen(false);
          setSelectedFlock(null);
        },
        onError: (error: Error) => {
          setIsArchiveDialogOpen(false);
          setSelectedFlock(null);
          handleError(error, handleConfirmArchive);
        },
      }
    );
  };

  // Sort flocks by created date (newest first)
  const sortedFlocks = flocks
    ? [...flocks].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    : [];

  // Filter by active/inactive status
  const filteredFlocks = sortedFlocks.filter(flock =>
    includeInactive ? true : flock.isActive
  );

  if (!coopId) {
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
        <Typography variant="h6" color="error">
          {t('common.error')}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Missing coop ID
        </Typography>
      </Container>
    );
  }

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
          {t('flocks.title')}
        </Typography>

        {/* Filter toggle */}
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <ToggleButtonGroup
            value={includeInactive ? 'all' : 'active'}
            exclusive
            onChange={(_, newValue) => {
              if (newValue !== null) {
                setIncludeInactive(newValue === 'all');
              }
            }}
            size="small"
            aria-label={t('flocks.filterStatus')}
          >
            <ToggleButton value="active" aria-label={t('flocks.active')}>
              {t('flocks.active')}
            </ToggleButton>
            <ToggleButton value="all" aria-label={t('common.all')}>
              {t('common.all')}
            </ToggleButton>
          </ToggleButtonGroup>
        </Box>

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
        ) : filteredFlocks.length === 0 ? (
          <FlocksEmptyState onAddClick={() => setIsCreateModalOpen(true)} />
        ) : (
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            {filteredFlocks.map((flock) => (
              <FlockCard
                key={flock.id}
                flock={flock}
                coopName={coop?.name || ''}
                onEdit={handleEdit}
                onArchive={handleArchive}
              />
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
        aria-label={t('flocks.addFlock')}
        sx={{
          position: 'fixed',
          bottom: 80,
          right: 16,
        }}
        onClick={() => setIsCreateModalOpen(true)}
      >
        <AddIcon />
      </Fab>

      <CreateFlockModal
        open={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        coopId={coopId}
      />

      {selectedFlock && (
        <EditFlockModal
          open={isEditModalOpen}
          onClose={handleCloseEditModal}
          flock={selectedFlock}
        />
      )}

      {selectedFlock && (
        <ArchiveFlockDialog
          open={isArchiveDialogOpen}
          onClose={handleCloseArchiveDialog}
          onConfirm={handleConfirmArchive}
          flockIdentifier={selectedFlock.identifier}
          isPending={isArchiving}
        />
      )}
    </Container>
  );
}
