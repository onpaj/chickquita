import { Box, Card, CardContent, Skeleton, Typography } from '@mui/material';
import type { ReactNode } from 'react';

interface StatCardProps {
  icon: ReactNode;
  label: string;
  value: string | number;
  trend?: {
    value: number;
    label: string;
    direction: 'up' | 'down' | 'neutral';
  };
  loading?: boolean;
  color?: 'primary' | 'secondary' | 'success' | 'error' | 'warning' | 'info';
}

/**
 * StatCard Component
 *
 * Displays a key statistic with an icon, label, value, and optional trend indicator.
 * Used on dashboard to show important metrics like total eggs, costs, etc.
 *
 * @example
 * <StatCard
 *   icon={<EggIcon />}
 *   label="Total Eggs"
 *   value={1234}
 *   trend={{ value: 12, label: "this week", direction: "up" }}
 *   color="primary"
 * />
 */
export function StatCard({
  icon,
  label,
  value,
  trend,
  loading = false,
  color = 'primary',
}: StatCardProps) {
  const getTrendColor = () => {
    if (trend?.direction === 'up') return 'success.main';
    if (trend?.direction === 'down') return 'error.main';
    return 'text.secondary';
  };

  if (loading) {
    return (
      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <Skeleton variant="circular" width={40} height={40} />
            <Skeleton
              variant="text"
              width={100}
              height={24}
              sx={{ ml: 'auto' }}
            />
          </Box>
          <Skeleton variant="text" width="60%" height={40} />
          <Skeleton variant="text" width="40%" height={20} sx={{ mt: 1 }} />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card
      sx={{
        height: '100%',
        transition: 'transform 0.2s ease, box-shadow 0.2s ease',
        '&:hover': {
          transform: 'translateY(-2px)',
        },
      }}
    >
      <CardContent>
        {/* Icon and Label Row */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            mb: 2,
          }}
        >
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              width: 40,
              height: 40,
              borderRadius: 1,
              bgcolor: `${color}.main`,
              color: `${color}.contrastText`,
              '& > *': {
                fontSize: 24,
              },
            }}
          >
            {icon}
          </Box>

          <Typography
            variant="subtitle2"
            color="text.secondary"
            sx={{ ml: 'auto', textTransform: 'uppercase', letterSpacing: 0.5 }}
          >
            {label}
          </Typography>
        </Box>

        {/* Value */}
        <Typography
          variant="h4"
          component="div"
          sx={{
            fontWeight: 700,
            color: 'text.primary',
            mb: trend ? 1 : 0,
          }}
        >
          {value}
        </Typography>

        {/* Trend Indicator */}
        {trend && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Typography
              variant="body2"
              sx={{
                fontWeight: 600,
                color: getTrendColor(),
              }}
            >
              {trend.direction === 'up' && '+'}
              {trend.value}
              {trend.direction !== 'neutral' && (
                <span style={{ marginLeft: 4 }}>
                  {trend.direction === 'up' ? '↑' : '↓'}
                </span>
              )}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {trend.label}
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  );
}
