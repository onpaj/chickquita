import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  CircularProgress,
  IconButton,
  Typography,
  Alert,
  Slide,
} from '@mui/material';
import type { TransitionProps } from '@mui/material/transitions';
import CloseIcon from '@mui/icons-material/Close';
import PetsIcon from '@mui/icons-material/Pets';
import { useTranslation } from 'react-i18next';
import { useMatureChicks } from '../hooks/useFlocks';
import type { Flock } from '../api/flocksApi';
import { NumericStepper } from '@/shared/components';
import {
  DIALOG_CONFIG,
  isMobileViewport,
  dialogTitleSx,
  dialogContentSx,
  dialogActionsSx,
  touchButtonSx,
  FORM_FIELD_SPACING,
} from '@/shared/constants/modalConfig';

const SlideUp = React.forwardRef(function SlideUp(
  props: TransitionProps & { children: React.ReactElement },
  ref: React.Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

interface MatureChicksModalProps {
  open: boolean;
  onClose: () => void;
  flock: Flock;
}

export function MatureChicksModal({ open, onClose, flock }: MatureChicksModalProps) {
  const { t } = useTranslation();
  const { mutate: matureChicks, isPending } = useMatureChicks();

  const [chicksToMature, setChicksToMature] = useState(1);
  const [hens, setHens] = useState(0);
  const [roosters, setRoosters] = useState(0);

  const totalAssigned = hens + roosters;
  const isValid = chicksToMature >= 1 &&
    chicksToMature <= flock.currentChicks &&
    totalAssigned === chicksToMature;

  const handleClose = () => {
    setChicksToMature(1);
    setHens(0);
    setRoosters(0);
    onClose();
  };

  const handleChicksChange = (value: number) => {
    const clamped = Math.max(1, Math.min(value, flock.currentChicks));
    setChicksToMature(clamped);
    // Reset hens/roosters when chicksToMature changes
    setHens(0);
    setRoosters(0);
  };

  const handleHensChange = (value: number) => {
    const clamped = Math.max(0, Math.min(value, chicksToMature));
    setHens(clamped);
  };

  const handleRoostersChange = (value: number) => {
    const clamped = Math.max(0, Math.min(value, chicksToMature));
    setRoosters(clamped);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!isValid) return;

    matureChicks(
      { flockId: flock.id, data: { chicksToMature, hens, roosters } },
      { onSuccess: handleClose }
    );
  };

  const validationError = totalAssigned !== chicksToMature
    ? t('flocks.matureChicks.sumError', { count: chicksToMature })
    : '';

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
          <Stack direction="row" alignItems="center" spacing={1}>
            <PetsIcon />
            <span>{t('flocks.matureChicks.title')}</span>
          </Stack>
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
            <Typography variant="body2" color="text.secondary">
              {t('flocks.matureChicks.description', { count: flock.currentChicks })}
            </Typography>

            {/* Chicks to mature */}
            <NumericStepper
              label={t('flocks.matureChicks.chicksCount')}
              value={chicksToMature}
              onChange={handleChicksChange}
              min={1}
              max={flock.currentChicks}
              disabled={isPending}
              helperText={t('flocks.matureChicks.chicksAvailable', { count: flock.currentChicks })}
              aria-label={t('flocks.matureChicks.chicksCount')}
            />

            <Typography variant="subtitle2" color="text.secondary">
              {t('flocks.matureChicks.assignSubtitle')}
            </Typography>

            {/* Hens */}
            <NumericStepper
              label={t('flocks.matureChicks.toHens')}
              value={hens}
              onChange={handleHensChange}
              min={0}
              max={chicksToMature}
              disabled={isPending}
              aria-label={t('flocks.matureChicks.toHens')}
            />

            {/* Roosters */}
            <NumericStepper
              label={t('flocks.matureChicks.toRoosters')}
              value={roosters}
              onChange={handleRoostersChange}
              min={0}
              max={chicksToMature}
              disabled={isPending}
              aria-label={t('flocks.matureChicks.toRoosters')}
            />

            {/* Real-time total display */}
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="body2" color="text.secondary">
                {t('flocks.matureChicks.total')}:
              </Typography>
              <Typography
                variant="body1"
                fontWeight="medium"
                color={totalAssigned === chicksToMature ? 'success.main' : 'error.main'}
              >
                {totalAssigned} / {chicksToMature}
              </Typography>
            </Stack>

            {/* Validation error */}
            {validationError && (
              <Alert severity="error" sx={{ mt: 1 }}>
                {validationError}
              </Alert>
            )}
          </Stack>
        </DialogContent>
        <DialogActions sx={dialogActionsSx}>
          <Button variant="text" onClick={handleClose} disabled={isPending} sx={touchButtonSx}>
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isPending || !isValid}
            startIcon={isPending ? <CircularProgress size={20} color="inherit" /> : undefined}
            sx={touchButtonSx}
          >
            {isPending ? t('common.saving') : t('flocks.matureChicks.confirm')}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
