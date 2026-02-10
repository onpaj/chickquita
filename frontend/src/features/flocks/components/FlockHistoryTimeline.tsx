import { useState } from 'react';
import {
  Timeline,
  TimelineItem,
  TimelineSeparator,
  TimelineConnector,
  TimelineContent,
  TimelineDot,
  TimelineOppositeContent,
} from '@mui/lab';
import {
  Box,
  Typography,
  Paper,
  IconButton,
  TextField,
  Button,
  Stack,
  Chip,
  CircularProgress,
  Alert,
} from '@mui/material';
import {
  Edit as EditIcon,
  Check as CheckIcon,
  Close as CloseIcon,
  Add as AddIcon,
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  FiberManualRecord as FiberManualRecordIcon,
} from '@mui/icons-material';
import { format } from 'date-fns';
import { cs } from 'date-fns/locale';
import { useTranslation } from 'react-i18next';
import type { FlockHistory } from '../api/flocksApi';
import { useUpdateFlockHistoryNotes } from '../hooks/useFlockHistory';

interface FlockHistoryTimelineProps {
  history: FlockHistory[];
  loading?: boolean;
  error?: Error | null;
}

/**
 * Timeline component displaying flock composition change history.
 * Features:
 * - Vertical timeline with change type icons
 * - Delta displays with +/- color coding
 * - Expandable/inline editable notes section
 * - Optimistic updates via React Query
 */
