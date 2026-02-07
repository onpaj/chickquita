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
        <Skeleton variant="circular" height={48} width={48} sx={{ mb: 2 }} />
        <Skeleton variant="text" height={40} width="60%" />
      </Box>

      {/* Main Content Card */}
      <Paper elevation={2} sx={{ p: 3 }}>
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
            <Skeleton variant="rounded" height={32} width={80} />
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
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            spacing={2}
            sx={{ pt: 2 }}
          >
            <Skeleton variant="rectangular" height={48} width="100%" />
            <Skeleton variant="rectangular" height={48} width="100%" />
            <Skeleton variant="rectangular" height={48} width="100%" />
            <Skeleton variant="rectangular" height={48} width="100%" />
          </Stack>
        </Stack>
      </Paper>
    </Container>
  );
}
