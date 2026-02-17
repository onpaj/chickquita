import { useState } from 'react';
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
  Skeleton,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Edit as EditIcon,
  Archive as ArchiveIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import { useCoopDetail } from '../features/coops/hooks/useCoopDetail';
import { useArchiveCoop, useDeleteCoop } from '../features/coops/hooks/useCoops';
import { EditCoopModal } from '../features/coops/components/EditCoopModal';
import { ArchiveCoopDialog } from '../features/coops/components/ArchiveCoopDialog';
import { DeleteCoopDialog } from '../features/coops/components/DeleteCoopDialog';
import { ResourceNotFound } from '../components/ResourceNotFound';
import { processApiError, ErrorType } from '../lib/errors';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { useToast } from '../hooks/useToast';
import { format } from 'date-fns';
import { cs, enUS } from 'date-fns/locale';

export function CoopDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const { data: coop, isLoading, error } = useCoopDetail(id!);
  const { mutate: archiveCoop, isPending: isArchiving } = useArchiveCoop();
  const { mutate: deleteCoop, isPending: isDeleting } = useDeleteCoop();
  const { handleError } = useErrorHandler();
  const { showSuccess, showError } = useToast();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isArchiveDialogOpen, setIsArchiveDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  const dateLocale = i18n.language === 'cs' ? cs : enUS;

  const handleBack = () => {
    navigate('/coops');
  };

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

  if (isLoading) {
    return (
      <Container maxWidth="sm" sx={{ py: 3 }}>
        <Box sx={{ mb: 3 }}>
          <Skeleton variant="rectangular" height={40} width={100} />
        </Box>
        <Paper sx={{ p: 3 }}>
          <Skeleton variant="text" height={40} width="60%" />
          <Skeleton variant="text" height={24} width="80%" sx={{ mt: 2 }} />
          <Skeleton variant="text" height={24} width="70%" sx={{ mt: 1 }} />
          <Skeleton variant="text" height={24} width="70%" sx={{ mt: 1 }} />
          <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
            <Skeleton variant="rectangular" height={36} width={100} />
            <Skeleton variant="rectangular" height={36} width={120} />
          </Box>
        </Paper>
      </Container>
    );
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
        <Box sx={{ mb: 3 }}>
          <IconButton onClick={handleBack} edge="start">
            <ArrowBackIcon />
          </IconButton>
        </Box>
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
    <Container maxWidth="sm" sx={{ py: 3, pb: 10 }}>
      {/* Header with Back Button */}
      <Box sx={{ mb: 3 }}>
        <IconButton
          onClick={handleBack}
          edge="start"
          aria-label={t('common.back')}
          sx={{
            mb: 2,
            minWidth: 48,
            minHeight: 48,
          }}
        >
          <ArrowBackIcon fontSize="large" />
        </IconButton>
        <Typography variant="h4" component="h1" gutterBottom>
          {t('coops.details')}
        </Typography>
      </Box>

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
              {format(new Date(coop.createdAt), 'PPP p', { locale: dateLocale })}
            </Typography>
          </Box>

          {/* Updated Date */}
          <Box>
            <Typography variant="overline" color="text.secondary">
              {t('coops.updatedAt', { date: '' }).replace(/\s*$/, '')}
            </Typography>
            <Typography variant="body2">
              {format(new Date(coop.updatedAt), 'PPP p', { locale: dateLocale })}
            </Typography>
          </Box>

          {/* Action Buttons */}
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            spacing={2}
            sx={{ pt: 2 }}
          >
            <Button
              variant="contained"
              onClick={() => navigate(`/coops/${coop.id}/flocks`)}
              sx={{ width: { xs: '100%', md: 'auto' }, minWidth: { md: 120 } }}
            >
              {t('flocks.title')}
            </Button>
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
              title={coop.flocksCount > 0 ? t('coops.deleteDisabledHasFlocks', 'Nejprve odstraňte všechna hejna') : !coop.isActive ? t('coops.deleteDisabledArchived', 'Kurník je archivován') : ''}
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

      {/* Edit Modal */}
      {coop && (
        <EditCoopModal
          open={isEditModalOpen}
          onClose={handleCloseEditModal}
          coop={coop}
        />
      )}

      {/* Archive Confirmation Dialog */}
      {coop && (
        <ArchiveCoopDialog
          open={isArchiveDialogOpen}
          onClose={handleCloseArchiveDialog}
          onConfirm={handleConfirmArchive}
          coopName={coop.name}
          isPending={isArchiving}
        />
      )}

      {/* Delete Confirmation Dialog */}
      {coop && (
        <DeleteCoopDialog
          open={isDeleteDialogOpen}
          onClose={handleCloseDeleteDialog}
          onConfirm={handleConfirmDelete}
          coopName={coop.name}
          isPending={isDeleting}
        />
      )}
    </Container>
  );
}
