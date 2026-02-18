import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Stack,
  CircularProgress,
  IconButton,
  Typography,
  Slide,
  Box,
  Divider,
} from '@mui/material';
import type { TransitionProps } from '@mui/material/transitions';
import CloseIcon from '@mui/icons-material/Close';
import { useTranslation } from 'react-i18next';
import { useCreateFlock } from '../hooks/useFlocks';
import { useErrorHandler } from '../../../hooks/useErrorHandler';
import { processApiError, ErrorType } from '../../../lib/errors';
import type { CreateFlockRequest } from '../api/flocksApi';
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

interface CreateFlockModalProps {
  open: boolean;
  onClose: () => void;
  coopId: string;
}

export function CreateFlockModal({ open, onClose, coopId }: CreateFlockModalProps) {
  const { t } = useTranslation();
  const { mutate: createFlock, isPending } = useCreateFlock();
  const { handleError } = useErrorHandler();

  const [identifier, setIdentifier] = useState('');
  const [hatchDate, setHatchDate] = useState('');
  const [hens, setHens] = useState(0);
  const [roosters, setRoosters] = useState(0);
  const [chicks, setChicks] = useState(0);

  const [identifierError, setIdentifierError] = useState('');
  const [hatchDateError, setHatchDateError] = useState('');
  const [countsError, setCountsError] = useState('');

  const handleClose = () => {
    setIdentifier('');
    setHatchDate('');
    setHens(0);
    setRoosters(0);
    setChicks(0);
    setIdentifierError('');
    setHatchDateError('');
    setCountsError('');
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
    // Parse as local date to avoid UTC timezone offset causing false "future date" errors
    const [year, month, day] = value.split('-').map(Number);
    const selectedDate = new Date(year, month - 1, day);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (selectedDate > today) {
      return t('flocks.form.hatchDateFuture');
    }
    return '';
  };

  const validateCounts = (hensCount: number, roostersCount: number, chicksCount: number): string => {
    if (hensCount < 0 || roostersCount < 0 || chicksCount < 0) {
      return t('validation.positiveNumber');
    }
    if (hensCount === 0 && roostersCount === 0 && chicksCount === 0) {
      return t('flocks.form.atLeastOne');
    }
    return '';
  };

  const validate = (): boolean => {
    const identifierErr = validateIdentifier(identifier);
    const hatchDateErr = validateHatchDate(hatchDate);
    const countsErr = validateCounts(hens, roosters, chicks);

    setIdentifierError(identifierErr);
    setHatchDateError(hatchDateErr);
    setCountsError(countsErr);

    return !identifierErr && !hatchDateErr && !countsErr;
  };

  const isFormValid = (): boolean => {
    return (
      identifier.trim().length > 0 &&
      identifier.length <= 50 &&
      hatchDate.length > 0 &&
      hens >= 0 &&
      roosters >= 0 &&
      chicks >= 0 &&
      (hens > 0 || roosters > 0 || chicks > 0)
    );
  };

  const handleHensChange = (value: number) => {
    setHens(value);
    if (countsError) {
      setCountsError(validateCounts(value, roosters, chicks));
    }
  };

  const handleRoostersChange = (value: number) => {
    setRoosters(value);
    if (countsError) {
      setCountsError(validateCounts(hens, value, chicks));
    }
  };

  const handleChicksChange = (value: number) => {
    setChicks(value);
    if (countsError) {
      setCountsError(validateCounts(hens, roosters, value));
    }
  };

  const submitFlock = () => {
    if (!validate()) {
      return;
    }

    const request: CreateFlockRequest = {
      coopId,
      identifier: identifier.trim(),
      hatchDate,
      initialHens: hens,
      initialRoosters: roosters,
      initialChicks: chicks,
    };

    createFlock(request, {
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
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    submitFlock();
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !(e.target as HTMLElement).closest('button')) {
      e.preventDefault();
      submitFlock();
    }
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
      <form onSubmit={handleSubmit} onKeyDown={handleKeyDown} style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <DialogTitle sx={{ ...dialogTitleSx, pr: 6 }}>
          {t('flocks.addFlock')}
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

            <Box>
              <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2 }}>
                {t('flocks.form.composition')}
              </Typography>

              <Stack spacing={2}>
                <NumericStepper
                  label={t('flocks.hens')}
                  value={hens}
                  onChange={handleHensChange}
                  min={0}
                  disabled={isPending}
                  aria-label={t('flocks.hens')}
                />

                <NumericStepper
                  label={t('flocks.roosters')}
                  value={roosters}
                  onChange={handleRoostersChange}
                  min={0}
                  disabled={isPending}
                  aria-label={t('flocks.roosters')}
                />

                <NumericStepper
                  label={t('flocks.chicks')}
                  value={chicks}
                  onChange={handleChicksChange}
                  min={0}
                  disabled={isPending}
                  error={!!countsError}
                  helperText={countsError}
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
            type="button"
            variant="contained"
            disabled={isPending || !isFormValid()}
            onClick={submitFlock}
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
