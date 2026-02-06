import { createContext } from 'react';
import type { AlertColor } from '@mui/material';

/**
 * Toast notification types
 */
export interface Toast {
  id: string;
  message: string;
  severity: AlertColor;
  translationKey?: string;
  translationParams?: Record<string, string | number>;
  onRetry?: () => void;
  autoHideDuration?: number;
}

/**
 * Toast context for managing toast notifications
 */
export interface ToastContextType {
  showToast: (toast: Omit<Toast, 'id'>) => void;
  showError: (message: string, translationKey?: string, onRetry?: () => void) => void;
  showSuccess: (message: string, translationKey?: string) => void;
  showWarning: (message: string, translationKey?: string) => void;
  showInfo: (message: string, translationKey?: string) => void;
  hideToast: () => void;
}

export const ToastContext = createContext<ToastContextType | undefined>(undefined);
