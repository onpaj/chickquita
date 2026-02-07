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
  Divider,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Edit as EditIcon,
  Archive as ArchiveIcon,
  History as HistoryIcon,
} from '@mui/icons-material';
import { useFlockDetail } from '../features/flocks/hooks/useFlocks';
import { useArchiveFlock } from '../features/flocks/hooks/useFlocks';
import { EditFlockModal } from '../features/flocks/components/EditFlockModal';
import { ArchiveFlockDialog } from '../features/flocks/components/ArchiveFlockDialog';
import { ResourceNotFound } from '../components/ResourceNotFound';
import { processApiError, ErrorType } from '../lib/errors';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { useToast } from '../hooks/useToast';
import { format } from 'date-fns';
import { cs, enUS } from 'date-fns/locale';

export function FlockDetailPage() {
  const { coopId, flockId } = useParams<{ coopId: string; flockId: string }>();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const { data: flock, isLoading, error } = useFlockDetail(coopId!, flockId!);
  const { mutate: archiveFlock, isPending: isArchiving } = useArchiveFlock();
  const { handleError } = useErrorHandler();
  const { showSuccess } = useToast();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isArchiveDialogOpen, setIsArchiveDialogOpen] = useState(false);

  const dateLocale = i18n.language === 'cs' ? cs : enUS;

  const handleBack = () => {
    navigate(`/coops/${coopId}/flocks`);
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
    if (!coopId || !flockId) return;

    archiveFlock(
      { coopId, flockId },
      {
        onSuccess: () => {
          showSuccess(t('flocks.archiveSuccess'));
          setIsArchiveDialogOpen(false);
          navigate(`/coops/${coopId}/flocks`);
        },
        onError: (error: Error) => {
          setIsArchiveDialogOpen(false);
          handleError(error, handleConfirmArchive);
        },
      }
    );
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

    if (processedError.type === ErrorType.NOT_FOUND) {
      return (
        <ResourceNotFound
          resourceType={t('flocks.title')}
          translationKey="flocks.flockNotFound"
          backPath={`/coops/${coopId}/flocks`}
          backButtonTranslationKey="errors.backToList"
        />
      );
    }

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

  if (!flock) {
    return null;
  }

  const totalAnimals = flock.currentHens + flock.currentRoosters + flock.currentChicks;

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
          {t('flocks.details')}
        </Typography>
      </Box>

      {/* Flock Details Card */}
      <Paper elevation={2} sx={{ p: 3 }}>
        <Stack spacing={3}>
          {/* Flock Identifier */}
          <Box>
            <Typography variant="overline" color="text.secondary">
              {t('flocks.identifier')}
            </Typography>
            <Typography variant="h5" component="h2">
              {flock.identifier}
            </Typography>
          </Box>

          {/* Status */}
          <Box>
            <Typography variant="overline" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
              {t('flocks.status')}
            </Typography>
            <Chip
              label={flock.isActive ? t('flocks.active') : t('flocks.archived')}
              color={flock.isActive ? 'success' : 'default'}
              size="medium"
              sx={{ fontWeight: 600, px: 1 }}
            />
          </Box>

          {/* Hatch Date */}
          {flock.hatchDate && (
            <Box>
              <Typography variant="overline" color="text.secondary">
                {t('flocks.hatchDate')}
              </Typography>
              <Typography variant="body1">
                {format(new Date(flock.hatchDate), 'PPP', { locale: dateLocale })}
              </Typography>
            </Box>
          )}

          <Divider />

          {/* Current Composition */}
          <Box>
            <Typography variant="overline" color="text.secondary" sx={{ display: 'block', mb: 2 }}>
              {t('flocks.currentComposition')}
            </Typography>
            <Stack spacing={1.5}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="body1" color="text.secondary">
                  {t('flocks.hens')}:
                </Typography>
                <Typography variant="body1" fontWeight="medium">
                  {flock.currentHens}
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="body1" color="text.secondary">
                  {t('flocks.roosters')}:
                </Typography>
                <Typography variant="body1" fontWeight="medium">
                  {flock.currentRoosters}
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="body1" color="text.secondary">
                  {t('flocks.chicks')}:
                </Typography>
                <Typography variant="body1" fontWeight="medium">
                  {flock.currentChicks}
                </Typography>
              </Box>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  pt: 1,
                  borderTop: 1,
                  borderColor: 'divider',
                }}
              >
                <Typography variant="body1" fontWeight="medium">
                  {t('flocks.total')}:
                </Typography>
                <Typography variant="body1" fontWeight="bold">
                  {totalAnimals}
                </Typography>
              </Box>
            </Stack>
          </Box>

          <Divider />

          {/* Created Date */}
          {flock.createdAt && (
            <Box>
              <Typography variant="overline" color="text.secondary">
                {t('flocks.createdAt', { date: '' }).replace(/\s*$/, '')}
              </Typography>
              <Typography variant="body2">
                {format(new Date(flock.createdAt), 'PPP p', { locale: dateLocale })}
              </Typography>
            </Box>
          )}

          {/* Updated Date */}
          {flock.updatedAt && (
            <Box>
              <Typography variant="overline" color="text.secondary">
                {t('flocks.updatedAt', { date: '' }).replace(/\s*$/, '')}
              </Typography>
              <Typography variant="body2">
                {format(new Date(flock.updatedAt), 'PPP p', { locale: dateLocale })}
              </Typography>
            </Box>
          )}

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
              disabled={!flock.isActive}
              sx={{ width: { xs: '100%', md: 'auto' } }}
            >
              {t('common.edit')}
            </Button>
            <Button
              variant="outlined"
              startIcon={<ArchiveIcon />}
              onClick={handleArchive}
              disabled={!flock.isActive || isArchiving}
              sx={{ width: { xs: '100%', md: 'auto' } }}
            >
              {t('flocks.archiveFlock')}
            </Button>
            <Button
              variant="outlined"
              startIcon={<HistoryIcon />}
              disabled
              sx={{ width: { xs: '100%', md: 'auto' } }}
            >
              {t('flocks.viewHistory')}
            </Button>
          </Stack>
        </Stack>
      </Paper>

      {/* Edit Modal */}
      {flock && (
        <EditFlockModal
          open={isEditModalOpen}
          onClose={handleCloseEditModal}
          flock={flock}
        />
      )}

      {/* Archive Confirmation Dialog */}
      {flock && (
        <ArchiveFlockDialog
          open={isArchiveDialogOpen}
          onClose={handleCloseArchiveDialog}
          onConfirm={handleConfirmArchive}
          flockIdentifier={flock.identifier}
          isPending={isArchiving}
        />
      )}
    </Container>
  );
}
