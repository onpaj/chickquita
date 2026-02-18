import { useState, useEffect } from 'react';
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
  Slide,
} from '@mui/material';
import type { TransitionProps } from '@mui/material/transitions';
import CloseIcon from '@mui/icons-material/Close';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { useUpdateCoop } from '../hooks/useCoops';
import { useErrorHandler } from '../../../hooks/useErrorHandler';
import { processApiError, ErrorType } from '../../../lib/errors';
import type { Coop, UpdateCoopRequest } from '../api/coopsApi';
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

interface EditCoopModalProps {
  open: boolean;
  onClose: () => void;
  coop: Coop;
}

export function EditCoopModal({ open, onClose, coop }: EditCoopModalProps) {
  const { t } = useTranslation();
  const { mutate: updateCoop, isPending } = useUpdateCoop();
  const { handleError } = useErrorHandler();

  const [name, setName] = useState('');
  const [location, setLocation] = useState('');
  const [nameError, setNameError] = useState('');
  const [locationError, setLocationError] = useState('');

  // Pre-fill form with coop data when modal opens
  /* eslint-disable react-hooks/set-state-in-effect */
  useEffect(() => {
    if (open) {
      setName(coop.name);
      setLocation(coop.location || '');
      setNameError('');
      setLocationError('');
    }
  }, [open, coop]);
  /* eslint-enable react-hooks/set-state-in-effect */

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

  const submitUpdate = () => {
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
          handleError(error, submitUpdate);
        }
      },
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    submitUpdate();
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
          {t('coops.editCoop')}
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
              label={t('coops.coopName')}
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                if (nameError) {
                  const newError = validateName(e.target.value);
                  setNameError(newError);
                }
              }}
              onBlur={() => {
                setNameError(validateName(name));
              }}
              error={!!nameError}
              helperText={nameError}
              required
              fullWidth
              disabled={isPending}
              type="text"
              inputProps={touchInputProps}
            />
            <TextField
              label={t('coops.location')}
              value={location}
              onChange={(e) => {
                setLocation(e.target.value);
                if (locationError) {
                  const newError = validateLocation(e.target.value);
                  setLocationError(newError);
                }
              }}
              onBlur={() => {
                setLocationError(validateLocation(location));
              }}
              error={!!locationError}
              helperText={locationError}
              fullWidth
              disabled={isPending}
              multiline
              rows={2}
              type="text"
              inputProps={touchInputProps}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={dialogActionsSx}>
          <Button onClick={handleClose} disabled={isPending} sx={touchButtonSx}>
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
