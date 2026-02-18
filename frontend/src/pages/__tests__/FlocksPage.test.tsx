import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import FlocksPage from '../FlocksPage';
import type { Flock } from '../../features/flocks/api/flocksApi';
import type { Coop } from '../../features/coops/api/coopsApi';

// Mock hooks
const mockRefetch = vi.fn();
const mockUseFlocks = vi.fn();
const mockUseCoopDetail = vi.fn();
const mockArchiveFlock = vi.fn();

vi.mock('../../features/flocks/hooks/useFlocks', () => ({
  useFlocks: (coopId: string, includeInactive: boolean) => mockUseFlocks(coopId, includeInactive),
  useArchiveFlock: () => ({
    mutate: mockArchiveFlock,
    isPending: false,
  }),
}));

vi.mock('../../features/coops/hooks/useCoops', () => ({
  useCoopDetail: (coopId: string) => mockUseCoopDetail(coopId),
}));

// Mock useErrorHandler hook
const mockHandleError = vi.fn();
vi.mock('../../hooks/useErrorHandler', () => ({
  useErrorHandler: () => ({
    handleError: mockHandleError,
  }),
}));

// Mock useToast hook
const mockShowSuccess = vi.fn();
vi.mock('../../hooks/useToast', () => ({
  useToast: () => ({
    showSuccess: mockShowSuccess,
  }),
}));

// Mock FlockCard component
vi.mock('../../features/flocks/components/FlockCard', () => ({
  FlockCard: ({ flock, onEdit, onArchive }: {
    flock: Flock;
    onEdit: (f: Flock) => void;
    onArchive: (f: Flock) => void;
  }) => (
    <div data-testid="flock-card">
      <p>{flock.identifier}</p>
      <span>{flock.isActive ? 'Active' : 'Archived'}</span>
      <button onClick={() => onEdit(flock)}>Edit</button>
      <button onClick={() => onArchive(flock)}>Archive</button>
    </div>
  ),
}));

// Mock FlocksEmptyState component
vi.mock('../../features/flocks/components/FlocksEmptyState', () => ({
  FlocksEmptyState: ({ onAddClick }: { onAddClick: () => void }) => (
    <div data-testid="empty-state">
      <p>No flocks yet</p>
      <button onClick={onAddClick}>Add Flock</button>
    </div>
  ),
}));

// Mock CreateFlockModal component
vi.mock('../../features/flocks/components/CreateFlockModal', () => ({
  CreateFlockModal: ({ open, onClose }: { open: boolean; onClose: () => void }) => (
    open ? (
      <div data-testid="create-flock-modal">
        <p>Create Flock Modal</p>
        <button onClick={onClose}>Close</button>
      </div>
    ) : null
  ),
}));

// Mock EditFlockModal component
vi.mock('../../features/flocks/components/EditFlockModal', () => ({
  EditFlockModal: ({ open, onClose, flock }: {
    open: boolean;
    onClose: () => void;
    flock: Flock;
  }) => (
    open ? (
      <div data-testid="edit-flock-modal">
        <p>Edit Flock Modal - {flock.identifier}</p>
        <button onClick={onClose}>Close</button>
      </div>
    ) : null
  ),
}));

// Mock ArchiveFlockDialog component
vi.mock('../../features/flocks/components/ArchiveFlockDialog', () => ({
  ArchiveFlockDialog: ({
    open,
    onClose,
    onConfirm,
    flockIdentifier
  }: {
    open: boolean;
    onClose: () => void;
    onConfirm: () => void;
    flockIdentifier: string;
  }) => (
    open ? (
      <div data-testid="archive-flock-dialog">
        <p>Archive {flockIdentifier}?</p>
        <button onClick={onConfirm}>Confirm</button>
        <button onClick={onClose}>Cancel</button>
      </div>
    ) : null
  ),
}));

// Mock i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, _params?: unknown) => {
      const translations: Record<string, string> = {
        'flocks.title': 'Flocks',
        'flocks.addFlock': 'Add Flock',
        'flocks.filterStatus': 'Filter by status',
        'flocks.active': 'Active',
        'flocks.archiveSuccess': 'Flock archived successfully',
        'common.all': 'All',
        'common.error': 'Error',
        'common.retry': 'Retry',
        'common.loading': 'Loading',
        'errors.network': 'Network error',
      };
      return translations[key] || key;
    },
  }),
}));

