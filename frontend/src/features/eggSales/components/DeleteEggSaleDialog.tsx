import { useTranslation } from 'react-i18next';
import { formatDate } from '@/lib/dateFormat';
import { ConfirmationDialog } from '@/shared/components/ConfirmationDialog';
import { useDeleteEggSale } from '../hooks/useEggSales';
import type { EggSaleDto } from '../types/eggSale.types';

interface DeleteEggSaleDialogProps {
  open: boolean;
  onClose: () => void;
  sale: EggSaleDto | null;
  onSuccess?: () => void;
}

export function DeleteEggSaleDialog({
  open,
  onClose,
  sale,
  onSuccess,
}: DeleteEggSaleDialogProps) {
  const { t } = useTranslation();
  const { deleteEggSale, isDeleting } = useDeleteEggSale();

  if (!sale) {
    return null;
  }

  const handleConfirm = () => {
    deleteEggSale(sale.id, {
      onSuccess: () => {
        onClose();
        onSuccess?.();
      },
    });
  };

  return (
    <ConfirmationDialog
      open={open}
      onClose={onClose}
      onConfirm={handleConfirm}
      title={t('eggSales.delete.title')}
      message={t('eggSales.delete.message')}
      entityName={formatDate(sale.date)}
      secondaryMessage={t('eggSales.delete.warning')}
      isPending={isDeleting}
      confirmText={t('common.delete')}
      confirmColor="error"
      confirmVariant="contained"
    />
  );
}
