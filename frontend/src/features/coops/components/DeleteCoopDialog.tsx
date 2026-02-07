import { useTranslation } from 'react-i18next';
import { ConfirmationDialog } from '@/shared/components/ConfirmationDialog';

interface DeleteCoopDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  coopName: string;
  isPending?: boolean;
}

export function DeleteCoopDialog({
  open,
  onClose,
  onConfirm,
  coopName,
  isPending = false,
}: DeleteCoopDialogProps) {
  const { t } = useTranslation();

  return (
    <ConfirmationDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title={t('coops.deleteConfirmTitle')}
      message={t('coops.deleteConfirmMessage')}
      entityName={coopName}
      isPending={isPending}
      confirmText={t('common.delete')}
      confirmColor="error"
      pendingText={t('common.deleting')}
    />
  );
}
