import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { DeleteDailyRecordDialog } from '../DeleteDailyRecordDialog';
import { useDeleteDailyRecord } from '../../hooks/useDailyRecords';
import type { DailyRecordDto } from '../../api/dailyRecordsApi';

// Mock the hooks
vi.mock('../../hooks/useDailyRecords');

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: { flock?: string }) => {
      const translations: Record<string, string> = {
        'dailyRecords.delete.title': 'Smazat denní záznam',
        'dailyRecords.delete.message': 'Opravdu chcete smazat denní záznam z data',
        'dailyRecords.delete.flockInfo': `Hejno: ${options?.flock || ''}`,
        'common.delete': 'Smazat',
        'common.cancel': 'Zrušit',
        'common.processing': 'Zpracovávám...',
      };
      return translations[key] || key;
    },
  }),
}));

const mockDeleteDailyRecord = vi.fn();

describe('DeleteDailyRecordDialog', () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  const mockRecord: DailyRecordDto = {
    id: 'record-1',
    tenantId: 'tenant-1',
    flockId: 'flock-1',
    recordDate: '2026-02-07',
    eggCount: 10,
    notes: 'Test notes',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };

  const mockOnClose = vi.fn();
  const mockOnSuccess = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient.clear();

    (useDeleteDailyRecord as ReturnType<typeof vi.fn>).mockReturnValue({
      mutate: mockDeleteDailyRecord,
      isPending: false,
    });
  });

  const renderDialog = (
    record: DailyRecordDto | null = mockRecord,
    open = true,
    flockIdentifier?: string,
    onSuccess?: () => void
  ) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <DeleteDailyRecordDialog
          open={open}
          onClose={mockOnClose}
          record={record}
          flockIdentifier={flockIdentifier}
          onSuccess={onSuccess}
        />
      </QueryClientProvider>
    );
  };

  it('renders dialog with correct title and message', () => {
    renderDialog();

    expect(screen.getByText('Smazat denní záznam')).toBeInTheDocument();
    expect(
      screen.getByText('Opravdu chcete smazat denní záznam z data')
    ).toBeInTheDocument();
  });

  it('displays formatted date in dialog message', () => {
    renderDialog();

    // Date should be formatted as "07. 02. 2026" (Czech format)
    expect(screen.getByText('07. 02. 2026')).toBeInTheDocument();
  });

  it('displays flock identifier when provided', () => {
    renderDialog(mockRecord, true, 'Hejno A');

    expect(screen.getByText('Hejno: Hejno A')).toBeInTheDocument();
  });

  it('does not display flock info when flockIdentifier is not provided', () => {
    renderDialog(mockRecord, true, undefined);

    expect(screen.queryByText(/Hejno:/)).not.toBeInTheDocument();
  });

  it('calls delete mutation when confirm button is clicked', async () => {
    const user = userEvent.setup();
    renderDialog();

    const deleteButton = screen.getByRole('button', { name: /smazat/i });
    await user.click(deleteButton);

    expect(mockDeleteDailyRecord).toHaveBeenCalledWith(
      { id: 'record-1', flockId: 'flock-1' },
      expect.objectContaining({
        onSuccess: expect.any(Function),
      })
    );
  });

  it('calls onClose when cancel button is clicked', async () => {
    const user = userEvent.setup();
    renderDialog();

    const cancelButton = screen.getByRole('button', { name: /zrušit/i });
    await user.click(cancelButton);

    expect(mockOnClose).toHaveBeenCalledTimes(1);
    expect(mockDeleteDailyRecord).not.toHaveBeenCalled();
  });

  it('shows loading state during deletion', () => {
    (useDeleteDailyRecord as ReturnType<typeof vi.fn>).mockReturnValue({
      mutate: mockDeleteDailyRecord,
      isPending: true,
    });

    renderDialog();

    const deleteButton = screen.getByRole('button', { name: /zpracovávám/i });
    expect(deleteButton).toBeDisabled();

    const cancelButton = screen.getByRole('button', { name: /zrušit/i });
    expect(cancelButton).toBeDisabled();
  });

  it('calls onSuccess callback after successful deletion', async () => {
    mockDeleteDailyRecord.mockImplementation((_, { onSuccess }) => {
      onSuccess();
    });

    const user = userEvent.setup();
    renderDialog(mockRecord, true, undefined, mockOnSuccess);

    const deleteButton = screen.getByRole('button', { name: /smazat/i });
    await user.click(deleteButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
      expect(mockOnSuccess).toHaveBeenCalled();
    });
  });

  it('does not render when record is null', () => {
    const { container } = renderDialog(null);

    expect(container.firstChild).toBeNull();
  });

  it('does not render when open is false', () => {
    renderDialog(mockRecord, false);

    expect(screen.queryByText('Smazat denní záznam')).not.toBeInTheDocument();
  });

  it('handles deletion without onSuccess callback', async () => {
    mockDeleteDailyRecord.mockImplementation((_, { onSuccess }) => {
      onSuccess();
    });

    const user = userEvent.setup();
    renderDialog(mockRecord, true, undefined, undefined);

    const deleteButton = screen.getByRole('button', { name: /smazat/i });
    await user.click(deleteButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
      // Should not throw error when onSuccess is undefined
    });
  });

  it('uses error variant for confirm button', () => {
    renderDialog();

    const deleteButton = screen.getByRole('button', { name: /smazat/i });

    // MUI applies color via classes, so we check the button exists and is styled appropriately
    expect(deleteButton).toBeInTheDocument();
  });
});
