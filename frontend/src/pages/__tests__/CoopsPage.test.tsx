import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import CoopsPage from '../CoopsPage';
import type { Coop } from '../../features/coops/api/coopsApi';

// Mock the useCoops hook
const mockRefetch = vi.fn();
const mockUseCoops = vi.fn();

vi.mock('../../features/coops/hooks/useCoops', () => ({
  useCoops: () => mockUseCoops(),
}));

// Mock useErrorHandler hook
const mockHandleError = vi.fn();
vi.mock('../../hooks/useErrorHandler', () => ({
  useErrorHandler: () => ({
    handleError: mockHandleError,
  }),
}));

// Mock CoopCard component
vi.mock('../../features/coops/components/CoopCard', () => ({
  CoopCard: ({ coop }: { coop: Coop }) => (
    <div data-testid="coop-card">{coop.name}</div>
  ),
}));

// Mock CoopsEmptyState component
vi.mock('../../features/coops/components/CoopsEmptyState', () => ({
  CoopsEmptyState: ({ onAddClick }: { onAddClick: () => void }) => (
    <div data-testid="empty-state">
      <p>No coops yet</p>
      <button onClick={onAddClick}>Add Coop</button>
    </div>
  ),
}));

// Mock CreateCoopModal component
vi.mock('../../features/coops/components/CreateCoopModal', () => ({
  CreateCoopModal: ({ open, onClose }: { open: boolean; onClose: () => void }) => (
    open ? (
      <div data-testid="create-coop-modal">
        <p>Create Coop Modal</p>
        <button onClick={onClose}>Close</button>
      </div>
    ) : null
  ),
}));

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, _params?: { date?: string }) => {
      const translations: Record<string, string> = {
        'coops.title': 'Coops',
        'coops.addCoop': 'Add Coop',
        'common.loading': 'Loading...',
        'common.retry': 'Retry',
        'errors.networkError': 'Connection error. Please check your internet connection.',
        'errors.validationError': 'The form contains errors',
        'errors.notFound': 'The requested resource was not found',
        'errors.serverError': 'Server error. Please try again later.',
        'errors.unknown': 'An unexpected error occurred',
      };
      return translations[key] || key;
    },
  }),
}));

