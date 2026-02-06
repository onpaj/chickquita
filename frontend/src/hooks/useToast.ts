import { useContext } from 'react';
import { ToastContext } from '../contexts/ToastContext';

/**
 * Hook to access toast notifications
 * Must be used within ToastProvider
 */
export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within ToastProvider');
  }
  return context;
}
