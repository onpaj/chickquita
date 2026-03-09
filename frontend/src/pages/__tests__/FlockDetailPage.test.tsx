import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ToastContext, type ToastContextType } from '../../contexts/ToastContext';
import { FlockDetailPage } from '../FlockDetailPage';

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
        'flocks.details': 'Flock Details',
        'flocks.title': 'Flocks',
        'flocks.identifier': 'Identifier',
        'flocks.status': 'Status',
        'flocks.active': 'Active',
        'flocks.hatchDate': 'Hatch Date',
        'flocks.currentComposition': 'Composition',
        'flocks.hens': 'Hens',
        'flocks.roosters': 'Roosters',
        'flocks.chicks': 'Chicks',
        'flocks.total': 'Total',
        'flocks.createdAt': 'Created At',
        'flocks.updatedAt': 'Updated At',
        'flocks.archiveFlock': 'Archive',
        'flocks.archiveSuccess': 'Archived!',
        'flocks.viewHistory': 'History',
        'flocks.flockNotFound': 'Not found',
        'flocks.matureChicks.action': 'Mature Chicks',
        'flocks.matureChicks.disabledInactive': 'Inactive',
        'flocks.matureChicks.disabledNoChicks': 'No chicks',
        'common.edit': 'Edit',
        'errors.backToList': 'Back to list',
      };
      return map[key] ?? key;
    },
  }),
}));

vi.mock('../../features/flocks/hooks/useFlocks', () => ({
  useFlockDetail: vi.fn(() => ({
    data: {
      id: 'flock-1',
      coopId: 'coop-1',
      identifier: 'Flock A',
      isActive: true,
      hatchDate: null,
      currentHens: 5,
      currentRoosters: 1,
      currentChicks: 0,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
    isLoading: false,
    error: null,
  })),
  useArchiveFlock: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock('../../hooks/useErrorHandler', () => ({
  useErrorHandler: () => ({ handleError: vi.fn() }),
}));

vi.mock('../../features/flocks/components/EditFlockModal', () => ({
  EditFlockModal: () => null,
}));
vi.mock('../../features/flocks/components/ArchiveFlockDialog', () => ({
  ArchiveFlockDialog: () => null,
}));
vi.mock('../../features/flocks/components/MatureChicksModal', () => ({
  MatureChicksModal: () => null,
}));
vi.mock('../../components/ResourceNotFound', () => ({
  ResourceNotFound: ({ translationKey }: { translationKey: string }) => (
    <div data-testid="resource-not-found">{translationKey}</div>
  ),
}));
vi.mock('../../shared/components', () => ({
  CoopDetailSkeleton: () => <div data-testid="skeleton" />,
  StatCard: ({ label, value }: { label: string; value: number }) => (
    <div>{label}: {value}</div>
  ),
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
        <MemoryRouter initialEntries={['/coops/coop-1/flocks/flock-1']}>
          <Routes>
            <Route path="/coops/:coopId/flocks/:flockId" element={<FlockDetailPage />} />
          </Routes>
        </MemoryRouter>
      </ToastContext.Provider>
    </QueryClientProvider>
  );
}

describe('FlockDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calls setAppBar with flock identifier and onBack when data loads', () => {
    renderPage();
    expect(mockSetAppBar).toHaveBeenCalledWith(
      expect.objectContaining({ title: 'Flock A', onBack: expect.any(Function) })
    );
  });

  it('calls resetAppBar on unmount', () => {
    const { unmount } = renderPage();
    unmount();
    expect(mockResetAppBar).toHaveBeenCalled();
  });

  it('renders flock identifier in the detail card', () => {
    renderPage();
    expect(screen.getByText('Flock A')).toBeInTheDocument();
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
    expect(screen.getByText('History')).toBeInTheDocument();
  });
});
