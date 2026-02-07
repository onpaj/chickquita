import { useTranslation } from 'react-i18next';
import { StatCard } from './StatCard';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';

interface EggCostWidgetProps {
  costPerEgg?: number;
  loading?: boolean;
}

/**
 * Egg Cost Calculation widget
 * Shows cost per egg (when purchases and daily records are implemented)
 */
export function EggCostWidget({ costPerEgg, loading = false }: EggCostWidgetProps) {
  const { t } = useTranslation();

  const formattedCost =
    costPerEgg !== undefined
      ? new Intl.NumberFormat('cs-CZ', {
          style: 'currency',
          currency: 'CZK',
        }).format(costPerEgg)
      : t('dashboard.widgets.eggCostCalc.notAvailable');

  return (
    <StatCard
      title={t('dashboard.widgets.eggCostCalc.title')}
      value={formattedCost}
      icon={<AttachMoneyIcon />}
      loading={loading}
      subtitle={
        costPerEgg !== undefined ? t('dashboard.widgets.eggCostCalc.costPerEgg') : undefined
      }
    />
  );
}
