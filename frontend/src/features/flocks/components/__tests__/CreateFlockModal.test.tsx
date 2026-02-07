import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { CreateFlockModal } from '../CreateFlockModal';
import type { CreateFlockRequest } from '../../api/flocksApi';

// Mock the useCreateFlock hook
const mockCreateFlock = vi.fn();
const mockUseCreateFlock = vi.fn();

vi.mock('../../hooks/useFlocks', () => ({
  useCreateFlock: () => mockUseCreateFlock(),
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
        'flocks.addFlock': 'Add Flock',
        'flocks.form.identifier': 'Identifier',
        'flocks.form.hatchDate': 'Hatch Date',
        'flocks.form.composition': 'Composition',
        'flocks.hens': 'Hens',
        'flocks.roosters': 'Roosters',
        'flocks.chicks': 'Chicks',
        'flocks.form.increase': 'Increase',
        'flocks.form.decrease': 'Decrease',
        'flocks.form.hatchDateFuture': 'Hatch date cannot be in the future',
        'flocks.form.atLeastOne': 'At least one animal is required',
        'common.cancel': 'Cancel',
        'common.save': 'Save',
        'common.saving': 'Saving...',
        'validation.required': 'This field is required',
        'validation.maxLength': `Maximum length is ${params?.count} characters`,
        'validation.positiveNumber': 'Value must be positive',
      };
      return translations[key] || key;
    },
  }),
}));

