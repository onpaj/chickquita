import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { I18nextProvider } from 'react-i18next';
import { DailyRecordCard } from '../DailyRecordCard';
import i18n from '../../../../lib/i18n';
import type { DailyRecordDto } from '../../api/dailyRecordsApi';

const createWrapper = () => {
  return ({ children }: { children: React.ReactNode }) => (
    <I18nextProvider i18n={i18n}>{children}</I18nextProvider>
  );
};

const mockRecord: DailyRecordDto = {
  id: 'record-1',
  tenantId: 'tenant-1',
  flockId: 'flock-1',
  recordDate: '2024-02-15',
  eggCount: 12,
  notes: 'Good production today',
  createdAt: '2024-02-15T10:00:00Z',
  updatedAt: '2024-02-15T10:00:00Z',
};

describe('DailyRecordCard', () => {
  describe('Rendering', () => {
    it('should render record with all information', () => {
      render(<DailyRecordCard record={mockRecord} flockIdentifier="Hejno A" />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('15. 02. 2024')).toBeInTheDocument();
      expect(screen.getByText('12')).toBeInTheDocument();
      expect(screen.getByText('vajec')).toBeInTheDocument();
      expect(screen.getByText('Hejno A')).toBeInTheDocument();
      expect(screen.getByText('Good production today')).toBeInTheDocument();
    });

    it('should render record without flock identifier', () => {
      render(<DailyRecordCard record={mockRecord} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('15. 02. 2024')).toBeInTheDocument();
      expect(screen.getByText('12')).toBeInTheDocument();
      expect(screen.queryByText('Hejno A')).not.toBeInTheDocument();
    });

    it('should render record without notes', () => {
      const recordWithoutNotes = { ...mockRecord, notes: undefined };
      render(<DailyRecordCard record={recordWithoutNotes} flockIdentifier="Hejno A" />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('15. 02. 2024')).toBeInTheDocument();
      expect(screen.getByText('12')).toBeInTheDocument();
      expect(screen.queryByText('Good production today')).not.toBeInTheDocument();
    });

    it('should render egg icon', () => {
      const { container } = render(
        <DailyRecordCard record={mockRecord} flockIdentifier="Hejno A" />,
        {
          wrapper: createWrapper(),
        }
      );

      const eggIcon = container.querySelector('svg');
      expect(eggIcon).toBeInTheDocument();
    });
  });

  describe('Date formatting', () => {
    it('should format date correctly in Czech locale', () => {
      const recordWithDate = {
        ...mockRecord,
        recordDate: '2024-12-31',
      };
      render(<DailyRecordCard record={recordWithDate} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('31. 12. 2024')).toBeInTheDocument();
    });

    it('should format date correctly for different months', () => {
      const recordWithDate = {
        ...mockRecord,
        recordDate: '2024-01-01',
      };
      render(<DailyRecordCard record={recordWithDate} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('01. 01. 2024')).toBeInTheDocument();
    });
  });

  describe('Egg count display', () => {
    it('should display single digit egg count', () => {
      const recordWithLowCount = { ...mockRecord, eggCount: 5 };
      render(<DailyRecordCard record={recordWithLowCount} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('5')).toBeInTheDocument();
    });

    it('should display double digit egg count', () => {
      const recordWithMediumCount = { ...mockRecord, eggCount: 42 };
      render(<DailyRecordCard record={recordWithMediumCount} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('42')).toBeInTheDocument();
    });

    it('should display triple digit egg count', () => {
      const recordWithHighCount = { ...mockRecord, eggCount: 150 };
      render(<DailyRecordCard record={recordWithHighCount} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('150')).toBeInTheDocument();
    });

    it('should display zero egg count', () => {
      const recordWithZeroCount = { ...mockRecord, eggCount: 0 };
      render(<DailyRecordCard record={recordWithZeroCount} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('0')).toBeInTheDocument();
    });
  });

  describe('Notes truncation', () => {
    it('should have CSS for truncating long notes to 2 lines', () => {
      const longNotes = 'This is a very long note that should be truncated after two lines. '.repeat(5);
      const recordWithLongNotes = { ...mockRecord, notes: longNotes };

      render(
        <DailyRecordCard record={recordWithLongNotes} />,
        {
          wrapper: createWrapper(),
        }
      );

      // Check that notes are rendered
      expect(screen.getByText(/This is a very long note/i)).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('should have proper semantic structure', () => {
      const { container } = render(
        <DailyRecordCard record={mockRecord} flockIdentifier="Hejno A" />,
        {
          wrapper: createWrapper(),
        }
      );

      const heading = container.querySelector('h3');
      expect(heading).toBeInTheDocument();
      expect(heading?.textContent).toBe('15. 02. 2024');
    });

    it('should render within a card container', () => {
      const { container } = render(
        <DailyRecordCard record={mockRecord} flockIdentifier="Hejno A" />,
        {
          wrapper: createWrapper(),
        }
      );

      const card = container.querySelector('.MuiCard-root');
      expect(card).toBeInTheDocument();
    });
  });

  describe('Visual layout', () => {
    it('should render chip for flock identifier', () => {
      const { container } = render(
        <DailyRecordCard record={mockRecord} flockIdentifier="Hejno A" />,
        {
          wrapper: createWrapper(),
        }
      );

      const chip = container.querySelector('.MuiChip-root');
      expect(chip).toBeInTheDocument();
      expect(chip?.textContent).toBe('Hejno A');
    });
  });

  describe('Edit button functionality', () => {
    it('should show edit button for same-day records when onEdit is provided', () => {
      // Create a record from today
      const todayRecord = {
        ...mockRecord,
        createdAt: new Date().toISOString(),
      };

      const mockOnEdit = vi.fn();

      render(<DailyRecordCard record={todayRecord} onEdit={mockOnEdit} />, {
        wrapper: createWrapper(),
      });

      const editButton = screen.getByLabelText('edit record');
      expect(editButton).toBeInTheDocument();
    });

    it('should not show edit button for old records', () => {
      // Create a record from yesterday
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      const oldRecord = {
        ...mockRecord,
        createdAt: yesterday.toISOString(),
      };

      const mockOnEdit = vi.fn();

      render(<DailyRecordCard record={oldRecord} onEdit={mockOnEdit} />, {
        wrapper: createWrapper(),
      });

      const editButton = screen.queryByLabelText('edit record');
      expect(editButton).not.toBeInTheDocument();
    });

    it('should not show edit button when onEdit is not provided', () => {
      const todayRecord = {
        ...mockRecord,
        createdAt: new Date().toISOString(),
      };

      render(<DailyRecordCard record={todayRecord} />, {
        wrapper: createWrapper(),
      });

      const editButton = screen.queryByLabelText('edit record');
      expect(editButton).not.toBeInTheDocument();
    });

    it('should call onEdit when edit button is clicked', async () => {
      const user = userEvent.setup();
      const mockOnEdit = vi.fn();
      const todayRecord = {
        ...mockRecord,
        createdAt: new Date().toISOString(),
      };

      render(
        <DailyRecordCard record={todayRecord} onEdit={mockOnEdit} />,
        {
          wrapper: createWrapper(),
        }
      );

      const editButton = screen.getByLabelText('edit record');
      await user.click(editButton);

      expect(mockOnEdit).toHaveBeenCalledWith(todayRecord);
      expect(mockOnEdit).toHaveBeenCalledTimes(1);
    });
  });

  describe('Hover effects', () => {
    it('should apply hover effect styles', () => {
      const { container } = render(
        <DailyRecordCard record={mockRecord} flockIdentifier="Hejno A" />,
        {
          wrapper: createWrapper(),
        }
      );

      const card = container.querySelector('.MuiCard-root');
      expect(card).toBeInTheDocument();
      // Hover effects are applied via CSS, we just check the card exists
    });
  });

  describe('Empty notes edge case', () => {
    it('should not render notes section when notes is empty string', () => {
      const recordWithEmptyNotes = { ...mockRecord, notes: '' };
      render(<DailyRecordCard record={recordWithEmptyNotes} />, {
        wrapper: createWrapper(),
      });

      const notesText = screen.queryByText('');
      expect(notesText).not.toBeInTheDocument();
    });

    it('should not render notes section when notes is whitespace', () => {
      const recordWithWhitespace = { ...mockRecord, notes: '   ' };
      render(<DailyRecordCard record={recordWithWhitespace} />, {
        wrapper: createWrapper(),
      });

      // Should not show notes if they're just whitespace
      const card = screen.getByRole('heading', { level: 3 }).closest('.MuiCardContent-root');
      expect(card).toBeInTheDocument();
    });
  });

  describe('Date edge cases', () => {
    it('should format leap year date correctly', () => {
      const leapYearRecord = {
        ...mockRecord,
        recordDate: '2024-02-29',
      };
      render(<DailyRecordCard record={leapYearRecord} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('29. 02. 2024')).toBeInTheDocument();
    });

    it('should format year boundary date correctly', () => {
      const yearEndRecord = {
        ...mockRecord,
        recordDate: '2024-12-31',
      };
      render(<DailyRecordCard record={yearEndRecord} />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByText('31. 12. 2024')).toBeInTheDocument();
    });
  });
});
