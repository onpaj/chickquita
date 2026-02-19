import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { FlockCard } from '../FlockCard';
import type { Flock } from '../../api/flocksApi';

// Mock react-router-dom navigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Mock i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, params?: Record<string, unknown>) => {
      const translations: Record<string, string> = {
        'flocks.active': 'Active',
        'flocks.archived': 'Archived',
        'flocks.coop': 'Coop',
        'flocks.hens': 'Hens',
        'flocks.roosters': 'Roosters',
        'flocks.chicks': 'Chicks',
        'flocks.total': 'Total',
        'common.edit': 'Edit',
        'common.more': 'More',
        'flocks.archiveFlock': 'Archive Flock',
        'flocks.viewHistory': 'View History',
        'flocks.flockCardAriaLabel': `Flock ${params?.identifier} in ${params?.coopName}`,
        'flocks.belongsToCoopAriaLabel': `Belongs to coop ${params?.coopName}`,
      };
      return translations[key] || key;
    },
  }),
}));

const mockFlock: Flock = {
  id: 'flock-1',
  tenantId: 'tenant-1',
  coopId: 'coop-1',
  identifier: 'Test Flock',
  hatchDate: '2024-01-01',
  currentHens: 10,
  currentRoosters: 2,
  currentChicks: 5,
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
  history: [],
};

const renderFlockCard = (
  flock: Flock = mockFlock,
  coopName = 'Test Coop',
  onEdit = vi.fn(),
  onArchive = vi.fn(),
  onViewHistory = vi.fn()
) => {
  return render(
    <BrowserRouter>
      <FlockCard
        flock={flock}
        coopName={coopName}
        onEdit={onEdit}
        onArchive={onArchive}
        onViewHistory={onViewHistory}
      />
    </BrowserRouter>
  );
};

