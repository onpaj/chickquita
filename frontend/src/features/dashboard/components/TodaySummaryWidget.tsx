import { useTranslation } from 'react-i18next';
import { StatCard } from './StatCard';
import TodayIcon from '@mui/icons-material/Today';

interface TodaySummaryWidgetProps {
  eggsToday?: number;
  loading?: boolean;
}

/**
 * Today's Summary widget
 * Shows today's egg production (when daily records are implemented)
 */
export function TodaySummaryWidget({ eggsToday, loading = false }: TodaySummaryWidgetProps) {
  const { t } = useTranslation();

  return (
    <StatCard
      title={t('dashboard.widgets.todaySummary.title')}
      value={eggsToday ?? t('dashboard.widgets.todaySummary.notAvailable')}
      icon={<TodayIcon />}
      loading={loading}
      subtitle={eggsToday !== undefined ? t('dashboard.widgets.todaySummary.eggsToday') : undefined}
    />
  );
}
