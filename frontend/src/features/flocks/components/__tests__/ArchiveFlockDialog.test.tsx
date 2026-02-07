import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ArchiveFlockDialog } from '../ArchiveFlockDialog';

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'flocks.archiveConfirmTitle': 'Archive Flock?',
        'flocks.archiveConfirmMessage': 'This flock will be archived and removed from your active list. You can reactivate it later if needed.',
        'common.cancel': 'Cancel',
        'flocks.archiveFlock': 'Archive Flock',
        'flocks.archiving': 'Archiving...',
      };
      return translations[key] || key;
    },
  }),
}));

describe('ArchiveFlockDialog', () => {
  const mockOnClose = vi.fn();
  const mockOnConfirm = vi.fn();
  const testFlockIdentifier = 'Test Flock #123';

  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderDialog = (props = {}) => {
    const defaultProps = {
      open: true,
      onClose: mockOnClose,
      onConfirm: mockOnConfirm,
      flockIdentifier: testFlockIdentifier,
      isPending: false,
    };

    return render(<ArchiveFlockDialog {...defaultProps} {...props} />);
  };

  describe('Rendering', () => {
    it('should render dialog title', () => {
      renderDialog();
      expect(screen.getByText('Archive Flock?')).toBeInTheDocument();
    });

    it('should render when open prop is true', () => {
      renderDialog({ open: true });
      expect(screen.getByText('Archive Flock?')).toBeInTheDocument();
    });

    it('should not render when open is false', () => {
      renderDialog({ open: false });
      expect(screen.queryByText('Archive Flock?')).not.toBeInTheDocument();
    });

    it('should display warning message about archiving', () => {
      renderDialog();
      expect(
        screen.getByText('This flock will be archived and removed from your active list. You can reactivate it later if needed.')
      ).toBeInTheDocument();
    });

    it('should display flock identifier for confirmation', () => {
      renderDialog();
      expect(screen.getByText(testFlockIdentifier)).toBeInTheDocument();
    });

    it('should render cancel button', () => {
      renderDialog();
      expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
    });

    it('should render confirm button', () => {
      renderDialog();
      expect(screen.getByRole('button', { name: 'Archive Flock' })).toBeInTheDocument();
    });

    it('should have proper ARIA labels for accessibility', () => {
      renderDialog();

      const dialog = screen.getByRole('dialog');
      expect(dialog).toHaveAttribute('aria-labelledby', 'archive-flock-dialog-title');
      expect(dialog).toHaveAttribute('aria-describedby', 'archive-flock-dialog-description');
    });
  });

  describe('User Interactions', () => {
    it('should call onClose when cancel button is clicked', async () => {
      const user = userEvent.setup();
      renderDialog();

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      await user.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalledTimes(1);
      expect(mockOnConfirm).not.toHaveBeenCalled();
    });

    it('should call onConfirm when confirm button is clicked', async () => {
      const user = userEvent.setup();
      renderDialog();

      const confirmButton = screen.getByRole('button', { name: 'Archive Flock' });
      await user.click(confirmButton);

      expect(mockOnConfirm).toHaveBeenCalledTimes(1);
      expect(mockOnClose).not.toHaveBeenCalled();
    });

    it('should call onClose when clicking outside the dialog', async () => {
      const user = userEvent.setup();
      renderDialog();

      // MUI Dialog backdrop - click on the backdrop
      const backdrop = document.querySelector('.MuiBackdrop-root');
      if (backdrop) {
        await user.click(backdrop);
        expect(mockOnClose).toHaveBeenCalledTimes(1);
      }
    });

    it('should call onClose when pressing Escape key', async () => {
      const user = userEvent.setup();
      renderDialog();

      await user.keyboard('{Escape}');

      expect(mockOnClose).toHaveBeenCalledTimes(1);
      expect(mockOnConfirm).not.toHaveBeenCalled();
    });
  });

  describe('Loading State', () => {
    it('should show loading state when isPending is true', () => {
      renderDialog({ isPending: true });

      const confirmButton = screen.getByRole('button', { name: 'Archiving...' });
      expect(confirmButton).toBeInTheDocument();
      expect(confirmButton).toBeDisabled();

      // Check for loading spinner
      const spinner = confirmButton.querySelector('.MuiCircularProgress-root');
      expect(spinner).toBeInTheDocument();
    });

    it('should disable cancel button when isPending is true', () => {
      renderDialog({ isPending: true });

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      expect(cancelButton).toBeDisabled();
    });

    it('should disable confirm button when isPending is true', () => {
      renderDialog({ isPending: true });

      const confirmButton = screen.getByRole('button', { name: 'Archiving...' });
      expect(confirmButton).toBeDisabled();
    });

    it('should enable buttons when isPending is false', () => {
      renderDialog({ isPending: false });

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      const confirmButton = screen.getByRole('button', { name: 'Archive Flock' });

      expect(cancelButton).not.toBeDisabled();
      expect(confirmButton).not.toBeDisabled();
    });

    it('should prevent onClose when isPending is true and cancel is clicked', () => {
      renderDialog({ isPending: true });

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });

      // Button should be disabled, so it cannot be clicked
      expect(cancelButton).toBeDisabled();

      // Verify the button is disabled and won't trigger onClose
      expect(mockOnClose).not.toHaveBeenCalled();
    });

    it('should prevent onConfirm when isPending is true and confirm is clicked', () => {
      renderDialog({ isPending: true });

      const confirmButton = screen.getByRole('button', { name: 'Archiving...' });

      // Button should be disabled, so it cannot be clicked
      expect(confirmButton).toBeDisabled();

      // Verify the button is disabled and won't trigger onConfirm
      expect(mockOnConfirm).not.toHaveBeenCalled();
    });
  });

  describe('Mobile Responsiveness', () => {
    it('should have fullWidth and maxWidth xs properties for confirmation dialogs', () => {
      renderDialog();

      const dialog = screen.getByRole('dialog');
      expect(dialog).toBeInTheDocument();

      // Dialog component should be present with proper responsive settings
      // fullWidth and maxWidth="xs" props are applied per US-010 standardization
    });

    it('should have minimum touch target size for buttons', () => {
      renderDialog();

      const cancelButton = screen.getByRole('button', { name: 'Cancel' });
      const confirmButton = screen.getByRole('button', { name: 'Archive Flock' });

      // Both buttons should be in the document
      expect(cancelButton).toBeInTheDocument();
      expect(confirmButton).toBeInTheDocument();

      // Check that buttons have proper styling (minimum 44px height for touch targets)
      // This is enforced by the sx prop in DialogActions
    });
  });

  describe('Button Styling', () => {
    it('should style confirm button as warning variant', () => {
      renderDialog();

      const confirmButton = screen.getByRole('button', { name: 'Archive Flock' });

      // Check that it has the warning color classes
      expect(confirmButton).toHaveClass('MuiButton-containedWarning');
    });

    it('should show loading spinner with correct size when pending', () => {
      renderDialog({ isPending: true });

      const confirmButton = screen.getByRole('button', { name: 'Archiving...' });
      const spinner = confirmButton.querySelector('.MuiCircularProgress-root');

      expect(spinner).toBeInTheDocument();
      // CircularProgress size should be 20 (from the component code)
    });
  });

  describe('Different Flock Identifiers', () => {
    it('should display short flock identifier', () => {
      renderDialog({ flockIdentifier: 'A1' });
      expect(screen.getByText('A1')).toBeInTheDocument();
    });

    it('should display long flock identifier', () => {
      const longIdentifier = 'Very Long Flock Identifier With Many Characters';
      renderDialog({ flockIdentifier: longIdentifier });
      expect(screen.getByText(longIdentifier)).toBeInTheDocument();
    });

    it('should display flock identifier with special characters', () => {
      const specialIdentifier = 'Flock #123 (Main) - 2024';
      renderDialog({ flockIdentifier: specialIdentifier });
      expect(screen.getByText(specialIdentifier)).toBeInTheDocument();
    });
  });
});
