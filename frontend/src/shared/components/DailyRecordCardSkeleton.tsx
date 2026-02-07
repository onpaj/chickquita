import { Card, CardContent, Skeleton, Box } from '@mui/material';

/**
 * DailyRecordCardSkeleton Component
 *
 * Loading skeleton for DailyRecordCard component.
 * Matches the layout of DailyRecordCard to provide a smooth loading experience.
 *
 * @example
 * {isLoading ? (
 *   <DailyRecordCardSkeleton />
 * ) : (
 *   <DailyRecordCard record={record} />
 * )}
 */
export function DailyRecordCardSkeleton() {
  return (
    <Card
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <CardContent sx={{ flexGrow: 1, pb: 2 }}>
        {/* Date header skeleton */}
        <Box sx={{ mb: 2 }}>
          <Skeleton variant="text" width="60%" height={32} />
        </Box>

        {/* Egg count box skeleton */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            mb: 2,
            p: 2,
            bgcolor: 'action.hover',
            borderRadius: 2,
          }}
        >
          <Skeleton variant="circular" width={32} height={32} />
          <Box sx={{ flexGrow: 1 }}>
            <Skeleton variant="text" width={60} height={48} />
            <Skeleton variant="text" width={40} height={16} />
          </Box>
        </Box>

        {/* Flock chip skeleton */}
        <Box sx={{ mb: 1.5 }}>
          <Skeleton variant="rounded" width={80} height={24} />
        </Box>

        {/* Notes skeleton */}
        <Box sx={{ mt: 1.5 }}>
          <Skeleton variant="text" width="100%" />
          <Skeleton variant="text" width="70%" />
        </Box>
      </CardContent>
    </Card>
  );
}
