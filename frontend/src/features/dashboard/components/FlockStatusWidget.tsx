import { Card, CardContent, Typography, Box, Skeleton } from '@mui/material';
import { useTranslation } from 'react-i18next';
import PetsIcon from '@mui/icons-material/Pets';

interface FlockStatusWidgetProps {
  totalHens: number;
  totalRoosters: number;
  totalChicks: number;
  activeFlocks: number;
  loading?: boolean;
}

/**
 * Flock Status widget
 * Shows aggregated statistics about all active flocks
 */
export function FlockStatusWidget({
  totalHens,
  totalRoosters,
  totalChicks,
  activeFlocks,
  loading = false,
}: FlockStatusWidgetProps) {
  const { t } = useTranslation();

  const stats = [
    {
      label: t('dashboard.widgets.flockStatus.totalHens'),
      value: totalHens,
      color: '#FF6B9D',
    },
    {
      label: t('dashboard.widgets.flockStatus.totalRoosters'),
      value: totalRoosters,
      color: '#4A90E2',
    },
    {
      label: t('dashboard.widgets.flockStatus.totalChicks'),
      value: totalChicks,
      color: '#FFD93D',
    },
    {
      label: t('dashboard.widgets.flockStatus.activeFlocks'),
      value: activeFlocks,
      color: '#6BCF7F',
    },
  ];

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
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {/* Header */}
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            {loading ? (
              <Skeleton variant="text" width="60%" height={28} />
            ) : (
              <Typography variant="h6" fontWeight="bold">
                {t('dashboard.widgets.flockStatus.title')}
              </Typography>
            )}
            {!loading && (
              <Box sx={{ color: 'primary.main', opacity: 0.7 }}>
                <PetsIcon />
              </Box>
            )}
          </Box>

          {/* Stats grid */}
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'repeat(2, 1fr)',
              gap: 2,
            }}
          >
            {stats.map((stat) => (
              <Box
                key={stat.label}
                sx={{
                  p: 1.5,
                  borderRadius: 1.5,
                  backgroundColor: 'background.default',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 0.5,
                }}
              >
                {loading ? (
                  <>
                    <Skeleton variant="text" width="80%" height={32} />
                    <Skeleton variant="text" width="60%" height={20} />
                  </>
                ) : (
                  <>
                    <Typography
                      variant="h4"
                      fontWeight="bold"
                      sx={{ color: stat.color }}
                    >
                      {stat.value}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {stat.label}
                    </Typography>
                  </>
                )}
              </Box>
            ))}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
}
