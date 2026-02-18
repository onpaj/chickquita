import { useTranslation } from 'react-i18next';
import { ConfirmationDialog } from '@/shared/components/ConfirmationDialog';

interface ArchiveFlockDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  flockIdentifier: string;
  isPending?: boolean;
}

export function ArchiveFlockDialog({
  open,
  onClose,
  onConfirm,
  flockIdentifier,
  isPending = false,
}: ArchiveFlockDialogProps) {
  const { t } = useTranslation();

  return (
    <ConfirmationDialog
      dialogId="archive-flock-dialog"
      open={open}
      onClose={onClose}
      onConfirm={onConfirm}
      title={t('flocks.archiveConfirmTitle')}
      message={t('flocks.archiveConfirmMessage')}
      entityName={flockIdentifier}
      isPending={isPending}
      confirmText={t('flocks.archiveFlock')}
      confirmColor="warning"
      pendingText={t('flocks.archiving')}
    />
  );
}
