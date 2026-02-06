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
  Alert,
  IconButton,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Edit as EditIcon,
  Archive as ArchiveIcon,
} from '@mui/icons-material';
import { useCoopDetail } from '../features/coops/hooks/useCoopDetail';
import { EditCoopModal } from '../features/coops/components/EditCoopModal';
import { format } from 'date-fns';
import { cs, enUS } from 'date-fns/locale';

export function CoopDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const { data: coop, isLoading, error } = useCoopDetail(id!);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

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
    // TODO: Implement archive functionality in future US
    console.log('Archive coop', id);
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

  if (error || !coop) {
    return (
      <Container maxWidth="sm" sx={{ py: 3 }}>
        <Box sx={{ mb: 3 }}>
          <IconButton onClick={handleBack} edge="start">
            <ArrowBackIcon />
          </IconButton>
        </Box>
        <Alert severity="error">
          {t('coops.coopNotFound')}
        </Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="sm" sx={{ py: 3, pb: 10 }}>
      {/* Header with Back Button */}
      <Box sx={{ mb: 3 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={handleBack}
          sx={{ mb: 2 }}
        >
          {t('common.back')}
        </Button>
        <Typography variant="h4" component="h1" gutterBottom>
          {t('coops.details')}
        </Typography>
      </Box>

      {/* Coop Details Card */}
      <Paper sx={{ p: 3 }}>
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
              Status
            </Typography>
            <Chip
              label={t('coops.active')}
              color="success"
              size="small"
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
          <Stack direction="row" spacing={2} sx={{ pt: 2 }}>
            <Button
              variant="contained"
              startIcon={<EditIcon />}
              onClick={handleEdit}
            >
              {t('common.edit')}
            </Button>
            <Button
              variant="outlined"
              startIcon={<ArchiveIcon />}
              onClick={handleArchive}
            >
              {t('coops.archiveCoop')}
            </Button>
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
    </Container>
  );
}
