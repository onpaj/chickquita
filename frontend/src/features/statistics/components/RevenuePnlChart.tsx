import { Box, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import type { RevenueTrendItem } from '../types';

/**
 * Revenue & P&L Chart Component
 *
 * Displays a grouped bar chart showing monthly revenue vs. costs.
 * Each month has two bars: revenue (green) and costs (orange).
 * Helps farmers visualise whether they're profitable month over month.
 */

interface RevenuePnlChartProps {
  data: RevenueTrendItem[];
}

interface RevenuePnlTooltipProps {
  active?: boolean;
  payload?: readonly { name: string; value: number; color: string }[];
  label?: string;
}

function RevenuePnlTooltip({ active, payload, label }: RevenuePnlTooltipProps) {
  const { t } = useTranslation();
  if (!active || !payload || payload.length === 0) return null;

  const revenue = payload.find((p) => p.name === t('statistics.revenuePnlChart.revenue'))?.value ?? 0;
  const costs = payload.find((p) => p.name === t('statistics.revenuePnlChart.costs'))?.value ?? 0;
  const net = revenue - costs;

  return (
    <Box
      sx={{
        backgroundColor: 'background.paper',
        p: 1.5,
        border: 1,
        borderColor: 'divider',
        borderRadius: 1,
        minWidth: 160,
      }}
    >
      <Typography variant="body2" fontWeight={600} sx={{ mb: 0.5 }}>
        {label}
      </Typography>
      {payload.map((entry) => (
        <Typography key={entry.name} variant="body2" sx={{ color: entry.color }}>
          {entry.name}: {entry.value.toLocaleString('cs-CZ', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} Kč
        </Typography>
      ))}
      <Typography
        variant="body2"
        fontWeight={600}
        sx={{ color: net >= 0 ? 'success.main' : 'error.main', mt: 0.5 }}
      >
        {t('statistics.revenuePnlChart.net')}: {net.toLocaleString('cs-CZ', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} Kč
      </Typography>
    </Box>
  );
}

export function RevenuePnlChart({ data }: RevenuePnlChartProps) {
  const { t } = useTranslation();

  if (!data || data.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="h6" gutterBottom>
          {t('statistics.revenuePnlChart.title')}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t('statistics.revenuePnlChart.noData')}
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" fontWeight={600} sx={{ mb: 1 }}>
        {t('statistics.revenuePnlChart.title')}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        {t('statistics.revenuePnlChart.description')}
      </Typography>

      <ResponsiveContainer width="100%" height={300}>
        <BarChart data={data} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="month" style={{ fontSize: '12px' }} />
          <YAxis
            tickFormatter={(v) => `${v.toLocaleString('cs-CZ')} Kč`}
            style={{ fontSize: '12px' }}
          />
          <Tooltip content={<RevenuePnlTooltip />} />
          <Legend />
          <Bar
            dataKey="revenue"
            name={t('statistics.revenuePnlChart.revenue')}
            fill="#4caf50"
            radius={[4, 4, 0, 0]}
          />
          <Bar
            dataKey="costs"
            name={t('statistics.revenuePnlChart.costs')}
            fill="#F7931E"
            radius={[4, 4, 0, 0]}
          />
        </BarChart>
      </ResponsiveContainer>
    </Box>
  );
}
