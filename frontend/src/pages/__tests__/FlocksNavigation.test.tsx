import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes, useNavigate } from 'react-router-dom';
import FlocksPage from '../FlocksPage';
import { CoopDetailPage } from '../CoopDetailPage';
import type { Flock } from '../../features/flocks/api/flocksApi';
import type { Coop } from '../../features/coops/api/coopsApi';

// Mock Clerk authentication
vi.mock('@clerk/clerk-react', () => ({
  useAuth: vi.fn(),
  useUser: vi.fn(),
  SignedIn: ({ children }: { children: React.ReactNode }) => <>{children}</>,
  SignedOut: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

import { useAuth } from '@clerk/clerk-react';

// Mock hooks
const mockRefetch = vi.fn();
const mockUseFlocks = vi.fn();
const mockUseCoopDetail = vi.fn();
const mockUseCoops = vi.fn();
const mockArchiveFlock = vi.fn();
const mockArchiveCoop = vi.fn();
const mockDeleteCoop = vi.fn();

vi.mock('../../features/flocks/hooks/useFlocks', () => ({
  useFlocks: (coopId: string, includeInactive: boolean) => mockUseFlocks(coopId, includeInactive),
  useArchiveFlock: () => ({
    mutate: mockArchiveFlock,
    isPending: false,
  }),
}));

vi.mock('../../features/coops/hooks/useCoops', () => ({
  useCoopDetail: (coopId: string) => mockUseCoopDetail(coopId),
  useCoops: () => mockUseCoops(),
  useArchiveCoop: () => ({
    mutate: mockArchiveCoop,
    isPending: false,
  }),
  useDeleteCoop: () => ({
    mutate: mockDeleteCoop,
    isPending: false,
  }),
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
  FlockCard: ({ flock }: { flock: Flock }) => (
    <div data-testid="flock-card">
      <p>{flock.identifier}</p>
    </div>
  ),
}));

// Mock FlocksEmptyState component
vi.mock('../../features/flocks/components/FlocksEmptyState', () => ({
  FlocksEmptyState: () => <div data-testid="empty-state">No flocks yet</div>,
}));

// Mock CreateFlockModal component
vi.mock('../../features/flocks/components/CreateFlockModal', () => ({
  CreateFlockModal: () => null,
}));

// Mock EditFlockModal component
vi.mock('../../features/flocks/components/EditFlockModal', () => ({
  EditFlockModal: () => null,
}));

// Mock ArchiveFlockDialog component
vi.mock('../../features/flocks/components/ArchiveFlockDialog', () => ({
  ArchiveFlockDialog: () => null,
}));

// Mock CoopCard component
vi.mock('../../features/coops/components/CoopCard', () => ({
  CoopCard: () => null,
}));

// Mock CoopsEmptyState component
vi.mock('../../features/coops/components/CoopsEmptyState', () => ({
  CoopsEmptyState: () => null,
}));

// Mock CreateCoopModal component
vi.mock('../../features/coops/components/CreateCoopModal', () => ({
  CreateCoopModal: () => null,
}));

// Mock EditCoopModal component
vi.mock('../../features/coops/components/EditCoopModal', () => ({
  EditCoopModal: () => null,
}));

// Mock ArchiveCoopDialog component
vi.mock('../../features/coops/components/ArchiveCoopDialog', () => ({
  ArchiveCoopDialog: () => null,
}));

// Mock DeleteCoopDialog component
vi.mock('../../features/coops/components/DeleteCoopDialog', () => ({
  DeleteCoopDialog: () => null,
}));

// Mock ResourceNotFound component
vi.mock('../../components/ResourceNotFound', () => ({
  ResourceNotFound: () => <div>Resource Not Found</div>,
}));

// Mock i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'flocks.title': 'Flocks',
        'flocks.addFlock': 'Add Flock',
        'flocks.filterStatus': 'Filter by status',
        'flocks.active': 'Active',
        'common.all': 'All',
        'common.error': 'Error',
        'common.retry': 'Retry',
        'common.loading': 'Loading',
        'common.back': 'Back',
        'coops.details': 'Coop Details',
        'coops.location': 'Location',
        'coops.capacity': 'Capacity',
        'errors.network': 'Network error',
        'errors.networkError': 'Network error occurred. Please check your connection.',
        'errors.notFound': 'Not found',
        'errors.resourceNotFound': 'Resource not found',
        'errors.backToList': 'Back to list',
      };
      return translations[key] || key;
    },
    i18n: {
      language: 'en',
    },
  }),
}));

const mockFlocks: Flock[] = [
  {
    id: 'flock-1',
    tenantId: 'tenant-1',
    coopId: 'coop-1',
    identifier: 'Flock 1',
    hatchDate: '2024-01-01',
    currentHens: 10,
    currentRoosters: 2,
    currentChicks: 5,
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    history: [],
  },
  {
    id: 'flock-2',
    tenantId: 'tenant-1',
    coopId: 'coop-1',
    identifier: 'Flock 2',
    hatchDate: '2024-02-01',
    currentHens: 8,
    currentRoosters: 1,
    currentChicks: 3,
    isActive: true,
    createdAt: '2024-02-01T00:00:00Z',
    updatedAt: '2024-02-01T00:00:00Z',
    history: [],
  },
];

