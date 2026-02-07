import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { PurchasesPage } from '../PurchasesPage';
import type { PurchaseDto } from '../../types/purchase.types';
import { PurchaseType, QuantityUnit } from '../../types/purchase.types';

// Mock hooks
const mockCreatePurchase = vi.fn();
const mockUpdatePurchase = vi.fn();

vi.mock('../../hooks/usePurchases', () => ({
  useCreatePurchase: () => ({
    createPurchase: mockCreatePurchase,
    isCreating: false,
  }),
  useUpdatePurchase: () => ({
    updatePurchase: mockUpdatePurchase,
    isUpdating: false,
  }),
}));

vi.mock('../../../coops/hooks/useCoops', () => ({
  useCoops: () => ({
    data: [
      { id: 'coop-1', name: 'Main Coop' },
      { id: 'coop-2', name: 'Secondary Coop' },
    ],
    isLoading: false,
    error: null,
  }),
}));

// Mock PurchaseList component
const mockOnEdit = vi.fn();
vi.mock('../../components/PurchaseList', () => ({
  PurchaseList: ({ onEdit }: { onEdit: (purchase: PurchaseDto) => void }) => {
    mockOnEdit.mockImplementation(onEdit);
    return (
      <div data-testid="purchase-list">
        <button onClick={() => onEdit(mockPurchase)}>Edit Purchase</button>
      </div>
    );
  },
}));

// Mock PurchaseForm component
vi.mock('../../components/PurchaseForm', () => ({
  PurchaseForm: ({
    initialData,
    onSubmit,
    onCancel,
  }: {
    initialData?: PurchaseDto;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    onSubmit: (data: any) => void;
    onCancel: () => void;
  }) => (
    <div data-testid="purchase-form">
      <p>{initialData ? 'Edit Mode' : 'Create Mode'}</p>
      <button
        onClick={() =>
          onSubmit(
            initialData
              ? { id: initialData.id, name: 'Test Purchase' }
              : { name: 'Test Purchase' }
          )
        }
      >
        Submit
      </button>
      <button onClick={onCancel}>Cancel</button>
    </div>
  ),
}));

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'purchases.title': 'Nákupy',
        'purchases.addPurchase': 'Přidat nákup',
        'purchases.createPurchase': 'Vytvořit nákup',
        'purchases.editPurchase': 'Upravit nákup',
      };
      return translations[key] || key;
    },
  }),
}));

const mockPurchase: PurchaseDto = {
  id: 'purchase-1',
  tenantId: 'tenant-1',
  coopId: 'coop-1',
  name: 'Test Purchase',
  type: PurchaseType.Feed,
  amount: 100,
  quantity: 10,
  unit: QuantityUnit.Kg,
  purchaseDate: '2024-01-15',
  consumedDate: null,
  notes: null,
  createdAt: '2024-01-15T10:00:00Z',
  updatedAt: '2024-01-15T10:00:00Z',
};

