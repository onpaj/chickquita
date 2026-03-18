import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { DashboardEmptyState } from '../DashboardEmptyState';

const mockNavigate = vi.fn();

vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

vi.mock('../../../../assets/illustrations', () => ({
  EmptyDashboardIllustration: ({ 'aria-label': ariaLabel }: { 'aria-label'?: string }) => (
    <svg data-testid="empty-dashboard-illustration" aria-label={ariaLabel} role="img" />
  ),
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'dashboard.emptyState.title': 'Welcome to Chickquita!',
        'dashboard.emptyState.message': 'Start tracking your chicken farm',
        'dashboard.emptyState.createFirstCoop': 'Create Your First Coop',
        'dashboard.emptyState.createFirstCoopDesc': 'Set up a coop to start managing your flocks',
        'dashboard.emptyState.createFirstFlock': 'Create Your First Flock',
        'dashboard.emptyState.createFirstFlockDesc': 'Add a flock to start tracking production',
      };
      return translations[key] || key;
    },
  }),
}));

const mockUseUserSettingsContext = vi.fn();
vi.mock('@/features/settings', () => ({
  useUserSettingsContext: () => mockUseUserSettingsContext(),
}));

const mockUseCoops = vi.fn();
vi.mock('@/features/coops/hooks/useCoops', () => ({
  useCoops: () => mockUseCoops(),
}));

describe('DashboardEmptyState', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseUserSettingsContext.mockReturnValue({ singleCoopMode: false });
    mockUseCoops.mockReturnValue({ data: [] });
  });

  describe('Normal mode (singleCoopMode = false)', () => {
    it('renders create first coop button', () => {
      render(<DashboardEmptyState />);
      expect(screen.getByRole('button', { name: /create your first coop/i })).toBeInTheDocument();
    });

    it('renders coop description', () => {
      render(<DashboardEmptyState />);
      expect(screen.getByText('Set up a coop to start managing your flocks')).toBeInTheDocument();
    });

    it('navigates to /coops on button click', async () => {
      const user = userEvent.setup();
      render(<DashboardEmptyState />);
      await user.click(screen.getByRole('button', { name: /create your first coop/i }));
      expect(mockNavigate).toHaveBeenCalledWith('/coops');
    });
  });

  describe('Single-coop mode with existing coop', () => {
    beforeEach(() => {
      mockUseUserSettingsContext.mockReturnValue({ singleCoopMode: true });
      mockUseCoops.mockReturnValue({ data: [{ id: 'coop-123', name: 'My Coop' }] });
    });

    it('renders create first flock button', () => {
      render(<DashboardEmptyState />);
      expect(screen.getByRole('button', { name: /create your first flock/i })).toBeInTheDocument();
    });

    it('renders flock description', () => {
      render(<DashboardEmptyState />);
      expect(screen.getByText('Add a flock to start tracking production')).toBeInTheDocument();
    });

    it('navigates to flocks page for the existing coop', async () => {
      const user = userEvent.setup();
      render(<DashboardEmptyState />);
      await user.click(screen.getByRole('button', { name: /create your first flock/i }));
      expect(mockNavigate).toHaveBeenCalledWith('/coops/coop-123/flocks');
    });
  });

  describe('Single-coop mode with no coops yet', () => {
    beforeEach(() => {
      mockUseUserSettingsContext.mockReturnValue({ singleCoopMode: true });
      mockUseCoops.mockReturnValue({ data: [] });
    });

    it('falls back to create first coop button', () => {
      render(<DashboardEmptyState />);
      expect(screen.getByRole('button', { name: /create your first coop/i })).toBeInTheDocument();
    });

    it('navigates to /coops as fallback', async () => {
      const user = userEvent.setup();
      render(<DashboardEmptyState />);
      await user.click(screen.getByRole('button', { name: /create your first coop/i }));
      expect(mockNavigate).toHaveBeenCalledWith('/coops');
    });
  });
});
