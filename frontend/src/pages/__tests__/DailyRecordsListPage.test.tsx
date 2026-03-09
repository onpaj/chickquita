import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { format, subDays } from 'date-fns';
import { DailyRecordsListPage } from '../DailyRecordsListPage';
import type { DailyRecordDto } from '../../features/dailyRecords/api/dailyRecordsApi';
import type { FlockForQuickAdd } from '../../features/flocks/hooks/useAllFlocks';

// Mock hooks
const mockUseDailyRecords = vi.fn();
const mockUseCoops = vi.fn();
const mockUseAllFlocks = vi.fn();

vi.mock('../../features/dailyRecords/hooks/useDailyRecords', () => ({
  useDailyRecords: (params: unknown) => mockUseDailyRecords(params),
}));

vi.mock('../../features/coops/hooks/useCoops', () => ({
  useCoops: () => mockUseCoops(),
}));

vi.mock('../../features/flocks/hooks/useAllFlocks', () => ({
  useAllFlocks: () => mockUseAllFlocks(),
}));

// Mock child components
vi.mock('../../features/dailyRecords/components/DailyRecordCard', () => ({
  DailyRecordCard: ({ record }: { record: DailyRecordDto }) => (
    <div data-testid="daily-record-card">{record.flockName}</div>
  ),
}));

vi.mock('../../features/dailyRecords/components/EditDailyRecordModal', () => ({
  EditDailyRecordModal: ({ open, onClose }: { open: boolean; onClose: () => void }) =>
    open ? (
      <div data-testid="edit-record-modal">
        <button onClick={onClose}>Close Edit</button>
      </div>
    ) : null,
}));

vi.mock('../../features/dailyRecords/components/QuickAddModal', () => ({
  QuickAddModal: ({
    open,
    onClose,
  }: {
    open: boolean;
    onClose: () => void;
    flocks: FlockForQuickAdd[];
  }) =>
    open ? (
      <div data-testid="quick-add-modal">
        <button onClick={onClose}>Close Quick Add</button>
      </div>
    ) : null,
}));

vi.mock('../../shared/components', () => ({
  IllustratedEmptyState: ({ title }: { title: string }) => (
    <div data-testid="empty-state">{title}</div>
  ),
  DailyRecordCardSkeleton: () => <div data-testid="skeleton" />,
}));

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, params?: Record<string, unknown>) => {
      const translations: Record<string, string> = {
        'dailyRecords.title': 'Daily Records',
        'dailyRecords.addRecord': 'Add Record',
        'dailyRecords.flock': 'Flock',
        'dailyRecords.clearFilters': 'Clear filters',
        'dailyRecords.recordsCount': `${params?.count ?? 0} records`,
        'dailyRecords.filters.today': 'Today',
        'dailyRecords.filters.lastWeek': 'Last week',
        'dailyRecords.filters.lastMonth': 'Last month',
        'dailyRecords.filters.startDate': 'Start date',
        'dailyRecords.filters.endDate': 'End date',
        'dailyRecords.emptyState.title': 'No records yet',
        'dailyRecords.emptyState.noRecords': 'No records found',
        'dailyRecords.emptyState.noRecordsFiltered': 'No records match filters',
        'common.filter': 'Filter',
        'common.all': 'All',
      };
      return translations[key] ?? key;
    },
  }),
}));

const mockFlocks: FlockForQuickAdd[] = [
  { id: 'flock-1', identifier: 'Flock A', coopName: 'Main Coop' },
  { id: 'flock-2', identifier: 'Flock B', coopName: 'Secondary Coop' },
];

const mockRecords: DailyRecordDto[] = [
  {
    id: 'rec-1',
    tenantId: 'tenant-1',
    flockId: 'flock-1',
    flockName: 'Flock A',
    flockCoopName: 'Main Coop',
    recordDate: '2024-01-15',
    eggCount: 10,
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-01-15T10:00:00Z',
  },
];