describe('FlockCard', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
  });

  describe('Rendering - Basic Information', () => {
    it('renders flock identifier', () => {
      renderFlockCard();
      expect(screen.getByText('Test Flock')).toBeInTheDocument();
    });

    it('renders coop name', () => {
      renderFlockCard();
      expect(screen.getByText(/Test Coop/i)).toBeInTheDocument();
    });

    it('displays current composition - hens', () => {
      renderFlockCard();
      expect(screen.getByText('Hens:')).toBeInTheDocument();
      expect(screen.getByText('10')).toBeInTheDocument();
    });

    it('displays current composition - roosters', () => {
      renderFlockCard();
      expect(screen.getByText('Roosters:')).toBeInTheDocument();
      expect(screen.getByText('2')).toBeInTheDocument();
    });

    it('displays current composition - chicks', () => {
      renderFlockCard();
      expect(screen.getByText('Chicks:')).toBeInTheDocument();
      expect(screen.getByText('5')).toBeInTheDocument();
    });

    it('calculates and displays total animals', () => {
      renderFlockCard();
      expect(screen.getByText('Total:')).toBeInTheDocument();
      expect(screen.getByText('17')).toBeInTheDocument();
    });

    it('displays active status chip', () => {
      renderFlockCard();
      expect(screen.getByText('Active')).toBeInTheDocument();
    });

    it('displays archived status chip for inactive flock', () => {
      const archivedFlock = { ...mockFlock, isActive: false };
      renderFlockCard(archivedFlock);
      expect(screen.getByText('Archived')).toBeInTheDocument();
    });
  });

  describe('Rendering - Status Badge', () => {
    it('shows active badge with success color for active flocks', () => {
      renderFlockCard();
      const activeChip = screen.getByText('Active');
      expect(activeChip).toBeInTheDocument();
      expect(activeChip.parentElement).toHaveClass('MuiChip-colorSuccess');
    });

    it('shows archived badge with default color for inactive flocks', () => {
      const archivedFlock = { ...mockFlock, isActive: false };
      renderFlockCard(archivedFlock);
      const archivedChip = screen.getByText('Archived');
      expect(archivedChip).toBeInTheDocument();
      expect(archivedChip.parentElement).toHaveClass('MuiChip-colorDefault');
    });
  });

  describe('User Interactions - Navigation', () => {
    it('navigates to flock detail page on card click', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const card = screen.getByTestId('flock-card');
      await user.click(card);

      expect(mockNavigate).toHaveBeenCalledWith('/coops/coop-1/flocks/flock-1');
    });

    it('does not navigate when clicking menu button', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      expect(mockNavigate).not.toHaveBeenCalled();
    });
  });

  describe('User Interactions - Action Menu', () => {
    it('opens action menu when clicking more button', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      expect(screen.getByRole('menu')).toBeInTheDocument();
    });

    it('displays edit option in menu', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      expect(screen.getByRole('menuitem', { name: /edit/i })).toBeInTheDocument();
    });

    it('displays archive option in menu', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      expect(screen.getByRole('menuitem', { name: /archive flock/i })).toBeInTheDocument();
    });

    it('displays view history option in menu', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      expect(screen.getByRole('menuitem', { name: /view history/i })).toBeInTheDocument();
    });

    it('calls onEdit when edit menu item is clicked', async () => {
      const user = userEvent.setup();
      const onEdit = vi.fn();
      renderFlockCard(mockFlock, 'Test Coop', onEdit);

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const editMenuItem = screen.getByRole('menuitem', { name: /edit/i });
      await user.click(editMenuItem);

      expect(onEdit).toHaveBeenCalledWith(mockFlock);
      expect(onEdit).toHaveBeenCalledTimes(1);
    });

    it('calls onArchive when archive menu item is clicked', async () => {
      const user = userEvent.setup();
      const onArchive = vi.fn();
      renderFlockCard(mockFlock, 'Test Coop', vi.fn(), onArchive);

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const archiveMenuItem = screen.getByRole('menuitem', { name: /archive flock/i });
      await user.click(archiveMenuItem);

      expect(onArchive).toHaveBeenCalledWith(mockFlock);
      expect(onArchive).toHaveBeenCalledTimes(1);
    });

    it('calls onViewHistory when view history menu item is clicked', async () => {
      const user = userEvent.setup();
      const onViewHistory = vi.fn();
      renderFlockCard(mockFlock, 'Test Coop', vi.fn(), vi.fn(), onViewHistory);

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const viewHistoryMenuItem = screen.getByRole('menuitem', { name: /view history/i });
      await user.click(viewHistoryMenuItem);

      expect(onViewHistory).toHaveBeenCalledWith(mockFlock);
      expect(onViewHistory).toHaveBeenCalledTimes(1);
    });

    it('closes menu after clicking edit', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const editMenuItem = screen.getByRole('menuitem', { name: /edit/i });
      await user.click(editMenuItem);

      expect(screen.queryByRole('menu')).not.toBeInTheDocument();
    });

    it('does not navigate when menu items are clicked', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const editMenuItem = screen.getByRole('menuitem', { name: /edit/i });
      await user.click(editMenuItem);

      expect(mockNavigate).not.toHaveBeenCalled();
    });
  });

  describe('User Interactions - Archived Flock Restrictions', () => {
    it('disables edit menu item for archived flocks', async () => {
      const user = userEvent.setup();
      const archivedFlock = { ...mockFlock, isActive: false };
      renderFlockCard(archivedFlock);

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const editMenuItem = screen.getByRole('menuitem', { name: /edit/i });
      expect(editMenuItem).toHaveAttribute('aria-disabled', 'true');
    });

    it('disables archive menu item for archived flocks', async () => {
      const user = userEvent.setup();
      const archivedFlock = { ...mockFlock, isActive: false };
      renderFlockCard(archivedFlock);

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const archiveMenuItem = screen.getByRole('menuitem', { name: /archive flock/i });
      expect(archiveMenuItem).toHaveAttribute('aria-disabled', 'true');
    });

    it('does not disable view history for archived flocks', async () => {
      const user = userEvent.setup();
      const archivedFlock = { ...mockFlock, isActive: false };
      renderFlockCard(archivedFlock);

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const viewHistoryMenuItem = screen.getByRole('menuitem', { name: /view history/i });
      expect(viewHistoryMenuItem).not.toHaveAttribute('aria-disabled', 'true');
    });
  });

  describe('Accessibility', () => {
    it('has article role on card', () => {
      renderFlockCard();
      const card = screen.getByTestId('flock-card');
      expect(card).toHaveAttribute('role', 'article');
    });

    it('has aria-label on card with flock and coop info', () => {
      renderFlockCard();
      const card = screen.getByTestId('flock-card');
      expect(card).toHaveAttribute('aria-label', 'Flock Test Flock in Test Coop');
    });

    it('has aria-expanded on menu button', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      expect(menuButton).toHaveAttribute('aria-expanded', 'false');

      await user.click(menuButton);
      expect(menuButton).toHaveAttribute('aria-expanded', 'true');
    });

    it('has proper heading hierarchy', () => {
      renderFlockCard();
      const heading = screen.getByRole('heading', { level: 2 });
      expect(heading).toHaveTextContent('Test Flock');
    });

    it('menu items have aria-label attributes', async () => {
      const user = userEvent.setup();
      renderFlockCard();

      const menuButton = screen.getByRole('button', { name: /more/i });
      await user.click(menuButton);

      const editMenuItem = screen.getByRole('menuitem', { name: /edit/i });
      expect(editMenuItem).toHaveAttribute('aria-label', 'Edit');
    });
  });

  describe('Edge Cases', () => {
    it('handles zero animals correctly', () => {
      const emptyFlock = {
        ...mockFlock,
        currentHens: 0,
        currentRoosters: 0,
        currentChicks: 0,
      };
      renderFlockCard(emptyFlock);

      expect(screen.getByText('Total:')).toBeInTheDocument();
      // There should be 4 instances of "0" (hens, roosters, chicks, and total)
      const zeros = screen.getAllByText('0');
      expect(zeros).toHaveLength(4);
    });

    it('calculates total with only hens', () => {
      const hensOnlyFlock = {
        ...mockFlock,
        currentHens: 15,
        currentRoosters: 0,
        currentChicks: 0,
      };
      renderFlockCard(hensOnlyFlock);

      expect(screen.getByText('Total:')).toBeInTheDocument();
      // Total should be 15 (only hens count)
      const totals = screen.getAllByText('15');
      expect(totals.length).toBeGreaterThanOrEqual(1);
    });

    it('calculates total with large numbers', () => {
      const largeFlock = {
        ...mockFlock,
        currentHens: 100,
        currentRoosters: 10,
        currentChicks: 50,
      };
      renderFlockCard(largeFlock);

      expect(screen.getByText('160')).toBeInTheDocument();
    });

    it('renders without optional callbacks', () => {
      render(
        <BrowserRouter>
          <FlockCard flock={mockFlock} coopName="Test Coop" />
        </BrowserRouter>
      );

      expect(screen.getByText('Test Flock')).toBeInTheDocument();
    });
  });
});
