import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
  CircularProgress,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useCreateCoop } from '../hooks/useCoops';
import { useErrorHandler } from '../../../hooks/useErrorHandler';
import { processApiError, ErrorType } from '../../../lib/errors';
import type { CreateCoopRequest } from '../api/coopsApi';

interface CreateCoopModalProps {
  open: boolean;
  onClose: () => void;
}

export function CreateCoopModal({ open, onClose }: CreateCoopModalProps) {
  const { t } = useTranslation();
  const { mutate: createCoop, isPending } = useCreateCoop();
  const { handleError } = useErrorHandler();

  const [name, setName] = useState('');
  const [location, setLocation] = useState('');
  const [nameError, setNameError] = useState('');
  const [locationError, setLocationError] = useState('');

  const handleClose = () => {
    setName('');
    setLocation('');
    setNameError('');
    setLocationError('');
    onClose();
  };

  const validateName = (value: string): string => {
    if (!value.trim()) {
      return t('validation.required');
    }
    if (value.length > 100) {
      return t('validation.maxLength', { count: 100 });
    }
    return '';
  };

  const validateLocation = (value: string): string => {
    if (value.length > 200) {
      return t('validation.maxLength', { count: 200 });
    }
    return '';
  };

  const validate = (): boolean => {
    const nameErr = validateName(name);
    const locationErr = validateLocation(location);

    setNameError(nameErr);
    setLocationError(locationErr);

    return !nameErr && !locationErr;
  };

  const isFormValid = (): boolean => {
    return name.trim().length > 0 &&
           name.length <= 100 &&
           location.length <= 200;
  };

  const submitCoop = () => {
    if (!validate()) {
      return;
    }

    const request: CreateCoopRequest = {
      name: name.trim(),
      location: location.trim() || undefined,
    };

    createCoop(request, {
      onSuccess: () => {
        handleClose();
      },
      onError: (error: Error) => {
        const processedError = processApiError(error);

        // Handle 409 Conflict error (duplicate name) - show as field error
        if (processedError.type === ErrorType.CONFLICT) {
          setNameError(t('coops.duplicateName'));
        }
        // Handle 400 Validation errors - show as field errors
        else if (processedError.type === ErrorType.VALIDATION && processedError.fieldErrors) {
          processedError.fieldErrors.forEach((fieldError) => {
            if (fieldError.field === 'name' || fieldError.field === 'Name') {
              setNameError(fieldError.message);
            } else if (fieldError.field === 'location' || fieldError.field === 'Location') {
              setLocationError(fieldError.message);
            }
          });
        }
        // For all other errors (network, server, etc.), show toast with retry
        else {
          handleError(error, submitCoop);
        }
      },
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    submitCoop();
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
          // Ensure form is scrollable on small screens
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '100vh',
        },
      }}
    >
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <DialogTitle>{t('coops.addCoop')}</DialogTitle>
        <DialogContent
          sx={{
            // Make content scrollable to prevent keyboard from obscuring submit button
            overflowY: 'auto',
            flex: 1,
          }}
        >
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              autoFocus
              label={t('coops.coopName')}
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                // Clear errors when user starts fixing input
                if (nameError) {
                  const newError = validateName(e.target.value);
                  setNameError(newError);
                }
              }}
              onBlur={() => {
                // Validate on blur to show errors
                setNameError(validateName(name));
              }}
              error={!!nameError}
              helperText={nameError}
              required
              fullWidth
              disabled={isPending}
              type="text"
              inputProps={{
                // Ensure touch-friendly height (min 44px)
                style: { minHeight: '44px' },
              }}
            />
            <TextField
              label={t('coops.location')}
              value={location}
              onChange={(e) => {
                setLocation(e.target.value);
                // Clear error when user starts fixing input
                if (locationError) {
                  const newError = validateLocation(e.target.value);
                  setLocationError(newError);
                }
              }}
              onBlur={() => {
                // Validate on blur to show errors
                setLocationError(validateLocation(location));
              }}
              error={!!locationError}
              helperText={locationError}
              fullWidth
              disabled={isPending}
              multiline
              rows={2}
              type="text"
              inputProps={{
                // Ensure touch-friendly height
                style: { minHeight: '44px' },
              }}
            />
          </Box>
        </DialogContent>
        <DialogActions
          sx={{
            // Ensure actions are always visible and not obscured by keyboard
            position: 'sticky',
            bottom: 0,
            backgroundColor: 'background.paper',
            zIndex: 1,
            // Touch-friendly button height
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
