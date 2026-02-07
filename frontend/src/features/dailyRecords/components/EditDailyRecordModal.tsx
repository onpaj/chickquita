import { useState, useEffect, useRef } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  CircularProgress,
  TextField,
  Alert,
} from '@mui/material';
import { Delete as DeleteIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useUpdateDailyRecord } from '../hooks/useDailyRecords';
import { useErrorHandler } from '../../../hooks/useErrorHandler';
import { processApiError, ErrorType } from '../../../lib/errors';
import { NumericStepper } from '@/shared/components/NumericStepper';
import {
  DIALOG_CONFIG,
  isMobileViewport,
  dialogTitleSx,
  dialogContentSx,
  dialogActionsSx,
  touchButtonSx,
  touchInputProps,
  FORM_FIELD_SPACING,
} from '@/shared/constants/modalConfig';
import type { DailyRecordDto } from '../api/dailyRecordsApi';
import { DeleteDailyRecordDialog } from './DeleteDailyRecordDialog';

interface EditDailyRecordModalProps {
  open: boolean;
  onClose: () => void;
  record: DailyRecordDto | null;
  flockIdentifier?: string;
}

const MAX_NOTES_LENGTH = 500;

/**
 * Checks if a record can be edited (same-day restriction).
 * Only records created today can be edited.
 */
function canEditRecord(record: DailyRecordDto): boolean {
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const createdDate = new Date(record.createdAt);
  createdDate.setHours(0, 0, 0, 0);

  return createdDate.getTime() === today.getTime();
}

/**
 * EditDailyRecordModal Component
 *
 * Modal for editing existing daily records with same-day restriction.
 * Features:
 * - Pre-filled with existing record data
 * - Date and flock are not editable (immutable)
 * - Only allows editing records created today
 * - Updates eggCount and notes only
 * - Mobile responsive (fullScreen on mobile)
 *
 * @example
 * <EditDailyRecordModal
 *   open={isOpen}
 *   onClose={handleClose}
 *   record={selectedRecord}
 *   flockIdentifier="Hejno A"
 * />
 */
