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
  Cell,
} from 'recharts';
import { FlockProductivityItem } from '../types';

/**
 * Flock Productivity Chart Component
 *
 * Displays a bar chart comparing productivity across flocks (eggs per hen per day).
 *
 * Features:
 * - Responsive bar chart with Recharts
 * - Sorted by productivity (highest first)
 * - Color gradient based on productivity
 * - Interactive tooltip with detailed stats
 */

interface FlockProductivityChartProps {
  data: FlockProductivityItem[];
}

// Color function based on productivity (0.0 - 1.0 scale)
const getColorByProductivity = (productivity: number): string => {
  if (productivity >= 0.8) return '#37A372'; // High: Green
  if (productivity >= 0.6) return '#FDC830'; // Medium: Yellow
  return '#FF6B35'; // Low: Orange/Red
};

export function FlockProductivityChart({ data }: FlockProductivityChartProps) {
  const { t } = useTranslation();

  // Sort data by productivity (highest first)
  const sortedData = [...(data || [])].sort(
    (a, b) => b.eggsPerHenPerDay - a.eggsPerHenPerDay
  );

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
            {data.flockName}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {t('statistics.flockProductivity.eggsPerHenPerDay')}:{' '}
            {data.eggsPerHenPerDay.toFixed(2)}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {t('statistics.flockProductivity.totalEggs')}: {data.totalEggs}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {t('statistics.flockProductivity.henCount')}: {data.henCount}
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
          {t('statistics.flockProductivity.title')}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t('statistics.flockProductivity.noData')}
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom fontWeight={600}>
        {t('statistics.flockProductivity.title')}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        {t('statistics.flockProductivity.description')}
      </Typography>

      <ResponsiveContainer width="100%" height={300}>
        <BarChart data={sortedData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="flockName"
            style={{ fontSize: '12px' }}
            angle={-45}
            textAnchor="end"
            height={80}
          />
          <YAxis
            label={{
              value: t('statistics.flockProductivity.yAxisLabel'),
              angle: -90,
              position: 'insideLeft',
              style: { fontSize: '12px' },
            }}
            style={{ fontSize: '12px' }}
          />
          <Tooltip content={<CustomTooltip />} />
          <Legend />
          <Bar
            dataKey="eggsPerHenPerDay"
            name={t('statistics.flockProductivity.eggsPerHenPerDay')}
            radius={[8, 8, 0, 0]}
          >
            {sortedData.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={getColorByProductivity(entry.eggsPerHenPerDay)} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>

      {/* Productivity Legend */}
      <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2, mt: 2, flexWrap: 'wrap' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box sx={{ width: 16, height: 16, bgcolor: '#37A372', borderRadius: 1 }} />
          <Typography variant="caption" color="text.secondary">
            {t('statistics.flockProductivity.highProductivity')} (≥0.8)
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box sx={{ width: 16, height: 16, bgcolor: '#FDC830', borderRadius: 1 }} />
          <Typography variant="caption" color="text.secondary">
            {t('statistics.flockProductivity.mediumProductivity')} (0.6-0.8)
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box sx={{ width: 16, height: 16, bgcolor: '#FF6B35', borderRadius: 1 }} />
          <Typography variant="caption" color="text.secondary">
            {t('statistics.flockProductivity.lowProductivity')} (&lt;0.6)
          </Typography>
        </Box>
      </Box>
    </Box>
  );
}