export function FlockHistoryTimeline({ history, loading, error }: FlockHistoryTimelineProps) {
  const { t, i18n } = useTranslation();
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editNotes, setEditNotes] = useState('');

  const updateNotesMutation = useUpdateFlockHistoryNotes();

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mt: 2 }}>
        {t('flockHistory.loadError', 'Failed to load flock history')}
      </Alert>
    );
  }

  if (!history || history.length === 0) {
    return (
      <Paper sx={{ p: 3, textAlign: 'center' }}>
        <Typography variant="body2" color="text.secondary">
          {t('flockHistory.empty', 'No history entries yet')}
        </Typography>
      </Paper>
    );
  }

  const handleEditStart = (entry: FlockHistory) => {
    setEditingId(entry.id);
    setEditNotes(entry.notes || '');
  };

  const handleEditCancel = () => {
    setEditingId(null);
    setEditNotes('');
  };

  const handleEditSave = async (historyId: string) => {
    try {
      await updateNotesMutation.mutateAsync({
        historyId,
        notes: editNotes.trim() || null,
      });
      setEditingId(null);
      setEditNotes('');
    } catch (err) {
      console.error('Failed to update notes:', err);
    }
  };

  const getReasonIcon = (reason: string) => {
    const reasonLower = reason.toLowerCase();
    if (reasonLower === 'initial') {
      return <FiberManualRecordIcon />;
    } else if (reasonLower === 'maturation') {
      return <TrendingUpIcon />;
    } else if (reasonLower === 'adjustment') {
      return <AddIcon />;
    }
    return <FiberManualRecordIcon />;
  };

  const getReasonColor = (reason: string): 'primary' | 'success' | 'warning' | 'grey' => {
    const reasonLower = reason.toLowerCase();
    if (reasonLower === 'initial') return 'primary';
    if (reasonLower === 'maturation') return 'success';
    if (reasonLower === 'adjustment') return 'warning';
    return 'grey';
  };

  const calculateDeltas = (currentEntry: FlockHistory, previousEntry?: FlockHistory) => {
    if (!previousEntry) return null;

    return {
      hens: currentEntry.hens - previousEntry.hens,
      roosters: currentEntry.roosters - previousEntry.roosters,
      chicks: currentEntry.chicks - previousEntry.chicks,
    };
  };

  const renderDelta = (value: number, label: string) => {
    if (value === 0) return null;

    const color = value > 0 ? 'success.main' : 'error.main';
    const icon = value > 0 ? <TrendingUpIcon fontSize="small" /> : <TrendingDownIcon fontSize="small" />;

    return (
      <Chip
        size="small"
        icon={icon}
        label={`${value > 0 ? '+' : ''}${value} ${label}`}
        sx={{ bgcolor: value > 0 ? 'success.light' : 'error.light', color, mr: 0.5, mb: 0.5 }}
      />
    );
  };

  return (
    <Timeline position="right">
      {history.map((entry, index) => {
        const previousEntry = history[index + 1];
        const deltas = calculateDeltas(entry, previousEntry);
        const isEditing = editingId === entry.id;
        const locale = i18n.language === 'cs' ? cs : undefined;

        return (
          <TimelineItem key={entry.id}>
            <TimelineOppositeContent color="text.secondary" sx={{ flex: 0.3 }}>
              <Typography variant="body2" fontWeight="medium">
                {format(new Date(entry.changeDate), 'dd. MMM yyyy', { locale })}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {format(new Date(entry.createdAt), 'HH:mm', { locale })}
              </Typography>
            </TimelineOppositeContent>

            <TimelineSeparator>
              <TimelineDot color={getReasonColor(entry.reason)}>
                {getReasonIcon(entry.reason)}
              </TimelineDot>
              {index < history.length - 1 && <TimelineConnector />}
            </TimelineSeparator>

            <TimelineContent>
              <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
                {/* Header */}
                <Stack direction="row" justifyContent="space-between" alignItems="flex-start" mb={1}>
                  <Box>
                    <Typography variant="subtitle2" component="h3" fontWeight="bold">
                      {t(`flockHistory.reason.${entry.reason.toLowerCase()}`, entry.reason)}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {t('flockHistory.composition', 'Composition')}: {entry.hens}{' '}
                      {t('flockHistory.hens', 'hens')}, {entry.roosters}{' '}
                      {t('flockHistory.roosters', 'roosters')}, {entry.chicks}{' '}
                      {t('flockHistory.chicks', 'chicks')}
                    </Typography>
                  </Box>
                  {!isEditing && (
                    <IconButton
                      size="small"
                      onClick={() => handleEditStart(entry)}
                      aria-label={t('flockHistory.editNotes', 'Edit notes')}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                  )}
                </Stack>

                {/* Deltas */}
                {deltas && (
                  <Box mb={1}>
                    {renderDelta(deltas.hens, t('flockHistory.hens', 'hens'))}
                    {renderDelta(deltas.roosters, t('flockHistory.roosters', 'roosters'))}
                    {renderDelta(deltas.chicks, t('flockHistory.chicks', 'chicks'))}
                  </Box>
                )}

                {/* Notes Section */}
                {isEditing ? (
                  <Box mt={2}>
                    <TextField
                      fullWidth
                      multiline
                      rows={3}
                      label={t('flockHistory.notes', 'Notes')}
                      value={editNotes}
                      onChange={(e) => setEditNotes(e.target.value)}
                      disabled={updateNotesMutation.isPending}
                      sx={{ mb: 1 }}
                    />
                    <Stack direction="row" spacing={1}>
                      <Button
                        size="small"
                        variant="contained"
                        startIcon={<CheckIcon />}
                        onClick={() => handleEditSave(entry.id)}
                        disabled={updateNotesMutation.isPending}
                      >
                        {t('common.save', 'Save')}
                      </Button>
                      <Button
                        size="small"
                        variant="outlined"
                        startIcon={<CloseIcon />}
                        onClick={handleEditCancel}
                        disabled={updateNotesMutation.isPending}
                      >
                        {t('common.cancel', 'Cancel')}
                      </Button>
                    </Stack>
                  </Box>
                ) : (
                  entry.notes && (
                    <Box
                      mt={2}
                      p={1.5}
                      sx={{
                        bgcolor: 'action.hover',
                        borderRadius: 1,
                        borderLeft: '3px solid',
                        borderColor: 'primary.main',
                      }}
                    >
                      <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
                        {entry.notes}
                      </Typography>
                    </Box>
                  )
                )}
              </Paper>
            </TimelineContent>
          </TimelineItem>
        );
      })}
    </Timeline>
  );
}
