import { Box, Card, CardContent, Skeleton, Stack } from '@mui/material';

/**
 * FlockCardSkeleton Component
 *
 * Loading skeleton placeholder for FlockCard.
 * Matches the structure and dimensions of the actual FlockCard component.
 *
 * @example
 * {isLoading ? (
 *   <FlockCardSkeleton />
 * ) : (
 *   <FlockCard flock={flock} coopName={coopName} />
 * )}
 */
export function FlockCardSkeleton() {
  return (
    <Card elevation={2}>
      <CardContent>
        {/* Header: Title, Chip, and Menu Icon */}
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'flex-start',
            mb: 2,
          }}
        >
          <Skeleton variant="text" width="60%" height={32} />
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Skeleton variant="rounded" width={60} height={24} />
            <Skeleton variant="circular" width={24} height={24} />
          </Box>
        </Box>

        {/* Coop Name */}
        <Skeleton variant="text" width="50%" height={20} sx={{ mb: 2 }} />

        {/* Composition Stack */}
        <Stack spacing={1}>
          {/* Hens */}
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}
          >
            <Skeleton variant="text" width={60} height={20} />
            <Skeleton variant="text" width={30} height={20} />
          </Box>

          {/* Roosters */}
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}
          >
            <Skeleton variant="text" width={60} height={20} />
            <Skeleton variant="text" width={30} height={20} />
          </Box>

          {/* Chicks */}
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}
          >
            <Skeleton variant="text" width={60} height={20} />
            <Skeleton variant="text" width={30} height={20} />
          </Box>

          {/* Total (with border) */}
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              pt: 1,
              borderTop: 1,
              borderColor: 'divider',
            }}
          >
            <Skeleton variant="text" width={60} height={20} />
            <Skeleton variant="text" width={30} height={20} />
          </Box>
        </Stack>
      </CardContent>
    </Card>
  );
}
