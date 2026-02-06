import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { CoopCard } from '../CoopCard';
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

describe('CoopCard', () => {
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
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render coop name', () => {
      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      expect(screen.getByText('Test Coop')).toBeInTheDocument();
    });

    it('should render location when provided', () => {
      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      expect(screen.getByText('Test Location')).toBeInTheDocument();
    });

    it('should not render location icon when location is not provided', () => {
      const coopWithoutLocation = { ...mockCoop, location: undefined };

      render(
        <MemoryRouter>
          <CoopCard coop={coopWithoutLocation} />
        </MemoryRouter>
      );

      expect(screen.queryByText('Test Location')).not.toBeInTheDocument();
    });

    it('should render active status chip', () => {
      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      expect(screen.getByText('Active')).toBeInTheDocument();
    });

    it('should render created date', () => {
      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      // Check for the "Created:" text (translation will be mocked)
      expect(screen.getByText(/Created:/)).toBeInTheDocument();
    });

    it('should render more menu button', () => {
      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      const moreButton = screen.getByLabelText('More');
      expect(moreButton).toBeInTheDocument();
    });
  });

  describe('User Interactions', () => {
    it('should navigate to coop detail page when card is clicked', async () => {
      const user = userEvent.setup();

      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      const card = screen.getByText('Test Coop').closest('.MuiCard-root');
      expect(card).toBeInTheDocument();

      if (card) {
        await user.click(card);
        expect(mockNavigate).toHaveBeenCalledWith('/coops/123e4567-e89b-12d3-a456-426614174000');
      }
    });

    it('should call onMenuClick when more button is clicked', async () => {
      const user = userEvent.setup();

      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} onMenuClick={mockOnMenuClick} />
        </MemoryRouter>
      );

      const moreButton = screen.getByLabelText('More');
      await user.click(moreButton);

      expect(mockOnMenuClick).toHaveBeenCalledTimes(1);
      expect(mockOnMenuClick).toHaveBeenCalledWith(
        expect.any(Object), // The click event
        mockCoop
      );
    });

    it('should not navigate when more button is clicked (event stopPropagation)', async () => {
      const user = userEvent.setup();

      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} onMenuClick={mockOnMenuClick} />
        </MemoryRouter>
      );

      const moreButton = screen.getByLabelText('More');
      await user.click(moreButton);

      // Navigate should not be called when clicking the menu button
      expect(mockNavigate).not.toHaveBeenCalled();
      expect(mockOnMenuClick).toHaveBeenCalledTimes(1);
    });

    it('should handle card click without onMenuClick callback', async () => {
      const user = userEvent.setup();

      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      const card = screen.getByText('Test Coop').closest('.MuiCard-root');

      if (card) {
        await user.click(card);
        expect(mockNavigate).toHaveBeenCalledWith('/coops/123e4567-e89b-12d3-a456-426614174000');
      }
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA label for more button', () => {
      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      const moreButton = screen.getByLabelText('More');
      expect(moreButton).toHaveAttribute('aria-label', 'More');
    });

    it('should have heading with proper semantic level', () => {
      render(
        <MemoryRouter>
          <CoopCard coop={mockCoop} />
        </MemoryRouter>
      );

      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('Test Coop');
    });
  });
});
