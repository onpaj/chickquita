import { Box, Card, CardContent, Skeleton } from '@mui/material';

/**
 * CoopCardSkeleton Component
 *
 * Loading skeleton placeholder for CoopCard.
 * Matches the structure and dimensions of the actual CoopCard component.
 *
 * @example
 * {isLoading ? (
 *   <CoopCardSkeleton />
 * ) : (
 *   <CoopCard coop={coop} />
 * )}
 */
export function CoopCardSkeleton() {
  return (
    <Card elevation={2} sx={{ minHeight: 120 }}>
      <CardContent>
        {/* Header: Title, Chip, and Menu Icon */}
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'flex-start',
            mb: 1,
          }}
        >
          <Skeleton variant="text" width="60%" height={32} />
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Skeleton variant="rounded" width={60} height={24} />
            <Skeleton variant="circular" width={24} height={24} />
          </Box>
        </Box>

        {/* Location Row */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1 }}>
          <Skeleton variant="circular" width={20} height={20} />
          <Skeleton variant="text" width="40%" height={20} />
        </Box>

        {/* Created Date */}
        <Skeleton variant="text" width="30%" height={16} />
      </CardContent>
    </Card>
  );
}