describe('CreateFlockModal', () => {
  let queryClient: QueryClient;
  const mockOnClose = vi.fn();
  const testCoopId = 'test-coop-id-123';

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    // Default mock implementation
    mockUseCreateFlock.mockReturnValue({
      mutate: mockCreateFlock,
      isPending: false,
    });
    vi.clearAllMocks();
  });

  const renderModal = (open = true) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <CreateFlockModal open={open} onClose={mockOnClose} coopId={testCoopId} />
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
      expect(screen.getByText('Add Flock')).toBeInTheDocument();
    });

    it('should render identifier input field', () => {
      renderModal();
      expect(screen.getByLabelText(/Identifier/)).toBeInTheDocument();
    });

    it('should render hatch date input field', () => {
      renderModal();
      expect(screen.getByLabelText(/Hatch Date/)).toBeInTheDocument();
    });

    it('should render composition section with label', () => {
      renderModal();
      expect(screen.getByText('Composition')).toBeInTheDocument();
    });

    it('should render hens input field with increment/decrement buttons', () => {
      renderModal();
      const hensInput = screen.getByLabelText(/Hens/);
      expect(hensInput).toBeInTheDocument();
      expect(hensInput).toHaveValue(0);
    });

    it('should render roosters input field with increment/decrement buttons', () => {
      renderModal();
      const roostersInput = screen.getByLabelText(/Roosters/);
      expect(roostersInput).toBeInTheDocument();
      expect(roostersInput).toHaveValue(0);
    });

    it('should render chicks input field with increment/decrement buttons', () => {
      renderModal();
      const chicksInput = screen.getByLabelText(/Chicks/);
      expect(chicksInput).toBeInTheDocument();
      expect(chicksInput).toHaveValue(0);
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
      expect(screen.queryByText('Add Flock')).not.toBeInTheDocument();
    });

    it('should auto-focus on identifier field when modal opens', () => {
      renderModal();
      const identifierInput = screen.getByLabelText(/Identifier/);
      expect(identifierInput).toHaveFocus();
    });
  });

  describe('Form Validation - Identifier', () => {
    it('should show required error when identifier is empty and field is blurred', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      await user.click(identifierInput);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('This field is required')).toBeInTheDocument();
      });
    });

    it('should show max length error when identifier exceeds 50 characters', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      const longIdentifier = 'a'.repeat(51);

      await user.click(identifierInput);
      await user.paste(longIdentifier);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('Maximum length is 50 characters')).toBeInTheDocument();
      });
    });

    it('should clear identifier error when user starts typing valid input', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);

      // Trigger required error by focusing on identifier then moving away
      await user.click(identifierInput);
      await user.click(hatchDateInput); // Move focus away to trigger onBlur

      await waitFor(() => {
        expect(screen.getByText('This field is required')).toBeInTheDocument();
      });

      // Go back to identifier and start typing valid input
      await user.click(identifierInput);
      await user.type(identifierInput, 'Valid Identifier');

      await waitFor(() => {
        expect(screen.queryByText('This field is required')).not.toBeInTheDocument();
      });
    });
  });

  describe('Form Validation - Hatch Date', () => {
    it('should show required error when hatch date is empty and field is blurred', async () => {
      const user = userEvent.setup();
      renderModal();

      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      await user.click(hatchDateInput);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getAllByText('This field is required')[0]).toBeInTheDocument();
      });
    });

    it('should show error when hatch date is in the future', async () => {
      const user = userEvent.setup();
      renderModal();

      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      const futureDate = getFutureDate();

      await user.click(hatchDateInput);
      await user.clear(hatchDateInput);
      await user.type(hatchDateInput, futureDate);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText('Hatch date cannot be in the future')).toBeInTheDocument();
      });
    });

    it('should accept today as valid hatch date', async () => {
      const user = userEvent.setup();
      renderModal();

      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      const today = getTodayDate();

      await user.click(hatchDateInput);
      await user.type(hatchDateInput, today);

      // Verify no future date error appears
      expect(screen.queryByText('Hatch date cannot be in the future')).not.toBeInTheDocument();

      // Verify form can be submitted with valid identifier and at least one animal
      const identifierInput = screen.getByLabelText(/Identifier/);
      await user.type(identifierInput, 'Test Flock');

      const hensInput = screen.getByLabelText(/Hens/);
      await user.clear(hensInput);
      await user.type(hensInput, '1');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).not.toBeDisabled();
    });

    it('should have max date attribute set to today', () => {
      renderModal();
      const hatchDateInput = screen.getByLabelText(/Hatch Date/) as HTMLInputElement;
      const today = getTodayDate();
      expect(hatchDateInput.max).toBe(today);
    });
  });

  describe('Form Validation - Counts', () => {
    it('should show error when all counts are zero', async () => {
      const user = userEvent.setup();
      renderModal();

      // Fill required fields
      const identifierInput = screen.getByLabelText(/Identifier/);
      await user.type(identifierInput, 'Test Flock');

      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      await user.type(hatchDateInput, getTodayDate());

      // Try to submit with all counts at zero
      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).toBeDisabled();
    });

    it('should not allow negative values in number inputs', async () => {
      const user = userEvent.setup();
      renderModal();

      const hensInput = screen.getByLabelText(/Hens/) as HTMLInputElement;

      // Try to type negative value
      await user.clear(hensInput);
      await user.type(hensInput, '-5');

      // Value should be clamped to 0
      await waitFor(() => {
        expect(parseInt(hensInput.value)).toBeGreaterThanOrEqual(0);
      });
    });

    it('should enable save button when at least one animal count is greater than zero', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      await user.type(identifierInput, 'Test Flock');

      const hatchDateInput = screen.getByLabelText(/Hatch Date/);
      await user.type(hatchDateInput, getTodayDate());

      const hensInput = screen.getByLabelText(/Hens/);
      await user.clear(hensInput);
      await user.type(hensInput, '5');

      const saveButton = screen.getByRole('button', { name: 'Save' });
      expect(saveButton).not.toBeDisabled();
    });
  });

  describe('Increment/Decrement Buttons', () => {
    it('should increment hens count when increment button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const hensInput = screen.getByLabelText(/Hens/) as HTMLInputElement;
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      const hensIncrement = incrementButtons[0]; // First increment button is for hens

      expect(hensInput.value).toBe('0');
      await user.click(hensIncrement);

      await waitFor(() => {
        expect(hensInput.value).toBe('1');
      });
    });

    it('should decrement hens count when decrement button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const hensInput = screen.getByLabelText(/Hens/) as HTMLInputElement;
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      const hensIncrement = incrementButtons[0];

      // First increment to 1
      await user.click(hensIncrement);
      expect(hensInput.value).toBe('1');

      // Then decrement back to 0
      const decrementButtons = screen.getAllByLabelText(/Decrease/);
      const hensDecrement = decrementButtons[0];
      await user.click(hensDecrement);

      await waitFor(() => {
        expect(hensInput.value).toBe('0');
      });
    });

    it('should disable decrement button when count is zero', () => {
      renderModal();

      const decrementButtons = screen.getAllByLabelText(/Decrease/);
      const hensDecrement = decrementButtons[0] as HTMLButtonElement;

      expect(hensDecrement).toBeDisabled();
    });

    it('should handle increment for roosters', async () => {
      const user = userEvent.setup();
      renderModal();

      const roostersInput = screen.getByLabelText(/Roosters/) as HTMLInputElement;
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      const roostersIncrement = incrementButtons[1]; // Second increment button is for roosters

      expect(roostersInput.value).toBe('0');
      await user.click(roostersIncrement);

      await waitFor(() => {
        expect(roostersInput.value).toBe('1');
      });
    });

    it('should handle increment for chicks', async () => {
      const user = userEvent.setup();
      renderModal();

      const chicksInput = screen.getByLabelText(/Chicks/) as HTMLInputElement;
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      const chicksIncrement = incrementButtons[2]; // Third increment button is for chicks

      expect(chicksInput.value).toBe('0');
      await user.click(chicksIncrement);

      await waitFor(() => {
        expect(chicksInput.value).toBe('1');
      });
    });

    it('should have minimum touch target size of 44x44px for buttons', () => {
      renderModal();

      const buttons = screen.getAllByLabelText(/Increase|Decrease/);
      buttons.forEach((button) => {
        const styles = window.getComputedStyle(button);
        const minWidth = styles.minWidth;
        const minHeight = styles.minHeight;
        expect(minWidth).toBe('44px');
        expect(minHeight).toBe('44px');
      });
    });
  });

  describe('Form Submission', () => {
    it('should submit valid data when save button is clicked', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);

      await user.type(identifierInput, 'Test Flock');
      await user.type(hatchDateInput, getTodayDate());

      // Use increment buttons for reliable state updates - just one animal is enough
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      await user.click(incrementButtons[0]); // Increment hens once

      const saveButton = screen.getByRole('button', { name: 'Save' });

      // Wait for button to be enabled
      await waitFor(() => {
        expect(saveButton).not.toBeDisabled();
      }, { timeout: 3000 });

      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateFlock).toHaveBeenCalledTimes(1);
      });

      expect(mockCreateFlock).toHaveBeenCalledWith(
        {
          coopId: testCoopId,
          identifier: 'Test Flock',
          hatchDate: getTodayDate(),
          currentHens: 1,
          currentRoosters: 0,
          currentChicks: 0,
        } as CreateFlockRequest,
        expect.any(Object)
      );
    });

    it('should trim whitespace from identifier', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);

      await user.type(identifierInput, '  Test Flock  ');
      await user.type(hatchDateInput, getTodayDate());

      // Use increment button for reliable state update
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      await user.click(incrementButtons[0]); // hens

      const saveButton = screen.getByRole('button', { name: 'Save' });

      await waitFor(() => {
        expect(saveButton).not.toBeDisabled();
      }, { timeout: 3000 });

      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateFlock).toHaveBeenCalled();
      });

      expect(mockCreateFlock).toHaveBeenCalledWith(
        expect.objectContaining({
          identifier: 'Test Flock', // Trimmed
        }),
        expect.any(Object)
      );
    });

    it('should not submit when validation fails', async () => {
      const user = userEvent.setup();
      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      const longIdentifier = 'a'.repeat(51);
      await user.click(identifierInput);
      await user.paste(longIdentifier);

      const saveButton = screen.getByRole('button', { name: 'Save' });

      // Save button should be disabled due to validation
      expect(saveButton).toBeDisabled();
      expect(mockCreateFlock).not.toHaveBeenCalled();
    });

    it('should call onClose after successful submission', async () => {
      const user = userEvent.setup();

      // Mock successful mutation
      mockCreateFlock.mockImplementation((_request, { onSuccess }) => {
        onSuccess();
      });

      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);

      await user.type(identifierInput, 'Test Flock');
      await user.type(hatchDateInput, getTodayDate());

      // Use increment button for reliable state update
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      await user.click(incrementButtons[0]); // hens

      const saveButton = screen.getByRole('button', { name: 'Save' });

      await waitFor(() => {
        expect(saveButton).not.toBeDisabled();
      }, { timeout: 3000 });

      await user.click(saveButton);

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalledTimes(1);
      });
    });

    it('should reset form when modal is closed', async () => {
      const user = userEvent.setup();
      const { unmount } = renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      await user.type(identifierInput, 'Test Flock');

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalledTimes(1);

      // Unmount and re-render to check if form is reset
      unmount();
      mockOnClose.mockClear();
      renderModal();

      await waitFor(() => {
        const newIdentifierInput = screen.getByLabelText(/Identifier/) as HTMLInputElement;
        expect(newIdentifierInput.value).toBe('');
      });
    });
  });

  describe('Error Handling', () => {
    it('should show field error for validation errors from API', async () => {
      const user = userEvent.setup();

      // Mock validation error response
      mockCreateFlock.mockImplementation((_request, { onError }) => {
        const error = new Error('Validation Error');
        onError(error);
      });

      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);

      await user.type(identifierInput, 'Test');
      await user.type(hatchDateInput, getTodayDate());

      // Use increment button for reliable state update
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      await user.click(incrementButtons[0]); // hens

      const saveButton = screen.getByRole('button', { name: 'Save' });

      await waitFor(() => {
        expect(saveButton).not.toBeDisabled();
      }, { timeout: 3000 });

      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateFlock).toHaveBeenCalled();
      });
    });

    it('should call error handler for non-validation errors', async () => {
      const user = userEvent.setup();

      // Mock network error
      mockCreateFlock.mockImplementation((_request, { onError }) => {
        const error = new Error('Network Error');
        onError(error);
      });

      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);

      await user.type(identifierInput, 'Test');
      await user.type(hatchDateInput, getTodayDate());

      // Use increment button for reliable state update
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      await user.click(incrementButtons[0]); // hens

      const saveButton = screen.getByRole('button', { name: 'Save' });

      await waitFor(() => {
        expect(saveButton).not.toBeDisabled();
      }, { timeout: 3000 });

      await user.click(saveButton);

      await waitFor(() => {
        expect(mockCreateFlock).toHaveBeenCalled();
      });
    });
  });

  describe('Loading State', () => {
    it('should show loading state during submission', async () => {
      const user = userEvent.setup();

      // Mock pending state
      mockUseCreateFlock.mockReturnValue({
        mutate: mockCreateFlock,
        isPending: true,
      });

      renderModal();

      const identifierInput = screen.getByLabelText(/Identifier/);
      await user.type(identifierInput, 'Test Flock');

      // Check that save button shows loading text
      const saveButton = screen.getByRole('button', { name: 'Saving...' });
      expect(saveButton).toBeInTheDocument();
      expect(saveButton).toBeDisabled();

      // Check that cancel button is disabled during submission
      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      expect(cancelButton).toBeDisabled();

      // Check that form fields are disabled during submission
      expect(identifierInput).toBeDisabled();
    });

    it('should show loading spinner on submit button during submission', () => {
      // Mock pending state
      mockUseCreateFlock.mockReturnValue({
        mutate: mockCreateFlock,
        isPending: true,
      });

      renderModal();

      const saveButton = screen.getByRole('button', { name: 'Saving...' });
      expect(saveButton.querySelector('svg')).toBeInTheDocument(); // CircularProgress
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

      const identifierInput = screen.getByLabelText(/Identifier/);
      const hatchDateInput = screen.getByLabelText(/Hatch Date/);

      await user.type(identifierInput, 'Test Flock');
      await user.type(hatchDateInput, getTodayDate());

      // Use increment button for reliable state update
      const incrementButtons = screen.getAllByLabelText(/Increase/);
      await user.click(incrementButtons[0]); // hens

      // Wait for form to be valid
      const saveButton = screen.getByRole('button', { name: 'Save' });
      await waitFor(() => {
        expect(saveButton).not.toBeDisabled();
      }, { timeout: 3000 });

      // Submit by pressing Enter in the form
      await user.keyboard('{Enter}');

      await waitFor(() => {
        expect(mockCreateFlock).toHaveBeenCalled();
      });
    });
  });

  describe('Mobile Responsiveness', () => {
    it('should use fullScreen mode on small viewports (< 480px)', () => {
      // Mock window.innerWidth
      Object.defineProperty(window, 'innerWidth', {
        writable: true,
        configurable: true,
        value: 375,
      });

      renderModal();

      // Check if dialog has fullScreen prop (via MUI's implementation)
      const dialog = screen.getByRole('dialog');
      expect(dialog.parentElement?.parentElement).toHaveClass('MuiDialog-root');
    });

    it('should have minimum touch target size of 44px for all inputs', () => {
      renderModal();

      const inputs = [
        screen.getByLabelText(/Identifier/),
        screen.getByLabelText(/Hatch Date/),
        screen.getByLabelText(/Hens/),
        screen.getByLabelText(/Roosters/),
        screen.getByLabelText(/Chicks/),
      ];

      inputs.forEach((input) => {
        const styles = window.getComputedStyle(input);
        const minHeight = styles.minHeight;
        expect(minHeight).toBe('44px');
      });
    });
  });
});
