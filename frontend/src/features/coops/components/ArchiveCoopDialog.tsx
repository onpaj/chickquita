import { useTranslation } from 'react-i18next';
import { ConfirmationDialog } from '@/shared/components/ConfirmationDialog';

interface ArchiveCoopDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  coopName: string;
  isPending?: boolean;
}

export function ArchiveCoopDialog({
  open,
  onClose,
  onConfirm,
  coopName,
  isPending = false,
}: ArchiveCoopDialogProps) {
  const { t } = useTranslation();

  return (
    <ConfirmationDialog
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title={t('coops.archiveConfirmTitle')}
      message={t('coops.archiveConfirmMessage')}
      entityName={coopName}
      isPending={isPending}
      confirmText={t('coops.archiveCoop')}
      confirmColor="warning"
      pendingText={t('coops.archiving')}
    />
  );
}
