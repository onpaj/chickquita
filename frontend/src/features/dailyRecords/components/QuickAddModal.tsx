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
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useCreateDailyRecord } from '../hooks/useDailyRecords';
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

interface QuickAddModalProps {
  open: boolean;
  onClose: () => void;
  flocks: Array<{ id: string; identifier: string; coopName: string }>;
  defaultFlockId?: string;
}

const LAST_FLOCK_KEY = 'chickquita_lastUsedFlockId';
const MAX_NOTES_LENGTH = 500;

/**
 * QuickAddModal Component
 *
 * Mobile-optimized modal for quick daily record entry with < 30 seconds target completion time.
 * Features:
 * - Auto-focus on egg count field
 * - Remembers last used flock
 * - Mobile responsive (fullScreen on mobile)
 * - Default date set to today
 * - Optional notes field
 *
 * @example
 * <QuickAddModal
 *   open={isOpen}
 *   onClose={handleClose}
 *   flocks={availableFlocks}
 * />
 */
export function QuickAddModal({
  open,
  onClose,
  flocks,
  defaultFlockId,
}: QuickAddModalProps) {
  const { t } = useTranslation();
  const { mutate: createDailyRecord, isPending } = useCreateDailyRecord();
  const { handleError } = useErrorHandler();

  // Get last used flock from localStorage or use provided default
  const getInitialFlockId = (): string => {
    if (defaultFlockId) return defaultFlockId;
    const savedFlockId = localStorage.getItem(LAST_FLOCK_KEY);
    if (savedFlockId && flocks.some((f) => f.id === savedFlockId)) {
      return savedFlockId;
    }
    return flocks[0]?.id || '';
  };

  const [flockId, setFlockId] = useState<string>(getInitialFlockId());
  const [eggCount, setEggCount] = useState<number>(0);
  const [recordDate, setRecordDate] = useState<string>(
    new Date().toISOString().split('T')[0]
  );
  const [notesLength, setNotesLength] = useState<number>(0);
  const [flockIdError, setFlockIdError] = useState('');
  const [eggCountError, setEggCountError] = useState('');
  const [recordDateError, setRecordDateError] = useState('');
  const [notesError, setNotesError] = useState('');

  const eggCountRef = useRef<HTMLInputElement>(null);
  const notesRef = useRef<HTMLTextAreaElement>(null);

  // Auto-focus on egg count when modal opens
  useEffect(() => {
    if (open) {
      // Small delay to ensure modal is fully rendered
      const timer = setTimeout(() => {
        // Only auto-focus if the user hasn't already started typing in a textarea
        const activeEl = document.activeElement;
        const isTextareaFocused = activeEl?.tagName === 'TEXTAREA';
        if (!isTextareaFocused) {
          eggCountRef.current?.focus();
        }
      }, 100);
      return () => clearTimeout(timer);
    }
  }, [open]);

  // Update flockId when modal opens
  useEffect(() => {
    if (open) {
      const savedFlockId = localStorage.getItem(LAST_FLOCK_KEY);
      const initialFlockId = defaultFlockId ||
        (savedFlockId && flocks.some((f) => f.id === savedFlockId) ? savedFlockId : null) ||
        flocks[0]?.id ||
        '';
      if (initialFlockId) {
        setFlockId(initialFlockId);
      }
    }
  }, [open, flocks, defaultFlockId]);

  const handleClose = () => {
    setFlockId(getInitialFlockId());
    setEggCount(0);
    setRecordDate(new Date().toISOString().split('T')[0]);
    if (notesRef.current) {
      notesRef.current.value = '';
    }
    setNotesLength(0);
    setFlockIdError('');
    setEggCountError('');
    setRecordDateError('');
    setNotesError('');
    onClose();
  };

  const validateFlockId = (value: string): string => {
    if (!value) {
      return t('validation.required');
    }
    if (!flocks.some((f) => f.id === value)) {
      return t('errors.notFound');
    }
    return '';
  };

  const validateEggCount = (value: number): string => {
    if (value < 0) {
      return t('validation.positiveNumber');
    }
    return '';
  };

  const validateRecordDate = (value: string): string => {
    if (!value) {
      return t('validation.required');
    }
    const [year, month, day] = value.split('-').map(Number);
    const recordDateObj = new Date(year, month - 1, day);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (recordDateObj > today) {
      return t('dailyRecords.dateFutureError');
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
    const flockErr = validateFlockId(flockId);
    const eggErr = validateEggCount(eggCount);
    const dateErr = validateRecordDate(recordDate);
    const notesErr = validateNotes(notesRef.current?.value ?? '');

    setFlockIdError(flockErr);
    setEggCountError(eggErr);
    setRecordDateError(dateErr);
    setNotesError(notesErr);

    return !flockErr && !eggErr && !dateErr && !notesErr;
  };

  const isFormValid = (): boolean => {
    return (
      flockId.length > 0 &&
      flocks.some((f) => f.id === flockId) &&
      eggCount >= 0 &&
      recordDate.length > 0 &&
      notesLength <= MAX_NOTES_LENGTH
    );
  };

  const submitRecord = () => {
    if (!validate()) {
      return;
    }

    const notesValue = notesRef.current?.value ?? '';

    // Save last used flock to localStorage
    localStorage.setItem(LAST_FLOCK_KEY, flockId);

    createDailyRecord(
      {
        flockId,
        data: {
          recordDate,
          eggCount,
          notes: notesValue.trim() || undefined,
        },
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
              if (field === 'flockid') {
                setFlockIdError(fieldError.message);
              } else if (field === 'eggcount') {
                setEggCountError(fieldError.message);
              } else if (field === 'recorddate') {
                setRecordDateError(fieldError.message);
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
          {t('dailyRecords.quickAdd.title')}
        </DialogTitle>
        <DialogContent
          sx={{
            ...dialogContentSx,
            overflowY: 'auto',
            flex: 1,
          }}
        >
          <Stack spacing={FORM_FIELD_SPACING}>
            <TextField
              select
              label={t('dailyRecords.flock')}
              value={flockId}
              onChange={(e) => {
                setFlockId(e.target.value);
                if (flockIdError) {
                  setFlockIdError(validateFlockId(e.target.value));
                }
              }}
              onBlur={() => {
                setFlockIdError(validateFlockId(flockId));
              }}
              error={!!flockIdError}
              helperText={flockIdError}
              required
              fullWidth
              disabled={isPending || flocks.length === 0}
              inputProps={touchInputProps}
              SelectProps={{ native: true }}
            >
              {flocks.length === 0 ? (
                <option value="" disabled>
                  {t('dailyRecords.noFlocks')}
                </option>
              ) : (
                flocks.map((flock) => (
                  <option
                    key={flock.id}
                    value={flock.id}
                    onClick={() => setFlockId(flock.id)}
                  >
                    {flock.identifier} ({flock.coopName})
                  </option>
                ))
              )}
            </TextField>

            <TextField
              type="date"
              label={t('dailyRecords.date')}
              value={recordDate}
              onChange={(e) => {
                setRecordDate(e.target.value);
                if (recordDateError) {
                  setRecordDateError(validateRecordDate(e.target.value));
                }
              }}
              onBlur={() => {
                setRecordDateError(validateRecordDate(recordDate));
              }}
              error={!!recordDateError}
              helperText={recordDateError}
              required
              fullWidth
              disabled={isPending}
              InputLabelProps={{ shrink: true }}
              inputProps={{
                ...touchInputProps,
                max: new Date().toISOString().split('T')[0],
              }}
            />

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
              disabled={isPending}
              error={!!eggCountError}
              helperText={eggCountError}
              aria-label="egg count"
              inputRef={eggCountRef}
            />

            <TextField
              label={t('dailyRecords.notes')}
              defaultValue=""
              onChange={(e) => {
                setNotesLength(e.target.value.length);
                if (notesError) {
                  setNotesError(validateNotes(e.target.value));
                }
              }}
              onBlur={(e) => {
                const value = e.target.value;
                setNotesLength(value.length);
                setNotesError(validateNotes(value));
              }}
              error={!!notesError}
              helperText={
                notesError ||
                `${notesLength}/${MAX_NOTES_LENGTH} ${t('common.characters')}`
              }
              fullWidth
              disabled={isPending}
              multiline
              rows={2}
              inputProps={{ ...touchInputProps }}
              inputRef={notesRef}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={dialogActionsSx}>
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
            disabled={isPending || !isFormValid()}
            startIcon={
              isPending ? <CircularProgress size={20} color="inherit" /> : undefined
            }
            sx={touchButtonSx}
          >
            {isPending ? t('common.saving') : t('common.save')}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
