import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { BottomNavigation } from '../BottomNavigation';

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
      const translations: Record<string, string> = {
        'navigation.dashboard': 'Dashboard',
        'navigation.coops': 'Coops',
        'navigation.dailyRecords': 'Daily Records',
        'navigation.statistics': 'Statistics',
        'navigation.purchases': 'Purchases',
        'navigation.settings': 'Settings',
      };
      return translations[key] ?? key;
    },
  }),
}));

function renderWithRouter(initialPath: string) {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <BottomNavigation />
    </MemoryRouter>
  );
}

describe('BottomNavigation', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('renders all navigation items', () => {
    it('shows all six navigation items', () => {
      renderWithRouter('/dashboard');
      expect(screen.getByText('Dashboard')).toBeInTheDocument();
      expect(screen.getByText('Coops')).toBeInTheDocument();
      expect(screen.getByText('Daily Records')).toBeInTheDocument();
      expect(screen.getByText('Statistics')).toBeInTheDocument();
      expect(screen.getByText('Purchases')).toBeInTheDocument();
      expect(screen.getByText('Settings')).toBeInTheDocument();
    });
  });

  describe('active tab detection', () => {
    it('activates dashboard tab on /dashboard', () => {
      renderWithRouter('/dashboard');
      const dashboardButton = screen.getByRole('button', { name: /dashboard/i });
      expect(dashboardButton).toHaveClass('Mui-selected');
    });

    it('activates purchases tab on /purchases', () => {
      renderWithRouter('/purchases');
      const purchasesButton = screen.getByRole('button', { name: /purchases/i });
      expect(purchasesButton).toHaveClass('Mui-selected');
    });

    it('activates purchases tab on /purchases/new', () => {
      renderWithRouter('/purchases/new');
      const purchasesButton = screen.getByRole('button', { name: /purchases/i });
      expect(purchasesButton).toHaveClass('Mui-selected');
    });

    it('does not activate purchases tab on /dashboard', () => {
      renderWithRouter('/dashboard');
      const purchasesButton = screen.getByRole('button', { name: /purchases/i });
      expect(purchasesButton).not.toHaveClass('Mui-selected');
    });

    it('activates coops tab on /coops', () => {
      renderWithRouter('/coops');
      const coopsButton = screen.getByRole('button', { name: /coops/i });
      expect(coopsButton).toHaveClass('Mui-selected');
    });

    it('activates settings tab on /settings', () => {
      renderWithRouter('/settings');
      const settingsButton = screen.getByRole('button', { name: /settings/i });
      expect(settingsButton).toHaveClass('Mui-selected');
    });
  });

  describe('navigation on click', () => {
    it('navigates to /purchases when clicking Purchases', async () => {
      const user = userEvent.setup();
      renderWithRouter('/dashboard');
      await user.click(screen.getByRole('button', { name: /purchases/i }));
      expect(mockNavigate).toHaveBeenCalledWith('/purchases');
    });

    it('navigates to /dashboard when clicking Dashboard', async () => {
      const user = userEvent.setup();
      renderWithRouter('/purchases');
      await user.click(screen.getByRole('button', { name: /dashboard/i }));
      expect(mockNavigate).toHaveBeenCalledWith('/dashboard');
    });

    it('navigates to /settings when clicking Settings', async () => {
      const user = userEvent.setup();
      renderWithRouter('/dashboard');
      await user.click(screen.getByRole('button', { name: /settings/i }));
      expect(mockNavigate).toHaveBeenCalledWith('/settings');
    });
  });
});
