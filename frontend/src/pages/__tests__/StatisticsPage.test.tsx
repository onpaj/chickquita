import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import StatisticsPage from '../StatisticsPage';

// Mock hooks
const mockUseStatistics = vi.fn();
const mockUseCoops = vi.fn();
const mockUseFlocks = vi.fn();

vi.mock('@/features/statistics/hooks/useStatistics', () => ({
  useStatistics: (...args: unknown[]) => mockUseStatistics(...args),
}));

vi.mock('@/features/coops/hooks/useCoops', () => ({
  useCoops: () => mockUseCoops(),
}));

vi.mock('@/features/flocks/hooks/useFlocks', () => ({
  useFlocks: (...args: unknown[]) => mockUseFlocks(...args),
}));

// Mock chart components to avoid canvas issues
vi.mock('@/features/statistics/components/EggCostBreakdownChart', () => ({
  EggCostBreakdownChart: () => <div data-testid="egg-cost-chart" />,
}));
vi.mock('@/features/statistics/components/ProductionTrendChart', () => ({
  ProductionTrendChart: () => <div data-testid="production-chart" />,
}));
vi.mock('@/features/statistics/components/CostPerEggTrendChart', () => ({
  CostPerEggTrendChart: () => <div data-testid="cost-per-egg-chart" />,
}));
vi.mock('@/features/statistics/components/FlockProductivityChart', () => ({
  FlockProductivityChart: () => <div data-testid="flock-productivity-chart" />,
}));

// Mock StatCard
vi.mock('@/shared/components', () => ({
  StatCard: ({ label }: { label: string }) => <div data-testid="stat-card">{label}</div>,
}));

// Mock MUI DatePicker to avoid full calendar rendering
vi.mock('@mui/x-date-pickers/DatePicker', () => ({
  DatePicker: ({ label }: { label: string }) => (
    <input aria-label={label} data-testid="date-picker" />
  ),
}));
vi.mock('@mui/x-date-pickers/LocalizationProvider', () => ({
  LocalizationProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));
vi.mock('@mui/x-date-pickers/AdapterDayjs', () => ({
  AdapterDayjs: class {},
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, params?: Record<string, unknown>) => {
      const translations: Record<string, string> = {
        'statistics.title': 'Statistics',
        'statistics.dateRange.title': 'Time Period',
        'statistics.dateRange.ariaLabel': 'Select time period',
        'statistics.dateRange.allTime': 'All time',
        'statistics.dateRange.last7Days': '7 days',
        'statistics.dateRange.last30Days': '30 days',
        'statistics.dateRange.last90Days': '90 days',
        'statistics.dateRange.custom': 'Custom',
        'statistics.dateRange.startDate': 'Start Date',
        'statistics.dateRange.endDate': 'End Date',
        'statistics.filters.title': 'Filters',
        'statistics.filters.coop': 'Coop',
        'statistics.filters.flock': 'Flock',
        'statistics.filters.allCoops': 'All Coops',
        'statistics.filters.allFlocks': 'All Flocks',
        'statistics.filters.summary': `${params?.range ?? ''} · ${params?.flock ?? ''}`,
        'statistics.summary.totalEggs': 'Total Eggs',
        'statistics.summary.totalCost': 'Total Costs',
        'statistics.summary.avgCostPerEgg': 'Avg. Cost/Egg',
        'statistics.error.loadFailed': 'Failed to load statistics',
        'statistics.emptyState.noData': 'No data for selected period',
      };
      return translations[key] ?? key;
    },
    i18n: { language: 'en' },
  }),
}));

const mockStats = {
  summary: { totalEggs: 100, totalCost: 50, avgCostPerEgg: 0.5 },
  costBreakdown: [],
  productionTrend: [{ date: '2024-01-01', eggs: 10 }],
  costPerEggTrend: [],
  flockProductivity: [],
};

