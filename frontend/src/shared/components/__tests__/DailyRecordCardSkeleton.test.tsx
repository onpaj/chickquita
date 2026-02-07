import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import { DailyRecordCardSkeleton } from '../DailyRecordCardSkeleton';

describe('DailyRecordCardSkeleton', () => {
  describe('Rendering', () => {
    it('should render skeleton card', () => {
      const { container } = render(<DailyRecordCardSkeleton />);

      const card = container.querySelector('.MuiCard-root');
      expect(card).toBeInTheDocument();
    });

    it('should render all skeleton elements', () => {
      const { container } = render(<DailyRecordCardSkeleton />);

      const skeletons = container.querySelectorAll('.MuiSkeleton-root');
      expect(skeletons.length).toBeGreaterThan(0);
    });

    it('should have proper structure matching DailyRecordCard', () => {
      const { container } = render(<DailyRecordCardSkeleton />);

      // Check for card content
      const cardContent = container.querySelector('.MuiCardContent-root');
      expect(cardContent).toBeInTheDocument();
    });
  });

  describe('Layout', () => {
    it('should render circular skeleton for icon', () => {
      const { container } = render(<DailyRecordCardSkeleton />);

      const circularSkeleton = container.querySelector('.MuiSkeleton-circular');
      expect(circularSkeleton).toBeInTheDocument();
    });

    it('should render rounded skeleton for chip', () => {
      const { container } = render(<DailyRecordCardSkeleton />);

      const roundedSkeleton = container.querySelector('.MuiSkeleton-rounded');
      expect(roundedSkeleton).toBeInTheDocument();
    });

    it('should render text skeletons', () => {
      const { container } = render(<DailyRecordCardSkeleton />);

      const textSkeletons = container.querySelectorAll('.MuiSkeleton-text');
      expect(textSkeletons.length).toBeGreaterThan(0);
    });
  });
});
