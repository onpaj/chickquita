import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { EditDailyRecordModal } from '../EditDailyRecordModal';
import { useUpdateDailyRecord, useDeleteDailyRecord } from '../../hooks/useDailyRecords';
import type { DailyRecordDto } from '../../api/dailyRecordsApi';

// Mock the hooks
vi.mock('../../hooks/useDailyRecords');
vi.mock('../../../../hooks/useErrorHandler', () => ({
  useErrorHandler: () => ({ handleError: vi.fn() }),
}));

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: { count?: number }) => {
      const translations: Record<string, string> = {
        'dailyRecords.edit.title': 'Upravit denní záznam',
        'dailyRecords.edit.dateNotEditable': 'Datum záznamu nelze změnit',
        'dailyRecords.edit.flockNotEditable': 'Hejno nelze změnit',
        'dailyRecords.edit.sameDayRestriction':
          'Záznamy lze upravovat pouze v den vytvoření',
        'dailyRecords.date': 'Datum',
        'dailyRecords.flock': 'Hejno',
        'dailyRecords.eggCount': 'Počet vajec',
        'dailyRecords.notes': 'Poznámky',
        'dailyRecords.noFlocks': 'Žádná dostupná hejna',
        'common.cancel': 'Zrušit',
        'common.delete': 'Smazat',
        'common.save': 'Uložit',
        'common.saving': 'Ukládám...',
        'common.characters': 'znaků',
        'validation.positiveNumber': 'Musí být kladné číslo',
        'validation.maxLength': `Maximální délka je ${options?.count} znaků`,
      };
      return translations[key] || key;
    },
  }),
}));

const mockUpdateDailyRecord = vi.fn();
const mockDeleteDailyRecord = vi.fn();

