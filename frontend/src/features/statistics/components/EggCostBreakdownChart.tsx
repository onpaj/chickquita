import { Box, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from 'recharts';
import type { CostBreakdownItem } from '../types';

/**
 * Egg Cost Breakdown Chart Component
 *
 * Displays a pie chart showing the breakdown of egg production costs by purchase type.
 *
 * Features:
 * - Responsive pie chart with Recharts
 * - Color-coded by purchase type
 * - Percentage labels
 * - Interactive tooltip
 * - Legend with percentages
 */

interface EggCostBreakdownChartProps {
  data: CostBreakdownItem[];
}

// Color palette for purchase types
const COLORS = ['#FF6B35', '#F7931E', '#FDC830', '#37A372', '#4ECDC4', '#3D5A80'];

// Custom label renderer for pie slices
const renderLabel = (props: any) => {
  return `${(props.percentage as number).toFixed(1)}%`;
};

interface EggCostBreakdownTooltipProps {
  active?: boolean;
  payload?: readonly { payload: CostBreakdownItem }[];
}

function EggCostBreakdownTooltip({ active, payload }: EggCostBreakdownTooltipProps) {
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
          {t(`purchases.types.${data.type}`)}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t('statistics.costBreakdown.amount')}: {data.amount.toFixed(2)} Kč
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {data.percentage.toFixed(1)}%
        </Typography>
      </Box>
    );
  }
  return null;
}

export function EggCostBreakdownChart({ data }: EggCostBreakdownChartProps) {
  const { t } = useTranslation();

  // Empty state
  if (!data || data.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="h6" gutterBottom>
          {t('statistics.costBreakdown.title')}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t('statistics.costBreakdown.noData')}
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom fontWeight={600}>
        {t('statistics.costBreakdown.title')}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        {t('statistics.costBreakdown.description')}
      </Typography>

      <ResponsiveContainer width="100%" height={300}>
        <PieChart>
          <Pie
            data={data}
            cx="50%"
            cy="50%"
            labelLine={false}
            label={renderLabel}
            outerRadius={80}
            fill="#8884d8"
            dataKey="amount"
          >
            {data.map((_entry, index) => (
              <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
            ))}
          </Pie>
          <Tooltip content={EggCostBreakdownTooltip} />
          <Legend
            formatter={(_value: string, entry: any) => {
              const item = entry.payload;
              return `${t(`purchases.types.${item.type}`)} (${item.percentage.toFixed(1)}%)`;
            }}
          />
        </PieChart>
      </ResponsiveContainer>
    </Box>
  );
}
