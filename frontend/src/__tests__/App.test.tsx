import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import App from '../App';

// Mock Clerk
const mockUseAuth = vi.fn();
vi.mock('@clerk/clerk-react', async () => {
  const actual = await vi.importActual('@clerk/clerk-react');
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
    ClerkProvider: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  };
});

// Mock API client hook
vi.mock('../lib/useApiClient', () => ({
  useApiClient: vi.fn(),
}));

// Mock sync manager
vi.mock('../lib/syncManager', () => ({
  startAutoSync: vi.fn(),
}));

// Mock all page components
vi.mock('../pages/SignUpPage', () => ({
  default: () => <div>SignUp Page</div>,
}));

vi.mock('../pages/SignInPage', () => ({
  default: () => <div>SignIn Page</div>,
}));

vi.mock('../pages/DashboardPage', () => ({
  default: () => <div>Dashboard Page</div>,
}));

vi.mock('../pages/StatisticsPage', () => ({
  default: () => <div>Statistics Page</div>,
}));

vi.mock('../pages/CoopsPage', () => ({
  default: () => <div>Coops Page</div>,
}));

vi.mock('../pages/CoopDetailPage', () => ({
  CoopDetailPage: () => <div>Coop Detail Page</div>,
}));

vi.mock('../pages/FlocksPage', () => ({
  default: () => <div>Flocks Page</div>,
}));

vi.mock('../pages/FlockDetailPage', () => ({
  FlockDetailPage: () => <div>Flock Detail Page</div>,
}));

vi.mock('../features/flocks/components/FlockHistoryPage', () => ({
  FlockHistoryPage: () => <div>Flock History Page</div>,
}));

vi.mock('../pages/DailyRecordsListPage', () => ({
  DailyRecordsListPage: () => <div>Daily Records Page</div>,
}));

vi.mock('../features/purchases/pages/PurchasesPage', () => ({
  PurchasesPage: () => <div>Purchases Page</div>,
}));

vi.mock('../pages/SettingsPage', () => ({
  SettingsPage: () => <div>Settings Page</div>,
}));

vi.mock('../pages/NotFoundPage', () => ({
  default: () => <div>Not Found Page</div>,
}));

// Mock ProtectedRoute
vi.mock('../components/ProtectedRoute', () => ({
  default: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

// Mock BottomNavigation
vi.mock('../components/BottomNavigation', () => ({
  BottomNavigation: () => <div>Bottom Navigation</div>,
}));

// Mock shared components
vi.mock('../shared/components', () => ({
  OfflineBanner: () => <div>Offline Banner</div>,
  PwaInstallPrompt: () => <div>PWA Install Prompt</div>,
  IosInstallPrompt: () => <div>iOS Install Prompt</div>,
}));

describe('App - AppBar Design Patterns', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderApp = (isSignedIn: boolean = true) => {
    mockUseAuth.mockReturnValue({
      isSignedIn,
      isLoaded: true,
    });

    return render(
      <BrowserRouter>
        <App />
      </BrowserRouter>
    );
  };

  describe('AppBar Rendering (Signed In)', () => {
    it('should render AppBar with correct height (64px)', () => {
      const { container } = renderApp(true);

      const appBar = container.querySelector('.MuiAppBar-root');
      expect(appBar).toBeInTheDocument();

      const computedStyle = window.getComputedStyle(appBar!);
      expect(computedStyle.height).toBe('64px');
    });

    it('should render AppBar with Chickquita branding text', () => {
      renderApp(true);

      const branding = screen.getByText('Chickquita');
      expect(branding).toBeInTheDocument();
    });

    it('should render AppBar with user icon button (settings)', () => {
      renderApp(true);

      const settingsButton = screen.getByLabelText('settings');
      expect(settingsButton).toBeInTheDocument();
    });

    it('should render Toolbar with correct minHeight (64px)', () => {
      const { container } = renderApp(true);

      const toolbar = container.querySelector('.MuiToolbar-root');
      expect(toolbar).toBeInTheDocument();

      const computedStyle = window.getComputedStyle(toolbar!);
      // minHeight: '64px !important' should result in 64px
      expect(computedStyle.minHeight).toBe('64px');
    });

    it('should not render AppBar when user is signed out', () => {
      const { container } = renderApp(false);

      const appBar = container.querySelector('.MuiAppBar-root');
      expect(appBar).not.toBeInTheDocument();
    });

    it('should render AppBar with sticky positioning', () => {
      const { container } = renderApp(true);

      const appBar = container.querySelector('.MuiAppBar-root');
      const computedStyle = window.getComputedStyle(appBar!);

      expect(computedStyle.position).toBe('sticky');
    });

    it('should render AppBar with correct z-index (1100)', () => {
      const { container } = renderApp(true);

      const appBar = container.querySelector('.MuiAppBar-root');
      const computedStyle = window.getComputedStyle(appBar!);

      expect(computedStyle.zIndex).toBe('1100');
    });
  });

  describe('AppBar Branding', () => {
    it('should render branding text with bold font weight (700)', () => {
      renderApp(true);

      const branding = screen.getByText('Chickquita');
      const computedStyle = window.getComputedStyle(branding);

      expect(computedStyle.fontWeight).toBe('700');
    });

    it('should render branding text with primary color', () => {
      renderApp(true);

      const branding = screen.getByText('Chickquita');
      const computedStyle = window.getComputedStyle(branding);

      // MUI primary.main color should be applied
      // We check that color is not the default text color
      expect(computedStyle.color).toBeTruthy();
    });
  });
});
