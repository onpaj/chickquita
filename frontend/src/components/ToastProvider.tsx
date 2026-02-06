import React, { useState, useCallback } from 'react';
import { Snackbar, Alert, Button } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { ToastContext } from '../contexts/ToastContext';
import type { Toast } from '../contexts/ToastContext';

/**
 * Toast Provider Component
 */
export function ToastProvider({ children }: { children: React.ReactNode }) {
  const { t } = useTranslation();
  const [toast, setToast] = useState<Toast | null>(null);

  const showToast = useCallback((newToast: Omit<Toast, 'id'>) => {
    setToast({
      id: Date.now().toString(),
      ...newToast,
    });
  }, []);

  const showError = useCallback(
    (message: string, translationKey?: string, onRetry?: () => void) => {
      showToast({
        message,
        translationKey,
        severity: 'error',
        onRetry,
        autoHideDuration: onRetry ? undefined : 6000, // Don't auto-hide if retry is available
      });
    },
    [showToast]
  );

  const showSuccess = useCallback(
    (message: string, translationKey?: string) => {
      showToast({
        message,
        translationKey,
        severity: 'success',
        autoHideDuration: 4000,
      });
    },
    [showToast]
  );

  const showWarning = useCallback(
    (message: string, translationKey?: string) => {
      showToast({
        message,
        translationKey,
        severity: 'warning',
        autoHideDuration: 5000,
      });
    },
    [showToast]
  );

  const showInfo = useCallback(
    (message: string, translationKey?: string) => {
      showToast({
        message,
        translationKey,
        severity: 'info',
        autoHideDuration: 4000,
      });
    },
    [showToast]
  );

  const hideToast = useCallback(() => {
    setToast(null);
  }, []);

  const handleClose = (_event?: React.SyntheticEvent | Event, reason?: string) => {
    // Don't close on clickaway if there's a retry button
    if (reason === 'clickaway' && toast?.onRetry) {
      return;
    }
    hideToast();
  };

  const handleRetry = () => {
    if (toast?.onRetry) {
      toast.onRetry();
      hideToast();
    }
  };

  return (
    <ToastContext.Provider
      value={{ showToast, showError, showSuccess, showWarning, showInfo, hideToast }}
    >
      {children}
      <Snackbar
        open={!!toast}
        autoHideDuration={toast?.autoHideDuration ?? null}
        onClose={handleClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
        sx={{ mb: 9 }} // Account for bottom navigation
      >
        <Alert
          onClose={handleClose}
          severity={toast?.severity ?? 'info'}
          sx={{ width: '100%' }}
          action={
            toast?.onRetry ? (
              <Button color="inherit" size="small" onClick={handleRetry}>
                {t('common.retry')}
              </Button>
            ) : undefined
          }
        >
          {toast?.translationKey
            ? t(toast.translationKey, toast.translationParams)
            : toast?.message}
        </Alert>
      </Snackbar>
    </ToastContext.Provider>
  );
}