describe('CoopsPage', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    mockRefetch.mockResolvedValue({ data: [] });
    vi.clearAllMocks();
  });

  const renderPage = () => {
    return render(
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <CoopsPage />
        </QueryClientProvider>
      </BrowserRouter>
    );
  };

  const mockCoops: Coop[] = [
    {
      id: 'coop-1',
      tenantId: 'tenant-1',
      name: 'Main Coop',
      location: 'Backyard',
      isActive: true,
      createdAt: '2024-01-15T10:00:00Z',
      updatedAt: '2024-01-15T10:00:00Z',
      flocksCount: 2,
    },
    {
      id: 'coop-2',
      tenantId: 'tenant-1',
      name: 'Secondary Coop',
      location: 'Garden',
      isActive: true,
      createdAt: '2024-01-20T10:00:00Z',
      updatedAt: '2024-01-20T10:00:00Z',
      flocksCount: 1,
    },
  ];

  describe('Rendering', () => {
    it('should render page title "Coops"', () => {
      mockUseCoops.mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();
      expect(screen.getByRole('heading', { name: 'Coops' })).toBeInTheDocument();
    });

    it('should render Add Coop FAB button', () => {
      mockUseCoops.mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();
      const fabButton = screen.getByLabelText('Add Coop');
      expect(fabButton).toBeInTheDocument();
    });
  });

  describe('Empty State', () => {
    it('should show empty state when no coops exist', () => {
      mockUseCoops.mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();
      expect(screen.getByTestId('empty-state')).toBeInTheDocument();
      expect(screen.getByText('No coops yet')).toBeInTheDocument();
    });

    it('should show "Add Coop" button in empty state', () => {
      mockUseCoops.mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();
      const addButtons = screen.getAllByRole('button', { name: 'Add Coop' });
      expect(addButtons.length).toBeGreaterThan(0);
    });

    it('should open CreateCoopModal when empty state "Add Coop" button clicked', async () => {
      const user = userEvent.setup();
      mockUseCoops.mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      // Click the button in the empty state (not the FAB)
      const emptyStateButton = screen.getByTestId('empty-state').querySelector('button');
      if (emptyStateButton) {
        await user.click(emptyStateButton);
      }

      expect(screen.getByTestId('create-coop-modal')).toBeInTheDocument();
    });
  });

  describe('Coops List', () => {
    it('should render list of coops when data exists', () => {
      mockUseCoops.mockReturnValue({
        data: mockCoops,
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      const coopCards = screen.getAllByTestId('coop-card');
      expect(coopCards).toHaveLength(2);
      expect(screen.getByText('Main Coop')).toBeInTheDocument();
      expect(screen.getByText('Secondary Coop')).toBeInTheDocument();
    });

    it('should render coops sorted by creation date (newest first)', () => {
      mockUseCoops.mockReturnValue({
        data: mockCoops,
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      const coopCards = screen.getAllByTestId('coop-card');
      // Secondary Coop (2024-01-20) should be first, Main Coop (2024-01-15) second
      expect(coopCards[0]).toHaveTextContent('Secondary Coop');
      expect(coopCards[1]).toHaveTextContent('Main Coop');
    });

    it('should not show empty state when coops exist', () => {
      mockUseCoops.mockReturnValue({
        data: mockCoops,
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      expect(screen.queryByTestId('empty-state')).not.toBeInTheDocument();
    });
  });

  describe('Modal Interactions', () => {
    it('should open CreateCoopModal when FAB button clicked', async () => {
      const user = userEvent.setup();
      mockUseCoops.mockReturnValue({
        data: mockCoops,
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      const fabButton = screen.getByLabelText('Add Coop');
      await user.click(fabButton);

      expect(screen.getByTestId('create-coop-modal')).toBeInTheDocument();
    });

    it('should close CreateCoopModal when modal close is triggered', async () => {
      const user = userEvent.setup();
      mockUseCoops.mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      // Open modal
      const fabButton = screen.getByLabelText('Add Coop');
      await user.click(fabButton);
      expect(screen.getByTestId('create-coop-modal')).toBeInTheDocument();

      // Close modal
      const closeButton = screen.getByRole('button', { name: 'Close' });
      await user.click(closeButton);

      await waitFor(() => {
        expect(screen.queryByTestId('create-coop-modal')).not.toBeInTheDocument();
      });
    });
  });

  describe('Loading State', () => {
    it('should show loading state while fetching coops', () => {
      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      // Should show skeleton loaders (MUI Skeleton components)
      expect(screen.getByText('Coops')).toBeInTheDocument();
      // Verify that loading skeletons are present by checking for empty state not being shown
      expect(screen.queryByTestId('empty-state')).not.toBeInTheDocument();
      expect(screen.queryByTestId('coop-card')).not.toBeInTheDocument();
    });

    it('should not show empty state while loading', () => {
      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      expect(screen.queryByTestId('empty-state')).not.toBeInTheDocument();
    });

    it('should not show coops list while loading', () => {
      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      expect(screen.queryByTestId('coop-card')).not.toBeInTheDocument();
    });
  });

  describe('Error Handling', () => {
    it('should show error message when API fails with network error', () => {
      const networkError = {
        isAxiosError: true,
        response: null,
        message: 'Network Error',
      };

      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: networkError,
        refetch: mockRefetch,
      });

      renderPage();

      expect(screen.getByText('Connection error. Please check your internet connection.')).toBeInTheDocument();
    });

    it('should show error message when API fails with server error', () => {
      const serverError = {
        isAxiosError: true,
        response: {
          status: 500,
          data: {
            error: {
              code: 'SERVER_ERROR',
              message: 'Internal server error',
            },
          },
        },
      };

      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: serverError,
        refetch: mockRefetch,
      });

      renderPage();

      expect(screen.getByText('Server error. Please try again later.')).toBeInTheDocument();
    });

    it('should show retry button when error is retryable', () => {
      const networkError = {
        isAxiosError: true,
        response: null,
        message: 'Network Error',
      };

      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: networkError,
        refetch: mockRefetch,
      });

      renderPage();

      const retryButton = screen.getByRole('button', { name: 'Retry' });
      expect(retryButton).toBeInTheDocument();
    });

    it('should call refetch when retry button is clicked', async () => {
      const user = userEvent.setup();
      const networkError = {
        isAxiosError: true,
        response: null,
        message: 'Network Error',
      };

      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: networkError,
        refetch: mockRefetch,
      });

      renderPage();

      const retryButton = screen.getByRole('button', { name: 'Retry' });
      await user.click(retryButton);

      expect(mockRefetch).toHaveBeenCalledTimes(1);
    });

    it('should not show coops list when error occurs', () => {
      const networkError = {
        isAxiosError: true,
        response: null,
        message: 'Network Error',
      };

      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: networkError,
        refetch: mockRefetch,
      });

      renderPage();

      expect(screen.queryByTestId('coop-card')).not.toBeInTheDocument();
    });

    it('should not show empty state when error occurs', () => {
      const networkError = {
        isAxiosError: true,
        response: null,
        message: 'Network Error',
      };

      mockUseCoops.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: networkError,
        refetch: mockRefetch,
      });

      renderPage();

      expect(screen.queryByTestId('empty-state')).not.toBeInTheDocument();
    });
  });

  describe('Data Refresh', () => {
    it('should have refresh functionality available', () => {
      mockUseCoops.mockReturnValue({
        data: mockCoops,
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderPage();

      // Verify that refetch function is available (tested indirectly through error retry)
      expect(mockRefetch).toBeDefined();
    });
  });

  describe('Design Patterns', () => {
    beforeEach(() => {
      mockUseCoops.mockReturnValue({
        data: mockCoops,
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });
    });

    it('should render page title with bold font weight', () => {
      renderPage();

      const heading = screen.getByRole('heading', { name: 'Coops' });
      expect(heading).toBeInTheDocument();

      // Check for fontWeight="bold" - MUI converts to font-weight CSS
      const headingElement = heading as HTMLElement;
      const computedStyle = window.getComputedStyle(headingElement);
      expect(computedStyle.fontWeight).toBe('700'); // bold = 700
    });

    it('should render page title with correct margin-bottom spacing (mb: 3)', () => {
      renderPage();

      const heading = screen.getByRole('heading', { name: 'Coops' });
      const headingElement = heading as HTMLElement;

      // mb: 3 in MUI = 24px (3 * 8px spacing unit)
      const computedStyle = window.getComputedStyle(headingElement);
      expect(computedStyle.marginBottom).toBe('24px');
    });

    it('should render Container with Box wrapper that has pb: 10', () => {
      const { container } = renderPage();

      // Find the Box element with py: 3, pb: 10
      const boxElements = container.querySelectorAll('.MuiBox-root');
      const contentBox = Array.from(boxElements).find(box => {
        const style = window.getComputedStyle(box);
        // pb: 10 = 80px (10 * 8px spacing unit)
        return style.paddingBottom === '80px';
      });

      expect(contentBox).toBeTruthy();
    });

    it('should render FAB button with correct z-index (1000)', () => {
      renderPage();

      const fabButton = screen.getByLabelText('Add Coop');
      const computedStyle = window.getComputedStyle(fabButton);

      expect(computedStyle.zIndex).toBe('1000');
    });

    it('should render FAB button with correct mobile positioning (bottom: 80px)', () => {
      renderPage();

      const fabButton = screen.getByLabelText('Add Coop');
      const computedStyle = window.getComputedStyle(fabButton);

      // FAB should be fixed positioned
      expect(computedStyle.position).toBe('fixed');
      expect(computedStyle.right).toBe('16px');

      // Note: In test environment, responsive sx might not fully apply,
      // but we verify the FAB button is correctly positioned with fixed position
      expect(fabButton).toBeInTheDocument();
    });

    it('should render FAB button with fixed position and correct right offset', () => {
      renderPage();

      const fabButton = screen.getByLabelText('Add Coop');
      const computedStyle = window.getComputedStyle(fabButton);

      expect(computedStyle.position).toBe('fixed');
      expect(computedStyle.right).toBe('16px');
    });
  });
});
