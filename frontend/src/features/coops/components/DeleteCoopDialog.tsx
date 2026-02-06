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

interface DeleteCoopDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  coopName: string;
  isPending?: boolean;
}

export function DeleteCoopDialog({
  open,
  onClose,
  onConfirm,
  coopName,
  isPending = false,
}: DeleteCoopDialogProps) {
  const { t } = useTranslation();

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="delete-dialog-title"
      aria-describedby="delete-dialog-description"
    >
      <DialogTitle id="delete-dialog-title">
        {t('coops.deleteConfirmTitle')}
      </DialogTitle>
      <DialogContent>
        <DialogContentText id="delete-dialog-description">
          {t('coops.deleteConfirmMessage')}
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
          color="error"
          disabled={isPending}
          startIcon={isPending ? <CircularProgress size={20} color="inherit" /> : undefined}
        >
          {isPending ? t('common.deleting') : t('common.delete')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
