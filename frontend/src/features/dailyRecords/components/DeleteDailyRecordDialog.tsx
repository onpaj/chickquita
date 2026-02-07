import { format } from 'date-fns';
import { cs } from 'date-fns/locale';
import { useTranslation } from 'react-i18next';
import { ConfirmationDialog } from '@/shared/components/ConfirmationDialog';
import { useDeleteDailyRecord } from '../hooks/useDailyRecords';
import type { DailyRecordDto } from '../api/dailyRecordsApi';

interface DeleteDailyRecordDialogProps {
  /**
   * Whether the dialog is open
   */
  open: boolean;

  /**
   * Handler called when the dialog should close
   */
  onClose: () => void;

  /**
   * The daily record to delete
   */
  record: DailyRecordDto | null;

  /**
   * Optional flock identifier for display
   */
  flockIdentifier?: string;

  /**
   * Optional callback when deletion succeeds
   */
  onSuccess?: () => void;
}

/**
 * DeleteDailyRecordDialog Component
 *
 * Confirmation dialog for deleting daily records.
 * Uses the shared ConfirmationDialog component for consistent UX.
 *
 * Features:
 * - Shows formatted date in confirmation message
 * - Displays flock identifier if provided
 * - Loading state during deletion
 * - Success toast notification via useDeleteDailyRecord hook
 * - Error handling via useDeleteDailyRecord hook
 *
 * @example
 * <DeleteDailyRecordDialog
 *   open={isOpen}
 *   onClose={handleClose}
 *   record={recordToDelete}
 *   flockIdentifier="Hejno A"
 * />
 */
export function DeleteDailyRecordDialog({
  open,
  onClose,
  record,
  flockIdentifier,
  onSuccess,
}: DeleteDailyRecordDialogProps) {
  const { t } = useTranslation();
  const { mutate: deleteDailyRecord, isPending } = useDeleteDailyRecord();

  if (!record) {
    return null;
  }

  const formattedDate = format(new Date(record.recordDate), 'dd. MM. yyyy', {
    locale: cs,
  });

  const handleConfirm = () => {
    if (!record) return;

    deleteDailyRecord(
      { id: record.id, flockId: record.flockId },
      {
        onSuccess: () => {
          onClose();
          onSuccess?.();
        },
      }
    );
  };

  return (
    <ConfirmationDialog
      open={open}
      onClose={onClose}
      onConfirm={handleConfirm}
      title={t('dailyRecords.delete.title')}
      message={t('dailyRecords.delete.message')}
      entityName={formattedDate}
      secondaryMessage={
        flockIdentifier
          ? t('dailyRecords.delete.flockInfo', { flock: flockIdentifier })
          : undefined
      }
      isPending={isPending}
      confirmText={t('common.delete')}
      confirmColor="error"
      confirmVariant="contained"
    />
  );
}
