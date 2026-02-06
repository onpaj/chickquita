import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
  Alert,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { AxiosError } from 'axios';
import { useUpdateCoop } from '../hooks/useCoops';
import type { Coop, UpdateCoopRequest } from '../api/coopsApi';

interface EditCoopModalProps {
  open: boolean;
  onClose: () => void;
  coop: Coop;
}

export function EditCoopModal({ open, onClose, coop }: EditCoopModalProps) {
  const { t } = useTranslation();
  const { mutate: updateCoop, isPending } = useUpdateCoop();

  const [name, setName] = useState('');
  const [location, setLocation] = useState('');
  const [nameError, setNameError] = useState('');
  const [locationError, setLocationError] = useState('');
  const [serverError, setServerError] = useState('');

  // Pre-fill form with coop data when modal opens
  useEffect(() => {
    if (open) {
      setName(coop.name);
      setLocation(coop.location || '');
      setNameError('');
      setLocationError('');
      setServerError('');
    }
  }, [open, coop]);

  const handleClose = () => {
    setName('');
    setLocation('');
    setNameError('');
    setLocationError('');
    setServerError('');
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

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) {
      return;
    }

    const request: UpdateCoopRequest = {
      id: coop.id,
      name: name.trim(),
      location: location.trim() || undefined,
    };

    updateCoop(request, {
      onSuccess: () => {
        handleClose();
      },
      onError: (error: Error) => {
        console.error('Failed to update coop:', error);

        // Handle 409 Conflict error (duplicate name)
        if (error instanceof AxiosError && error.response?.status === 409) {
          setNameError(t('coops.duplicateName'));
        } else if (error instanceof AxiosError && error.response?.data?.error?.message) {
          // For other errors, show the server error message
          setServerError(error.response.data.error.message);
        } else {
          // Fallback to generic error message
          setServerError(t('errors.generic'));
        }
      },
    });
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
        <DialogTitle>{t('coops.editCoop')}</DialogTitle>
        <DialogContent
          sx={{
            // Make content scrollable to prevent keyboard from obscuring submit button
            overflowY: 'auto',
            flex: 1,
          }}
        >
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            {serverError && (
              <Alert severity="error" onClose={() => setServerError('')}>
                {serverError}
              </Alert>
            )}
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
                if (serverError) {
                  setServerError('');
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
              label={t('coops.coopDescription')}
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
          >
            {t('common.save')}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