describe('EditDailyRecordModal', () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  const todayRecord: DailyRecordDto = {
    id: 'record-1',
    tenantId: 'tenant-1',
    flockId: 'flock-1',
    flockName: 'Test Flock',
    flockCoopName: 'Test Coop',
    recordDate: '2026-02-07',
    eggCount: 10,
    notes: 'Test notes',
    createdAt: new Date().toISOString(), // Created today
    updatedAt: new Date().toISOString(),
  };

  const oldRecord: DailyRecordDto = {
    id: 'record-2',
    tenantId: 'tenant-1',
    flockId: 'flock-1',
    flockName: 'Test Flock',
    flockCoopName: 'Test Coop',
    recordDate: '2026-02-01',
    eggCount: 15,
    notes: 'Old notes',
    createdAt: '2026-02-01T10:00:00Z', // Created a week ago
    updatedAt: '2026-02-01T10:00:00Z',
  };

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient.clear();

    (useUpdateDailyRecord as ReturnType<typeof vi.fn>).mockReturnValue({
      mutate: mockUpdateDailyRecord,
      isPending: false,
    });

    (useDeleteDailyRecord as ReturnType<typeof vi.fn>).mockReturnValue({
      mutate: mockDeleteDailyRecord,
      isPending: false,
    });
  });

  const renderModal = (record: DailyRecordDto | null, open = true) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <EditDailyRecordModal
          open={open}
          onClose={vi.fn()}
          record={record}
          flockIdentifier="Hejno A"
        />
      </QueryClientProvider>
    );
  };

  it('renders modal with pre-filled data from record', () => {
    renderModal(todayRecord);

    expect(screen.getByText('Upravit denní záznam')).toBeInTheDocument();
    const eggCountInput = screen.getByRole('spinbutton', { name: /egg count/i });
    expect(eggCountInput).toHaveValue(10);
    expect(screen.getByLabelText(/poznámky/i)).toHaveValue('Test notes');
  });

  it('displays date as read-only field with formatted value', () => {
    renderModal(todayRecord);

    const dateField = screen.getByLabelText('Datum');
    expect(dateField).toBeDisabled();
    expect(dateField).toHaveValue('7. 2. 2026');
  });

  it('displays flock identifier as read-only field', () => {
    renderModal(todayRecord);

    const flockField = screen.getByLabelText('Hejno');
    expect(flockField).toBeDisabled();
    expect(flockField).toHaveValue('Hejno A');
  });

  it('allows editing egg count and notes for same-day records', async () => {
    const user = userEvent.setup();
    renderModal(todayRecord);

    const eggCountInput = screen.getByRole('spinbutton', { name: /egg count/i });
    const notesInput = screen.getByLabelText(/poznámky/i);

    expect(eggCountInput).not.toBeDisabled();
    expect(notesInput).not.toBeDisabled();

    await user.clear(eggCountInput);
    await user.type(eggCountInput, '20');
    await user.clear(notesInput);
    await user.type(notesInput, 'Updated notes');

    expect(eggCountInput).toHaveValue(20);
    expect(notesInput).toHaveValue('Updated notes');
  });

  it('shows same-day restriction error for old records', () => {
    renderModal(oldRecord);

    expect(
      screen.getByText('Záznamy lze upravovat pouze v den vytvoření')
    ).toBeInTheDocument();
  });

  it('disables form fields for old records', () => {
    renderModal(oldRecord);

    const eggCountInput = screen.getByRole('spinbutton', { name: /egg count/i });
    const notesInput = screen.getByLabelText(/poznámky/i);
    const saveButton = screen.getByRole('button', { name: /uložit/i });

    expect(eggCountInput).toBeDisabled();
    expect(notesInput).toBeDisabled();
    expect(saveButton).toBeDisabled();
  });

  it('accepts valid egg count values', async () => {
    const user = userEvent.setup();
    renderModal(todayRecord);

    const eggCountInput = screen.getByRole('spinbutton', { name: /egg count/i });

    // Update with a valid value
    await user.clear(eggCountInput);
    await user.type(eggCountInput, '100');

    expect(eggCountInput).toHaveValue(100);
  });

  it('displays character count for notes field', async () => {
    const user = userEvent.setup();
    renderModal(todayRecord);

    const notesInput = screen.getByLabelText(/poznámky/i);

    // Type some text
    await user.clear(notesInput);
    await user.type(notesInput, 'Test');

    // Check for character count helper text
    expect(screen.getByText(/4\/500 znaků/i)).toBeInTheDocument();
  });

  it('submits form with updated data when valid', async () => {
    const user = userEvent.setup();
    const mockOnClose = vi.fn();
    let capturedData: { id: string; eggCount: number; notes?: string } | null = null;

    (useUpdateDailyRecord as ReturnType<typeof vi.fn>).mockReturnValue({
      mutate: (data: { id: string; eggCount: number; notes?: string }) => {
        capturedData = data;
      },
      isPending: false,
    });

    render(
      <QueryClientProvider client={queryClient}>
        <EditDailyRecordModal
          open={true}
          onClose={mockOnClose}
          record={todayRecord}
          flockIdentifier="Hejno A"
        />
      </QueryClientProvider>
    );

    const eggCountInput = screen.getByRole('spinbutton', { name: /egg count/i });
    const notesInput = screen.getByLabelText(/poznámky/i);

    // Update values
    await user.clear(eggCountInput);
    await user.type(eggCountInput, '25');
    await user.clear(notesInput);
    await user.type(notesInput, 'New notes');

    // Submit
    const saveButton = screen.getByRole('button', { name: /uložit/i });
    await user.click(saveButton);

    await waitFor(() => {
      expect(capturedData).toEqual({
        id: 'record-1',
        eggCount: 25,
        notes: 'New notes',
      });
    });
  });

  it('calls onSuccess callback and closes modal on successful update', async () => {
    const user = userEvent.setup();
    const mockOnClose = vi.fn();

    (useUpdateDailyRecord as ReturnType<typeof vi.fn>).mockReturnValue({
      mutate: (data: unknown, options: { onSuccess?: () => void }) => {
        if (options.onSuccess) {
          options.onSuccess();
        }
      },
      isPending: false,
    });

    render(
      <QueryClientProvider client={queryClient}>
        <EditDailyRecordModal
          open={true}
          onClose={mockOnClose}
          record={todayRecord}
          flockIdentifier="Hejno A"
        />
      </QueryClientProvider>
    );

    const saveButton = screen.getByRole('button', { name: /uložit/i });
    await user.click(saveButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
    });
  });

  it('displays loading state while submitting', () => {
    (useUpdateDailyRecord as ReturnType<typeof vi.fn>).mockReturnValue({
      mutate: mockUpdateDailyRecord,
      isPending: true,
    });

    renderModal(todayRecord);

    expect(screen.getByText('Ukládám...')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /ukládám/i })).toBeDisabled();
  });

  it('does not render when record is null', () => {
    const { container } = renderModal(null);
    expect(container.firstChild).toBeNull();
  });

  it('clears form and closes when cancel button is clicked', async () => {
    const user = userEvent.setup();
    const mockOnClose = vi.fn();

    render(
      <QueryClientProvider client={queryClient}>
        <EditDailyRecordModal
          open={true}
          onClose={mockOnClose}
          record={todayRecord}
          flockIdentifier="Hejno A"
        />
      </QueryClientProvider>
    );

    const cancelButton = screen.getByRole('button', { name: /zrušit/i });
    await user.click(cancelButton);

    expect(mockOnClose).toHaveBeenCalled();
  });

  it('trims whitespace from notes before submission', async () => {
    const user = userEvent.setup();
    renderModal(todayRecord);

    const notesInput = screen.getByLabelText(/poznámky/i);

    await user.clear(notesInput);
    await user.type(notesInput, '  Test with spaces  ');

    const saveButton = screen.getByRole('button', { name: /uložit/i });
    await user.click(saveButton);

    await waitFor(() => {
      expect(mockUpdateDailyRecord).toHaveBeenCalledWith(
        expect.objectContaining({
          notes: 'Test with spaces',
        }),
        expect.any(Object)
      );
    });
  });

  it('sends undefined for empty notes', async () => {
    const user = userEvent.setup();
    renderModal(todayRecord);

    const notesInput = screen.getByLabelText(/poznámky/i);
    const saveButton = screen.getByRole('button', { name: /uložit/i });

    await user.clear(notesInput);
    await user.click(saveButton);

    await waitFor(() => {
      expect(mockUpdateDailyRecord).toHaveBeenCalledWith(
        expect.objectContaining({
          notes: undefined,
        }),
        expect.any(Object)
      );
    });
  });
});
