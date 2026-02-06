import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useCreateCoop } from '../hooks/useCoops';
import type { CreateCoopRequest } from '../api/coopsApi';

interface CreateCoopModalProps {
  open: boolean;
  onClose: () => void;
}

export function CreateCoopModal({ open, onClose }: CreateCoopModalProps) {
  const { t } = useTranslation();
  const { mutate: createCoop, isPending } = useCreateCoop();

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

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

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
      onError: (error) => {
        console.error('Failed to create coop:', error);
        // Don't clear existing validation errors, just log the error
        // The user can see what went wrong in the console
      },
    });
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>{t('coops.addCoop')}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              autoFocus
              label={t('coops.coopName')}
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                // Clear error when user starts fixing input
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
            />
          </Box>
        </DialogContent>
        <DialogActions>
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
