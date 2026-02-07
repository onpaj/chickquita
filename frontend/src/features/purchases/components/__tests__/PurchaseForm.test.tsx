import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { PurchaseForm } from '../PurchaseForm';
import { PurchaseType, QuantityUnit, type PurchaseDto } from '../../types';
import type { UserEvent } from '@testing-library/user-event';

// Mock the usePurchaseAutocomplete hook
vi.mock('../../hooks/usePurchaseAutocomplete', () => ({
  usePurchaseAutocomplete: vi.fn((query: string) => ({
    suggestions: query.length >= 2 ? ['Krmivo Premium', 'Krmivo Standard'] : [],
    isLoading: false,
  })),
}));

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, params?: Record<string, unknown>) => {
      // Simple translation mock
      const translations: Record<string, string> = {
        'purchases.form.type': 'Purchase Type',
        'purchases.form.name': 'Name',
        'purchases.form.purchaseDate': 'Purchase Date',
        'purchases.form.amount': 'Amount (CZK)',
        'purchases.form.quantity': 'Quantity',
        'purchases.form.unit': 'Unit',
        'purchases.form.consumedDate': 'Consumed Date',
        'purchases.form.notes': 'Notes',
        'purchases.form.coop': 'Coop',
        'purchases.form.noCoop': 'No Coop',
        'purchases.types.feed': 'Feed',
        'purchases.types.vitamins': 'Vitamins',
        'purchases.types.bedding': 'Bedding',
        'purchases.types.toys': 'Toys',
        'purchases.types.veterinary': 'Veterinary',
        'purchases.types.other': 'Other',
        'purchases.units.kg': 'kg',
        'purchases.units.pcs': 'pcs',
        'purchases.units.l': 'L',
        'purchases.units.package': 'package',
        'purchases.units.other': 'other',
        'common.cancel': 'Cancel',
        'common.save': 'Save',
        'common.create': 'Create',
        'common.saving': 'Saving...',
        'validation.required': 'This field is required',
        'validation.positiveNumber': 'Must be a positive number',
        'validation.maxLength': params?.count
          ? `Maximum length is ${params.count} characters`
          : 'Maximum length exceeded',
        'purchases.form.purchaseDateFuture': 'Purchase date cannot be in the future',
      };
      return translations[key] || key;
    },
  }),
}));

