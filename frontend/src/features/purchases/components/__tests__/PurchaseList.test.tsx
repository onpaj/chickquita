import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { PurchaseList } from '../PurchaseList';
import { usePurchases, useDeletePurchase } from '../../hooks/usePurchases';
import { useCoops } from '../../../coops/hooks/useCoops';
import { PurchaseType, QuantityUnit, type PurchaseDto } from '../../types/purchase.types';

// Mock hooks
vi.mock('../../hooks/usePurchases');
vi.mock('../../../coops/hooks/useCoops');
vi.mock('../../../flocks/hooks/useFlocks');
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, params?: Record<string, unknown>) => {
      if (key === 'purchases.delete.message') {
        return `Are you sure you want to delete purchase "${params?.name}"?`;
      }
      if (key === 'common.locale') {
        return 'en-US';
      }
      return key;
    },
  }),
}));

const mockPurchases: PurchaseDto[] = [
  {
    id: '1',
    tenantId: 'tenant-1',
    coopId: 'coop-1',
    name: 'Chicken Feed',
    type: PurchaseType.Feed,
    amount: 500,
    quantity: 25,
    unit: QuantityUnit.Kg,
    purchaseDate: '2024-02-01T00:00:00Z',
    consumedDate: null,
    notes: null,
    createdAt: '2024-02-01T00:00:00Z',
    updatedAt: '2024-02-01T00:00:00Z',
  },
  {
    id: '2',
    tenantId: 'tenant-1',
    coopId: null,
    name: 'Vitamins',
    type: PurchaseType.Vitamins,
    amount: 150,
    quantity: 10,
    unit: QuantityUnit.Pcs,
    purchaseDate: '2024-02-05T00:00:00Z',
    consumedDate: null,
    notes: null,
    createdAt: '2024-02-05T00:00:00Z',
    updatedAt: '2024-02-05T00:00:00Z',
  },
  {
    id: '3',
    tenantId: 'tenant-1',
    coopId: 'coop-2',
    name: 'Bedding Material',
    type: PurchaseType.Bedding,
    amount: 200,
    quantity: 50,
    unit: QuantityUnit.L,
    purchaseDate: '2024-02-10T00:00:00Z',
    consumedDate: null,
    notes: null,
    createdAt: '2024-02-10T00:00:00Z',
    updatedAt: '2024-02-10T00:00:00Z',
  },
];