describe('StatisticsPage', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    vi.clearAllMocks();

    mockUseStatistics.mockReturnValue({ data: mockStats, isLoading: false, error: null });
    mockUseCoops.mockReturnValue({ data: [] });
    mockUseFlocks.mockReturnValue({ data: [] });
  });

  const renderPage = () =>
    render(
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <StatisticsPage />
        </QueryClientProvider>
      </BrowserRouter>
    );

  describe('page structure', () => {
    it('renders page title', () => {
      renderPage();
      expect(screen.getByRole('heading', { name: 'Statistics' })).toBeInTheDocument();
    });

    it('renders stat cards', () => {
      renderPage();
      expect(screen.getAllByTestId('stat-card')).toHaveLength(3);
    });
  });

  describe('collapsible filter accordion', () => {
    it('renders filter accordion collapsed by default', () => {
      renderPage();
      // AccordionDetails should not be visible (MUI hides content when collapsed)
      // The panel is collapsed — aria-expanded should be false on the header button
      const header = document.getElementById('statistics-filters-header');
      expect(header?.getAttribute('aria-expanded')).toBe('false');
    });

    it('shows filter summary with default date range and flock label', () => {
      renderPage();
      expect(screen.getByText('All time · All Flocks')).toBeInTheDocument();
    });

    it('expands accordion when clicking the summary', async () => {
      const user = userEvent.setup();
      renderPage();

      const header = document.getElementById('statistics-filters-header');
      await user.click(header!);

      expect(header?.getAttribute('aria-expanded')).toBe('true');
    });

    it('shows date range toggle buttons when expanded', async () => {
      const user = userEvent.setup();
      renderPage();

      await user.click(document.getElementById('statistics-filters-header')!);

      expect(screen.getByRole('button', { name: '7 days' })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: '30 days' })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: '90 days' })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: 'Custom' })).toBeInTheDocument();
    });

    it('shows coop and flock selects when expanded', async () => {
      const user = userEvent.setup();
      renderPage();

      await user.click(document.getElementById('statistics-filters-header')!);

      expect(screen.getAllByText('Coop').length).toBeGreaterThan(0);
      expect(screen.getAllByText('Flock').length).toBeGreaterThan(0);
    });
  });

  describe('filter summary label updates', () => {
    it('updates summary when date range changes to 7 days', async () => {
      const user = userEvent.setup();
      renderPage();

      // Expand accordion first
      await user.click(document.getElementById('statistics-filters-header')!);

      // Click 7 days button
      await user.click(screen.getByRole('button', { name: '7 days' }));

      expect(screen.getByText('7 days · All Flocks')).toBeInTheDocument();
    });

    it('updates summary when date range changes to 90 days', async () => {
      const user = userEvent.setup();
      renderPage();

      await user.click(document.getElementById('statistics-filters-header')!);
      await user.click(screen.getByRole('button', { name: '90 days' }));

      expect(screen.getByText('90 days · All Flocks')).toBeInTheDocument();
    });
  });

  describe('loading state', () => {
    it('shows skeletons while loading', () => {
      mockUseStatistics.mockReturnValue({ data: undefined, isLoading: true, error: null });
      renderPage();

      // MUI Skeleton renders as a div — check there are multiple
      const skeletons = document.querySelectorAll('.MuiSkeleton-root');
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });

  describe('error state', () => {
    it('shows error alert when statistics fail to load', () => {
      mockUseStatistics.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: new Error('Network error'),
      });
      renderPage();

      expect(screen.getByText('Failed to load statistics')).toBeInTheDocument();
    });
  });

  describe('charts', () => {
    it('renders all four charts when data is available', () => {
      renderPage();
      expect(screen.getByTestId('egg-cost-chart')).toBeInTheDocument();
      expect(screen.getByTestId('production-chart')).toBeInTheDocument();
      expect(screen.getByTestId('cost-per-egg-chart')).toBeInTheDocument();
      expect(screen.getByTestId('flock-productivity-chart')).toBeInTheDocument();
    });

    it('shows empty state when production trend is empty', () => {
      mockUseStatistics.mockReturnValue({
        data: { ...mockStats, productionTrend: [] },
        isLoading: false,
        error: null,
      });
      renderPage();
      expect(screen.getByText('No data for selected period')).toBeInTheDocument();
    });
  });
});
