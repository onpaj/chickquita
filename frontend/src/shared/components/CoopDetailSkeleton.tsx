import { Box, Container, Paper, Skeleton, Stack } from '@mui/material';

/**
 * CoopDetailSkeleton Component
 *
 * Loading skeleton placeholder for CoopDetailPage.
 * Matches the structure and layout of the actual coop detail view.
 *
 * @example
 * {isLoading ? (
 *   <CoopDetailSkeleton />
 * ) : (
 *   <CoopDetail coop={coop} />
 * )}
 */
export function CoopDetailSkeleton() {
  return (
    <Container maxWidth="sm" sx={{ py: 3 }}>
      {/* Header with Back Button */}
      <Box sx={{ mb: 3 }}>
        <Skeleton variant="rectangular" height={40} width={100} />
      </Box>

      {/* Main Content Card */}
      <Paper sx={{ p: 3 }}>
        <Stack spacing={3}>
          {/* Coop Name */}
          <Box>
            <Skeleton variant="text" width={80} height={16} />
            <Skeleton variant="text" height={40} width="60%" />
          </Box>

          {/* Location */}
          <Box>
            <Skeleton variant="text" width={80} height={16} />
            <Skeleton variant="text" height={24} width="80%" />
          </Box>

          {/* Status */}
          <Box>
            <Skeleton variant="text" width={80} height={16} sx={{ mb: 1 }} />
            <Skeleton variant="rounded" height={24} width={60} />
          </Box>

          {/* Created Date */}
          <Box>
            <Skeleton variant="text" width={80} height={16} />
            <Skeleton variant="text" height={20} width="70%" />
          </Box>

          {/* Updated Date */}
          <Box>
            <Skeleton variant="text" width={80} height={16} />
            <Skeleton variant="text" height={20} width="70%" />
          </Box>

          {/* Action Buttons */}
          <Stack direction="row" spacing={2} sx={{ pt: 2, flexWrap: 'wrap' }}>
            <Skeleton variant="rectangular" height={36} width={120} />
            <Skeleton variant="rectangular" height={36} width={100} />
            <Skeleton variant="rectangular" height={36} width={120} />
            <Skeleton variant="rectangular" height={36} width={100} />
          </Stack>
        </Stack>
      </Paper>
    </Container>
  );
}
