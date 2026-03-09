import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ToastContext, type ToastContextType } from '../../contexts/ToastContext';
import { CoopDetailPage } from '../CoopDetailPage';

// Capture what gets set on the AppBar
const mockSetAppBar = vi.fn();
const mockResetAppBar = vi.fn();

vi.mock('../../context/AppBarContext', async () => {
  const actual = await vi.importActual('../../context/AppBarContext');
  return {
    ...actual,
    useAppBar: () => ({
      title: null,
      onBack: null,
      setAppBar: mockSetAppBar,
      resetAppBar: mockResetAppBar,
    }),
  };
});

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'coops.details': 'Coop Details',
        'coops.title': 'Coops',
        'coops.coopName': 'Coop Name',
        'coops.location': 'Location',
        'coops.status': 'Status',
        'coops.active': 'Active',
        'coops.archived': 'Archived',
        'coops.createdAt': 'Created At',
        'coops.updatedAt': 'Updated At',
        'coops.archiveCoop': 'Archive',
        'coops.archiveSuccess': 'Archived!',
        'coops.deleteSuccess': 'Deleted!',
        'coops.deleteErrorHasFlocks': 'Has flocks',
        'coops.coopNotFound': 'Not found',
        'common.edit': 'Edit',
        'common.delete': 'Delete',
        'common.all': 'All',
        'errors.backToList': 'Back to list',
        'flocks.title': 'Flocks',
        'flocks.active': 'Active',
        'flocks.filterStatus': 'Filter Status',
        'flocks.addFlock': 'Add Flock',
      };
      return map[key] ?? key;
    },
  }),
}));

vi.mock('../../features/coops/hooks/useCoopDetail', () => ({
  useCoopDetail: vi.fn(() => ({
    data: {
      id: 'coop-1',
      name: 'Test Coop',
      location: 'Farm A',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
      flocksCount: 0,
    },
    isLoading: false,
    error: null,
  })),
}));

vi.mock('../../features/coops/hooks/useCoops', () => ({
  useArchiveCoop: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteCoop: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock('../../features/flocks/hooks/useFlocks', () => ({
  useFlocks: vi.fn(() => ({ data: [], isLoading: false })),
  useArchiveFlock: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock('../../hooks/useErrorHandler', () => ({
  useErrorHandler: () => ({ handleError: vi.fn() }),
}));

vi.mock('../../features/coops/components/EditCoopModal', () => ({
  EditCoopModal: () => null,
}));
vi.mock('../../features/coops/components/ArchiveCoopDialog', () => ({
  ArchiveCoopDialog: () => null,
}));
vi.mock('../../features/coops/components/DeleteCoopDialog', () => ({
  DeleteCoopDialog: () => null,
}));
vi.mock('../../features/flocks/components/CreateFlockModal', () => ({
  CreateFlockModal: () => null,
}));
vi.mock('../../features/flocks/components/EditFlockModal', () => ({
  EditFlockModal: () => null,
}));
vi.mock('../../features/flocks/components/ArchiveFlockDialog', () => ({
  ArchiveFlockDialog: () => null,
}));
vi.mock('../../features/flocks/components/FlockCard', () => ({
  FlockCard: ({ flock }: { flock: { identifier: string } }) => (
    <div data-testid="flock-card">{flock.identifier}</div>
  ),
}));
vi.mock('../../features/flocks/components/FlocksEmptyState', () => ({
  FlocksEmptyState: () => <div data-testid="flocks-empty-state" />,
}));
vi.mock('../../features/flocks/components/FlockCardSkeleton', () => ({
  FlockCardSkeleton: () => <div data-testid="flock-card-skeleton" />,
}));
vi.mock('../../components/ResourceNotFound', () => ({
  ResourceNotFound: ({ translationKey }: { translationKey: string }) => (
    <div data-testid="resource-not-found">{translationKey}</div>
  ),
}));
vi.mock('../../shared/components', () => ({
  CoopDetailSkeleton: () => <div data-testid="skeleton" />,
}));

const mockToast: ToastContextType = {
  showToast: vi.fn(),
  showError: vi.fn(),
  showSuccess: vi.fn(),
  showWarning: vi.fn(),
  showInfo: vi.fn(),
  hideToast: vi.fn(),
};

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <ToastContext.Provider value={mockToast}>
        <MemoryRouter initialEntries={['/coops/coop-1']}>
          <Routes>
            <Route path="/coops/:id" element={<CoopDetailPage />} />
          </Routes>
        </MemoryRouter>
      </ToastContext.Provider>
    </QueryClientProvider>
  );
}

describe('CoopDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calls setAppBar with coop name and onBack when data loads', () => {
    renderPage();
    expect(mockSetAppBar).toHaveBeenCalledWith(
      expect.objectContaining({ title: 'Test Coop', onBack: expect.any(Function) })
    );
  });

  it('calls resetAppBar on unmount', () => {
    const { unmount } = renderPage();
    unmount();
    expect(mockResetAppBar).toHaveBeenCalled();
  });

  it('renders coop name in the detail card', () => {
    renderPage();
    expect(screen.getByText('Test Coop')).toBeInTheDocument();
  });

  it('does not render an inline back button inside the page', () => {
    renderPage();
    const backButtons = screen.queryAllByRole('button', { name: /back/i });
    expect(backButtons).toHaveLength(0);
  });

  it('renders action buttons', () => {
    renderPage();
    expect(screen.getByText('Edit')).toBeInTheDocument();
    expect(screen.getByText('Archive')).toBeInTheDocument();
    expect(screen.getByText('Delete')).toBeInTheDocument();
  });

  it('renders Flocks section heading', () => {
    renderPage();
    expect(screen.getByText('Flocks')).toBeInTheDocument();
  });

  it('renders empty state when no flocks', () => {
    renderPage();
    expect(screen.getByTestId('flocks-empty-state')).toBeInTheDocument();
  });

  it('renders flock cards when flocks are present', async () => {
    const { useFlocks } = await import('../../features/flocks/hooks/useFlocks');
    vi.mocked(useFlocks).mockReturnValue({
      data: [
        {
          id: 'flock-1',
          coopId: 'coop-1',
          identifier: 'Flock Alpha',
          isActive: true,
          hatchDate: null,
          currentHens: 5,
          currentRoosters: 1,
          currentChicks: 0,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ],
      isLoading: false,
    } as unknown as ReturnType<typeof useFlocks>);

    renderPage();
    expect(screen.getByTestId('flock-card')).toBeInTheDocument();
    expect(screen.getByText('Flock Alpha')).toBeInTheDocument();
  });

  it('renders Add Flock FAB', () => {
    renderPage();
    expect(screen.getByRole('button', { name: 'Add Flock' })).toBeInTheDocument();
  });

  it('onBack navigates to /coops', () => {
    renderPage();
    const callArgs = mockSetAppBar.mock.calls[0][0];
    callArgs.onBack();
    expect(mockNavigate).toHaveBeenCalledWith('/coops');
  });
});
