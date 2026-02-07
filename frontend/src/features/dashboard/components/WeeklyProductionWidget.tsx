import { useTranslation } from 'react-i18next';
import { StatCard } from './StatCard';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';

interface WeeklyProductionWidgetProps {
  eggsThisWeek?: number;
  loading?: boolean;
}

/**
 * Weekly Production widget
 * Shows this week's egg production (when daily records are implemented)
 */
export function WeeklyProductionWidget({
  eggsThisWeek,
  loading = false,
}: WeeklyProductionWidgetProps) {
  const { t } = useTranslation();

  return (
    <StatCard
      title={t('dashboard.widgets.weeklyProduction.title')}
      value={eggsThisWeek ?? t('dashboard.widgets.weeklyProduction.notAvailable')}
      icon={<CalendarMonthIcon />}
      loading={loading}
      subtitle={
        eggsThisWeek !== undefined
          ? t('dashboard.widgets.weeklyProduction.eggsThisWeek')
          : undefined
      }
    />
  );
}
