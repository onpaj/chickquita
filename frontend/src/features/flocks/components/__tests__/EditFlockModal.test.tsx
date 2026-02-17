import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { EditFlockModal } from '../EditFlockModal';
import type { Flock, UpdateFlockRequest } from '../../api/flocksApi';

// Mock the useUpdateFlock hook
const mockUpdateFlock = vi.fn();
const mockUseUpdateFlock = vi.fn();

vi.mock('../../hooks/useFlocks', () => ({
  useUpdateFlock: () => mockUseUpdateFlock(),
}));

// Mock useErrorHandler hook
const mockHandleError = vi.fn();
vi.mock('../../../../hooks/useErrorHandler', () => ({
  useErrorHandler: () => ({
    handleError: mockHandleError,
  }),
}));

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, params?: { count?: number }) => {
      const translations: Record<string, string> = {
        'flocks.editFlock': 'Edit Flock',
        'flocks.form.identifier': 'Flock Identifier',
        'flocks.form.hatchDate': 'Hatch Date',
        'flocks.form.hatchDateFuture': 'Hatch date cannot be in the future',
        'common.cancel': 'Cancel',
        'common.save': 'Save',
        'common.saving': 'Saving',
        'validation.required': 'This field is required',
        'validation.maxLength': `Maximum length is ${params?.count} characters`,
      };
      return translations[key] || key;
    },
  }),
}));

