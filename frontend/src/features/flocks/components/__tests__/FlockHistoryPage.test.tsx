import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { FlockHistoryPage } from '../FlockHistoryPage';

const mockSetAppBar = vi.fn();
const mockResetAppBar = vi.fn();

vi.mock('../../../../context/AppBarContext', async () => {
  const actual = await vi.importActual('../../../../context/AppBarContext');
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
      if (key === 'flockHistory.title') return 'Flock History';
      return key;
    },
  }),
}));

vi.mock('../../hooks/useFlockHistory', () => ({
  useFlockHistory: vi.fn(() => ({
    data: [],
    isLoading: false,
    error: null,
  })),
}));

vi.mock('../FlockHistoryTimeline', () => ({
  FlockHistoryTimeline: ({ loading }: { loading: boolean }) => (
    <div data-testid="timeline">{loading ? 'Loading...' : 'Timeline'}</div>
  ),
}));

function renderPage() {
  return render(
    <MemoryRouter initialEntries={['/coops/coop-1/flocks/flock-1/history']}>
      <Routes>
        <Route path="/coops/:coopId/flocks/:flockId/history" element={<FlockHistoryPage />} />
      </Routes>
    </MemoryRouter>
  );
}

describe('FlockHistoryPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calls setAppBar with history title and onBack', () => {
    renderPage();
    expect(mockSetAppBar).toHaveBeenCalledWith(
      expect.objectContaining({ title: 'Flock History', onBack: expect.any(Function) })
    );
  });

  it('calls resetAppBar on unmount', () => {
    const { unmount } = renderPage();
    unmount();
    expect(mockResetAppBar).toHaveBeenCalled();
  });

  it('renders the timeline component', () => {
    renderPage();
    expect(screen.getByTestId('timeline')).toBeInTheDocument();
  });

  it('does not render an inline page header', () => {
    renderPage();
    expect(screen.queryByRole('heading')).not.toBeInTheDocument();
    const backButtons = screen.queryAllByRole('button', { name: /back/i });
    expect(backButtons).toHaveLength(0);
  });
});
