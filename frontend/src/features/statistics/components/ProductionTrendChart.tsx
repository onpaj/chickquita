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
import dayjs from 'dayjs';

/**
 * Production Trend Chart Component
 *
 * Displays a line chart showing egg production trend over time.
 *
 * Features:
 * - Responsive line chart with Recharts
 * - Date formatting on X-axis
 * - Interactive tooltip with daily totals
 * - Smooth curve animation
 */

interface ProductionTrendChartProps {
  data: ProductionTrendItem[];
}

interface ProductionTrendTooltipProps {
  active?: boolean;
  payload?: readonly { payload: ProductionTrendItem }[];
}

function ProductionTrendTooltip({ active, payload }: ProductionTrendTooltipProps) {
  const { t } = useTranslation();
  if (active && payload && payload.length) {
    const data = payload[0].payload;
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
          {dayjs(data.date).format('DD MMMM YYYY')}
        </Typography>
        <Typography variant="body2" color="primary">
          {t('statistics.productionTrend.eggs')}: {data.eggs}
        </Typography>
      </Box>
    );
  }
  return null;
}

export function ProductionTrendChart({ data }: ProductionTrendChartProps) {
  const { t } = useTranslation();

  // Format date for display
  const formatDate = (dateString: string) => {
    return dayjs(dateString).format('DD/MM');
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
        <LineChart data={data} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="date"
            tickFormatter={formatDate}
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
