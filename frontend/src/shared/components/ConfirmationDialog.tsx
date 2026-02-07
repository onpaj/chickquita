/**
 * Reusable confirmation dialog component
 * Standardized based on US-009 requirements
 */

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
import {
  DIALOG_CONFIG,
  isMobileViewport,
  dialogTitleSx,
  dialogContentSx,
  dialogActionsSimpleSx,
  touchButtonSx,
} from '@/shared/constants/modalConfig';

interface ConfirmationDialogProps {
  /**
   * Whether the dialog is open
   */
  open: boolean;

  /**
   * Handler called when the dialog should close
   */
  onClose: () => void;

  /**
   * Handler called when the user confirms the action
   */
  onConfirm: () => void;

  /**
   * Dialog title text
   */
  title: string;

  /**
   * Main message content
   */
  message: string;

  /**
   * Optional secondary message (displayed below main message)
   */
  secondaryMessage?: string;

  /**
   * Name/identifier of the entity being acted upon
   * Displayed in bold after the main message
   */
  entityName?: string;

  /**
   * Whether the confirmation action is in progress
   */
  isPending?: boolean;

  /**
   * Text for the confirm button
   */
  confirmText: string;

  /**
   * Text for the cancel button
   * @default 'common.cancel'
   */
  cancelText?: string;

  /**
   * Color of the confirm button
   * @default 'primary'
   */
  confirmColor?: 'primary' | 'error' | 'warning' | 'success';

  /**
   * Text to display in confirm button while pending
   * @default 'common.processing'
   */
  pendingText?: string;
}

/**
 * Standardized confirmation dialog component
 *
 * Features:
 * - Consistent sizing (maxWidth: 'sm')
 * - Mobile fullscreen support
 * - Standardized padding (DialogTitle: p: 2, DialogContent: p: 3)
 * - Touch-friendly buttons (minHeight: 44px)
 * - Loading state with spinner
 * - Configurable button colors
 */
export function ConfirmationDialog({
  open,
  onClose,
  onConfirm,
  title,
  message,
  secondaryMessage,
  entityName,
  isPending = false,
  confirmText,
  cancelText,
  confirmColor = 'primary',
  pendingText,
}: ConfirmationDialogProps) {
  const { t } = useTranslation();

  const defaultCancelText = cancelText || t('common.cancel');
  const defaultPendingText = pendingText || t('common.processing');

  return (
    <Dialog
      open={open}
      onClose={isPending ? undefined : onClose}
      maxWidth={DIALOG_CONFIG.maxWidth}
      fullWidth={DIALOG_CONFIG.fullWidth}
      fullScreen={isMobileViewport()}
    >
      <DialogTitle sx={dialogTitleSx}>{title}</DialogTitle>

      <DialogContent sx={dialogContentSx}>
        <DialogContentText>
          {message}
          {entityName && (
            <>
              {' '}
              <strong>{entityName}</strong>
            </>
          )}
        </DialogContentText>

        {secondaryMessage && (
          <DialogContentText sx={{ mt: 2 }}>
            {secondaryMessage}
          </DialogContentText>
        )}
      </DialogContent>

      <DialogActions sx={dialogActionsSimpleSx}>
        <Button
          onClick={onClose}
          disabled={isPending}
          sx={touchButtonSx}
        >
          {defaultCancelText}
        </Button>
        <Button
          onClick={onConfirm}
          color={confirmColor}
          variant="contained"
          disabled={isPending}
          sx={touchButtonSx}
          startIcon={
            isPending ? (
              <CircularProgress size={20} color="inherit" />
            ) : undefined
          }
        >
          {isPending ? defaultPendingText : confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
