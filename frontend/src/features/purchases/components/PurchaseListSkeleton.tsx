import { Box, Card, CardContent, Skeleton, Stack } from '@mui/material';

/**
 * Loading skeleton component that matches the PurchaseList layout.
 * Displays placeholder content while purchase data is being fetched.
 */
export function PurchaseListSkeleton() {
  return (
    <Box>
      {/* Monthly Summary Skeleton */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Skeleton variant="text" width={120} height={16} sx={{ mb: 1 }} />
          <Skeleton variant="text" width={150} height={40} />
        </CardContent>
      </Card>

      {/* Filters Skeleton */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Stack spacing={2}>
            <Skeleton variant="text" width={100} height={28} />
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1 }} />
              <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1 }} />
            </Stack>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1 }} />
              <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1 }} />
            </Stack>
          </Stack>
        </CardContent>
      </Card>

      {/* Purchase Cards Skeleton */}
      <Stack spacing={2}>
        {[1, 2, 3, 4, 5].map((i) => (
          <Card key={i} elevation={2}>
            <CardContent>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'flex-start',
                  mb: 2,
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexGrow: 1 }}>
                  <Skeleton variant="rectangular" width={40} height={40} sx={{ borderRadius: 1 }} />
                  <Box sx={{ flexGrow: 1 }}>
                    <Skeleton variant="text" width="60%" height={28} />
                    <Skeleton variant="text" width="40%" height={20} />
                  </Box>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Skeleton variant="circular" width={44} height={44} />
                  <Skeleton variant="circular" width={44} height={44} />
                </Box>
              </Box>

              <Stack spacing={1}>
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <Skeleton variant="text" width={80} height={20} />
                  <Skeleton variant="rectangular" width={100} height={24} sx={{ borderRadius: 12 }} />
                </Box>
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <Skeleton variant="text" width={80} height={20} />
                  <Skeleton variant="text" width={120} height={28} />
                </Box>
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <Skeleton variant="text" width={80} height={20} />
                  <Skeleton variant="text" width={80} height={20} />
                </Box>
              </Stack>
            </CardContent>
          </Card>
        ))}
      </Stack>
    </Box>
  );
}
