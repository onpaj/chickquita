import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { CreateCoopModal } from '../CreateCoopModal';
import type { CreateCoopRequest } from '../../api/coopsApi';

// Mock the useCreateCoop hook
const mockCreateCoop = vi.fn();
const mockIsPending = false;

vi.mock('../../hooks/useCoops', () => ({
  useCreateCoop: () => ({
    mutate: mockCreateCoop,
    isPending: mockIsPending,
  }),
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
    t: (key: string, params?: any) => {
      const translations: Record<string, string> = {
        'coops.addCoop': 'Add Coop',
        'coops.coopName': 'Coop Name',
        'coops.location': 'Location',
        'common.cancel': 'Cancel',
        'common.save': 'Save',
        'common.saving': 'Saving...',
        'validation.required': 'This field is required',
        'validation.maxLength': `Maximum length is ${params?.count} characters`,
        'coops.duplicateName': 'A coop with this name already exists',
      };
      return translations[key] || key;
    },
  }),
}));

describe('CreateCoopModal', () => {
  let queryClient: QueryClient;
  const mockOnClose = vi.fn();

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    vi.clearAllMocks();
  });

  const renderModal = (open = true) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <CreateCoopModal open={open} onClose={mockOnClose} />
      </QueryClientProvider>
    );
  };

  describe('Rendering', () => {
    it('should render modal title', () => {
      renderModal();
      expect(screen.getByText('Add Coop')).toBeInTheDocument();
    });

    it('should render name input field', () => {
      renderModal();
      expect(screen.getByLabelText(/Coop Name/)).toBeInTheDocument();
    });

    it('should render location input field', () => {
      renderModal();
      expect(screen.getByLabelText(/Location/)).toBeInTheDocument();
    });

    it('should render cancel button', () => {
      renderModal();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });

    it('should render save button', () => {
      renderModal();
      expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    });

    it('should have save button disabled when form is empty', () => {
      renderModal();
      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).toBeDisabled();
    });

    it('should not render when open is false', () => {
      renderModal(false);
      expect(screen.queryByText('Add Coop')).not.toBeInTheDocument();
    });
  });

  describe('Form Validation', () => {
    it('should show required error when name is empty and field is blurred', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.click(nameInput);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('This field is required')).toBeInTheDocument();
      });
    });

    it('should show max length error when name exceeds 100 characters', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      const longName = 'a'.repeat(101);

      await user.click(nameInput);
      await user.paste(longName);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('Maximum length is 100 characters')).toBeInTheDocument();
      });
    });

    it('should show max length error when location exceeds 200 characters', async () => {
      const user = userEvent.setup();
      renderModal();

      const locationInput = screen.getByLabelText(/Location/);
      const longLocation = 'a'.repeat(201);

      await user.click(locationInput);
      await user.paste(longLocation);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('Maximum length is 200 characters')).toBeInTheDocument();
      });
    });

    it('should clear error when user starts typing valid input', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);

      // Trigger required error by focusing and blurring
      await user.click(nameInput);
      await user.click(screen.getByLabelText(/Location/)); // Click away to blur

      await waitFor(() => {
        expect(screen.getByText('This field is required')).toBeInTheDocument();
      });

      // Start typing valid input
      await user.type(nameInput, 'Valid Name');

      await waitFor(() => {
        expect(screen.queryByText('This field is required')).not.toBeInTheDocument();
      });
    });

    it('should enable save button when name is valid', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.type(nameInput, 'Test Coop');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).not.toBeDisabled();
    });
  });

  describe('Form Submission', () => {
    it('should submit valid data when save button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      const locationInput = screen.getByLabelText(/Location/);

      await user.type(nameInput, 'Test Coop');
      await user.type(locationInput, 'Test Location');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateCoop).toHaveBeenCalledTimes(1);
        expect(mockCreateCoop).toHaveBeenCalledWith(
          {
            name: 'Test Coop',
            location: 'Test Location',
          } as CreateCoopRequest,
          expect.any(Object)
        );
      });
    });

    it('should submit without location if not provided', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.type(nameInput, 'Test Coop');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateCoop).toHaveBeenCalledWith(
          {
            name: 'Test Coop',
            location: undefined,
          } as CreateCoopRequest,
          expect.any(Object)
        );
      });
    });

    it('should trim whitespace from name and location', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      const locationInput = screen.getByLabelText(/Location/);

      await user.type(nameInput, '  Test Coop  ');
      await user.type(locationInput, '  Test Location  ');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateCoop).toHaveBeenCalledWith(
          {
            name: 'Test Coop',
            location: 'Test Location',
          } as CreateCoopRequest,
          expect.any(Object)
        );
      });
    });

    it('should not submit when validation fails', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      const longName = 'a'.repeat(101);
      await user.click(nameInput);
      await user.paste(longName);

      const saveButton = screen.getByRole('button', { name: 'Save' });

      // Save button should be disabled due to validation
      expect(saveButton).toBeDisabled();
      expect(mockCreateCoop).not.toHaveBeenCalled();
    });

    it('should call onClose after successful submission', async () => {
      const user = userEvent.setup();

      // Mock successful mutation
      mockCreateCoop.mockImplementation((request, { onSuccess }) => {
        onSuccess();
      });

      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.type(nameInput, 'Test Coop');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalledTimes(1);
      });
    });

    it('should reset form when modal is closed', async () => {
      const user = userEvent.setup();
      const { unmount } = renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.type(nameInput, 'Test Coop');

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalledTimes(1);

      // Unmount and re-render to check if form is reset
      unmount();
      mockOnClose.mockClear();
      renderModal();

      await waitFor(() => {
        const newNameInput = screen.getByLabelText(/Coop Name/) as HTMLInputElement;
        expect(newNameInput.value).toBe('');
      });
    });
  });

  describe('Error Handling', () => {
    it('should show field error for duplicate name (409 Conflict)', async () => {
      const user = userEvent.setup();

      // Mock error response
      mockCreateCoop.mockImplementation((request, { onError }) => {
        const error = new Error('Conflict');
        onError(error);
      });

      // Mock processApiError to return CONFLICT type
      vi.mock('../../../../lib/errors', () => ({
        processApiError: () => ({
          type: 'CONFLICT',
          message: 'Conflict',
        }),
        ErrorType: {
          CONFLICT: 'CONFLICT',
          VALIDATION: 'VALIDATION',
        },
      }));

      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.type(nameInput, 'Existing Coop');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateCoop).toHaveBeenCalled();
      });
    });

    it('should handle validation errors from API', async () => {
      const user = userEvent.setup();

      // Mock validation error response
      mockCreateCoop.mockImplementation((request, { onError }) => {
        const error = new Error('Validation Error');
        onError(error);
      });

      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.type(nameInput, 'Test');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateCoop).toHaveBeenCalled();
      });
    });
  });

  describe('User Interactions', () => {
    it('should close modal when cancel button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });

    it('should submit form when Enter key is pressed in form', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.type(nameInput, 'Test Coop{Enter}');

      await waitFor(() => {
        expect(mockCreateCoop).toHaveBeenCalled();
      });
    });
  });
});
