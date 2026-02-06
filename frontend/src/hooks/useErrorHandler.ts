import { useCallback } from 'react';
import { useToast } from './useToast';
import { processApiError, ErrorType } from '../lib/errors';

/**
 * Hook for handling API errors consistently across components
 */
export function useErrorHandler() {
  const { showError } = useToast();

  const handleError = useCallback(
    (error: unknown, onRetry?: () => void) => {
      const processedError = processApiError(error);

      // Show toast notification for all errors except validation errors
      // Validation errors should be displayed as field-level errors
      if (processedError.type !== ErrorType.VALIDATION) {
        showError(
          processedError.message,
          processedError.translationKey,
          processedError.canRetry && onRetry ? onRetry : undefined
        );
      }

      return processedError;
    },
    [showError]
  );

  return { handleError };
}
