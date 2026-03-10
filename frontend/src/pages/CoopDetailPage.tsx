import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Box,
  Container,
  Typography,
  Paper,
  Stack,
  Button,
  Chip,
  Tooltip,
  Fab,
  Divider,
  ToggleButtonGroup,
  ToggleButton,
} from '@mui/material';
import {
  Edit as EditIcon,
  Archive as ArchiveIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
} from '@mui/icons-material';
import { useAppBar } from '../context/AppBarContext';
import { useCoopDetail } from '../features/coops/hooks/useCoopDetail';
import { useArchiveCoop, useDeleteCoop } from '../features/coops/hooks/useCoops';
import { useFlocks, useArchiveFlock } from '../features/flocks/hooks/useFlocks';
import { EditCoopModal } from '../features/coops/components/EditCoopModal';
import { ArchiveCoopDialog } from '../features/coops/components/ArchiveCoopDialog';
import { DeleteCoopDialog } from '../features/coops/components/DeleteCoopDialog';
import { CreateFlockModal } from '../features/flocks/components/CreateFlockModal';
import { EditFlockModal } from '../features/flocks/components/EditFlockModal';
import { ArchiveFlockDialog } from '../features/flocks/components/ArchiveFlockDialog';
import { FlockCard } from '../features/flocks/components/FlockCard';
import { FlocksEmptyState } from '../features/flocks/components/FlocksEmptyState';
import { FlockCardSkeleton } from '../shared/components/FlockCardSkeleton';
import { ResourceNotFound } from '../components/ResourceNotFound';
import { CoopDetailSkeleton } from '../shared/components';
import { processApiError, ErrorType } from '../lib/errors';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { useToast } from '../hooks/useToast';
import { formatDateTime } from '../lib/dateFormat';
import type { Flock } from '../features/flocks/api/flocksApi';