describe('PurchasesPage', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    vi.clearAllMocks();
  });

  const renderPage = () => {
    return render(
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <PurchasesPage />
        </QueryClientProvider>
      </BrowserRouter>
    );
  };

  describe('Page Rendering', () => {
    it('should render page title "Nákupy"', () => {
      renderPage();
      expect(screen.getByRole('heading', { name: 'Nákupy' })).toBeInTheDocument();
    });

    it('should render PurchaseList component', () => {
      renderPage();
      expect(screen.getByTestId('purchase-list')).toBeInTheDocument();
    });

    it('should render desktop create button on larger screens', () => {
      // Mock useMediaQuery to return false (desktop)
      vi.mock('@mui/material', async () => {
        const actual = await vi.importActual('@mui/material');
        return {
          ...actual,
          useMediaQuery: () => false,
        };
      });

      renderPage();
      const createButtons = screen.getAllByRole('button', { name: 'Přidat nákup' });
      expect(createButtons.length).toBeGreaterThan(0);
    });

    it('should render mobile FAB button', () => {
      renderPage();
      const fabButton = screen.getByLabelText('Přidat nákup');
      expect(fabButton).toBeInTheDocument();
    });
  });

  describe('Modal Interactions - Create', () => {
    it('should open create modal when FAB button is clicked', async () => {
      const user = userEvent.setup();
      renderPage();

      const fabButton = screen.getByLabelText('Přidat nákup');
      await user.click(fabButton);

      expect(screen.getByTestId('purchase-form')).toBeInTheDocument();
      expect(screen.getByText('Create Mode')).toBeInTheDocument();
    });

    it('should display create modal title', async () => {
      const user = userEvent.setup();
      renderPage();

      const fabButton = screen.getByLabelText('Přidat nákup');
      await user.click(fabButton);

      expect(screen.getByText('Vytvořit nákup')).toBeInTheDocument();
    });

    it('should close create modal when cancel is clicked', async () => {
      const user = userEvent.setup();
      renderPage();

      // Open modal
      const fabButton = screen.getByLabelText('Přidat nákup');
      await user.click(fabButton);
      expect(screen.getByTestId('purchase-form')).toBeInTheDocument();

      // Close modal
      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      await waitFor(() => {
        expect(screen.queryByTestId('purchase-form')).not.toBeInTheDocument();
      });
    });

    it('should call createPurchase when form is submitted', async () => {
      const user = userEvent.setup();
      mockCreatePurchase.mockImplementation((data, { onSuccess }) => {
        onSuccess?.();
      });

      renderPage();

      // Open modal
      const fabButton = screen.getByLabelText('Přidat nákup');
      await user.click(fabButton);

      // Submit form
      const submitButton = screen.getByRole('button', { name: 'Submit' });
      await user.click(submitButton);

      expect(mockCreatePurchase).toHaveBeenCalledWith(
        { name: 'Test Purchase' },
        expect.any(Object)
      );
    });

    it('should close modal after successful create', async () => {
      const user = userEvent.setup();
      mockCreatePurchase.mockImplementation((data, { onSuccess }) => {
        onSuccess?.();
      });

      renderPage();

      // Open modal
      const fabButton = screen.getByLabelText('Přidat nákup');
      await user.click(fabButton);

      // Submit form
      const submitButton = screen.getByRole('button', { name: 'Submit' });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.queryByTestId('purchase-form')).not.toBeInTheDocument();
      });
    });
  });

  describe('Modal Interactions - Edit', () => {
    it('should open edit modal when edit button in list is clicked', async () => {
      const user = userEvent.setup();
      renderPage();

      const editButton = screen.getByRole('button', { name: 'Edit Purchase' });
      await user.click(editButton);

      expect(screen.getByTestId('purchase-form')).toBeInTheDocument();
      expect(screen.getByText('Edit Mode')).toBeInTheDocument();
    });

    it('should display edit modal title', async () => {
      const user = userEvent.setup();
      renderPage();

      const editButton = screen.getByRole('button', { name: 'Edit Purchase' });
      await user.click(editButton);

      expect(screen.getByText('Upravit nákup')).toBeInTheDocument();
    });

    it('should close edit modal when cancel is clicked', async () => {
      const user = userEvent.setup();
      renderPage();

      // Open edit modal
      const editButton = screen.getByRole('button', { name: 'Edit Purchase' });
      await user.click(editButton);
      expect(screen.getByTestId('purchase-form')).toBeInTheDocument();

      // Close modal
      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      await waitFor(() => {
        expect(screen.queryByTestId('purchase-form')).not.toBeInTheDocument();
      });
    });

    it('should call updatePurchase when edit form is submitted', async () => {
      const user = userEvent.setup();
      mockUpdatePurchase.mockImplementation((data, { onSuccess }) => {
        onSuccess?.();
      });

      renderPage();

      // Open edit modal
      const editButton = screen.getByRole('button', { name: 'Edit Purchase' });
      await user.click(editButton);

      // Submit form - the mock form returns data with an id for edit mode
      const submitButton = screen.getByRole('button', { name: 'Submit' });
      await user.click(submitButton);

      // Note: In the actual implementation, the form determines if it's create or update
      // based on whether initialData has an id
      expect(mockUpdatePurchase).toHaveBeenCalled();
    });

    it('should close modal after successful update', async () => {
      const user = userEvent.setup();
      mockUpdatePurchase.mockImplementation((data, { onSuccess }) => {
        onSuccess?.();
      });

      renderPage();

      // Open edit modal
      const editButton = screen.getByRole('button', { name: 'Edit Purchase' });
      await user.click(editButton);

      // Submit form
      const submitButton = screen.getByRole('button', { name: 'Submit' });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.queryByTestId('purchase-form')).not.toBeInTheDocument();
      });
    });
  });

  describe('Modal State Management', () => {
    it('should not show modal initially', () => {
      renderPage();
      expect(screen.queryByTestId('purchase-form')).not.toBeInTheDocument();
    });

    it('should clear editing state when opening create modal after edit', async () => {
      const user = userEvent.setup();
      renderPage();

      // Open edit modal
      const editButton = screen.getByRole('button', { name: 'Edit Purchase' });
      await user.click(editButton);
      expect(screen.getByText('Edit Mode')).toBeInTheDocument();

      // Close modal
      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      // Open create modal
      const fabButton = screen.getByLabelText('Přidat nákup');
      await user.click(fabButton);

      expect(screen.getByText('Create Mode')).toBeInTheDocument();
    });
  });

  describe('Responsive Layout', () => {
    it('should render with mobile-first padding', () => {
      const { container } = renderPage();
      const mainContainer = container.querySelector('[class*="MuiContainer"]');
      expect(mainContainer).toBeInTheDocument();
    });
  });
});
