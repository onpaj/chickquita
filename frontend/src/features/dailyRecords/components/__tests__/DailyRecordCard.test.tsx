import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
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
});
