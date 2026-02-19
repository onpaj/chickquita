/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { describe, it, expect, vi, beforeAll, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { I18nextProvider } from 'react-i18next';
import { QuickAddModal } from '../QuickAddModal';
import i18n from '../../../../lib/i18n';
import * as useDailyRecordsHook from '../../hooks/useDailyRecords';
import * as useErrorHandlerHook from '../../../../hooks/useErrorHandler';

// Mock the hooks
vi.mock('../../hooks/useDailyRecords');
vi.mock('../../../../hooks/useErrorHandler');

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};

  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => {
      store[key] = value.toString();
    },
    removeItem: (key: string) => {
      delete store[key];
    },
    clear: () => {
      store = {};
    },
  };
})();

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

const mockFlocks = [
  { id: 'flock-1', identifier: 'Red Hens', coopName: 'Main Coop' },
  { id: 'flock-2', identifier: 'Brown Chickens', coopName: 'Side Coop' },
  { id: 'flock-3', identifier: 'White Roosters', coopName: 'Main Coop' },
];

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <I18nextProvider i18n={i18n}>
        {children}
      </I18nextProvider>
    </QueryClientProvider>
  );
};

describe('QuickAddModal', () => {
  const mockOnClose = vi.fn();
  const mockMutate = vi.fn();
  const mockHandleError = vi.fn();

  beforeAll(async () => {
    // Ensure i18n uses Czech (cs) language for all tests in this file.
    // The LanguageDetector may pick up 'en' from navigator in jsdom,
    // but the tests expect Czech translations.
    await i18n.changeLanguage('cs');
  });

  beforeEach(() => {
    // Reset mocks
    vi.clearAllMocks();
    localStorageMock.clear();

    // Mock the hooks
    vi.spyOn(useDailyRecordsHook, 'useCreateDailyRecord').mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    } as any);

    vi.spyOn(useErrorHandlerHook, 'useErrorHandler').mockReturnValue({
      handleError: mockHandleError,
    } as any);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Rendering', () => {
    it('should render the modal when open', () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.getByText('Rychlý záznam vajec')).toBeInTheDocument();
      expect(screen.getByLabelText(/hejno/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/datum/i)).toBeInTheDocument();
      expect(screen.getByText('Počet vajec')).toBeInTheDocument();
      expect(screen.getByLabelText(/poznámky/i)).toBeInTheDocument();
    });

    it('should not render when closed', () => {
      render(
        <QuickAddModal
          open={false}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.queryByText('Rychlý záznam vajec')).not.toBeInTheDocument();
    });

    it('should render all available flocks in the select', () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const flockSelect = screen.getByLabelText(/hejno/i);
      expect(flockSelect).toBeInTheDocument();
    });
  });

  describe('Auto-focus', () => {
    it('should auto-focus on egg count field when modal opens', async () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        // Check if any input with aria-label="egg count" has focus
        const eggCountInput = screen.getByLabelText('egg count increase').parentElement?.parentElement?.querySelector('input[type="number"]');
        expect(document.activeElement).toBe(eggCountInput);
      }, { timeout: 200 });
    });
  });

  describe('Last-used flock memory', () => {
    it('should remember the last used flock from localStorage', () => {
      localStorageMock.setItem('chickquita_lastUsedFlockId', 'flock-2');

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const flockSelect = screen.getByLabelText(/hejno/i) as HTMLInputElement;
      expect(flockSelect.value).toBe('flock-2');
    });

    it('should use defaultFlockId when provided', () => {
      localStorageMock.setItem('chickquita_lastUsedFlockId', 'flock-2');

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
          defaultFlockId="flock-3"
        />,
        { wrapper: createWrapper() }
      );

      const flockSelect = screen.getByLabelText(/hejno/i) as HTMLInputElement;
      expect(flockSelect.value).toBe('flock-3');
    });

    it('should use first flock if no saved flock exists', () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const flockSelect = screen.getByLabelText(/hejno/i) as HTMLInputElement;
      expect(flockSelect.value).toBe('flock-1');
    });

    it('should save selected flock to localStorage on successful submission', async () => {
      const user = userEvent.setup();
      mockMutate.mockImplementation((_data, { onSuccess }) => {
        onSuccess();
      });

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      // Select a different flock
      const flockSelect = screen.getByLabelText(/hejno/i);
      await user.click(flockSelect);
      const flock2Option = screen.getByText('Brown Chickens (Side Coop)');
      await user.click(flock2Option);

      // Submit the form
      const saveButton = screen.getByRole('button', { name: /uložit/i });
      await user.click(saveButton);

      await waitFor(() => {
        expect(localStorageMock.getItem('chickquita_lastUsedFlockId')).toBe('flock-2');
      });
    });
  });

  describe('Form validation', () => {
    it('should validate required flock field', async () => {
      const user = userEvent.setup();

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={[]}
        />,
        { wrapper: createWrapper() }
      );

      const saveButton = screen.getByRole('button', { name: /uložit/i });
      expect(saveButton).toBeDisabled();
    });

    it('should validate egg count is non-negative', async () => {
      const user = userEvent.setup();

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      // Try to set negative egg count
      const decrementButton = screen.getByLabelText('egg count decrease');

      // Egg count should start at 0 and decrement button should be disabled
      expect(decrementButton).toBeDisabled();
    });

    it('should validate date is not in the future', async () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const dateInput = screen.getByLabelText(/datum/i);

      // Build tomorrow's date using local time to avoid UTC timezone mismatch
      // (toISOString() returns UTC which can differ from local date in UTC+ zones)
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 1);
      const year = futureDate.getFullYear();
      const month = String(futureDate.getMonth() + 1).padStart(2, '0');
      const day = String(futureDate.getDate()).padStart(2, '0');
      const futureDateStr = `${year}-${month}-${day}`;

      // Use fireEvent instead of userEvent.type - date inputs in jsdom don't support
      // character-by-character typing (same pattern as notes validation test above)
      fireEvent.change(dateInput, { target: { value: futureDateStr } });
      fireEvent.blur(dateInput);

      await waitFor(() => {
        expect(screen.getByText(/nemůže být v budoucnosti/i)).toBeInTheDocument();
      });
    });

    it('should validate notes max length (500 characters)', async () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const notesInput = screen.getByLabelText(/poznámky/i);
      const longText = 'a'.repeat(501);

      // Use fireEvent.change instead of userEvent.type to avoid timeout with 501 characters
      fireEvent.change(notesInput, { target: { value: longText } });
      fireEvent.blur(notesInput);

      await waitFor(() => {
        expect(screen.getByText(/maximální délka/i)).toBeInTheDocument();
      });
    });

    it('should display character count for notes', () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.getByText(/0\/500 znaků/i)).toBeInTheDocument();
    });
  });

  describe('Form submission', () => {
    it('should submit form with valid data', async () => {
      const user = userEvent.setup();
      mockMutate.mockImplementation((_data, { onSuccess }) => {
        onSuccess();
      });

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      // Wait for auto-focus to complete (100ms timer moves focus to egg count input)
      await waitFor(() => {
        const eggCountInput = screen.getByLabelText('egg count increase')
          .parentElement?.parentElement?.querySelector('input[type="number"]');
        expect(document.activeElement).toBe(eggCountInput);
      }, { timeout: 200 });

      // Fill in the form
      const incrementButton = screen.getByLabelText('egg count increase');
      await user.click(incrementButton);
      await user.click(incrementButton);
      await user.click(incrementButton); // Set egg count to 3

      // Click the notes input to ensure focus before typing (avoid auto-focus race condition)
      const notesInput = screen.getByLabelText(/poznámky/i);
      await user.click(notesInput);
      await user.type(notesInput, 'Test notes');

      // Submit
      const saveButton = screen.getByRole('button', { name: /uložit/i });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockMutate).toHaveBeenCalledWith(
          expect.objectContaining({
            flockId: 'flock-1',
            data: expect.objectContaining({
              eggCount: 3,
              notes: 'Test notes',
            }),
          }),
          expect.any(Object)
        );
      });
    });

    it('should close modal on successful submission', async () => {
      const user = userEvent.setup();
      mockMutate.mockImplementation((_data, { onSuccess }) => {
        onSuccess();
      });

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const saveButton = screen.getByRole('button', { name: /uložit/i });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockOnClose).toHaveBeenCalled();
      });
    });

    it('should reset form after closing', async () => {
      const user = userEvent.setup();

      const { rerender } = render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      // Fill in some data
      const incrementButton = screen.getByLabelText('egg count increase');
      await user.click(incrementButton);

      const notesInput = screen.getByLabelText(/poznámky/i);
      await user.type(notesInput, 'Test notes');

      // Close modal
      const cancelButton = screen.getByRole('button', { name: /zrušit/i });
      await user.click(cancelButton);

      // Reopen modal
      rerender(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />
      );

      // Check that form is reset
      const notesInputAfter = screen.getByLabelText(/poznámky/i) as HTMLTextAreaElement;
      expect(notesInputAfter.value).toBe('');
    });

    it('should handle submission error with field errors', async () => {
      const user = userEvent.setup();
      const fieldError = {
        type: 'VALIDATION_ERROR' as const,
        fieldErrors: [
          { field: 'eggCount', message: 'Invalid egg count' },
        ],
      };

      mockMutate.mockImplementation((_data, { onError }) => {
        onError(fieldError as any);
      });

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const saveButton = screen.getByRole('button', { name: /uložit/i });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockMutate).toHaveBeenCalled();
      });
    });

    it('should handle generic submission errors', async () => {
      const user = userEvent.setup();
      const networkError = new Error('Network error');

      mockMutate.mockImplementation((_data, { onError }) => {
        onError(networkError);
      });

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const saveButton = screen.getByRole('button', { name: /uložit/i });
      await user.click(saveButton);

      await waitFor(() => {
        expect(mockHandleError).toHaveBeenCalled();
      });
    });
  });

  describe('Mobile responsive', () => {
    it('should render in fullScreen mode on mobile viewport', () => {
      // Mock mobile viewport
      window.innerWidth = 400;
      window.dispatchEvent(new Event('resize'));

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      // MUI Dialog renders via a portal into document.body, not into container
      const dialog = document.querySelector('.MuiDialog-root');
      expect(dialog).toBeInTheDocument();
    });
  });

  describe('Disabled states', () => {
    it('should disable all inputs when form is submitting', () => {
      vi.spyOn(useDailyRecordsHook, 'useCreateDailyRecord').mockReturnValue({
        mutate: mockMutate,
        isPending: true,
      } as any);

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const flockSelect = screen.getByLabelText(/hejno/i);
      const dateInput = screen.getByLabelText(/datum/i);
      const notesInput = screen.getByLabelText(/poznámky/i);
      const saveButton = screen.getByRole('button', { name: /ukládám/i });
      const cancelButton = screen.getByRole('button', { name: /zrušit/i });

      expect(flockSelect).toBeDisabled();
      expect(dateInput).toBeDisabled();
      expect(notesInput).toBeDisabled();
      expect(saveButton).toBeDisabled();
      expect(cancelButton).toBeDisabled();
    });

    it('should show loading indicator when submitting', () => {
      vi.spyOn(useDailyRecordsHook, 'useCreateDailyRecord').mockReturnValue({
        mutate: mockMutate,
        isPending: true,
      } as any);

      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      expect(screen.getByRole('button', { name: /ukládám/i })).toBeInTheDocument();
    });
  });

  describe('Default values', () => {
    it('should set today as default date', () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const dateInput = screen.getByLabelText(/datum/i) as HTMLInputElement;
      const today = new Date().toISOString().split('T')[0];
      expect(dateInput.value).toBe(today);
    });

    it('should set egg count to 0 by default', () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const eggCountInput = screen.getByLabelText('egg count increase').parentElement?.parentElement?.querySelector('input[type="number"]') as HTMLInputElement;
      expect(eggCountInput?.value).toBe('0');
    });

    it('should have empty notes by default', () => {
      render(
        <QuickAddModal
          open={true}
          onClose={mockOnClose}
          flocks={mockFlocks}
        />,
        { wrapper: createWrapper() }
      );

      const notesInput = screen.getByLabelText(/poznámky/i) as HTMLTextAreaElement;
      expect(notesInput.value).toBe('');
    });
  });
});
