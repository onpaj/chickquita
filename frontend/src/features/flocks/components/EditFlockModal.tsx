import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Stack,
  CircularProgress,
  Typography,
  Box,
  Divider,
  IconButton,
  Slide,
} from '@mui/material';
import type { TransitionProps } from '@mui/material/transitions';
import CloseIcon from '@mui/icons-material/Close';
import { useTranslation } from 'react-i18next';
import { useUpdateFlock } from '../hooks/useFlocks';
import { useErrorHandler } from '../../../hooks/useErrorHandler';
import { processApiError, ErrorType } from '../../../lib/errors';
import type { Flock, UpdateFlockRequest } from '../api/flocksApi';
import { NumericStepper } from '@/shared/components';
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

const SlideUp = React.forwardRef(function SlideUp(
  props: TransitionProps & { children: React.ReactElement },
  ref: React.Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

interface EditFlockModalProps {
  open: boolean;
  onClose: () => void;
  flock: Flock;
}

export function EditFlockModal({ open, onClose, flock }: EditFlockModalProps) {
  const { t } = useTranslation();
  const { mutate: updateFlock, isPending } = useUpdateFlock();
  const { handleError } = useErrorHandler();

  const [identifier, setIdentifier] = useState('');
  const [hatchDate, setHatchDate] = useState('');
  const [hens, setHens] = useState(0);
  const [roosters, setRoosters] = useState(0);
  const [chicks, setChicks] = useState(0);

  const [identifierError, setIdentifierError] = useState('');
  const [hatchDateError, setHatchDateError] = useState('');

  // Initialize form values when modal opens or flock changes
  /* eslint-disable react-hooks/set-state-in-effect */
  useEffect(() => {
    if (open && flock) {
      setIdentifier(flock.identifier);
      setHatchDate(flock.hatchDate.split('T')[0]); // Extract date part from ISO string
      setHens(flock.currentHens);
      setRoosters(flock.currentRoosters);
      setChicks(flock.currentChicks);
      setIdentifierError('');
      setHatchDateError('');
    }
  }, [open, flock]);
  /* eslint-enable react-hooks/set-state-in-effect */

  const handleClose = () => {
    setIdentifierError('');
    setHatchDateError('');
    onClose();
  };

  const validateIdentifier = (value: string): string => {
    if (!value.trim()) {
      return t('validation.required');
    }
    if (value.length > 50) {
      return t('validation.maxLength', { count: 50 });
    }
    return '';
  };

  const validateHatchDate = (value: string): string => {
    if (!value) {
      return t('validation.required');
    }
    const selectedDate = new Date(value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (selectedDate > today) {
      return t('flocks.form.hatchDateFuture');
    }
    return '';
  };

  const validate = (): boolean => {
    const identifierErr = validateIdentifier(identifier);
    const hatchDateErr = validateHatchDate(hatchDate);

    setIdentifierError(identifierErr);
    setHatchDateError(hatchDateErr);

    return !identifierErr && !hatchDateErr;
  };

  const isFormValid = (): boolean => {
    return (
      identifier.trim().length > 0 &&
      identifier.length <= 50 &&
      hatchDate.length > 0
    );
  };

  const submitFlock = () => {
    if (!validate()) {
      return;
    }

    const request: UpdateFlockRequest = {
      flockId: flock.id,
      identifier: identifier.trim(),
      hatchDate,
      currentHens: hens,
      currentRoosters: roosters,
      currentChicks: chicks,
    };

    updateFlock(
      { coopId: flock.coopId, data: request },
      {
        onSuccess: () => {
          handleClose();
        },
        onError: (error: Error) => {
          const processedError = processApiError(error);

          // Handle validation errors - show as field errors
          if (processedError.type === ErrorType.VALIDATION && processedError.fieldErrors) {
            processedError.fieldErrors.forEach((fieldError) => {
              if (fieldError.field === 'identifier' || fieldError.field === 'Identifier') {
                setIdentifierError(fieldError.message);
              } else if (fieldError.field === 'hatchDate' || fieldError.field === 'HatchDate') {
                setHatchDateError(fieldError.message);
              }
            });
          }
          // For all other errors (network, server, etc.), show toast with retry
          else {
            handleError(error, submitFlock);
          }
        },
      }
    );
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    submitFlock();
  };

  const getTodayDate = (): string => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth={DIALOG_CONFIG.maxWidth}
      fullWidth={DIALOG_CONFIG.fullWidth}
      fullScreen={isMobileViewport()}
      TransitionComponent={SlideUp}
      sx={{
        '& .MuiDialog-paper': {
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '100vh',
        },
      }}
    >
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <DialogTitle sx={{ ...dialogTitleSx, pr: 6 }}>
          {t('flocks.editFlock')}
          <IconButton
            aria-label={t('common.close')}
            onClick={handleClose}
            disabled={isPending}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent
          dividers
          sx={{
            ...dialogContentSx,
            overflowY: 'auto',
            flex: 1,
          }}
        >
          <Stack spacing={FORM_FIELD_SPACING}>
            <TextField
              autoFocus
              label={t('flocks.form.identifier')}
              value={identifier}
              onChange={(e) => {
                setIdentifier(e.target.value);
                if (identifierError) {
                  const newError = validateIdentifier(e.target.value);
                  setIdentifierError(newError);
                }
              }}
              onBlur={() => {
                setIdentifierError(validateIdentifier(identifier));
              }}
              error={!!identifierError}
              helperText={identifierError}
              required
              fullWidth
              disabled={isPending}
              type="text"
              inputProps={touchInputProps}
            />

            <TextField
              label={t('flocks.form.hatchDate')}
              value={hatchDate}
              onChange={(e) => {
                setHatchDate(e.target.value);
                if (hatchDateError) {
                  const newError = validateHatchDate(e.target.value);
                  setHatchDateError(newError);
                }
              }}
              onBlur={() => {
                setHatchDateError(validateHatchDate(hatchDate));
              }}
              error={!!hatchDateError}
              helperText={hatchDateError}
              required
              fullWidth
              disabled={isPending}
              type="date"
              InputLabelProps={{
                shrink: true,
              }}
              inputProps={{
                max: getTodayDate(),
                ...touchInputProps,
              }}
            />

            <Divider />

            {/* Composition Section */}
            <Box>
              <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2 }}>
                {t('flocks.form.composition')}
              </Typography>

              <Stack spacing={2}>
                <NumericStepper
                  label={t('flocks.hens')}
                  value={hens}
                  onChange={setHens}
                  min={0}
                  disabled={isPending}
                  aria-label={t('flocks.hens')}
                />

                <NumericStepper
                  label={t('flocks.roosters')}
                  value={roosters}
                  onChange={setRoosters}
                  min={0}
                  disabled={isPending}
                  aria-label={t('flocks.roosters')}
                />

                <NumericStepper
                  label={t('flocks.chicks')}
                  value={chicks}
                  onChange={setChicks}
                  min={0}
                  disabled={isPending}
                  aria-label={t('flocks.chicks')}
                />
              </Stack>
            </Box>
          </Stack>
        </DialogContent>
        <DialogActions sx={dialogActionsSx}>
          <Button variant="text" onClick={handleClose} disabled={isPending} sx={touchButtonSx}>
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isPending || !isFormValid()}
            startIcon={isPending ? <CircularProgress size={20} color="inherit" /> : undefined}
            sx={touchButtonSx}
          >
            {isPending ? t('common.saving') : t('common.save')}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
