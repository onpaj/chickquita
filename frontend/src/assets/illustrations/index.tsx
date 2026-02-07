import { Box } from '@mui/material';
import type { SxProps, Theme } from '@mui/material';
import EmptyCoopsSvg from './EmptyCoops.svg?react';
import EmptyFlocksSvg from './EmptyFlocks.svg?react';
import EmptyDashboardSvg from './EmptyDashboard.svg?react';

interface IllustrationProps {
  sx?: SxProps<Theme>;
  'aria-label'?: string;
}

/**
 * Empty Coops Illustration
 * Displays a chicken coop with fence and hay bales
 */
export function EmptyCoopsIllustration({ sx, 'aria-label': ariaLabel }: IllustrationProps) {
  return (
    <Box
      component={EmptyCoopsSvg}
      sx={{
        width: { xs: 120, sm: 200 },
        height: { xs: 120, sm: 200 },
        ...sx,
      }}
      aria-label={ariaLabel || 'Empty chicken coop illustration'}
      role="img"
    />
  );
}

/**
 * Empty Flocks Illustration
 * Displays chickens, a chick, and scattered feed
 */
export function EmptyFlocksIllustration({ sx, 'aria-label': ariaLabel }: IllustrationProps) {
  return (
    <Box
      component={EmptyFlocksSvg}
      sx={{
        width: { xs: 120, sm: 200 },
        height: { xs: 120, sm: 200 },
        ...sx,
      }}
      aria-label={ariaLabel || 'Empty flocks illustration with chickens'}
      role="img"
    />
  );
}

/**
 * Empty Dashboard Illustration
 * Displays a farm scene with barn, windmill, and landscape
 */
export function EmptyDashboardIllustration({ sx, 'aria-label': ariaLabel }: IllustrationProps) {
  return (
    <Box
      component={EmptyDashboardSvg}
      sx={{
        width: { xs: 120, sm: 200 },
        height: { xs: 120, sm: 200 },
        ...sx,
      }}
      aria-label={ariaLabel || 'Empty dashboard farm scene illustration'}
      role="img"
    />
  );
}
