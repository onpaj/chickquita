import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { RecordsPage } from '../RecordsPage';

// Mock child pages so tests stay fast and isolated
vi.mock('../DailyRecordsListPage', () => ({
  DailyRecordsListPage: () => <div data-testid="daily-records-page">Daily Records Content</div>,
}));

vi.mock('../StatisticsPage', () => ({
  default: () => <div data-testid="statistics-page">Statistics Content</div>,
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'navigation.dailyRecords': 'Daily Records',
        'navigation.statistics': 'Statistics',
      };
      return map[key] ?? key;
    },
  }),
}));

const renderAt = (path: string) =>
  render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/records/*" element={<RecordsPage />} />
      </Routes>
    </MemoryRouter>
  );

describe('RecordsPage', () => {
  describe('Tab rendering', () => {
    it('renders both tab labels', () => {
      renderAt('/records/list');

      expect(screen.getByRole('tab', { name: 'Daily Records' })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: 'Statistics' })).toBeInTheDocument();
    });

    it('renders the DailyRecordsListPage on /records/list', () => {
      renderAt('/records/list');

      expect(screen.getByTestId('daily-records-page')).toBeInTheDocument();
      expect(screen.queryByTestId('statistics-page')).not.toBeInTheDocument();
    });

    it('renders StatisticsPage on /records/stats', () => {
      renderAt('/records/stats');

      expect(screen.getByTestId('statistics-page')).toBeInTheDocument();
      expect(screen.queryByTestId('daily-records-page')).not.toBeInTheDocument();
    });
  });

  describe('Active tab state', () => {
    it('Daily Records tab is selected on /records/list', () => {
      renderAt('/records/list');

      const listTab = screen.getByRole('tab', { name: 'Daily Records' });
      expect(listTab).toHaveAttribute('aria-selected', 'true');
    });

    it('Statistics tab is selected on /records/stats', () => {
      renderAt('/records/stats');

      const statsTab = screen.getByRole('tab', { name: 'Statistics' });
      expect(statsTab).toHaveAttribute('aria-selected', 'true');
    });

    it('Statistics tab is not selected on /records/list', () => {
      renderAt('/records/list');

      const statsTab = screen.getByRole('tab', { name: 'Statistics' });
      expect(statsTab).toHaveAttribute('aria-selected', 'false');
    });
  });

  describe('Tab navigation', () => {
    it('clicking Statistics tab shows StatisticsPage', async () => {
      const user = userEvent.setup();
      renderAt('/records/list');

      await user.click(screen.getByRole('tab', { name: 'Statistics' }));

      expect(screen.getByTestId('statistics-page')).toBeInTheDocument();
    });

    it('clicking Daily Records tab shows DailyRecordsListPage', async () => {
      const user = userEvent.setup();
      renderAt('/records/stats');

      await user.click(screen.getByRole('tab', { name: 'Daily Records' }));

      expect(screen.getByTestId('daily-records-page')).toBeInTheDocument();
    });
  });

  describe('Default redirect', () => {
    it('redirects /records to stats tab', () => {
      renderAt('/records');

      // After redirect, statistics content should be visible
      expect(screen.getByTestId('statistics-page')).toBeInTheDocument();
    });
  });
});
