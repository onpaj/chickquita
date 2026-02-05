import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import type { UseAuthReturn } from '@clerk/clerk-react';
import { ProtectedRoute } from '../ProtectedRoute';

// Mock Clerk's useAuth hook
vi.mock('@clerk/clerk-react', () => ({
  useAuth: vi.fn(),
}));

// Import after mocking
import { useAuth } from '@clerk/clerk-react';

describe('ProtectedRoute', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render child routes when user is authenticated', () => {
    // Mock authenticated state
    vi.mocked(useAuth).mockReturnValue({
      isSignedIn: true,
      isLoaded: true,
    } as UseAuthReturn);

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/protected" element={<div>Protected Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Protected Content')).toBeInTheDocument();
  });

  it('should redirect to /sign-in when user is not authenticated', () => {
    // Mock unauthenticated state
    vi.mocked(useAuth).mockReturnValue({
      isSignedIn: false,
      isLoaded: true,
    } as UseAuthReturn);

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/protected" element={<div>Protected Content</div>} />
          </Route>
          <Route path="/sign-in" element={<div>Sign In Page</div>} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
    expect(screen.getByText('Sign In Page')).toBeInTheDocument();
  });

  it('should render nothing while Clerk is loading', () => {
    // Mock loading state
    vi.mocked(useAuth).mockReturnValue({
      isSignedIn: false,
      isLoaded: false,
    } as UseAuthReturn);

    const { container } = render(
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/protected" element={<div>Protected Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    // Should render nothing (null)
    expect(container.firstChild).toBeNull();
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('should handle nested protected routes', () => {
    // Mock authenticated state
    vi.mocked(useAuth).mockReturnValue({
      isSignedIn: true,
      isLoaded: true,
    } as UseAuthReturn);

    render(
      <MemoryRouter initialEntries={['/dashboard/settings']}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/dashboard" element={<div>Dashboard</div>} />
            <Route path="/dashboard/settings" element={<div>Settings</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Settings')).toBeInTheDocument();
  });
});
