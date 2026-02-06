import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
  CircularProgress,
  IconButton,
  InputAdornment,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import RemoveIcon from '@mui/icons-material/Remove';
import { useTranslation } from 'react-i18next';
import { useUpdateFlock } from '../hooks/useFlocks';
import { useErrorHandler } from '../../../hooks/useErrorHandler';
import { processApiError, ErrorType } from '../../../lib/errors';
import type { Flock, UpdateFlockRequest } from '../api/flocksApi';

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
  const [countsError, setCountsError] = useState('');

  // Initialize form values when modal opens or flock changes
  useEffect(() => {
    if (open && flock) {
      setIdentifier(flock.identifier);
      setHatchDate(flock.hatchDate.split('T')[0]); // Extract date part from ISO string
      setHens(flock.currentHens);
      setRoosters(flock.currentRoosters);
      setChicks(flock.currentChicks);
      setIdentifierError('');
      setHatchDateError('');
      setCountsError('');
    }
  }, [open, flock]);

  const handleClose = () => {
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
    const selectedDate = new Date(value);
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

  const handleNumberChange = (
    setter: (value: number) => void,
    value: number
  ) => {
    const newValue = Math.max(0, value);
    setter(newValue);
    // Clear counts error when user changes values
    if (countsError) {
      const err = validateCounts(
        setter === setHens ? newValue : hens,
        setter === setRoosters ? newValue : roosters,
        setter === setChicks ? newValue : chicks
      );
      setCountsError(err);
    }
  };

  const incrementValue = (setter: (value: number) => void, currentValue: number) => {
    handleNumberChange(setter, currentValue + 1);
  };

  const decrementValue = (setter: (value: number) => void, currentValue: number) => {
    handleNumberChange(setter, currentValue - 1);
  };

  const submitFlock = () => {
    if (!validate()) {
      return;
    }

    const request: UpdateFlockRequest = {
      id: flock.id,
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
      maxWidth="sm"
      fullWidth
      fullScreen={window.innerWidth < 480}
      sx={{
        '& .MuiDialog-paper': {
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '100vh',
        },
      }}
    >
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <DialogTitle>{t('flocks.editFlock')}</DialogTitle>
        <DialogContent
          sx={{
            overflowY: 'auto',
            flex: 1,
          }}
        >
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
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
              inputProps={{
                style: { minHeight: '44px' },
              }}
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
                style: { minHeight: '44px' },
              }}
            />

            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
              <Typography variant="subtitle2" color="text.secondary">
                {t('flocks.form.composition')}
              </Typography>

              <TextField
                label={t('flocks.hens')}
                value={hens}
                onChange={(e) => {
                  const value = parseInt(e.target.value) || 0;
                  handleNumberChange(setHens, value);
                }}
                fullWidth
                disabled={isPending}
                type="number"
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => decrementValue(setHens, hens)}
                        disabled={isPending || hens <= 0}
                        size="small"
                        aria-label={t('flocks.form.decrease')}
                        sx={{ minWidth: '44px', minHeight: '44px' }}
                      >
                        <RemoveIcon />
                      </IconButton>
                      <IconButton
                        onClick={() => incrementValue(setHens, hens)}
                        disabled={isPending}
                        size="small"
                        aria-label={t('flocks.form.increase')}
                        sx={{ minWidth: '44px', minHeight: '44px' }}
                      >
                        <AddIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                inputProps={{
                  min: 0,
                  style: { minHeight: '44px' },
                }}
              />

              <TextField
                label={t('flocks.roosters')}
                value={roosters}
                onChange={(e) => {
                  const value = parseInt(e.target.value) || 0;
                  handleNumberChange(setRoosters, value);
                }}
                fullWidth
                disabled={isPending}
                type="number"
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => decrementValue(setRoosters, roosters)}
                        disabled={isPending || roosters <= 0}
                        size="small"
                        aria-label={t('flocks.form.decrease')}
                        sx={{ minWidth: '44px', minHeight: '44px' }}
                      >
                        <RemoveIcon />
                      </IconButton>
                      <IconButton
                        onClick={() => incrementValue(setRoosters, roosters)}
                        disabled={isPending}
                        size="small"
                        aria-label={t('flocks.form.increase')}
                        sx={{ minWidth: '44px', minHeight: '44px' }}
                      >
                        <AddIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                inputProps={{
                  min: 0,
                  style: { minHeight: '44px' },
                }}
              />

              <TextField
                label={t('flocks.chicks')}
                value={chicks}
                onChange={(e) => {
                  const value = parseInt(e.target.value) || 0;
                  handleNumberChange(setChicks, value);
                }}
                fullWidth
                disabled={isPending}
                type="number"
                error={!!countsError}
                helperText={countsError}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => decrementValue(setChicks, chicks)}
                        disabled={isPending || chicks <= 0}
                        size="small"
                        aria-label={t('flocks.form.decrease')}
                        sx={{ minWidth: '44px', minHeight: '44px' }}
                      >
                        <RemoveIcon />
                      </IconButton>
                      <IconButton
                        onClick={() => incrementValue(setChicks, chicks)}
                        disabled={isPending}
                        size="small"
                        aria-label={t('flocks.form.increase')}
                        sx={{ minWidth: '44px', minHeight: '44px' }}
                      >
                        <AddIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                inputProps={{
                  min: 0,
                  style: { minHeight: '44px' },
                }}
              />
            </Box>
          </Box>
        </DialogContent>
        <DialogActions
          sx={{
            position: 'sticky',
            bottom: 0,
            backgroundColor: 'background.paper',
            zIndex: 1,
            '& .MuiButton-root': {
              minHeight: '44px',
            },
          }}
        >
          <Button onClick={handleClose} disabled={isPending}>
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isPending || !isFormValid()}
            startIcon={isPending ? <CircularProgress size={20} color="inherit" /> : undefined}
          >
            {isPending ? t('common.saving') : t('common.save')}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
