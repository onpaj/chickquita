import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import DashboardPage from '../DashboardPage';

// Mock hooks
const mockUseDashboardStats = vi.fn();
const mockUseAllFlocks = vi.fn();

vi.mock('@/features/dashboard/hooks/useDashboardStats', () => ({
  useDashboardStats: () => mockUseDashboardStats(),
}));

vi.mock('@/features/flocks/hooks/useAllFlocks', () => ({
  useAllFlocks: () => mockUseAllFlocks(),
}));

// Mock widgets and components
vi.mock('@/features/dashboard/components/TodaySummaryWidget', () => ({
  TodaySummaryWidget: () => <div data-testid="today-summary-widget" />,
}));
vi.mock('@/features/dashboard/components/WeeklyProductionWidget', () => ({
  WeeklyProductionWidget: () => <div data-testid="weekly-production-widget" />,
}));
vi.mock('@/features/dashboard/components/FlockStatusWidget', () => ({
  FlockStatusWidget: () => <div data-testid="flock-status-widget" />,
}));
vi.mock('@/features/dashboard/components/EggCostWidget', () => ({
  EggCostWidget: () => <div data-testid="egg-cost-widget" />,
}));
vi.mock('@/features/dashboard/components/DashboardEmptyState', () => ({
  DashboardEmptyState: () => <div data-testid="dashboard-empty-state" />,
}));
vi.mock('@/features/dailyRecords/components/QuickAddModal', () => ({
  QuickAddModal: () => <div data-testid="quick-add-modal" />,
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <DashboardPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('when user has data (active flocks)', () => {
    beforeEach(() => {
      mockUseDashboardStats.mockReturnValue({
        data: { activeFlocks: 2, todayEggs: 10, thisWeekEggs: 50, costPerEgg: 0.5, totalHens: 20, totalRoosters: 2, totalChicks: 0 },
        isLoading: false,
        error: null,
      });
      mockUseAllFlocks.mockReturnValue({ data: [{ id: '1', name: 'Flock 1' }] });
    });

    it('renders stat widgets', () => {
      renderPage();
      expect(screen.getByTestId('today-summary-widget')).toBeInTheDocument();
      expect(screen.getByTestId('weekly-production-widget')).toBeInTheDocument();
      expect(screen.getByTestId('egg-cost-widget')).toBeInTheDocument();
      expect(screen.getByTestId('flock-status-widget')).toBeInTheDocument();
    });

    it('does not render Quick Action Cards', () => {
      renderPage();
      expect(screen.queryByText('dashboard.quickActions.title')).not.toBeInTheDocument();
      expect(screen.queryByText('dashboard.quickActions.manageCoops')).not.toBeInTheDocument();
      expect(screen.queryByText('dashboard.quickActions.viewStatistics')).not.toBeInTheDocument();
    });

    it('renders the FAB', () => {
      renderPage();
      expect(screen.getByRole('button', { name: 'dashboard.quickActions.addDailyRecordAriaLabel' })).toBeInTheDocument();
    });

    it('does not render empty state', () => {
      renderPage();
      expect(screen.queryByTestId('dashboard-empty-state')).not.toBeInTheDocument();
    });
  });

  describe('when user has no data', () => {
    beforeEach(() => {
      mockUseDashboardStats.mockReturnValue({
        data: { activeFlocks: 0, todayEggs: 0, thisWeekEggs: 0, costPerEgg: null, totalHens: 0, totalRoosters: 0, totalChicks: 0 },
        isLoading: false,
        error: null,
      });
      mockUseAllFlocks.mockReturnValue({ data: [] });
    });

    it('renders empty state', () => {
      renderPage();
      expect(screen.getByTestId('dashboard-empty-state')).toBeInTheDocument();
    });

    it('does not render stat widgets', () => {
      renderPage();
      expect(screen.queryByTestId('today-summary-widget')).not.toBeInTheDocument();
    });

    it('does not render FAB', () => {
      renderPage();
      expect(screen.queryByRole('button', { name: 'dashboard.quickActions.addDailyRecordAriaLabel' })).not.toBeInTheDocument();
    });
  });

  describe('loading state', () => {
    beforeEach(() => {
      mockUseDashboardStats.mockReturnValue({ data: undefined, isLoading: true, error: null });
      mockUseAllFlocks.mockReturnValue({ data: [] });
    });

    it('renders widgets while loading', () => {
      renderPage();
      expect(screen.getByTestId('today-summary-widget')).toBeInTheDocument();
    });

    it('does not render empty state while loading', () => {
      renderPage();
      expect(screen.queryByTestId('dashboard-empty-state')).not.toBeInTheDocument();
    });
  });

  describe('error state', () => {
    beforeEach(() => {
      mockUseDashboardStats.mockReturnValue({ data: undefined, isLoading: false, error: new Error('Network error') });
      mockUseAllFlocks.mockReturnValue({ data: [] });
    });

    it('renders error alert', () => {
      renderPage();
      expect(screen.getByText('Network error')).toBeInTheDocument();
    });
  });
});
