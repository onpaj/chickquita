import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { EditCoopModal } from '../EditCoopModal';
import type { Coop, UpdateCoopRequest } from '../../api/coopsApi';

// Mock the useUpdateCoop hook
const mockUpdateCoop = vi.fn();
const mockUseUpdateCoop = vi.fn();

vi.mock('../../hooks/useCoops', () => ({
  useUpdateCoop: () => mockUseUpdateCoop(),
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
        'coops.editCoop': 'Edit Coop',
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

describe('EditCoopModal', () => {
  let queryClient: QueryClient;
  const mockOnClose = vi.fn();

  // Mock coop data for testing
  const mockCoop: Coop = {
    id: 'coop-123',
    tenantId: 'tenant-123',
    name: 'Existing Coop',
    location: 'Existing Location',
    isActive: true,
    flocksCount: 0,
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
    mockUseUpdateCoop.mockReturnValue({
      mutate: mockUpdateCoop,
      isPending: false,
    });
    vi.clearAllMocks();
  });

  const renderModal = (open = true, coop = mockCoop) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <EditCoopModal open={open} onClose={mockOnClose} coop={coop} />
      </QueryClientProvider>
    );
  };

  describe('Rendering', () => {
    it('should render modal title', () => {
      renderModal();
      expect(screen.getByText('Edit Coop')).toBeInTheDocument();
    });

    it('should render modal when open prop is true', () => {
      renderModal(true);
      expect(screen.getByText('Edit Coop')).toBeInTheDocument();
      expect(screen.getByLabelText(/Coop Name/)).toBeInTheDocument();
    });

    it('should not render when open is false', () => {
      renderModal(false);
      expect(screen.queryByText('Edit Coop')).not.toBeInTheDocument();
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
  });

  describe('Form Pre-fill', () => {
    it('should pre-fill form with existing coop data', () => {
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/) as HTMLInputElement;
      const locationInput = screen.getByLabelText(/Location/) as HTMLTextAreaElement;

      expect(nameInput.value).toBe('Existing Coop');
      expect(locationInput.value).toBe('Existing Location');
    });

    it('should pre-fill form with coop data when location is null', () => {
      const coopWithoutLocation: Coop = {
        ...mockCoop,
        location: undefined,
      };

      renderModal(true, coopWithoutLocation);

      const nameInput = screen.getByLabelText(/Coop Name/) as HTMLInputElement;
      const locationInput = screen.getByLabelText(/Location/) as HTMLTextAreaElement;

      expect(nameInput.value).toBe('Existing Coop');
      expect(locationInput.value).toBe('');
    });
  });

  describe('Form Validation', () => {
    it('should show required error when name is empty and field is blurred', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.clear(nameInput);
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

      await user.clear(nameInput);
      await user.click(nameInput);
      await user.paste(longName);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('Maximum length is 100 characters')).toBeInTheDocument();
      });
    });

    it('should validate name field max length correctly', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);

      // Clear existing value
      await user.clear(nameInput);

      // Test with exactly 100 characters (should be valid)
      const validName = 'a'.repeat(100);
      await user.click(nameInput);
      await user.paste(validName);
      await user.tab();

      await waitFor(() => {
        expect(screen.queryByText('Maximum length is 100 characters')).not.toBeInTheDocument();
      });
    });

    it('should show max length error when location exceeds 200 characters', async () => {
      const user = userEvent.setup();
      renderModal();

      const locationInput = screen.getByLabelText(/Location/);
      const longLocation = 'a'.repeat(201);

      await user.clear(locationInput);
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

      // Clear the field to trigger required error
      await user.clear(nameInput);
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

    it('should have location field as optional', async () => {
      const user = userEvent.setup();
      renderModal();

      const locationInput = screen.getByLabelText(/Location/);
      await user.clear(locationInput);

      // Save button should still be enabled if name is valid
      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).not.toBeDisabled();
    });
  });

  describe('Location Field', () => {
    it('should have correct label "Location" (not "coopDescription")', () => {
      renderModal();

      // Check that the label is "Location" and not "coopDescription"
      const locationInput = screen.getByLabelText(/Location/);
      expect(locationInput).toBeInTheDocument();

      // Verify "coopDescription" is not used
      expect(screen.queryByLabelText(/coopDescription/i)).not.toBeInTheDocument();
      expect(screen.queryByLabelText(/Description/i)).not.toBeInTheDocument();
    });

    it('should allow location field to be empty (optional)', async () => {
      const user = userEvent.setup();
      renderModal();

      const locationInput = screen.getByLabelText(/Location/);
      await user.clear(locationInput);

      expect(locationInput).toHaveValue('');

      // Save button should be enabled
      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).not.toBeDisabled();
    });
  });

  describe('Form Submission', () => {
    it('should submit form with updated data when save button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      const locationInput = screen.getByLabelText(/Location/);

      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Coop');
      await user.clear(locationInput);
      await user.type(locationInput, 'Updated Location');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateCoop).toHaveBeenCalledTimes(1);
        expect(mockUpdateCoop).toHaveBeenCalledWith(
          {
            id: 'coop-123',
            name: 'Updated Coop',
            location: 'Updated Location',
          } as UpdateCoopRequest,
          expect.any(Object)
        );
      });
    });

    it('should submit without location if cleared', async () => {
      const user = userEvent.setup();
      renderModal();

      const locationInput = screen.getByLabelText(/Location/);
      await user.clear(locationInput);

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateCoop).toHaveBeenCalledWith(
          {
            id: 'coop-123',
            name: 'Existing Coop',
            location: undefined,
          } as UpdateCoopRequest,
          expect.any(Object)
        );
      });
    });

    it('should trim whitespace from name and location', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      const locationInput = screen.getByLabelText(/Location/);

      await user.clear(nameInput);
      await user.type(nameInput, '  Updated Coop  ');
      await user.clear(locationInput);
      await user.type(locationInput, '  Updated Location  ');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateCoop).toHaveBeenCalledWith(
          {
            id: 'coop-123',
            name: 'Updated Coop',
            location: 'Updated Location',
          } as UpdateCoopRequest,
          expect.any(Object)
        );
      });
    });

    it('should not submit when validation fails', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      const longName = 'a'.repeat(101);
      await user.clear(nameInput);
      await user.click(nameInput);
      await user.paste(longName);

      const saveButton = screen.getByRole('button', { name: 'Save' });

      // Save button should be disabled due to validation
      expect(saveButton).toBeDisabled();
      expect(mockUpdateCoop).not.toHaveBeenCalled();
    });

    it('should call onSuccess callback after successful update', async () => {
      const user = userEvent.setup();

      // Mock successful mutation
      mockUpdateCoop.mockImplementation((request, { onSuccess }) => {
        onSuccess();
      });

      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Coop');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalledTimes(1);
      });
    });

    it('should submit form when Enter key is pressed in form', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Coop{Enter}');

      await waitFor(() => {
        expect(mockUpdateCoop).toHaveBeenCalled();
      });
    });
  });

  describe('Cancel Behavior', () => {
    it('should close modal without updating coop when cancel button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.clear(nameInput);
      await user.type(nameInput, 'Modified Name');

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalledTimes(1);
      expect(mockUpdateCoop).not.toHaveBeenCalled();
    });
  });

  describe('Loading State', () => {
    it('should show loading state during form submission', async () => {
      const user = userEvent.setup();

      // Mock pending state
      mockUseUpdateCoop.mockReturnValue({
        mutate: mockUpdateCoop,
        isPending: true,
      });

      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.type(nameInput, 'Test');

      // Check that save button shows loading text
      const saveButton = screen.getByRole('button', { name: 'Saving...' });
      expect(saveButton).toBeInTheDocument();
      expect(saveButton).toBeDisabled();

      // Check that cancel button is disabled during submission
      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      expect(cancelButton).toBeDisabled();

      // Check that form fields are disabled during submission
      expect(nameInput).toBeDisabled();
      const locationInput = screen.getByLabelText(/Location/);
      expect(locationInput).toBeDisabled();
    });
  });

  describe('Error Handling', () => {
    it('should show error message on API failure (duplicate name)', async () => {
      const user = userEvent.setup();

      // Mock error response
      mockUpdateCoop.mockImplementation((request, { onError }) => {
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
      await user.clear(nameInput);
      await user.type(nameInput, 'Duplicate Name');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateCoop).toHaveBeenCalled();
      });
    });

    it('should handle validation errors from API', async () => {
      const user = userEvent.setup();

      // Mock validation error response
      mockUpdateCoop.mockImplementation((request, { onError }) => {
        const error = new Error('Validation Error');
        onError(error);
      });

      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.clear(nameInput);
      await user.type(nameInput, 'Test');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateCoop).toHaveBeenCalled();
      });
    });

    it('should call handleError for non-field errors', async () => {
      const user = userEvent.setup();

      // Mock generic error (e.g., network error)
      mockUpdateCoop.mockImplementation((request, { onError }) => {
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
          CONFLICT: 'CONFLICT',
          VALIDATION: 'VALIDATION',
          UNKNOWN: 'UNKNOWN',
        },
      }));

      renderModal();

      const nameInput = screen.getByLabelText(/Coop Name/);
      await user.clear(nameInput);
      await user.type(nameInput, 'Test Coop');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateCoop).toHaveBeenCalled();
      });
    });
  });
});
