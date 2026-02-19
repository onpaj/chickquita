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
  Grid,
  IconButton,
  Divider,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Edit as EditIcon,
  Archive as ArchiveIcon,
  History as HistoryIcon,
  Pets as PetsIcon,
  Female as FemaleIcon,
  Male as MaleIcon,
  EggAlt as EggAltIcon,
  Diversity3 as Diversity3Icon,
} from '@mui/icons-material';
import { useFlockDetail } from '../features/flocks/hooks/useFlocks';
import { useArchiveFlock } from '../features/flocks/hooks/useFlocks';
import { EditFlockModal } from '../features/flocks/components/EditFlockModal';
import { ArchiveFlockDialog } from '../features/flocks/components/ArchiveFlockDialog';
import { MatureChicksModal } from '../features/flocks/components/MatureChicksModal';
import { ResourceNotFound } from '../components/ResourceNotFound';
import { processApiError, ErrorType } from '../lib/errors';
import { useErrorHandler } from '../hooks/useErrorHandler';
import { useToast } from '../hooks/useToast';
import { CoopDetailSkeleton } from '../shared/components';
import { StatCard } from '../shared/components';
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
  const [isMatureChicksModalOpen, setIsMatureChicksModalOpen] = useState(false);

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

  const handleViewHistory = () => {
    navigate(`/coops/${coopId}/flocks/${flockId}/history`);
  };

  if (isLoading) {
    return <CoopDetailSkeleton />;
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
            <Grid container spacing={2}>
              <Grid size={{ xs: 6, sm: 3 }}>
                <StatCard
                  icon={<FemaleIcon />}
                  label={t('flocks.hens')}
                  value={flock.currentHens}
                  color="error"
                />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <StatCard
                  icon={<MaleIcon />}
                  label={t('flocks.roosters')}
                  value={flock.currentRoosters}
                  color="info"
                />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <StatCard
                  icon={<EggAltIcon />}
                  label={t('flocks.chicks')}
                  value={flock.currentChicks}
                  color="warning"
                />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <StatCard
                  icon={<Diversity3Icon />}
                  label={t('flocks.total')}
                  value={totalAnimals}
                  color="primary"
                />
              </Grid>
            </Grid>
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
              variant="contained"
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
              onClick={handleViewHistory}
              sx={{ width: { xs: '100%', md: 'auto' } }}
            >
              {t('flocks.viewHistory')}
            </Button>
            <Button
              variant="outlined"
              startIcon={<PetsIcon />}
              onClick={() => setIsMatureChicksModalOpen(true)}
              disabled={!flock.isActive || flock.currentChicks === 0}
              sx={{ width: { xs: '100%', md: 'auto' } }}
            >
              {t('flocks.matureChicks.action')}
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

      {/* Mature Chicks Modal */}
      {flock && isMatureChicksModalOpen && (
        <MatureChicksModal
          open={isMatureChicksModalOpen}
          onClose={() => setIsMatureChicksModalOpen(false)}
          flock={flock}
        />
      )}
    </Container>
  );
}
