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

  const handleClose = () => {
    setName('');
    setLocation('');
    setNameError('');
    onClose();
  };

  const validate = (): boolean => {
    if (!name.trim()) {
      setNameError(t('validation.required'));
      return false;
    }
    if (name.length > 100) {
      setNameError(t('validation.maxLength', { count: 100 }));
      return false;
    }
    setNameError('');
    return true;
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
        setNameError(t('errors.generic'));
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
                if (nameError) setNameError('');
              }}
              error={!!nameError}
              helperText={nameError}
              required
              fullWidth
              disabled={isPending}
              inputProps={{ maxLength: 100 }}
            />
            <TextField
              label={t('coops.coopDescription')}
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              fullWidth
              disabled={isPending}
              inputProps={{ maxLength: 200 }}
              multiline
              rows={2}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose} disabled={isPending}>
            {t('common.cancel')}
          </Button>
          <Button type="submit" variant="contained" disabled={isPending}>
            {t('common.save')}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
