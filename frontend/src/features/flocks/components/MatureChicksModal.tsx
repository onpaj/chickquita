import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  CircularProgress,
  IconButton,
  InputAdornment,
  Typography,
  TextField,
  Alert,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import RemoveIcon from '@mui/icons-material/Remove';
import PetsIcon from '@mui/icons-material/Pets';
import { useTranslation } from 'react-i18next';
import { useMatureChicks } from '../hooks/useFlocks';
import type { Flock } from '../api/flocksApi';
import {
  DIALOG_CONFIG,
  isMobileViewport,
  dialogTitleSx,
  dialogContentSx,
  dialogActionsSx,
  touchButtonSx,
  touchInputProps,
  numberStepperButtonSx,
  FORM_FIELD_SPACING,
} from '@/shared/constants/modalConfig';

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
      sx={{
        '& .MuiDialog-paper': {
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '100vh',
        },
      }}
    >
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <DialogTitle sx={dialogTitleSx}>
          <Stack direction="row" alignItems="center" spacing={1}>
            <PetsIcon />
            <span>{t('flocks.matureChicks.title')}</span>
          </Stack>
        </DialogTitle>
        <DialogContent
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
            <TextField
              label={t('flocks.matureChicks.chicksCount')}
              value={chicksToMature}
              onChange={(e) => handleChicksChange(parseInt(e.target.value) || 1)}
              fullWidth
              disabled={isPending}
              type="number"
              helperText={t('flocks.matureChicks.chicksAvailable', { count: flock.currentChicks })}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => handleChicksChange(chicksToMature - 1)}
                      disabled={isPending || chicksToMature <= 1}
                      size="small"
                      aria-label={t('flocks.form.decrease')}
                      sx={numberStepperButtonSx}
                    >
                      <RemoveIcon />
                    </IconButton>
                    <IconButton
                      onClick={() => handleChicksChange(chicksToMature + 1)}
                      disabled={isPending || chicksToMature >= flock.currentChicks}
                      size="small"
                      aria-label={t('flocks.form.increase')}
                      sx={numberStepperButtonSx}
                    >
                      <AddIcon />
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              inputProps={{ min: 1, max: flock.currentChicks, ...touchInputProps }}
            />

            <Typography variant="subtitle2" color="text.secondary">
              {t('flocks.matureChicks.assignSubtitle')}
            </Typography>

            {/* Hens */}
            <TextField
              label={t('flocks.matureChicks.toHens')}
              value={hens}
              onChange={(e) => handleHensChange(parseInt(e.target.value) || 0)}
              fullWidth
              disabled={isPending}
              type="number"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => handleHensChange(hens - 1)}
                      disabled={isPending || hens <= 0}
                      size="small"
                      aria-label={t('flocks.form.decrease')}
                      sx={numberStepperButtonSx}
                    >
                      <RemoveIcon />
                    </IconButton>
                    <IconButton
                      onClick={() => handleHensChange(hens + 1)}
                      disabled={isPending || hens >= chicksToMature}
                      size="small"
                      aria-label={t('flocks.form.increase')}
                      sx={numberStepperButtonSx}
                    >
                      <AddIcon />
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              inputProps={{ min: 0, max: chicksToMature, ...touchInputProps }}
            />

            {/* Roosters */}
            <TextField
              label={t('flocks.matureChicks.toRoosters')}
              value={roosters}
              onChange={(e) => handleRoostersChange(parseInt(e.target.value) || 0)}
              fullWidth
              disabled={isPending}
              type="number"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => handleRoostersChange(roosters - 1)}
                      disabled={isPending || roosters <= 0}
                      size="small"
                      aria-label={t('flocks.form.decrease')}
                      sx={numberStepperButtonSx}
                    >
                      <RemoveIcon />
                    </IconButton>
                    <IconButton
                      onClick={() => handleRoostersChange(roosters + 1)}
                      disabled={isPending || roosters >= chicksToMature}
                      size="small"
                      aria-label={t('flocks.form.increase')}
                      sx={numberStepperButtonSx}
                    >
                      <AddIcon />
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              inputProps={{ min: 0, max: chicksToMature, ...touchInputProps }}
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
          <Button onClick={handleClose} disabled={isPending} sx={touchButtonSx}>
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