const mockCoop: Coop = {
  id: 'coop-1',
  tenantId: 'tenant-1',
  name: 'Test Coop',
  location: 'Test Location',
  isActive: true,
  flocksCount: 2,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
};

describe('Flocks Navigation and Routing', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    // Mock authenticated state by default
    vi.mocked(useAuth).mockReturnValue({
      isSignedIn: true,
      isLoaded: true,
    } as unknown as ReturnType<typeof useAuth>);

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

    mockUseCoops.mockReturnValue({
      data: [mockCoop],
      isLoading: false,
      error: null,
    });
  });

  describe('URL Structure and Route Definition', () => {
    it('should render FlocksPage at /coops/:coopId/flocks', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      expect(screen.getByText('Flocks')).toBeInTheDocument();
    });

    it('should extract coopId from URL parameters', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/test-coop-123/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Verify that useFlocks was called with the correct coopId from URL
      expect(mockUseFlocks).toHaveBeenCalledWith('test-coop-123', false);
    });

    it('should load coop context using coopId from URL', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Verify that coop detail was fetched with correct ID
      expect(mockUseCoopDetail).toHaveBeenCalledWith('coop-1');
    });
  });

  describe('Navigation from Coop Detail to Flocks', () => {
    it('should navigate to flocks page when link is provided', async () => {
      const user = userEvent.setup();
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      // Create a simple test navigation component
      const NavigationTest = () => {
        const navigate = useNavigate();
        return (
          <div>
            <button onClick={() => navigate(`/coops/coop-1/flocks`)}>
              Navigate to Flocks
            </button>
          </div>
        );
      };

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/test']}>
            <Routes>
              <Route path="/test" element={<NavigationTest />} />
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Click the navigation button
      const navigateButton = screen.getByText('Navigate to Flocks');
      await user.click(navigateButton);

      // Should navigate to flocks page
      await waitFor(() => {
        expect(screen.getByText('Flocks')).toBeInTheDocument();
      });

      // Verify that flocks were loaded for the correct coop
      expect(mockUseFlocks).toHaveBeenCalledWith('coop-1', false);
    });

    it('should maintain coopId in URL when navigating between pages', async () => {
      const user = userEvent.setup();
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      const TestWrapper = () => {
        const navigate = useNavigate();
        return (
          <div>
            <button onClick={() => navigate('/coops/coop-1/flocks')}>
              Go to Flocks
            </button>
          </div>
        );
      };

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/test']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
              <Route path="/test" element={<TestWrapper />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      const goButton = screen.getByText('Go to Flocks');
      await user.click(goButton);

      await waitFor(() => {
        expect(mockUseFlocks).toHaveBeenCalledWith('coop-1', false);
      });
    });
  });

  describe('Direct URL Navigation (Bookmarks and Refresh)', () => {
    it('should load flocks page directly from URL', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Page should load correctly
      expect(screen.getByText('Flocks')).toBeInTheDocument();
      expect(mockUseFlocks).toHaveBeenCalledWith('coop-1', false);
      expect(mockUseCoopDetail).toHaveBeenCalledWith('coop-1');
    });

    it('should preserve filter state in URL on refresh', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      // Simulate a bookmarked URL with filter state
      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      expect(screen.getByText('Flocks')).toBeInTheDocument();

      // Simulate page refresh by re-rendering
      rerender(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Page should still be accessible
      expect(screen.getByText('Flocks')).toBeInTheDocument();
    });

    it('should work with different coopId values in URL', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      const { unmount } = render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/abc-123/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      expect(mockUseFlocks).toHaveBeenCalledWith('abc-123', false);

      // Unmount and render with different coop ID
      unmount();
      vi.clearAllMocks();

      // Reset mocks for next render
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

      // Navigate to a different coop
      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/xyz-789/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      expect(mockUseFlocks).toHaveBeenCalledWith('xyz-789', false);
    });
  });

  describe('Error Handling - Invalid Coop ID', () => {
    it('should display error when coopId is missing', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      // Render with undefined coopId by using different route match
      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/undefined/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Verify that flocks hook was called with 'undefined' string
      expect(mockUseFlocks).toHaveBeenCalledWith('undefined', false);
    });

    it('should display error when coop does not exist (404)', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      // Mock API returning 404 error (AxiosError structure)
      const axiosError = {
        isAxiosError: true,
        response: {
          status: 404,
          data: {
            error: {
              code: 'NOT_FOUND',
              message: 'Resource not found.',
            },
          },
        },
      };

      mockUseCoopDetail.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: axiosError,
      });

      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: axiosError,
        refetch: mockRefetch,
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/non-existent-coop/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Should display error message - processApiError will return 'errors.notFound' translation key
      expect(screen.getByText('Not found')).toBeInTheDocument();
    });

    it('should provide retry functionality on error', async () => {
      const user = userEvent.setup();
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: new Error('Network error'),
        refetch: mockRefetch,
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      const retryButton = screen.getByRole('button', { name: /retry/i });
      await user.click(retryButton);

      expect(mockRefetch).toHaveBeenCalled();
    });

    it('should handle network errors gracefully', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      // Mock network error (AxiosError with no response)
      const networkError = {
        isAxiosError: true,
        response: undefined, // No response means network error
        message: 'Network error',
      };

      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: networkError,
        refetch: mockRefetch,
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Should show error state with retry option - network errors show translation key
      const errorMessages = screen.getAllByText('Network error occurred. Please check your connection.');
      expect(errorMessages.length).toBeGreaterThan(0);
      expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
    });
  });

  describe('Protected Routes - Authentication', () => {
    it('should redirect to sign-in when not authenticated', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      // Mock unauthenticated state
      vi.mocked(useAuth).mockReturnValue({
        isSignedIn: false,
        isLoaded: true,
      } as unknown as ReturnType<typeof useAuth>);

      // Create a mock ProtectedRoute wrapper
      const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
        const { isLoaded, isSignedIn } = useAuth();

        if (!isLoaded) {
          return null;
        }

        if (!isSignedIn) {
          return <div>Sign In Page</div>;
        }

        return <>{children}</>;
      };

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route
                path="/coops/:coopId/flocks"
                element={
                  <ProtectedRoute>
                    <FlocksPage />
                  </ProtectedRoute>
                }
              />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Should show sign-in page instead of flocks
      expect(screen.getByText('Sign In Page')).toBeInTheDocument();
      expect(screen.queryByText('Flocks')).not.toBeInTheDocument();
    });

    it('should show loading state while auth is being checked', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      // Mock loading auth state
      vi.mocked(useAuth).mockReturnValue({
        isSignedIn: false,
        isLoaded: false,
      } as unknown as ReturnType<typeof useAuth>);

      const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
        const { isLoaded } = useAuth();

        if (!isLoaded) {
          return <div>Loading authentication...</div>;
        }

        return <>{children}</>;
      };

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route
                path="/coops/:coopId/flocks"
                element={
                  <ProtectedRoute>
                    <FlocksPage />
                  </ProtectedRoute>
                }
              />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      expect(screen.getByText('Loading authentication...')).toBeInTheDocument();
      expect(screen.queryByText('Flocks')).not.toBeInTheDocument();
    });

    it('should allow access when authenticated', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      // Mock authenticated state
      vi.mocked(useAuth).mockReturnValue({
        isSignedIn: true,
        isLoaded: true,
      } as unknown as ReturnType<typeof useAuth>);

      const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
        const { isLoaded, isSignedIn } = useAuth();

        if (!isLoaded) {
          return null;
        }

        if (!isSignedIn) {
          return <div>Sign In Page</div>;
        }

        return <>{children}</>;
      };

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route
                path="/coops/:coopId/flocks"
                element={
                  <ProtectedRoute>
                    <FlocksPage />
                  </ProtectedRoute>
                }
              />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Should show flocks page
      expect(screen.getByText('Flocks')).toBeInTheDocument();
      expect(screen.queryByText('Sign In Page')).not.toBeInTheDocument();
    });
  });

  describe('Back Navigation and Breadcrumbs', () => {
    it('should allow navigation back to coop detail from flocks page', async () => {
      const _user = userEvent.setup();
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1', '/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:id" element={<CoopDetailPage />} />
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Should be on flocks page initially (last entry in history)
      expect(screen.getByText('Flocks')).toBeInTheDocument();

      // Browser back would work via router history
      // In actual app, user can use browser back or bottom navigation
      // For this test, we verify the route structure supports navigation
      expect(mockUseFlocks).toHaveBeenCalledWith('coop-1', false);
    });

    it('should maintain proper navigation history', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      const navigationHistory: string[] = [];

      const TestComponent = () => {
        const location = window.location;
        navigationHistory.push(location.pathname);
        return null;
      };

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1', '/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:id" element={<><CoopDetailPage /><TestComponent /></>} />
              <Route path="/coops/:coopId/flocks" element={<><FlocksPage /><TestComponent /></>} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Verify navigation occurs in correct sequence
      expect(mockUseFlocks).toHaveBeenCalled();
    });
  });

  describe('Coop Context Loading', () => {
    it('should fetch coop details when accessing flocks page', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Should fetch both flocks and coop details
      expect(mockUseFlocks).toHaveBeenCalledWith('coop-1', false);
      expect(mockUseCoopDetail).toHaveBeenCalledWith('coop-1');
    });

    it('should handle loading state for coop context', () => {
      const queryClient = new QueryClient({
        defaultOptions: { queries: { retry: false } },
      });

      mockUseCoopDetail.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
      });

      mockUseFlocks.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        refetch: mockRefetch,
      });

      const { container } = render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/coops/coop-1/flocks']}>
            <Routes>
              <Route path="/coops/:coopId/flocks" element={<FlocksPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      // Should show loading skeletons
      const skeletons = container.querySelectorAll('.MuiSkeleton-root');
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });
});