const mockFlocks: Flock[] = [
  {
    id: 'flock-1',
    coopId: 'coop-1',
    identifier: 'Flock 1',
    hatchDate: '2024-01-01',
    currentHens: 10,
    currentRoosters: 2,
    currentChicks: 5,
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 'flock-2',
    coopId: 'coop-1',
    identifier: 'Flock 2',
    hatchDate: '2024-02-01',
    currentHens: 8,
    currentRoosters: 1,
    currentChicks: 3,
    isActive: true,
    createdAt: '2024-02-01T00:00:00Z',
    updatedAt: '2024-02-01T00:00:00Z',
  },
  {
    id: 'flock-3',
    coopId: 'coop-1',
    identifier: 'Flock 3 (Archived)',
    hatchDate: '2023-01-01',
    currentHens: 0,
    currentRoosters: 0,
    currentChicks: 0,
    isActive: false,
    createdAt: '2023-01-01T00:00:00Z',
    updatedAt: '2024-03-01T00:00:00Z',
  },
];

const mockCoop: Coop = {
  id: 'coop-1',
  name: 'Test Coop',
  location: 'Test Location',
  capacity: 50,
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

const renderFlocksPage = (coopId = 'coop-1') => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  // Set initial route to match the expected path
  window.history.pushState({}, 'Test page', `/coops/${coopId}/flocks`);

  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('FlocksPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseFlocks.mockReturnValue({
      data: mockFlocks,
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    });
    mockUseCoopDetail.mockReturnValue({
      data: mockCoop,
      isLoading: false,
      error: null,
    });
  });

  describe('Rendering - Basic Layout', () => {
    it('renders page title', () => {
      renderFlocksPage();
      expect(screen.getByText('Flocks')).toBeInTheDocument();
    });

    it('renders filter toggle buttons', () => {
      renderFlocksPage();
      expect(screen.getByRole('button', { name: /active/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /all/i })).toBeInTheDocument();
    });

    it('renders add flock FAB button', () => {
      renderFlocksPage();
      expect(screen.getByRole('button', { name: /add flock/i })).toBeInTheDocument();
    });

    it('renders within coop context', () => {
      renderFlocksPage();
      expect(mockUseCoopDetail).toHaveBeenCalledWith('coop-1');
    });
  });

  describe('Rendering - Flocks List', () => {
    it('displays flock cards when data is loaded', () => {
      renderFlocksPage();
      const cards = screen.getAllByTestId('flock-card');
      expect(cards).toHaveLength(2); // Only active flocks by default
    });

    it('displays only active flocks by default', () => {
      renderFlocksPage();
      expect(screen.getByText('Flock 1')).toBeInTheDocument();
      expect(screen.getByText('Flock 2')).toBeInTheDocument();
      expect(screen.queryByText('Flock 3 (Archived)')).not.toBeInTheDocument();
    });

    it('sorts flocks by creation date (newest first)', () => {
      renderFlocksPage();
      const cards = screen.getAllByTestId('flock-card');

      // Flock 2 was created after Flock 1, so it should be first
      expect(cards[0]).toHaveTextContent('Flock 2');
      expect(cards[1]).toHaveTextContent('Flock 1');
    });
  });

  describe('Rendering - Empty State', () => {
    it('shows empty state when no flocks exist', () => {
      mockUseFlocks.mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderFlocksPage();
      expect(screen.getByTestId('empty-state')).toBeInTheDocument();
      expect(screen.getByText('No flocks yet')).toBeInTheDocument();
    });

    it('shows empty state when all flocks are archived and filter is active', () => {
      mockUseFlocks.mockReturnValue({
        data: [mockFlocks[2]], // Only archived flock
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderFlocksPage();
      expect(screen.getByTestId('empty-state')).toBeInTheDocument();
    });

    it('clicking add button in empty state opens create modal', async () => {
      const user = userEvent.setup();
      mockUseFlocks.mockReturnValue({
        data: [],
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderFlocksPage();

      const addButton = screen.getByText('Add Flock');
      await user.click(addButton);

      expect(screen.getByTestId('create-flock-modal')).toBeInTheDocument();
    });
  });

  describe('Rendering - Loading State', () => {
    it('shows loading skeletons when data is loading', () => {
      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        refetch: mockRefetch,
      });

      const { container } = renderFlocksPage();
      const skeletons = container.querySelectorAll('.MuiSkeleton-root');
      expect(skeletons.length).toBeGreaterThan(0);
    });

    it('does not show flock cards while loading', () => {
      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        refetch: mockRefetch,
      });

      renderFlocksPage();
      expect(screen.queryByTestId('flock-card')).not.toBeInTheDocument();
    });

    it('does not show empty state while loading', () => {
      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        refetch: mockRefetch,
      });

      renderFlocksPage();
      expect(screen.queryByTestId('empty-state')).not.toBeInTheDocument();
    });
  });

  describe('Rendering - Error State', () => {
    it('shows error message on API failure', () => {
      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: new Error('Network error'),
        refetch: mockRefetch,
      });

      renderFlocksPage();
      // Should show error - using getAllByText since there might be multiple error messages
      const errorElements = screen.getAllByText(/error/i);
      expect(errorElements.length).toBeGreaterThan(0);
    });

    it('shows retry button on error with retry capability', async () => {
      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: new Error('Network error'),
        refetch: mockRefetch,
      });

      renderFlocksPage();
      const retryButton = screen.getByRole('button', { name: /retry/i });
      expect(retryButton).toBeInTheDocument();
    });

    it('calls refetch when retry button is clicked', async () => {
      const user = userEvent.setup();
      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: new Error('Network error'),
        refetch: mockRefetch,
      });

      renderFlocksPage();
      const retryButton = screen.getByRole('button', { name: /retry/i });
      await user.click(retryButton);

      expect(mockRefetch).toHaveBeenCalled();
    });
  });

  describe('Filter Controls', () => {
    it('defaults to active filter', () => {
      renderFlocksPage();

      // Check that useFlocks is called with includeInactive = false
      expect(mockUseFlocks).toHaveBeenCalledWith('coop-1', false);
    });

    it('switches to show all flocks when All is clicked', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const allButton = screen.getByRole('button', { name: /all/i });
      await user.click(allButton);

      // Should call useFlocks with includeInactive = true
      await waitFor(() => {
        expect(mockUseFlocks).toHaveBeenCalledWith('coop-1', true);
      });
    });

    it('displays archived flocks when All filter is selected', async () => {
      const user = userEvent.setup();

      // Start with all flocks available
      mockUseFlocks.mockReturnValue({
        data: mockFlocks,
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderFlocksPage();

      // Click to switch to All filter
      const allButton = screen.getByRole('button', { name: /all/i });
      await user.click(allButton);

      // After clicking All, should show all 3 flocks (including archived)
      await waitFor(() => {
        const cards = screen.getAllByTestId('flock-card');
        expect(cards).toHaveLength(3);
      });
    });

    it('visual indicator shows archived status on archived flocks', async () => {
      const user = userEvent.setup();

      // Start with all flocks
      mockUseFlocks.mockReturnValue({
        data: mockFlocks,
        isLoading: false,
        error: null,
        refetch: mockRefetch,
      });

      renderFlocksPage();

      // Switch to All filter to see archived flocks
      const allButton = screen.getByRole('button', { name: /all/i });
      await user.click(allButton);

      // Wait for the archived flock to appear
      await waitFor(() => {
        const archivedCard = screen.getByText('Flock 3 (Archived)').closest('[data-testid="flock-card"]');
        expect(archivedCard).toHaveTextContent('Archived');
      });
    });
  });

  describe('FAB Button and Modals', () => {
    it('opens create modal when FAB is clicked', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const fabButton = screen.getByRole('button', { name: /add flock/i });
      await user.click(fabButton);

      expect(screen.getByTestId('create-flock-modal')).toBeInTheDocument();
    });

    it('closes create modal when close is called', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const fabButton = screen.getByRole('button', { name: /add flock/i });
      await user.click(fabButton);

      const closeButton = screen.getByText('Close');
      await user.click(closeButton);

      expect(screen.queryByTestId('create-flock-modal')).not.toBeInTheDocument();
    });
  });

  describe('Edit Flock Flow', () => {
    it('opens edit modal when edit button is clicked on a flock card', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const editButtons = screen.getAllByText('Edit');
      await user.click(editButtons[0]);

      expect(screen.getByTestId('edit-flock-modal')).toBeInTheDocument();
    });

    it('shows correct flock data in edit modal', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const editButtons = screen.getAllByText('Edit');
      await user.click(editButtons[0]);

      // Flock 2 is first due to sorting by newest
      expect(screen.getByText(/Edit Flock Modal - Flock 2/i)).toBeInTheDocument();
    });

    it('closes edit modal when close is called', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const editButtons = screen.getAllByText('Edit');
      await user.click(editButtons[0]);

      const closeButton = screen.getByText('Close');
      await user.click(closeButton);

      expect(screen.queryByTestId('edit-flock-modal')).not.toBeInTheDocument();
    });
  });

  describe('Archive Flock Flow', () => {
    it('opens archive dialog when archive button is clicked', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const archiveButtons = screen.getAllByText('Archive');
      await user.click(archiveButtons[0]);

      expect(screen.getByTestId('archive-flock-dialog')).toBeInTheDocument();
    });

    it('shows correct flock identifier in archive dialog', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const archiveButtons = screen.getAllByText('Archive');
      await user.click(archiveButtons[0]);

      expect(screen.getByText(/Archive Flock 2\?/i)).toBeInTheDocument();
    });

    it('calls archive mutation when confirmed', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const archiveButtons = screen.getAllByText('Archive');
      await user.click(archiveButtons[0]);

      const confirmButton = screen.getByText('Confirm');
      await user.click(confirmButton);

      expect(mockArchiveFlock).toHaveBeenCalled();
    });

    it('closes archive dialog when cancel is clicked', async () => {
      const user = userEvent.setup();
      renderFlocksPage();

      const archiveButtons = screen.getAllByText('Archive');
      await user.click(archiveButtons[0]);

      const cancelButton = screen.getByText('Cancel');
      await user.click(cancelButton);

      expect(screen.queryByTestId('archive-flock-dialog')).not.toBeInTheDocument();
    });

    it('shows success toast on successful archive', async () => {
      const user = userEvent.setup();

      // Mock successful archive
      mockArchiveFlock.mockImplementation((params, callbacks) => {
        callbacks.onSuccess();
      });

      renderFlocksPage();

      const archiveButtons = screen.getAllByText('Archive');
      await user.click(archiveButtons[0]);

      const confirmButton = screen.getByText('Confirm');
      await user.click(confirmButton);

      await waitFor(() => {
        expect(mockShowSuccess).toHaveBeenCalledWith('Flock archived successfully');
      });
    });
  });

  describe('Mobile Responsiveness', () => {
    it('FAB button has fixed positioning', () => {
      renderFlocksPage();
      const fabButton = screen.getByRole('button', { name: /add flock/i });
      const styles = window.getComputedStyle(fabButton);

      expect(styles.position).toBe('fixed');
    });

    it('FAB button is positioned in bottom-right corner', () => {
      renderFlocksPage();
      const fabButton = screen.getByRole('button', { name: /add flock/i });
      const styles = window.getComputedStyle(fabButton);

      expect(styles.bottom).toBeTruthy();
      expect(styles.right).toBeTruthy();
    });
  });

  describe('Accessibility', () => {
    it('page has proper heading hierarchy', () => {
      renderFlocksPage();
      const heading = screen.getByRole('heading', { level: 1 });
      expect(heading).toHaveTextContent('Flocks');
    });

    it('filter toggle has aria-label', () => {
      renderFlocksPage();
      const toggleGroup = screen.getByRole('group', { name: /filter by status/i });
      expect(toggleGroup).toBeInTheDocument();
    });

    it('FAB button has aria-label', () => {
      renderFlocksPage();
      const fabButton = screen.getByRole('button', { name: /add flock/i });
      expect(fabButton).toHaveAttribute('aria-label', 'Add Flock');
    });
  });
});
