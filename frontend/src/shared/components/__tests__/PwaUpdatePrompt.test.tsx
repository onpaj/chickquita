import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { useRegisterSW } from 'virtual:pwa-register/react';
import { PwaUpdatePrompt } from '../PwaUpdatePrompt';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

const mockUpdateServiceWorker = vi.fn();

describe('PwaUpdatePrompt', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useRegisterSW).mockReturnValue({
      needRefresh: [false, vi.fn()],
      offlineReady: [false, vi.fn()],
      updateServiceWorker: mockUpdateServiceWorker,
    });
  });

  it('does not show snackbar when no update is available', () => {
    render(<PwaUpdatePrompt />);

    expect(screen.queryByText('pwa.update.message')).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'pwa.update.reload' })).not.toBeInTheDocument();
  });

  it('shows snackbar with reload button when update is available', () => {
    vi.mocked(useRegisterSW).mockReturnValue({
      needRefresh: [true, vi.fn()],
      offlineReady: [false, vi.fn()],
      updateServiceWorker: mockUpdateServiceWorker,
    });

    render(<PwaUpdatePrompt />);

    expect(screen.getByText('pwa.update.message')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'pwa.update.reload' })).toBeInTheDocument();
  });

  it('calls updateServiceWorker(true) when reload button is clicked', () => {
    vi.mocked(useRegisterSW).mockReturnValue({
      needRefresh: [true, vi.fn()],
      offlineReady: [false, vi.fn()],
      updateServiceWorker: mockUpdateServiceWorker,
    });

    render(<PwaUpdatePrompt />);

    fireEvent.click(screen.getByRole('button', { name: 'pwa.update.reload' }));

    expect(mockUpdateServiceWorker).toHaveBeenCalledWith(true);
    expect(mockUpdateServiceWorker).toHaveBeenCalledTimes(1);
  });
});