describe('PurchaseList', () => {
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

  const renderComponent = (onEdit = vi.fn()) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <PurchaseList onEdit={onEdit} />
      </QueryClientProvider>
    );
  };

  describe('Loading State', () => {
    it('should display loading skeleton when data is loading', () => {
      vi.mocked(usePurchases).mockReturnValue({
        purchases: undefined,
        isLoading: true,
        error: null,
        refetch: vi.fn(),
      });
      vi.mocked(useCoops).mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        refetch: vi.fn(),
      } as unknown as ReturnType<typeof useCoops>);
      vi.mocked(useDeletePurchase).mockReturnValue({
        deletePurchase: vi.fn(),
        isDeleting: false,
      });

      renderComponent();

      // Check for skeleton elements
      const skeletons = screen.getAllByTestId(/skeleton/i);
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });

  describe('Empty State', () => {
    it('should display empty state when no purchases exist', () => {
      vi.mocked(usePurchases).mockReturnValue({
        purchases: [],
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
      vi.mocked(useCoops).mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      } as unknown as ReturnType<typeof useCoops>);
      vi.mocked(useDeletePurchase).mockReturnValue({
        deletePurchase: vi.fn(),
        isDeleting: false,
      });

      renderComponent();

      expect(screen.getByText('purchases.emptyState.title')).toBeInTheDocument();
      expect(screen.getByText('purchases.emptyState.description')).toBeInTheDocument();
    });
  });

  describe('Purchase Cards', () => {
    beforeEach(() => {
      vi.mocked(usePurchases).mockReturnValue({
        purchases: mockPurchases,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
      vi.mocked(useCoops).mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      } as unknown as ReturnType<typeof useCoops>);
      vi.mocked(useDeletePurchase).mockReturnValue({
        deletePurchase: vi.fn(),
        isDeleting: false,
      });
    });

    it('should render all purchases', () => {
      renderComponent();

      expect(screen.getByText('Chicken Feed')).toBeInTheDocument();
      expect(screen.getByText('Vitamins')).toBeInTheDocument();
      expect(screen.getByText('Bedding Material')).toBeInTheDocument();
    });

    it('should display purchase details correctly', () => {
      renderComponent();

      const firstPurchaseCard = screen.getByText('Chicken Feed').closest('li');
      expect(firstPurchaseCard).toBeInTheDocument();

      // Amount is rendered with currency key ("500.00 purchases.currency") and
      // quantity with unit key ("25 purchases.units.kg"), so check textContent directly
      expect(firstPurchaseCard?.textContent).toContain('500.00');
      expect(firstPurchaseCard?.textContent).toContain('25');
    });

    it('should call onEdit when edit button is clicked', async () => {
      const onEdit = vi.fn();
      const user = userEvent.setup();

      renderComponent(onEdit);

      const editButtons = screen.getAllByLabelText('common.edit');
      await user.click(editButtons[0]);

      expect(onEdit).toHaveBeenCalledWith(mockPurchases[0]);
    });

    it('should open delete confirmation dialog when delete button is clicked', async () => {
      const user = userEvent.setup();

      renderComponent();

      const deleteButtons = screen.getAllByLabelText('common.delete');
      await user.click(deleteButtons[0]);

      await waitFor(() => {
        expect(screen.getByText('purchases.delete.title')).toBeInTheDocument();
        expect(
          screen.getByText('Are you sure you want to delete purchase "Chicken Feed"?')
        ).toBeInTheDocument();
      });
    });

    it('should call deletePurchase when delete is confirmed', async () => {
      const deletePurchase = vi.fn();
      const user = userEvent.setup();

      vi.mocked(useDeletePurchase).mockReturnValue({
        deletePurchase,
        isDeleting: false,
      });

      renderComponent();

      const deleteButtons = screen.getAllByLabelText('common.delete');
      await user.click(deleteButtons[0]);

      await waitFor(() => {
        expect(screen.getByText('purchases.delete.title')).toBeInTheDocument();
      });

      const confirmButton = screen.getByRole('button', { name: 'common.delete' });
      await user.click(confirmButton);

      expect(deletePurchase).toHaveBeenCalledWith('1');
    });
  });

  describe('Monthly Summary', () => {
    it('should display monthly summary correctly', () => {
      // Mock current date to February 2024
      vi.useFakeTimers();
      vi.setSystemTime(new Date('2024-02-15T00:00:00Z'));

      vi.mocked(usePurchases).mockReturnValue({
        purchases: mockPurchases,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
      vi.mocked(useCoops).mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      } as unknown as ReturnType<typeof useCoops>);
      vi.mocked(useDeletePurchase).mockReturnValue({
        deletePurchase: vi.fn(),
        isDeleting: false,
      });

      renderComponent();

      // All purchases are from February 2024, so total should be 850
      expect(screen.getByText('850.00')).toBeInTheDocument();

      vi.useRealTimers();
    });
  });

  describe('Filters', () => {
    beforeEach(() => {
      vi.mocked(usePurchases).mockReturnValue({
        purchases: mockPurchases,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
      vi.mocked(useCoops).mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      } as unknown as ReturnType<typeof useCoops>);
      vi.mocked(useDeletePurchase).mockReturnValue({
        deletePurchase: vi.fn(),
        isDeleting: false,
      });
    });

    it('should render filter controls', () => {
      renderComponent();

      expect(screen.getByLabelText('purchases.filters.fromDate')).toBeInTheDocument();
      expect(screen.getByLabelText('purchases.filters.toDate')).toBeInTheDocument();
      expect(screen.getByLabelText('purchases.filters.type')).toBeInTheDocument();
      expect(screen.getByLabelText('purchases.filters.flock')).toBeInTheDocument();
    });

    it('should update fromDate filter when date is selected', async () => {
      const user = userEvent.setup();

      renderComponent();

      const fromDateInput = screen.getByLabelText('purchases.filters.fromDate');
      await user.type(fromDateInput, '2024-02-01');

      expect(fromDateInput).toHaveValue('2024-02-01');
    });

    it('should update toDate filter when date is selected', async () => {
      const user = userEvent.setup();

      renderComponent();

      const toDateInput = screen.getByLabelText('purchases.filters.toDate');
      await user.type(toDateInput, '2024-02-28');

      expect(toDateInput).toHaveValue('2024-02-28');
    });

    it('should update type filter when type is selected', async () => {
      const user = userEvent.setup();

      renderComponent();

      const typeSelect = screen.getByLabelText('purchases.filters.type');
      await user.click(typeSelect);

      const feedOption = await screen.findByText('purchases.types.feed');
      await user.click(feedOption);

      expect(typeSelect).toHaveTextContent('purchases.types.feed');
    });
  });

  describe('Accessibility', () => {
    beforeEach(() => {
      vi.mocked(usePurchases).mockReturnValue({
        purchases: mockPurchases,
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      });
      vi.mocked(useCoops).mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: vi.fn(),
      } as unknown as ReturnType<typeof useCoops>);
      vi.mocked(useDeletePurchase).mockReturnValue({
        deletePurchase: vi.fn(),
        isDeleting: false,
      });
    });

    it('should have proper ARIA labels on interactive elements', () => {
      renderComponent();

      const editButtons = screen.getAllByLabelText('common.edit');
      expect(editButtons.length).toBeGreaterThan(0);

      const deleteButtons = screen.getAllByLabelText('common.delete');
      expect(deleteButtons.length).toBeGreaterThan(0);
    });

    it('should support keyboard navigation', async () => {
      const user = userEvent.setup();

      renderComponent();

      const editButtons = screen.getAllByLabelText('common.edit');
      editButtons[0].focus();

      expect(editButtons[0]).toHaveFocus();

      await user.keyboard('{Tab}');
      const deleteButtons = screen.getAllByLabelText('common.delete');
      expect(deleteButtons[0]).toHaveFocus();
    });
  });
});
