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
  CONFIRMATION_DIALOG_CONFIG,
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
   * Variant of the confirm button
   * @default 'contained'
   */
  confirmVariant?: 'text' | 'outlined' | 'contained';

  /**
   * Variant of the cancel button
   * @default 'text'
   */
  cancelVariant?: 'text' | 'outlined' | 'contained';

  /**
   * Text to display in confirm button while pending
   * @default 'common.processing'
   */
  pendingText?: string;
}

/**
 * Standardized confirmation dialog component
 * US-010 Standardization
 *
 * Features:
 * - Consistent sizing (maxWidth: 'xs' for confirmations)
 * - Mobile fullscreen support
 * - Clear warning message with context
 * - Danger action button (red, outlined or contained)
 * - Cancel button (text or outlined)
 * - Loading state with disabled buttons
 * - Standardized padding and spacing (DialogTitle: p: 2, DialogContent: p: 3)
 * - Touch-friendly buttons (minHeight: 44px)
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
  confirmVariant = 'contained',
  cancelVariant = 'text',
  pendingText,
}: ConfirmationDialogProps) {
  const { t } = useTranslation();

  const defaultCancelText = cancelText || t('common.cancel');
  const defaultPendingText = pendingText || t('common.processing');

  return (
    <Dialog
      open={open}
      onClose={isPending ? undefined : onClose}
      maxWidth={CONFIRMATION_DIALOG_CONFIG.maxWidth}
      fullWidth={CONFIRMATION_DIALOG_CONFIG.fullWidth}
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
          variant={cancelVariant}
          sx={touchButtonSx}
        >
          {defaultCancelText}
        </Button>
        <Button
          onClick={onConfirm}
          color={confirmColor}
          variant={confirmVariant}
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
