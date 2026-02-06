import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
  CircularProgress,
} from '@mui/material';
import { useTranslation } from 'react-i18next';

interface ArchiveFlockDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  flockIdentifier: string;
  isPending?: boolean;
}

export function ArchiveFlockDialog({
  open,
  onClose,
  onConfirm,
  flockIdentifier,
  isPending = false,
}: ArchiveFlockDialogProps) {
  const { t } = useTranslation();

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="archive-flock-dialog-title"
      aria-describedby="archive-flock-dialog-description"
    >
      <DialogTitle id="archive-flock-dialog-title">
        {t('flocks.archiveConfirmTitle')}
      </DialogTitle>
      <DialogContent>
        <DialogContentText id="archive-flock-dialog-description">
          {t('flocks.archiveConfirmMessage')}
        </DialogContentText>
        <DialogContentText sx={{ mt: 2, fontWeight: 'medium' }}>
          {flockIdentifier}
        </DialogContentText>
      </DialogContent>
      <DialogActions
        sx={{
          // Touch-friendly button height
          '& .MuiButton-root': {
            minHeight: '44px',
          },
        }}
      >
        <Button onClick={onClose} disabled={isPending}>
          {t('common.cancel')}
        </Button>
        <Button
          onClick={onConfirm}
          variant="contained"
          color="warning"
          disabled={isPending}
          startIcon={isPending ? <CircularProgress size={20} color="inherit" /> : undefined}
        >
          {isPending ? t('flocks.archiving') : t('flocks.archiveFlock')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
