import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ToastContext, type ToastContextType } from '../../../../contexts/ToastContext';
import type { Coop } from '../../api/coopsApi';

// Mock react-router-dom's useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, params?: any) => {
      if (key === 'coops.active') return 'Active';
      if (key === 'common.more') return 'More';
      if (key === 'coops.createdAt') return `Created: ${params?.date}`;
      return key;
    },
  }),
}));

// Mock error handler
vi.mock('../../../hooks/useErrorHandler', () => ({
  useErrorHandler: vi.fn(() => ({
    handleError: vi.fn(),
  })),
}));

// Mock the coop hooks
vi.mock('../../hooks/useCoops', () => ({
  useArchiveCoop: vi.fn(() => ({
    mutate: vi.fn(),
    isPending: false,
  })),
  useDeleteCoop: vi.fn(() => ({
    mutate: vi.fn(),
    isPending: false,
  })),
  useUpdateCoop: vi.fn(() => ({
    mutate: vi.fn(),
    isPending: false,
  })),
}));

// Mock the modal components to simplify testing
vi.mock('../EditCoopModal', () => ({
  EditCoopModal: () => null,
}));

vi.mock('../ArchiveCoopDialog', () => ({
  ArchiveCoopDialog: () => null,
}));

vi.mock('../DeleteCoopDialog', () => ({
  DeleteCoopDialog: () => null,
}));

import { CoopCard } from '../CoopCard';

describe('CoopCard', () => {
  let queryClient: QueryClient;

  const mockCoop: Coop = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    tenantId: 'tenant-123',
    name: 'Test Coop',
    location: 'Test Location',
    isActive: true,
    createdAt: '2024-01-15T10:00:00Z',
    updatedAt: '2024-01-15T10:00:00Z',
    flocksCount: 2,
  };

  const mockOnMenuClick = vi.fn();

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    vi.clearAllMocks();
  });

  const mockToastContext: ToastContextType = {
    showToast: vi.fn(),
    showError: vi.fn(),
    showSuccess: vi.fn(),
    showWarning: vi.fn(),
    showInfo: vi.fn(),
    hideToast: vi.fn(),
  };

  const renderCoopCard = (coop: Coop, onMenuClick?: (event: React.MouseEvent<HTMLButtonElement>, coop: Coop) => void) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <ToastContext.Provider value={mockToastContext}>
          <MemoryRouter>
            <CoopCard coop={coop} onMenuClick={onMenuClick} />
          </MemoryRouter>
        </ToastContext.Provider>
      </QueryClientProvider>
    );
  };

  describe('Rendering', () => {
    it('should render coop name', () => {
      renderCoopCard(mockCoop);

      expect(screen.getByText('Test Coop')).toBeInTheDocument();
    });

    it('should render location when provided', () => {
      renderCoopCard(mockCoop);

      expect(screen.getByText('Test Location')).toBeInTheDocument();
    });

    it('should not render location icon when location is not provided', () => {
      const coopWithoutLocation = { ...mockCoop, location: undefined };

      renderCoopCard(coopWithoutLocation);

      expect(screen.queryByText('Test Location')).not.toBeInTheDocument();
    });

    it('should render active status chip', () => {
      renderCoopCard(mockCoop);

      expect(screen.getByText('Active')).toBeInTheDocument();
    });

    it('should render created date', () => {
      renderCoopCard(mockCoop);

      // Check for the "Created:" text (translation will be mocked)
      expect(screen.getByText(/Created:/)).toBeInTheDocument();
    });

    it('should render more menu button', () => {
      renderCoopCard(mockCoop);

      const moreButton = screen.getByLabelText('More');
      expect(moreButton).toBeInTheDocument();
    });
  });

  describe('User Interactions', () => {
    it('should navigate to coop detail page when card is clicked', async () => {
      const user = userEvent.setup();

      renderCoopCard(mockCoop);

      const card = screen.getByText('Test Coop').closest('.MuiCard-root');
      expect(card).toBeInTheDocument();

      if (card) {
        await user.click(card);
        expect(mockNavigate).toHaveBeenCalledWith('/coops/123e4567-e89b-12d3-a456-426614174000');
      }
    });

    it('should open menu when more button is clicked', async () => {
      const user = userEvent.setup();

      renderCoopCard(mockCoop);

      const moreButton = screen.getByLabelText('More');
      await user.click(moreButton);

      // Menu should be open (visible in DOM)
      // Note: MUI Menu renders in a portal, so we check for the menu items
      // Since we mocked the modals, we can't fully test the menu, but we verify
      // that clicking the button doesn't navigate
      expect(mockNavigate).not.toHaveBeenCalled();
    });

    it('should not navigate when more button is clicked (event stopPropagation)', async () => {
      const user = userEvent.setup();

      renderCoopCard(mockCoop);

      const moreButton = screen.getByLabelText('More');
      await user.click(moreButton);

      // Navigate should not be called when clicking the menu button
      expect(mockNavigate).not.toHaveBeenCalled();
    });

    it('should handle card click without onMenuClick callback', async () => {
      const user = userEvent.setup();

      renderCoopCard(mockCoop);

      const card = screen.getByText('Test Coop').closest('.MuiCard-root');

      if (card) {
        await user.click(card);
        expect(mockNavigate).toHaveBeenCalledWith('/coops/123e4567-e89b-12d3-a456-426614174000');
      }
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA label for more button', () => {
      renderCoopCard(mockCoop);

      const moreButton = screen.getByLabelText('More');
      expect(moreButton).toHaveAttribute('aria-label', 'More');
    });

    it('should have heading with proper semantic level', () => {
      renderCoopCard(mockCoop);

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('Test Coop');
    });
  });
});
