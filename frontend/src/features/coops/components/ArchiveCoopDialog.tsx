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

interface ArchiveCoopDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  coopName: string;
  isPending?: boolean;
}

export function ArchiveCoopDialog({
  open,
  onClose,
  onConfirm,
  coopName,
  isPending = false,
}: ArchiveCoopDialogProps) {
  const { t } = useTranslation();

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="archive-dialog-title"
      aria-describedby="archive-dialog-description"
    >
      <DialogTitle id="archive-dialog-title">
        {t('coops.archiveConfirmTitle')}
      </DialogTitle>
      <DialogContent>
        <DialogContentText id="archive-dialog-description">
          {t('coops.archiveConfirmMessage')}
        </DialogContentText>
        <DialogContentText sx={{ mt: 2, fontWeight: 'medium' }}>
          {coopName}
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
          {isPending ? t('coops.archiving') : t('coops.archiveCoop')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
