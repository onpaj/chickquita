import { Box, Button, Typography } from '@mui/material';
import type { ReactNode } from 'react';

interface IllustratedEmptyStateProps {
  illustration: ReactNode;
  title: string;
  description: string;
  actionLabel?: string;
  onAction?: () => void;
  actionIcon?: ReactNode;
}

/**
 * IllustratedEmptyState Component
 *
 * Displays an empty state with illustration, title, description, and optional action button.
 * Used when there's no data to display or when guiding users to perform their first action.
 *
 * @example
 * <IllustratedEmptyState
 *   illustration={<SomeIcon sx={{ fontSize: 120 }} />}
 *   title="No coops yet"
 *   description="Get started by creating your first coop"
 *   actionLabel="Create Coop"
 *   onAction={() => navigate('/coops/new')}
 * />
 */
export function IllustratedEmptyState({
  illustration,
  title,
  description,
  actionLabel,
  onAction,
  actionIcon,
}: IllustratedEmptyStateProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        py: 6,
        px: 3,
        minHeight: 300,
      }}
    >
      {/* Illustration */}
      <Box
        sx={{
          mb: 3,
          color: 'text.disabled',
          '& > *': {
            fontSize: { xs: 80, sm: 120 },
          },
        }}
      >
        {illustration}
      </Box>

      {/* Title */}
      <Typography
        variant="h5"
        component="h2"
        gutterBottom
        sx={{
          fontWeight: 600,
          color: 'text.primary',
        }}
      >
        {title}
      </Typography>

      {/* Description */}
      <Typography
        variant="body1"
        color="text.secondary"
        sx={{
          mb: actionLabel ? 3 : 0,
          maxWidth: 400,
        }}
      >
        {description}
      </Typography>

      {/* Action Button */}
      {actionLabel && onAction && (
        <Button
          variant="contained"
          color="primary"
          onClick={onAction}
          startIcon={actionIcon}
          size="large"
        >
          {actionLabel}
        </Button>
      )}
    </Box>
  );
}
