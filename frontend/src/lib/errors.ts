import { AxiosError } from 'axios';

/**
 * Standard API error response structure
 */
export interface ApiErrorResponse {
  error: {
    code: string;
    message: string;
    details?: Array<{
      field: string;
      message: string;
    }>;
  };
}

/**
 * Error types for consistent error handling
 */
export const ErrorType = {
  NETWORK: 'NETWORK',
  VALIDATION: 'VALIDATION',
  NOT_FOUND: 'NOT_FOUND',
  CONFLICT: 'CONFLICT',
  SERVER: 'SERVER',
  UNAUTHORIZED: 'UNAUTHORIZED',
  FORBIDDEN: 'FORBIDDEN',
  UNKNOWN: 'UNKNOWN',
} as const;

export type ErrorType = typeof ErrorType[keyof typeof ErrorType];

/**
 * Processed error for UI display
 */
export interface ProcessedError {
  type: ErrorType;
  message: string;
  translationKey: string;
  fieldErrors?: Array<{
    field: string;
    message: string;
  }>;
  canRetry: boolean;
  originalError?: unknown;
}

/**
 * Process an Axios error into a standardized format for UI display
 */
export function processApiError(error: unknown): ProcessedError {
  // Check if it's an Axios error
  if (error && typeof error === 'object' && 'isAxiosError' in error) {
    const axiosError = error as AxiosError<ApiErrorResponse>;

    // Network error (no response received)
    if (!axiosError.response) {
      return {
        type: ErrorType.NETWORK,
        message: 'Network error occurred. Please check your connection.',
        translationKey: 'errors.networkError',
        canRetry: true,
        originalError: error,
      };
    }

    const { status, data } = axiosError.response;

    // 400 - Validation error
    if (status === 400) {
      return {
        type: ErrorType.VALIDATION,
        message: data?.error?.message || 'Validation error occurred.',
        translationKey: 'errors.validationError',
        fieldErrors: data?.error?.details,
        canRetry: false,
        originalError: error,
      };
    }

    // 401 - Unauthorized
    if (status === 401) {
      return {
        type: ErrorType.UNAUTHORIZED,
        message: 'Unauthorized. Please sign in again.',
        translationKey: 'errors.unauthorized',
        canRetry: false,
        originalError: error,
      };
    }

    // 403 - Forbidden
    if (status === 403) {
      return {
        type: ErrorType.FORBIDDEN,
        message: 'You do not have permission to perform this action.',
        translationKey: 'errors.forbidden',
        canRetry: false,
        originalError: error,
      };
    }

    // 404 - Not found
    if (status === 404) {
      return {
        type: ErrorType.NOT_FOUND,
        message: data?.error?.message || 'Resource not found.',
        translationKey: 'errors.notFound',
        canRetry: false,
        originalError: error,
      };
    }

    // 409 - Conflict (duplicate name, etc.)
    if (status === 409) {
      return {
        type: ErrorType.CONFLICT,
        message: data?.error?.message || 'Conflict occurred.',
        translationKey: data?.error?.code === 'DUPLICATE_NAME'
          ? 'errors.duplicateName'
          : 'errors.conflict',
        canRetry: false,
        originalError: error,
      };
    }

    // 500 - Server error
    if (status >= 500) {
      return {
        type: ErrorType.SERVER,
        message: 'Server error occurred. Please try again later.',
        translationKey: 'errors.serverError',
        canRetry: true,
        originalError: error,
      };
    }
  }

  // Unknown error
  return {
    type: ErrorType.UNKNOWN,
    message: 'An unexpected error occurred.',
    translationKey: 'errors.unknown',
    canRetry: true,
    originalError: error,
  };
}

/**
 * Retry utility for network and server errors
 */
export async function retryRequest<T>(
  requestFn: () => Promise<T>,
  maxRetries = 3,
  delayMs = 1000
): Promise<T> {
  let lastError: unknown;

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await requestFn();
    } catch (error) {
      lastError = error;
      const processedError = processApiError(error);

      // Don't retry if error is not retryable
      if (!processedError.canRetry || attempt === maxRetries) {
        throw error;
      }

      // Wait before retrying (exponential backoff)
      await new Promise((resolve) => setTimeout(resolve, delayMs * Math.pow(2, attempt)));
    }
  }

  throw lastError;
}