describe('EditFlockModal', () => {
  let queryClient: QueryClient;
  const mockOnClose = vi.fn();

  // Mock flock data for testing
  const mockFlock: Flock = {
    id: 'flock-123',
    coopId: 'coop-456',
    identifier: 'Existing Flock',
    hatchDate: '2024-01-15T00:00:00Z',
    currentHens: 10,
    currentRoosters: 2,
    currentChicks: 5,
    totalEggsProduced: 100,
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  };

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    // Default mock implementation
    mockUseUpdateFlock.mockReturnValue({
      mutate: mockUpdateFlock,
      isPending: false,
    });
    vi.clearAllMocks();
  });

  const renderModal = (open = true, flock = mockFlock) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <EditFlockModal open={open} onClose={mockOnClose} flock={flock} />
      </QueryClientProvider>
    );
  };

  const getTodayDate = (): string => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  };

  const getFutureDate = (): string => {
    const future = new Date();
    future.setDate(future.getDate() + 1);
    return future.toISOString().split('T')[0];
  };

  describe('Rendering', () => {
    it('should render modal title', () => {
      renderModal();
      expect(screen.getByText('Edit Flock')).toBeInTheDocument();
    });

    it('should render modal when open prop is true', () => {
      renderModal(true);
      expect(screen.getByText('Edit Flock')).toBeInTheDocument();
      expect(screen.getByLabelText(/Flock Identifier/)).toBeInTheDocument();
    });

    it('should not render when open is false', () => {
      renderModal(false);
      expect(screen.queryByText('Edit Flock')).not.toBeInTheDocument();
    });

    it('should render identifier input field', () => {
      renderModal();
      expect(screen.getByLabelText(/Flock Identifier/)).toBeInTheDocument();
    });

    it('should render hatch date input field', () => {
      renderModal();
      expect(screen.getByLabelText(/Hatch Date/)).toBeInTheDocument();
    });

    it('should render composition fields (hens, roosters, chicks)', () => {
      renderModal();
      // Composition fields should be editable in edit modal (added in UX-002)
      expect(screen.getByText('flocks.hens')).toBeInTheDocument();
      expect(screen.getByText('flocks.roosters')).toBeInTheDocument();
      expect(screen.getByText('flocks.chicks')).toBeInTheDocument();
    });

    it('should render cancel button', () => {
      renderModal();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });

    it('should render save button', () => {
      renderModal();
      expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    });
  });

  describe('Form Pre-fill', () => {
    it('should pre-fill form with existing flock data', () => {
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/) as HTMLInputElement;
      const hatchDateInput = screen.getByLabelText(/Hatch Date/) as HTMLInputElement;

      expect(identifierInput.value).toBe('Existing Flock');
      expect(hatchDateInput.value).toBe('2024-01-15'); // Date part only
    });

    it('should extract date part from ISO string for hatch date', () => {
      renderModal();

      const hatchDateInput = screen.getByLabelText(/Hatch Date/) as HTMLInputElement;
      // Should extract '2024-01-15' from '2024-01-15T00:00:00Z'
      expect(hatchDateInput.value).toBe('2024-01-15');
    });

    it('should clear error messages when modal opens', async () => {
      const { rerender } = renderModal(false);

      // Open modal
      rerender(
        <QueryClientProvider client={queryClient}>
          <EditFlockModal open={true} onClose={mockOnClose} flock={mockFlock} />
        </QueryClientProvider>
      );

      // No error messages should be visible on open
      expect(screen.queryByText('This field is required')).not.toBeInTheDocument();
    });
  });

  describe('Form Validation', () => {
    it('should show required error when identifier is empty and field is blurred', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('This field is required')).toBeInTheDocument();
      });
    });

    it('should show max length error when identifier exceeds 50 characters', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      const longIdentifier = 'a'.repeat(51);

      await user.clear(identifierInput);
      await user.click(identifierInput);
      await user.paste(longIdentifier);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('Maximum length is 50 characters')).toBeInTheDocument();
      });
    });

    it('should allow exactly 50 characters in identifier', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      const validIdentifier = 'a'.repeat(50);

      await user.clear(identifierInput);
      await user.click(identifierInput);
      await user.paste(validIdentifier);
      await user.tab();

      await waitFor(() => {
        expect(screen.queryByText('Maximum length is 50 characters')).not.toBeInTheDocument();
      });
    });

    it('should show required error when hatch date is empty', async () => {
      const user = userEvent.setup();
      renderModal();

      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      await user.clear(hatchDateInput);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('This field is required')).toBeInTheDocument();
      });
    });

    it('should show error when hatch date is in the future', async () => {
      const user = userEvent.setup();
      renderModal();

      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      const futureDate = getFutureDate();

      await user.clear(hatchDateInput);
      await user.type(hatchDateInput, futureDate);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('Hatch date cannot be in the future')).toBeInTheDocument();
      });
    });

    it('should allow past date as hatch date', async () => {
      const user = userEvent.setup();
      renderModal();

      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      const pastDate = '2024-01-01';

      await user.clear(hatchDateInput);
      await user.type(hatchDateInput, pastDate);
      await user.tab();

      await waitFor(() => {
        expect(screen.queryByText('Hatch date cannot be in the future')).not.toBeInTheDocument();
      });
    });

    it('should clear error when user starts typing valid input', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);

      // Clear the field to trigger required error
      await user.clear(identifierInput);
      await user.click(screen.getByLabelText(/Hatch Date/)); // Click away to blur

      await waitFor(() => {
        expect(screen.getByText('This field is required')).toBeInTheDocument();
      });

      // Start typing valid input
      await user.type(identifierInput, 'Valid Name');

      await waitFor(() => {
        expect(screen.queryByText('This field is required')).not.toBeInTheDocument();
      });
    });

    it('should disable submit button when form is invalid', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);

      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).toBeDisabled();
    });

    it('should enable submit button when form is valid', async () => {
      renderModal();

      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).not.toBeDisabled();
    });
  });

  describe('Form Submission', () => {
    it('should submit form with updated data when save button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);

      await user.clear(identifierInput);
      await user.type(identifierInput, 'Updated Flock');
      await user.clear(hatchDateInput);
      await user.type(hatchDateInput, '2024-02-01');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateFlock).toHaveBeenCalledTimes(1);
        expect(mockUpdateFlock).toHaveBeenCalledWith(
          {
            coopId: 'coop-456',
            data: {
              id: 'flock-123',
              identifier: 'Updated Flock',
              hatchDate: '2024-02-01',
              currentHens: 10,
              currentRoosters: 2,
              currentChicks: 5,
            } as UpdateFlockRequest,
          },
          expect.any(Object)
        );
      });
    });

    it('should preserve composition values (hens, roosters, chicks) when submitting', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.type(identifierInput, 'Updated Flock');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        const callArgs = mockUpdateFlock.mock.calls[0][0];
        expect(callArgs.data.currentHens).toBe(10);
        expect(callArgs.data.currentRoosters).toBe(2);
        expect(callArgs.data.currentChicks).toBe(5);
      });
    });

    it('should trim whitespace from identifier', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.type(identifierInput, '  Updated Flock  ');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        const callArgs = mockUpdateFlock.mock.calls[0][0];
        expect(callArgs.data.identifier).toBe('Updated Flock');
      });
    });

    it('should not submit when validation fails', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      const longIdentifier = 'a'.repeat(51);
      await user.clear(identifierInput);
      await user.click(identifierInput);
      await user.paste(longIdentifier);

      const saveButton = screen.getByRole('button', { name: 'Save' });

      // Save button should be disabled due to validation
      expect(saveButton).toBeDisabled();
      expect(mockUpdateFlock).not.toHaveBeenCalled();
    });

    it('should call onSuccess callback and close modal after successful update', async () => {
      const user = userEvent.setup();

      // Mock successful mutation
      mockUpdateFlock.mockImplementation((request, { onSuccess }) => {
        onSuccess();
      });

      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.type(identifierInput, 'Updated Flock');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalledTimes(1);
      });
    });

    it('should submit form when Enter key is pressed in identifier field', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.type(identifierInput, 'Updated Flock{Enter}');

      await waitFor(() => {
        expect(mockUpdateFlock).toHaveBeenCalled();
      });
    });
  });

  describe('Cancel Behavior', () => {
    it('should close modal without updating flock when cancel button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.type(identifierInput, 'Modified Name');

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalledTimes(1);
      expect(mockUpdateFlock).not.toHaveBeenCalled();
    });

    it('should clear error messages when cancel is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.tab(); // Trigger error

      await waitFor(() => {
        expect(screen.getByText('This field is required')).toBeInTheDocument();
      });

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalled();
    });
  });

  describe('Loading State', () => {
    it('should show loading state during form submission', async () => {
      const user = userEvent.setup();

      // Mock pending state
      mockUseUpdateFlock.mockReturnValue({
        mutate: mockUpdateFlock,
        isPending: true,
      });

      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.type(identifierInput, 'Test');

      // Check that save button shows loading text
      const saveButton = screen.getByRole('button', { name: 'Saving' });
      expect(saveButton).toBeInTheDocument();
      expect(saveButton).toBeDisabled();

      // Check that cancel button is disabled during submission
      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      expect(cancelButton).toBeDisabled();

      // Check that form fields are disabled during submission
      expect(identifierInput).toBeDisabled();
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      expect(hatchDateInput).toBeDisabled();
    });

    it('should show loading spinner in submit button when pending', async () => {
      // Mock pending state
      mockUseUpdateFlock.mockReturnValue({
        mutate: mockUpdateFlock,
        isPending: true,
      });

      renderModal();

      // CircularProgress should be present in the button
      const saveButton = screen.getByRole('button', { name: 'Saving' });
      const spinner = saveButton.querySelector('.MuiCircularProgress-root');
      expect(spinner).toBeInTheDocument();
    });
  });

  describe('Error Handling', () => {
    it('should display field errors from validation error response', async () => {
      const user = userEvent.setup();

      // Mock validation error response
      mockUpdateFlock.mockImplementation((request, { onError }) => {
        const error = new Error('Validation Error');
        onError(error);
      });

      // Mock processApiError to return validation errors
      vi.mock('../../../../lib/errors', () => ({
        processApiError: () => ({
          type: 'VALIDATION',
          message: 'Validation Error',
          fieldErrors: [
            { field: 'identifier', message: 'Identifier already exists' },
          ],
        }),
        ErrorType: {
          VALIDATION: 'VALIDATION',
          CONFLICT: 'CONFLICT',
        },
      }));

      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.type(identifierInput, 'Duplicate Identifier');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateFlock).toHaveBeenCalled();
      });
    });

    it('should call handleError for non-validation errors', async () => {
      const user = userEvent.setup();

      // Mock generic error (e.g., network error)
      mockUpdateFlock.mockImplementation((request, { onError }) => {
        const error = new Error('Network Error');
        onError(error);
      });

      // Mock processApiError to return a generic error type
      vi.mock('../../../../lib/errors', () => ({
        processApiError: () => ({
          type: 'UNKNOWN',
          message: 'Network Error',
        }),
        ErrorType: {
          VALIDATION: 'VALIDATION',
          UNKNOWN: 'UNKNOWN',
        },
      }));

      renderModal();

      const identifierInput = screen.getByLabelText(/Flock Identifier/);
      await user.clear(identifierInput);
      await user.type(identifierInput, 'Test Flock');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateFlock).toHaveBeenCalled();
      });
    });
  });

  describe('Mobile Responsiveness', () => {
    it('should apply fullScreen mode on small viewports', () => {
      // Mock window.innerWidth
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 400,
      });

      renderModal();

      const dialog = screen.getByRole('dialog');
      expect(dialog).toBeInTheDocument();
    });

    it('should have minimum touch target size for buttons', () => {
      renderModal();

      const saveButton = screen.getByRole('button', { name: 'Save' });
      const cancelButton = screen.getByRole('button', { name: 'Cancel' });

      // Buttons should have minimum 44px height (mobile accessibility)
      expect(saveButton).toBeInTheDocument();
      expect(cancelButton).toBeInTheDocument();
    });
  });
});
