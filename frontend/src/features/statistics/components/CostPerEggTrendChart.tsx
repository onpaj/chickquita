import { Box, Typography, Chip } from '@mui/material';
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
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import type { CostPerEggTrendItem } from '../types';
import dayjs from 'dayjs';

/**
 * Cost Per Egg Trend Chart Component
 *
 * Displays a line chart showing the trend of cost per egg over time.
 *
 * Features:
 * - Responsive line chart with Recharts
 * - Date formatting on X-axis
 * - Currency formatting on Y-axis
 * - Trend indicator (up/down)
 * - Interactive tooltip
 */

interface CostPerEggTrendChartProps {
  data: CostPerEggTrendItem[];
}

export function CostPerEggTrendChart({ data }: CostPerEggTrendChartProps) {
  const { t } = useTranslation();

  // Format date for display
  const formatDate = (dateString: string) => {
    return dayjs(dateString).format('DD/MM');
  };

  // Calculate trend (compare first and last values)
  const calculateTrend = () => {
    if (!data || data.length < 2) return null;

    const firstValue = data[0].costPerEgg;
    const lastValue = data[data.length - 1].costPerEgg;
    const change = lastValue - firstValue;
    const percentageChange = (change / firstValue) * 100;

    return {
      direction: change > 0 ? 'up' : 'down',
      percentage: Math.abs(percentageChange),
    };
  };

  const trend = calculateTrend();

  // Custom tooltip
  const CustomTooltip = ({ active, payload }: any) => {
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
            {t('statistics.costPerEggTrend.costPerEgg')}: {data.costPerEgg.toFixed(2)} Kč
          </Typography>
        </Box>
      );
    }
    return null;
  };

  // Empty state
  if (!data || data.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="h6" gutterBottom>
          {t('statistics.costPerEggTrend.title')}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t('statistics.costPerEggTrend.noData')}
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
        <Typography variant="h6" fontWeight={600}>
          {t('statistics.costPerEggTrend.title')}
        </Typography>

        {/* Trend Indicator */}
        {trend && (
          <Chip
            size="small"
            icon={trend.direction === 'up' ? <TrendingUpIcon /> : <TrendingDownIcon />}
            label={`${trend.percentage.toFixed(1)}%`}
            color={trend.direction === 'up' ? 'error' : 'success'}
          />
        )}
      </Box>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        {t('statistics.costPerEggTrend.description')}
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
              value: t('statistics.costPerEggTrend.yAxisLabel'),
              angle: -90,
              position: 'insideLeft',
              style: { fontSize: '12px' },
            }}
            tickFormatter={(value) => `${value.toFixed(2)} Kč`}
            style={{ fontSize: '12px' }}
          />
          <Tooltip content={<CustomTooltip />} />
          <Legend />
          <Line
            type="monotone"
            dataKey="costPerEgg"
            stroke="#F7931E"
            strokeWidth={2}
            dot={{ r: 4 }}
            activeDot={{ r: 6 }}
            name={t('statistics.costPerEggTrend.costPerEgg')}
          />
        </LineChart>
      </ResponsiveContainer>
    </Box>
  );
}