export function CoopDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: coop, isLoading, error } = useCoopDetail(id!);
  const { mutate: archiveCoop, isPending: isArchiving } = useArchiveCoop();
  const { mutate: deleteCoop, isPending: isDeleting } = useDeleteCoop();
  const { handleError } = useErrorHandler();
  const { showSuccess, showError } = useToast();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isArchiveDialogOpen, setIsArchiveDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  // Flocks state
  const [includeInactive, setIncludeInactive] = useState(false);
  const [isCreateFlockModalOpen, setIsCreateFlockModalOpen] = useState(false);
  const [isEditFlockModalOpen, setIsEditFlockModalOpen] = useState(false);
  const [isArchiveFlockDialogOpen, setIsArchiveFlockDialogOpen] = useState(false);
  const [selectedFlock, setSelectedFlock] = useState<Flock | null>(null);

  const { data: flocks, isLoading: isFlocksLoading } = useFlocks(id!, includeInactive);
  const { mutate: archiveFlock, isPending: isArchivingFlock } = useArchiveFlock();

  const { setAppBar, resetAppBar } = useAppBar();

  const handleBack = useCallback(() => {
    navigate('/coops');
  }, [navigate]);

  useEffect(() => {
    setAppBar({ title: coop?.name ?? t('coops.details'), onBack: handleBack });
    return () => resetAppBar();
  }, [coop?.name, handleBack, t, setAppBar, resetAppBar]);

  const handleEdit = () => {
    setIsEditModalOpen(true);
  };

  const handleCloseEditModal = () => {
    setIsEditModalOpen(false);
  };

  const handleArchive = () => {
    setIsArchiveDialogOpen(true);
  };

  const handleCloseArchiveDialog = () => {
    setIsArchiveDialogOpen(false);
  };

  const handleConfirmArchive = () => {
    if (!id) return;

    archiveCoop(id, {
      onSuccess: () => {
        showSuccess(t('coops.archiveSuccess'));
        setIsArchiveDialogOpen(false);
        navigate('/coops');
      },
      onError: (error: Error) => {
        setIsArchiveDialogOpen(false);
        handleError(error, handleConfirmArchive);
      },
    });
  };

  const handleDelete = () => {
    setIsDeleteDialogOpen(true);
  };

  const handleCloseDeleteDialog = () => {
    setIsDeleteDialogOpen(false);
  };

  const handleConfirmDelete = () => {
    if (!id) return;

    deleteCoop(id, {
      onSuccess: () => {
        showSuccess(t('coops.deleteSuccess'));
        setIsDeleteDialogOpen(false);
        navigate('/coops');
      },
      onError: (error: Error) => {
        setIsDeleteDialogOpen(false);
        const processedError = processApiError(error);

        // Check if the error is due to coop having flocks
        if (processedError.type === ErrorType.VALIDATION) {
          showError(t('coops.deleteErrorHasFlocks'));
        } else {
          handleError(error, handleConfirmDelete);
        }
      },
    });
  };

  // Flock handlers
  const handleFlockEdit = (flock: Flock) => {
    setSelectedFlock(flock);
    setIsEditFlockModalOpen(true);
  };

  const handleCloseEditFlockModal = () => {
    setIsEditFlockModalOpen(false);
    setSelectedFlock(null);
  };

  const handleFlockArchive = (flock: Flock) => {
    setSelectedFlock(flock);
    setIsArchiveFlockDialogOpen(true);
  };

  const handleCloseArchiveFlockDialog = () => {
    setIsArchiveFlockDialogOpen(false);
    setSelectedFlock(null);
  };

  const handleConfirmArchiveFlock = () => {
    if (!selectedFlock) return;

    archiveFlock(
      { coopId: selectedFlock.coopId, flockId: selectedFlock.id },
      {
        onSuccess: () => {
          showSuccess(t('flocks.archiveSuccess'));
          setIsArchiveFlockDialogOpen(false);
          setSelectedFlock(null);
        },
        onError: (error: Error) => {
          setIsArchiveFlockDialogOpen(false);
          setSelectedFlock(null);
          handleError(error, handleConfirmArchiveFlock);
        },
      }
    );
  };

  const sortedFlocks = flocks
    ? [...flocks].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    : [];

  const filteredFlocks = sortedFlocks.filter(flock =>
    includeInactive ? true : flock.isActive
  );

  if (isLoading) {
    return <CoopDetailSkeleton />;
  }

  if (error) {
    const processedError = processApiError(error);

    // Show ResourceNotFound component for 404 errors
    if (processedError.type === ErrorType.NOT_FOUND) {
      return (
        <ResourceNotFound
          resourceType={t('coops.title')}
          translationKey="coops.coopNotFound"
          backPath="/coops"
          backButtonTranslationKey="errors.backToList"
        />
      );
    }

    // For other errors, show a generic error with back button
    return (
      <Container maxWidth="sm" sx={{ py: 3 }}>
        <Typography variant="h6" color="error" gutterBottom>
          {t(processedError.translationKey)}
        </Typography>
        <Button variant="contained" onClick={handleBack} sx={{ mt: 2 }}>
          {t('errors.backToList')}
        </Button>
      </Container>
    );
  }

  if (!coop) {
    return null;
  }

  return (
    <Container maxWidth="sm" sx={{ py: 3 }}>
      {/* Coop Details Card */}
      <Paper elevation={2} sx={{ p: 3 }}>
        <Stack spacing={3}>
          {/* Coop Name */}
          <Box>
            <Typography variant="overline" color="text.secondary">
              {t('coops.coopName')}
            </Typography>
            <Typography variant="h5" component="h2">
              {coop.name}
            </Typography>
          </Box>

          {/* Location */}
          {coop.location && (
            <Box>
              <Typography variant="overline" color="text.secondary">
                {t('coops.location')}
              </Typography>
              <Typography variant="body1">
                {coop.location}
              </Typography>
            </Box>
          )}

          {/* Status */}
          <Box>
            <Typography variant="overline" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
              {t('coops.status')}
            </Typography>
            <Chip
              label={coop.isActive ? t('coops.active') : t('coops.archived')}
              color={coop.isActive ? 'success' : 'default'}
              size="medium"
              sx={{ fontWeight: 600, px: 1 }}
            />
          </Box>

          {/* Created Date */}
          <Box>
            <Typography variant="overline" color="text.secondary">
              {t('coops.createdAt', { date: '' }).replace(/\s*$/, '')}
            </Typography>
            <Typography variant="body2">
              {formatDateTime(coop.createdAt)}
            </Typography>
          </Box>

          {/* Updated Date */}
          <Box>
            <Typography variant="overline" color="text.secondary">
              {t('coops.updatedAt', { date: '' }).replace(/\s*$/, '')}
            </Typography>
            <Typography variant="body2">
              {formatDateTime(coop.updatedAt)}
            </Typography>
          </Box>

          {/* Action Buttons */}
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            spacing={2}
            sx={{ pt: 2 }}
          >
            <Button
              variant="outlined"
              startIcon={<EditIcon />}
              onClick={handleEdit}
              disabled={!coop.isActive}
              sx={{ width: { xs: '100%', md: 'auto' } }}
            >
              {t('common.edit')}
            </Button>
            <Button
              variant="outlined"
              startIcon={<ArchiveIcon />}
              onClick={handleArchive}
              disabled={!coop.isActive || isArchiving}
              sx={{ width: { xs: '100%', md: 'auto' } }}
            >
              {t('coops.archiveCoop')}
            </Button>
            <Tooltip
              title={coop.flocksCount > 0 ? t('coops.deleteDisabledHasFlocks', 'Remove all flocks first') : !coop.isActive ? t('coops.deleteDisabledArchived', 'Coop is archived') : ''}
              disableHoverListener={coop.isActive && !isDeleting && coop.flocksCount === 0}
            >
              <span>
                <Button
                  variant="text"
                  color="error"
                  startIcon={<DeleteIcon />}
                  onClick={handleDelete}
                  disabled={!coop.isActive || isDeleting || coop.flocksCount > 0}
                  sx={{ width: { xs: '100%', md: 'auto' } }}
                >
                  {t('common.delete')}
                </Button>
              </span>
            </Tooltip>
          </Stack>
        </Stack>
      </Paper>

      {/* Flocks Section */}
      <Box sx={{ mt: 4 }}>
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
          <Typography variant="h6" component="h2">
            {t('flocks.title')}
          </Typography>
          <ToggleButtonGroup
            value={includeInactive ? 'all' : 'active'}
            exclusive
            onChange={(_, newValue) => {
              if (newValue !== null) {
                setIncludeInactive(newValue === 'all');
              }
            }}
            size="small"
            color="primary"
            aria-label={t('flocks.filterStatus')}
          >
            <ToggleButton value="active" aria-label={t('flocks.active')}>
              {t('flocks.active')}
            </ToggleButton>
            <ToggleButton value="all" aria-label={t('common.all')}>
              {t('common.all')}
            </ToggleButton>
          </ToggleButtonGroup>
        </Stack>

        <Divider sx={{ mb: 2 }} />

        {isFlocksLoading ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {[1, 2].map((index) => (
              <FlockCardSkeleton key={index} />
            ))}
          </Box>
        ) : filteredFlocks.length === 0 ? (
          <FlocksEmptyState onAddClick={() => setIsCreateFlockModalOpen(true)} />
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {filteredFlocks.map((flock) => (
              <FlockCard
                key={flock.id}
                flock={flock}
                coopName={coop.name}
                onEdit={handleFlockEdit}
                onArchive={handleFlockArchive}
              />
            ))}
          </Box>
        )}
      </Box>

      {/* FAB for adding flock */}
      <Fab
        color="primary"
        aria-label={t('flocks.addFlock')}
        data-testid="add-flock-fab"
        sx={{
          position: 'fixed',
          bottom: { xs: 'calc(env(safe-area-inset-bottom) + 80px)', sm: 24 },
          right: 16,
        }}
        onClick={() => setIsCreateFlockModalOpen(true)}
      >
        <AddIcon />
      </Fab>

      {/* Coop Edit Modal */}
      {coop && (
        <EditCoopModal
          open={isEditModalOpen}
          onClose={handleCloseEditModal}
          coop={coop}
        />
      )}

      {/* Archive Coop Confirmation Dialog */}
      {coop && (
        <ArchiveCoopDialog
          open={isArchiveDialogOpen}
          onClose={handleCloseArchiveDialog}
          onConfirm={handleConfirmArchive}
          coopName={coop.name}
          isPending={isArchiving}
        />
      )}

      {/* Delete Coop Confirmation Dialog */}
      {coop && (
        <DeleteCoopDialog
          open={isDeleteDialogOpen}
          onClose={handleCloseDeleteDialog}
          onConfirm={handleConfirmDelete}
          coopName={coop.name}
          isPending={isDeleting}
        />
      )}

      {/* Create Flock Modal */}
      <CreateFlockModal
        open={isCreateFlockModalOpen}
        onClose={() => setIsCreateFlockModalOpen(false)}
        coopId={id!}
      />

      {/* Edit Flock Modal */}
      {selectedFlock && (
        <EditFlockModal
          open={isEditFlockModalOpen}
          onClose={handleCloseEditFlockModal}
          flock={selectedFlock}
        />
      )}

      {/* Archive Flock Dialog */}
      {selectedFlock && (
        <ArchiveFlockDialog
          open={isArchiveFlockDialogOpen}
          onClose={handleCloseArchiveFlockDialog}
          onConfirm={handleConfirmArchiveFlock}
          flockIdentifier={selectedFlock.identifier}
          isPending={isArchivingFlock}
        />
      )}
    </Container>
  );
}
