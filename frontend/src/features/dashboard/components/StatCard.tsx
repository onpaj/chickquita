import { Card, CardContent, Typography, Box, Skeleton } from '@mui/material';
import type { ReactNode } from 'react';

interface StatCardProps {
  title: string;
  value: string | number;
  icon?: ReactNode;
  loading?: boolean;
  subtitle?: string;
}

/**
 * Reusable statistic card component with skeleton loading state
 * Used for displaying dashboard statistics
 */
export function StatCard({ title, value, icon, loading = false, subtitle }: StatCardProps) {
  return (
    <Card
      elevation={2}
      sx={{
        borderRadius: 2,
        height: '100%',
        transition: 'transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 4,
        },
      }}
    >
      <CardContent>
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            gap: 1,
          }}
        >
          {/* Title row with optional icon */}
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
            }}
          >
            {loading ? (
              <Skeleton variant="text" width="60%" height={24} />
            ) : (
              <Typography variant="body2" color="text.secondary" fontWeight={500}>
                {title}
              </Typography>
            )}
            {icon && !loading && (
              <Box sx={{ color: 'primary.main', opacity: 0.7 }}>{icon}</Box>
            )}
          </Box>

          {/* Main value */}
          {loading ? (
            <Skeleton variant="text" width="80%" height={48} />
          ) : (
            <Typography variant="h3" fontWeight="bold" color="primary.main">
              {value}
            </Typography>
          )}

          {/* Optional subtitle */}
          {subtitle && (
            loading ? (
              <Skeleton variant="text" width="50%" height={20} />
            ) : (
              <Typography variant="caption" color="text.secondary">
                {subtitle}
              </Typography>
            )
          )}
        </Box>
      </CardContent>
    </Card>
  );
}

/**
 * Skeleton variant for loading state
 * Can be used independently
 */
export function StatCardSkeleton() {
  return (
    <Card
      elevation={2}
      sx={{
        borderRadius: 2,
        height: '100%',
      }}
    >
      <CardContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          <Skeleton variant="text" width="60%" height={24} />
          <Skeleton variant="text" width="80%" height={48} />
          <Skeleton variant="text" width="50%" height={20} />
        </Box>
      </CardContent>
    </Card>
  );
}
