import { Box, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import type { ProductionTrendItem } from '../types';
import { aggregateProductionData } from '../utils/aggregateProductionData';
import type { AggregatedProductionItem } from '../utils/aggregateProductionData';
import dayjs from 'dayjs';

/**
 * Production Trend Chart Component
 *
 * Displays a line chart showing egg production trend over time.
 * Automatically adjusts x-axis granularity based on the date range:
 *   - <= 30 days  → daily
 *   - <= 112 days → weekly
 *   - >  112 days → monthly
 */

interface ProductionTrendChartProps {
  data: ProductionTrendItem[];
}

interface ProductionTrendTooltipProps {
  active?: boolean;
  payload?: readonly { payload: AggregatedProductionItem }[];
}

function ProductionTrendTooltip({ active, payload }: ProductionTrendTooltipProps) {
  const { t, i18n } = useTranslation();
  if (active && payload && payload.length) {
    const item = payload[0].payload;
    const d = dayjs(item.date);

    let label: string;
    if (item.granularity === 'monthly') {
      label =
        i18n.language === 'cs' ? d.format('MMMM YYYY') : d.format('MMMM YYYY');
    } else if (item.granularity === 'weekly') {
      label =
        i18n.language === 'cs'
          ? t('statistics.productionTrend.weekOf', { date: d.format('D. M. YYYY') })
          : t('statistics.productionTrend.weekOf', { date: d.format('D MMM YYYY') });
    } else {
      label =
        i18n.language === 'cs' ? d.format('D. M. YYYY') : d.format('D MMM YYYY');
    }

    return (
      <Box
        sx={{
          backgroundColor: 'background.paper',
          p: 1.5,
          border: 1,
          borderColor: 'divider',
          borderRadius: 1,
        }}
      >
        <Typography variant="body2" fontWeight={600}>
          {label}
        </Typography>
        <Typography variant="body2" color="primary">
          {t('statistics.productionTrend.eggs')}: {item.eggs}
        </Typography>
      </Box>
    );
  }
  return null;
}

export function ProductionTrendChart({ data }: ProductionTrendChartProps) {
  const { t, i18n } = useTranslation();

  const aggregated = aggregateProductionData(data ?? []);
  const granularity = aggregated[0]?.granularity ?? 'daily';

  const formatXAxis = (dateString: string) => {
    const d = dayjs(dateString);
    if (granularity === 'monthly') {
      return i18n.language === 'cs' ? d.format('MMM YYYY') : d.format('MMM YYYY');
    }
    if (granularity === 'weekly') {
      return i18n.language === 'cs' ? d.format('D. M.') : d.format('D MMM');
    }
    return i18n.language === 'cs' ? d.format('D. M.') : d.format('D MMM');
  };

  // Empty state
  if (!data || data.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="h6" gutterBottom>
          {t('statistics.productionTrend.title')}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t('statistics.productionTrend.noData')}
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom fontWeight={600}>
        {t('statistics.productionTrend.title')}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        {t('statistics.productionTrend.description')}
      </Typography>

      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={aggregated} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="date"
            tickFormatter={formatXAxis}
            style={{ fontSize: '12px' }}
          />
          <YAxis
            label={{
              value: t('statistics.productionTrend.yAxisLabel'),
              angle: -90,
              position: 'insideLeft',
              style: { fontSize: '12px' },
            }}
            style={{ fontSize: '12px' }}
          />
          <Tooltip content={ProductionTrendTooltip} />
          <Legend />
          <Line
            type="monotone"
            dataKey="eggs"
            stroke="#FF6B35"
            strokeWidth={2}
            dot={{ r: 4 }}
            activeDot={{ r: 6 }}
            name={t('statistics.productionTrend.eggs')}
          />
        </LineChart>
      </ResponsiveContainer>
    </Box>
  );
}