describe('DailyRecordsListPage', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    vi.clearAllMocks();

    // Default: data loaded, no error
    mockUseCoops.mockReturnValue({ isLoading: false });
    mockUseDailyRecords.mockReturnValue({ data: [], isLoading: false });
    mockUseAllFlocks.mockReturnValue({ data: [] });
  });

  const renderPage = () =>
    render(
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <DailyRecordsListPage />
        </QueryClientProvider>
      </BrowserRouter>
    );

  describe('FAB rendering', () => {
    it('renders Add Record FAB when flocks exist', () => {
      mockUseAllFlocks.mockReturnValue({ data: mockFlocks });

      renderPage();

      expect(screen.getByRole('button', { name: 'Add Record' })).toBeInTheDocument();
    });

    it('renders FAB even when no flocks exist (disabled)', () => {
      mockUseAllFlocks.mockReturnValue({ data: [] });

      renderPage();

      expect(screen.getByRole('button', { name: 'Add Record' })).toBeInTheDocument();
    });

    it('FAB is disabled when no flocks exist', () => {
      mockUseAllFlocks.mockReturnValue({ data: [] });

      renderPage();

      expect(screen.getByRole('button', { name: 'Add Record' })).toBeDisabled();
    });

    it('FAB is enabled when flocks exist', () => {
      mockUseAllFlocks.mockReturnValue({ data: mockFlocks });

      renderPage();

      expect(screen.getByRole('button', { name: 'Add Record' })).not.toBeDisabled();
    });
  });

  describe('QuickAddModal', () => {
    it('QuickAddModal is not shown by default', () => {
      mockUseAllFlocks.mockReturnValue({ data: mockFlocks });

      renderPage();

      expect(screen.queryByTestId('quick-add-modal')).not.toBeInTheDocument();
    });

    it('clicking FAB opens QuickAddModal', async () => {
      const user = userEvent.setup();
      mockUseAllFlocks.mockReturnValue({ data: mockFlocks });

      renderPage();

      await user.click(screen.getByRole('button', { name: 'Add Record' }));

      expect(screen.getByTestId('quick-add-modal')).toBeInTheDocument();
    });

    it('QuickAddModal closes when onClose is called', async () => {
      const user = userEvent.setup();
      mockUseAllFlocks.mockReturnValue({ data: mockFlocks });

      renderPage();

      await user.click(screen.getByRole('button', { name: 'Add Record' }));
      expect(screen.getByTestId('quick-add-modal')).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Close Quick Add' }));

      await waitFor(() => {
        expect(screen.queryByTestId('quick-add-modal')).not.toBeInTheDocument();
      });
    });
  });

  describe('Records list', () => {
    it('renders records when data is available', () => {
      mockUseDailyRecords.mockReturnValue({ data: mockRecords, isLoading: false });
      mockUseAllFlocks.mockReturnValue({ data: mockFlocks });

      renderPage();

      expect(screen.getByTestId('daily-record-card')).toBeInTheDocument();
    });

    it('shows empty state when no records', () => {
      mockUseDailyRecords.mockReturnValue({ data: [], isLoading: false });

      renderPage();

      expect(screen.getByTestId('empty-state')).toBeInTheDocument();
    });

    it('shows skeletons while loading', () => {
      mockUseCoops.mockReturnValue({ isLoading: true });
      mockUseDailyRecords.mockReturnValue({ data: undefined, isLoading: true });

      renderPage();

      expect(screen.getAllByTestId('skeleton').length).toBeGreaterThan(0);
    });
  });

  describe('Page structure', () => {
    it('renders page title', () => {
      renderPage();

      expect(screen.getByRole('heading', { name: 'Daily Records' })).toBeInTheDocument();
    });
  });

  describe('Default filters', () => {
    it('calls useDailyRecords with last 30 days by default', () => {
      const today = format(new Date(), 'yyyy-MM-dd');
      const thirtyDaysAgo = format(subDays(new Date(), 30), 'yyyy-MM-dd');

      renderPage();

      expect(mockUseDailyRecords).toHaveBeenCalledWith(
        expect.objectContaining({ startDate: thirtyDaysAgo, endDate: today })
      );
    });

    it('does not show "Clear filters" button when using default filters', () => {
      renderPage();

      expect(screen.queryByRole('button', { name: 'Clear filters' })).not.toBeInTheDocument();
    });

    it('shows start date field pre-filled with 30 days ago', () => {
      const thirtyDaysAgo = format(subDays(new Date(), 30), 'yyyy-MM-dd');

      renderPage();

      const startDateInput = screen.getByLabelText('Start date') as HTMLInputElement;
      expect(startDateInput.value).toBe(thirtyDaysAgo);
    });

    it('shows end date field pre-filled with today', () => {
      const today = format(new Date(), 'yyyy-MM-dd');

      renderPage();

      const endDateInput = screen.getByLabelText('End date') as HTMLInputElement;
      expect(endDateInput.value).toBe(today);
    });

    it('shows "Clear filters" button after applying a quick filter', async () => {
      const user = userEvent.setup();
      renderPage();

      // "Last week" sets startDate to 7 days ago — different from default 30 days ago
      await user.click(screen.getByRole('button', { name: 'Last week' }));

      expect(screen.getByRole('button', { name: 'Clear filters' })).toBeInTheDocument();
    });

    it('"Clear filters" resets to default 30-day range, not empty', async () => {
      const user = userEvent.setup();
      const today = format(new Date(), 'yyyy-MM-dd');
      const thirtyDaysAgo = format(subDays(new Date(), 30), 'yyyy-MM-dd');

      renderPage();

      // Apply "Last week" filter (changes startDate to 7 days ago)
      await user.click(screen.getByRole('button', { name: 'Last week' }));

      const clearButton = screen.getByRole('button', { name: 'Clear filters' });
      await user.click(clearButton);

      // Dates should be back to 30-day default
      const startDateInput = screen.getByLabelText('Start date') as HTMLInputElement;
      const endDateInput = screen.getByLabelText('End date') as HTMLInputElement;
      expect(startDateInput.value).toBe(thirtyDaysAgo);
      expect(endDateInput.value).toBe(today);
    });
  });
});