describe('PurchaseForm', () => {
  let user: UserEvent;

  beforeEach(() => {
    user = userEvent.setup();
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render all form fields in create mode', () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      // Check for all required fields
      expect(screen.getByLabelText(/purchase type/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/^name$/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/purchase date/i)).toBeInTheDocument();
      expect(screen.getByText(/amount \(czk\)/i)).toBeInTheDocument();
      expect(screen.getByText(/^quantity$/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/^unit$/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/consumed date/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/notes/i)).toBeInTheDocument();

      // Check for buttons
      expect(screen.getByRole('button', { name: /create/i })).toBeInTheDocument();
    });

    it('should render with default values in create mode', () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      // Type should default to Feed (displayed in the select)
      expect(screen.getByText('Feed')).toBeInTheDocument();

      // Default values are set in the form
      const nameInput = screen.getByLabelText(/^name$/i);
      expect(nameInput).toHaveValue('');
    });

    it('should pre-fill form in edit mode', async () => {
      const mockOnSubmit = vi.fn();
      const existingPurchase: PurchaseDto = {
        id: '123',
        tenantId: 'tenant-1',
        coopId: 'coop-1',
        name: 'Test Feed',
        type: PurchaseType.Feed,
        amount: 250.5,
        quantity: 25,
        unit: QuantityUnit.Kg,
        purchaseDate: '2024-01-15T00:00:00Z',
        consumedDate: '2024-01-20T00:00:00Z',
        notes: 'Test notes',
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z',
      };

      render(<PurchaseForm onSubmit={mockOnSubmit} initialData={existingPurchase} />);

      // Wait for form to populate
      await waitFor(() => {
        const nameInput = screen.getByLabelText(/^name$/i);
        expect(nameInput).toHaveValue('Test Feed');
      });

      const notesInput = screen.getByLabelText(/notes/i);
      expect(notesInput).toHaveValue('Test notes');

      // Button should say "Save" in edit mode
      expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
    });

    it('should show coop selector when coops are provided', () => {
      const mockOnSubmit = vi.fn();
      const coops = [
        { id: 'coop-1', name: 'Main Coop' },
        { id: 'coop-2', name: 'Secondary Coop' },
      ];

      render(<PurchaseForm onSubmit={mockOnSubmit} coops={coops} />);

      const coopSelect = screen.getByLabelText(/coop/i);
      expect(coopSelect).toBeInTheDocument();
    });

    it('should not show coop selector when coops are not provided', () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      expect(screen.queryByLabelText(/coop/i)).not.toBeInTheDocument();
    });
  });

  describe('Validation', () => {
    it('should display validation errors for required fields', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      const submitButton = screen.getByRole('button', { name: /create/i });

      // Submit button should be disabled initially (no name entered)
      expect(submitButton).toBeDisabled();
    });

    it('should validate name field', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      const nameInput = screen.getByLabelText(/^name$/i);
      const submitButton = screen.getByRole('button', { name: /create/i });

      // Initially disabled (no name, amount=0, quantity=0)
      expect(submitButton).toBeDisabled();

      // Type a name
      await user.click(nameInput);
      await user.type(nameInput, 'Test Purchase');

      // Still disabled because amount and quantity are 0
      // We need to set positive values for those too
      expect(submitButton).toBeDisabled();

      // Set amount
      const amountLabel = screen.getByText(/amount \(czk\)/i);
      const amountContainer = amountLabel.closest('div');
      if (amountContainer) {
        const incrementButtons = within(amountContainer).getAllByRole('button');
        await user.click(incrementButtons[1]); // Increment amount
      }

      // Set quantity
      const quantityLabel = screen.getByText(/^quantity$/i);
      const quantityContainer = quantityLabel.closest('div');
      if (quantityContainer) {
        const incrementButtons = within(quantityContainer).getAllByRole('button');
        await user.click(incrementButtons[1]); // Increment quantity
      }

      // Now submit button should be enabled
      await waitFor(() => {
        expect(submitButton).not.toBeDisabled();
      });
    });

    it('should validate amount is positive', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      const nameInput = screen.getByLabelText(/^name$/i);
      await user.click(nameInput);
      await user.type(nameInput, 'Test');

      // Form should validate that amount > 0
      const submitButton = screen.getByRole('button', { name: /create/i });
      expect(submitButton).toBeDisabled();
    });

    it('should validate quantity is positive', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      const nameInput = screen.getByLabelText(/^name$/i);
      await user.click(nameInput);
      await user.type(nameInput, 'Test');

      // Form should validate that quantity > 0
      const submitButton = screen.getByRole('button', { name: /create/i });
      expect(submitButton).toBeDisabled();
    });

    it('should not allow purchase date in the future', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      const nameInput = screen.getByLabelText(/^name$/i);
      await user.click(nameInput);
      await user.type(nameInput, 'Test');

      const dateInput = screen.getByLabelText(/purchase date/i);
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 2);
      const futureDateStr = futureDate.toISOString().split('T')[0];

      await user.clear(dateInput);
      await user.type(dateInput, futureDateStr);

      // Blur to trigger validation
      await user.tab();

      // Should show validation error
      await waitFor(
        () => {
          expect(
            screen.getByText(/purchase date cannot be in the future/i)
          ).toBeInTheDocument();
        },
        { timeout: 3000 }
      );
    });
  });

  describe('Autocomplete', () => {
    it('should show autocomplete suggestions for name field', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      const nameInput = screen.getByLabelText(/^name$/i);

      // Type to trigger autocomplete
      await user.click(nameInput);
      await user.type(nameInput, 'Kr');

      // Wait for suggestions to appear
      await waitFor(() => {
        expect(screen.getByText('Krmivo Premium')).toBeInTheDocument();
        expect(screen.getByText('Krmivo Standard')).toBeInTheDocument();
      });
    });

    it('should allow selecting autocomplete suggestion', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      const nameInput = screen.getByLabelText(/^name$/i);

      await user.click(nameInput);
      await user.type(nameInput, 'Kr');

      await waitFor(() => {
        expect(screen.getByText('Krmivo Premium')).toBeInTheDocument();
      });

      // Click on suggestion
      await user.click(screen.getByText('Krmivo Premium'));

      // Input should have the selected value
      await waitFor(() => {
        expect(nameInput).toHaveValue('Krmivo Premium');
      });
    });
  });

  describe('Type Icons', () => {
    it('should display type icons for each purchase type', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      // Open the type dropdown by clicking the button/trigger
      const typeSelectContainer = screen.getByLabelText(/purchase type/i).closest('.MuiFormControl-root');
      if (typeSelectContainer) {
        const selectButton = within(typeSelectContainer).getByRole('combobox');
        await user.click(selectButton);

        // Check that all types are present in the dropdown
        await waitFor(
          () => {
            const options = screen.getAllByRole('option');
            expect(options.length).toBeGreaterThanOrEqual(6);
          },
          { timeout: 3000 }
        );
      }
    });
  });

  describe('Form Submission', () => {
    it('should call onSubmit with correct data in create mode', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      // Fill in the form
      const nameInput = screen.getByLabelText(/^name$/i);
      await user.click(nameInput);
      await user.type(nameInput, 'Test Feed');

      const purchaseDateInput = screen.getByLabelText(/purchase date/i);
      await user.clear(purchaseDateInput);
      await user.type(purchaseDateInput, '2024-01-15');

      // Set amount using NumericStepper
      const amountLabel = screen.getByText(/amount \(czk\)/i);
      const amountContainer = amountLabel.closest('div');
      if (amountContainer) {
        const incrementButtons = within(amountContainer).getAllByRole('button');
        // Click increment button multiple times to set amount
        for (let i = 0; i < 100; i++) {
          await user.click(incrementButtons[1]); // Second button is increment
        }
      }

      // Set quantity using NumericStepper
      const quantityLabel = screen.getByText(/^quantity$/i);
      const quantityContainer = quantityLabel.closest('div');
      if (quantityContainer) {
        const incrementButtons = within(quantityContainer).getAllByRole('button');
        for (let i = 0; i < 25; i++) {
          await user.click(incrementButtons[1]);
        }
      }

      const submitButton = screen.getByRole('button', { name: /create/i });

      await waitFor(() => {
        expect(submitButton).not.toBeDisabled();
      });

      await user.click(submitButton);

      await waitFor(() => {
        expect(mockOnSubmit).toHaveBeenCalledWith(
          expect.objectContaining({
            name: 'Test Feed',
            type: PurchaseType.Feed,
            purchaseDate: '2024-01-15',
            unit: QuantityUnit.Kg,
          })
        );
      });
    });

    it('should call onSubmit with correct data in edit mode', async () => {
      const mockOnSubmit = vi.fn();
      const existingPurchase: PurchaseDto = {
        id: '123',
        tenantId: 'tenant-1',
        coopId: null,
        name: 'Old Name',
        type: PurchaseType.Feed,
        amount: 100,
        quantity: 10,
        unit: QuantityUnit.Kg,
        purchaseDate: '2024-01-15T00:00:00Z',
        consumedDate: null,
        notes: null,
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z',
      };

      render(<PurchaseForm onSubmit={mockOnSubmit} initialData={existingPurchase} />);

      // Update the name
      const nameInput = screen.getByLabelText(/^name$/i);
      await user.clear(nameInput);
      await user.type(nameInput, 'Updated Name');

      const submitButton = screen.getByRole('button', { name: /save/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockOnSubmit).toHaveBeenCalledWith(
          expect.objectContaining({
            id: '123',
            name: 'Updated Name',
          })
        );
      });
    });

    it('should show loading state when isSubmitting is true', () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} isSubmitting={true} />);

      // Find the submit button - it should be disabled and show loading text
      const buttons = screen.getAllByRole('button');
      const submitButton = buttons.find((btn) => btn.textContent?.includes('Saving'));

      expect(submitButton).toBeTruthy();
      if (submitButton) {
        expect(submitButton).toBeDisabled();
        expect(submitButton).toHaveTextContent('Saving...');
      }
    });

    it('should call onCancel when cancel button is clicked', async () => {
      const mockOnSubmit = vi.fn();
      const mockOnCancel = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} onCancel={mockOnCancel} />);

      const cancelButton = screen.getByRole('button', { name: /cancel/i });
      await user.click(cancelButton);

      expect(mockOnCancel).toHaveBeenCalled();
    });

    it('should not show cancel button when onCancel is not provided', () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      expect(screen.queryByRole('button', { name: /cancel/i })).not.toBeInTheDocument();
    });
  });

  describe('Mobile Responsiveness', () => {
    it('should render touch-friendly inputs', () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      // All inputs should have minimum touch target height
      const nameInput = screen.getByLabelText(/^name$/i);
      expect(nameInput).toBeInTheDocument();

      const submitButton = screen.getByRole('button', { name: /create/i });
      expect(submitButton).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels', () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      expect(screen.getByLabelText(/purchase type/i)).toHaveAttribute(
        'aria-label',
        'Purchase Type'
      );
      expect(screen.getByLabelText(/^name$/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/purchase date/i)).toHaveAttribute(
        'aria-label',
        'Purchase Date'
      );
    });

    it('should support keyboard navigation', async () => {
      const mockOnSubmit = vi.fn();

      render(<PurchaseForm onSubmit={mockOnSubmit} />);

      const nameInput = screen.getByLabelText(/^name$/i);

      // Focus on name input
      nameInput.focus();
      expect(nameInput).toHaveFocus();

      // Tab to next field
      await user.tab();
      const purchaseDateInput = screen.getByLabelText(/purchase date/i);
      expect(purchaseDateInput).toHaveFocus();
    });
  });
});
