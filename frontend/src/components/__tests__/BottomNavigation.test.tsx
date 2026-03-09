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
        'navigation.records': 'Records',
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

  describe('renders navigation items', () => {
    it('shows five navigation items', () => {
      renderWithRouter('/dashboard');
      expect(screen.getByText('Dashboard')).toBeInTheDocument();
      expect(screen.getByText('Coops')).toBeInTheDocument();
      expect(screen.getByText('Records')).toBeInTheDocument();
      expect(screen.getByText('Purchases')).toBeInTheDocument();
      expect(screen.getByText('Settings')).toBeInTheDocument();
    });

    it('does not show separate Daily Records and Statistics items', () => {
      renderWithRouter('/dashboard');
      expect(screen.queryByText('Daily Records')).not.toBeInTheDocument();
      expect(screen.queryByText('Statistics')).not.toBeInTheDocument();
    });
  });

  describe('active tab detection', () => {
    it('activates dashboard tab on /dashboard', () => {
      renderWithRouter('/dashboard');
      expect(screen.getByRole('button', { name: /dashboard/i })).toHaveClass('Mui-selected');
    });

    it('activates records tab on /records/list', () => {
      renderWithRouter('/records/list');
      expect(screen.getByRole('button', { name: /records/i })).toHaveClass('Mui-selected');
    });

    it('activates records tab on /records/stats', () => {
      renderWithRouter('/records/stats');
      expect(screen.getByRole('button', { name: /records/i })).toHaveClass('Mui-selected');
    });

    it('activates records tab on legacy /daily-records path', () => {
      renderWithRouter('/daily-records');
      expect(screen.getByRole('button', { name: /records/i })).toHaveClass('Mui-selected');
    });

    it('activates records tab on legacy /statistics path', () => {
      renderWithRouter('/statistics');
      expect(screen.getByRole('button', { name: /records/i })).toHaveClass('Mui-selected');
    });

    it('activates purchases tab on /purchases', () => {
      renderWithRouter('/purchases');
      expect(screen.getByRole('button', { name: /purchases/i })).toHaveClass('Mui-selected');
    });

    it('activates purchases tab on /purchases/new', () => {
      renderWithRouter('/purchases/new');
      expect(screen.getByRole('button', { name: /purchases/i })).toHaveClass('Mui-selected');
    });

    it('activates coops tab on /coops', () => {
      renderWithRouter('/coops');
      expect(screen.getByRole('button', { name: /coops/i })).toHaveClass('Mui-selected');
    });

    it('activates settings tab on /settings', () => {
      renderWithRouter('/settings');
      expect(screen.getByRole('button', { name: /settings/i })).toHaveClass('Mui-selected');
    });

    it('does not activate purchases tab on /dashboard', () => {
      renderWithRouter('/dashboard');
      expect(screen.getByRole('button', { name: /purchases/i })).not.toHaveClass('Mui-selected');
    });
  });

  describe('navigation on click', () => {
    it('navigates to /records/list when clicking Records', async () => {
      const user = userEvent.setup();
      renderWithRouter('/dashboard');
      await user.click(screen.getByRole('button', { name: /records/i }));
      expect(mockNavigate).toHaveBeenCalledWith('/records/list');
    });

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