export function EditDailyRecordModal({
  open,
  onClose,
  record,
  flockIdentifier,
}: EditDailyRecordModalProps) {
  const { t } = useTranslation();
  const { mutate: updateDailyRecord, isPending } = useUpdateDailyRecord();
  const { handleError } = useErrorHandler();

  const [eggCount, setEggCount] = useState<number>(0);
  const [notes, setNotes] = useState<string>('');
  const [eggCountError, setEggCountError] = useState('');
  const [notesError, setNotesError] = useState('');
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  const eggCountRef = useRef<HTMLInputElement>(null);

  // Pre-fill form when record changes
  // eslint-disable-next-line react-hooks/set-state-in-effect
  useEffect(() => {
    if (record && open) {
      setEggCount(record.eggCount);
      setNotes(record.notes || '');
      setEggCountError('');
      setNotesError('');
    }
  }, [record, open]);

  // Auto-focus on egg count when modal opens
  useEffect(() => {
    if (open) {
      // Small delay to ensure modal is fully rendered
      const timer = setTimeout(() => {
        eggCountRef.current?.focus();
      }, 100);
      return () => clearTimeout(timer);
    }
  }, [open]);

  const handleClose = () => {
    setEggCount(0);
    setNotes('');
    setEggCountError('');
    setNotesError('');
    setIsDeleteDialogOpen(false);
    onClose();
  };

  const handleDeleteClick = () => {
    setIsDeleteDialogOpen(true);
  };

  const handleDeleteDialogClose = () => {
    setIsDeleteDialogOpen(false);
  };

  const validateEggCount = (value: number): string => {
    if (value < 0) {
      return t('validation.positiveNumber');
    }
    return '';
  };

  const validateNotes = (value: string): string => {
    if (value.length > MAX_NOTES_LENGTH) {
      return t('validation.maxLength', { count: MAX_NOTES_LENGTH });
    }
    return '';
  };

  const validate = (): boolean => {
    const eggErr = validateEggCount(eggCount);
    const notesErr = validateNotes(notes);

    setEggCountError(eggErr);
    setNotesError(notesErr);

    return !eggErr && !notesErr;
  };

  const isFormValid = (): boolean => {
    return eggCount >= 0 && notes.length <= MAX_NOTES_LENGTH;
  };

  const submitRecord = () => {
    if (!record || !validate()) {
      return;
    }

    updateDailyRecord(
      {
        id: record.id,
        eggCount,
        notes: notes.trim() || undefined,
      },
      {
        onSuccess: () => {
          handleClose();
        },
        onError: (error: Error) => {
          const processedError = processApiError(error);

          // Handle validation errors - show as field errors
          if (
            processedError.type === ErrorType.VALIDATION &&
            processedError.fieldErrors
          ) {
            processedError.fieldErrors.forEach((fieldError) => {
              const field = fieldError.field.toLowerCase();
              if (field === 'eggcount') {
                setEggCountError(fieldError.message);
              } else if (field === 'notes') {
                setNotesError(fieldError.message);
              }
            });
          }
          // For all other errors (network, server, etc.), show toast with retry
          else {
            handleError(error, submitRecord);
          }
        },
      }
    );
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    submitRecord();
  };

  if (!record) {
    return null;
  }

  const canEdit = canEditRecord(record);
  const formattedDate = new Date(record.recordDate).toLocaleDateString('cs-CZ');

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth={DIALOG_CONFIG.maxWidth}
      fullWidth={DIALOG_CONFIG.fullWidth}
      fullScreen={isMobileViewport()}
      sx={{
        '& .MuiDialog-paper': {
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '100vh',
        },
      }}
    >
      <form
        onSubmit={handleSubmit}
        style={{ display: 'flex', flexDirection: 'column', height: '100%' }}
      >
        <DialogTitle sx={dialogTitleSx}>
          {t('dailyRecords.edit.title')}
        </DialogTitle>
        <DialogContent
          sx={{
            ...dialogContentSx,
            overflowY: 'auto',
            flex: 1,
          }}
        >
          <Stack spacing={FORM_FIELD_SPACING}>
            {/* Same-day restriction warning */}
            {!canEdit && (
              <Alert severity="error">
                {t('dailyRecords.edit.sameDayRestriction')}
              </Alert>
            )}

            {/* Date (read-only) */}
            <TextField
              label={t('dailyRecords.date')}
              value={formattedDate}
              disabled
              fullWidth
              InputLabelProps={{ shrink: true }}
              helperText={t('dailyRecords.edit.dateNotEditable')}
            />

            {/* Flock (read-only) */}
            <TextField
              label={t('dailyRecords.flock')}
              value={flockIdentifier || t('dailyRecords.noFlocks')}
              disabled
              fullWidth
              helperText={t('dailyRecords.edit.flockNotEditable')}
            />

            {/* Egg Count (editable) */}
            <NumericStepper
              label={t('dailyRecords.eggCount')}
              value={eggCount}
              onChange={(value) => {
                setEggCount(value);
                if (eggCountError) {
                  setEggCountError(validateEggCount(value));
                }
              }}
              min={0}
              max={9999}
              disabled={isPending || !canEdit}
              error={!!eggCountError}
              helperText={eggCountError}
              aria-label="egg count"
            />

            {/* Notes (editable) */}
            <TextField
              label={t('dailyRecords.notes')}
              value={notes}
              onChange={(e) => {
                setNotes(e.target.value);
                if (notesError) {
                  setNotesError(validateNotes(e.target.value));
                }
              }}
              onBlur={() => {
                setNotesError(validateNotes(notes));
              }}
              error={!!notesError}
              helperText={
                notesError ||
                `${notes.length}/${MAX_NOTES_LENGTH} ${t('common.characters')}`
              }
              fullWidth
              disabled={isPending || !canEdit}
              multiline
              rows={2}
              inputProps={touchInputProps}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={dialogActionsSx}>
          <Button
            onClick={handleDeleteClick}
            disabled={isPending || !canEdit}
            color="error"
            startIcon={<DeleteIcon />}
            sx={{ ...touchButtonSx, mr: 'auto' }}
          >
            {t('common.delete')}
          </Button>
          <Button
            onClick={handleClose}
            disabled={isPending}
            sx={touchButtonSx}
          >
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isPending || !isFormValid() || !canEdit}
            startIcon={
              isPending ? <CircularProgress size={20} color="inherit" /> : undefined
            }
            sx={touchButtonSx}
          >
            {isPending ? t('common.saving') : t('common.save')}
          </Button>
        </DialogActions>
      </form>

      {/* Delete Confirmation Dialog */}
      <DeleteDailyRecordDialog
        open={isDeleteDialogOpen}
        onClose={handleDeleteDialogClose}
        record={record}
        flockIdentifier={flockIdentifier}
        onSuccess={handleClose}
      />
    </Dialog>
  );
}
