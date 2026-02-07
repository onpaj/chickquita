/**
 * Standardized configuration for modal/dialog components
 * Based on US-009 acceptance criteria
 */

import type { SxProps, Theme } from '@mui/material';

/**
 * Standard Dialog sizing for form modals
 */
export const DIALOG_CONFIG = {
  maxWidth: 'sm' as const,
  fullWidth: true,
} as const;

/**
 * Standard Dialog sizing for confirmation dialogs
 * US-010: Smaller size for confirmations
 */
export const CONFIRMATION_DIALOG_CONFIG = {
  maxWidth: 'xs' as const,
  fullWidth: true,
} as const;

/**
 * Breakpoint for mobile fullscreen dialogs
 */
export const MOBILE_BREAKPOINT = 480;

/**
 * Check if current viewport is mobile
 */
export const isMobileViewport = (): boolean => {
  return typeof window !== 'undefined' && window.innerWidth < MOBILE_BREAKPOINT;
};

/**
 * Standard padding for DialogTitle
 */
export const DIALOG_TITLE_PADDING = 2;

/**
 * Standard padding for DialogContent
 */
export const DIALOG_CONTENT_PADDING = 3;

/**
 * Standard spacing for form field Stack
 */
export const FORM_FIELD_SPACING = 2;

/**
 * Minimum touch target size (iOS standard)
 */
export const MIN_TOUCH_TARGET = '44px';

/**
 * Standard DialogTitle sx props
 */
export const dialogTitleSx: SxProps<Theme> = {
  p: DIALOG_TITLE_PADDING,
};

/**
 * Standard DialogContent sx props
 */
export const dialogContentSx: SxProps<Theme> = {
  p: DIALOG_CONTENT_PADDING,
};

/**
 * Standard DialogActions sx props for form modals
 */
export const dialogActionsSx: SxProps<Theme> = {
  position: 'sticky',
  bottom: 0,
  zIndex: 1,
  backgroundColor: 'background.paper',
  p: 2,
};

/**
 * Standard DialogActions sx props for simple confirmation dialogs
 */
export const dialogActionsSimpleSx: SxProps<Theme> = {
  p: 2,
};

/**
 * Standard button sx props for touch-friendly buttons
 */
export const touchButtonSx: SxProps<Theme> = {
  minHeight: MIN_TOUCH_TARGET,
};

/**
 * Standard input props for touch-friendly text fields
 */
export const touchInputProps = {
  style: {
    minHeight: MIN_TOUCH_TARGET,
  },
};

/**
 * Standard number input button sx props
 */
export const numberStepperButtonSx: SxProps<Theme> = {
  minWidth: MIN_TOUCH_TARGET,
  minHeight: MIN_TOUCH_TARGET,
};
