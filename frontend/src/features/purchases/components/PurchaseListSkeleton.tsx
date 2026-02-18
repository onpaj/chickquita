import {
  Box,
  Card,
  CardContent,
  Skeleton,
  Stack,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Divider,
} from '@mui/material';

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
          <Skeleton data-testid="skeleton" variant="text" width={120} height={16} sx={{ mb: 1 }} />
          <Skeleton data-testid="skeleton" variant="text" width={150} height={40} />
        </CardContent>
      </Card>

      {/* Filters Skeleton */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Stack spacing={2}>
            <Skeleton variant="text" width={100} height={28} />
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1, flex: 1 }} />
              <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1, flex: 1 }} />
            </Stack>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1, flex: 1 }} />
              <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1, flex: 1 }} />
            </Stack>
          </Stack>
        </CardContent>
      </Card>

      {/* Purchase List Skeleton — matches List with Actions pattern */}
      <Card>
        <List disablePadding>
          {[1, 2, 3, 4, 5].map((i) => (
            <Box key={i}>
              {i > 1 && <Divider component="li" />}
              <ListItem
                alignItems="flex-start"
                secondaryAction={
                  <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <Skeleton variant="circular" width={44} height={44} />
                    <Skeleton variant="circular" width={44} height={44} />
                  </Box>
                }
                sx={{ pr: 14 }}
              >
                <ListItemAvatar>
                  <Skeleton variant="circular" width={40} height={40} />
                </ListItemAvatar>
                <ListItemText
                  primary={<Skeleton variant="text" width="55%" height={24} />}
                  secondary={
                    <Stack component="span" spacing={0.25} sx={{ display: 'flex', flexDirection: 'column' }}>
                      <Skeleton variant="text" width="35%" height={20} />
                      <Skeleton variant="text" width="40%" height={20} />
                      <Skeleton variant="text" width="50%" height={20} />
                    </Stack>
                  }
                />
              </ListItem>
            </Box>
          ))}
        </List>
      </Card>
    </Box>
  );
}
