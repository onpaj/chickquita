import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { FlocksEmptyState } from '../FlocksEmptyState';

// Mock i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'flocks.emptyState.title': 'No flocks yet',
        'flocks.emptyState.message': 'Start tracking your chickens by creating your first flock',
        'flocks.addFlock': 'Add Flock',
      };
      return translations[key] || key;
    },
  }),
}));

describe('FlocksEmptyState', () => {
  describe('Rendering', () => {
    it('renders empty state with title', () => {
      const mockOnAddClick = vi.fn();
      render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      expect(screen.getByText('No flocks yet')).toBeInTheDocument();
    });

    it('renders empty state with message', () => {
      const mockOnAddClick = vi.fn();
      render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      expect(screen.getByText('Start tracking your chickens by creating your first flock')).toBeInTheDocument();
    });

    it('renders add flock button', () => {
      const mockOnAddClick = vi.fn();
      render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      const button = screen.getByRole('button', { name: /add flock/i });
      expect(button).toBeInTheDocument();
    });

    it('renders egg icon', () => {
      const mockOnAddClick = vi.fn();
      const { container } = render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      // Check for MUI EggIcon - it uses SVG with specific test id or class
      const icon = container.querySelector('svg[data-testid="EggIcon"]');
      expect(icon).toBeInTheDocument();
    });
  });

  describe('User Interactions', () => {
    it('calls onAddClick when add button is clicked', async () => {
      const user = userEvent.setup();
      const mockOnAddClick = vi.fn();
      render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      const button = screen.getByRole('button', { name: /add flock/i });
      await user.click(button);

      expect(mockOnAddClick).toHaveBeenCalledTimes(1);
    });

    it('calls onAddClick only once per click', async () => {
      const user = userEvent.setup();
      const mockOnAddClick = vi.fn();
      render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      const button = screen.getByRole('button', { name: /add flock/i });
      await user.click(button);
      await user.click(button);
      await user.click(button);

      expect(mockOnAddClick).toHaveBeenCalledTimes(3);
    });
  });

  describe('Accessibility', () => {
    it('has accessible button role', () => {
      const mockOnAddClick = vi.fn();
      render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      const button = screen.getByRole('button', { name: /add flock/i });
      expect(button).toHaveAttribute('type', 'button');
    });

    it('has proper heading hierarchy', () => {
      const mockOnAddClick = vi.fn();
      render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      // h6 variant is used for the title
      const heading = screen.getByText('No flocks yet');
      expect(heading).toBeInTheDocument();
      expect(heading.tagName).toBe('H6');
    });
  });

  describe('Layout and Styling', () => {
    it('centers content vertically and horizontally', () => {
      const mockOnAddClick = vi.fn();
      const { container } = render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      const wrapper = container.firstChild as HTMLElement;
      const styles = window.getComputedStyle(wrapper);

      expect(styles.display).toBe('flex');
      expect(styles.flexDirection).toBe('column');
      expect(styles.alignItems).toBe('center');
      expect(styles.justifyContent).toBe('center');
    });

    it('has minimum height for proper spacing', () => {
      const mockOnAddClick = vi.fn();
      const { container } = render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      const wrapper = container.firstChild as HTMLElement;
      const styles = window.getComputedStyle(wrapper);

      expect(styles.minHeight).toBe('60vh');
    });

    it('centers text alignment', () => {
      const mockOnAddClick = vi.fn();
      const { container } = render(<FlocksEmptyState onAddClick={mockOnAddClick} />);

      const wrapper = container.firstChild as HTMLElement;
      const styles = window.getComputedStyle(wrapper);

      expect(styles.textAlign).toBe('center');
    });
  });
});
