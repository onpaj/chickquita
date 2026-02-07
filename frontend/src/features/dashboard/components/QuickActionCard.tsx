import { Card, CardActionArea, CardContent, Typography, Box } from '@mui/material';
import type { ReactNode } from 'react';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';

interface QuickActionCardProps {
  title: string;
  description: string;
  icon: ReactNode;
  onClick: () => void;
  disabled?: boolean;
}

/**
 * Quick action card component for dashboard
 * Provides navigation to common tasks
 */
export function QuickActionCard({
  title,
  description,
  icon,
  onClick,
  disabled = false,
}: QuickActionCardProps) {
  return (
    <Card
      elevation={2}
      sx={{
        borderRadius: 2,
        height: '100%',
        opacity: disabled ? 0.6 : 1,
      }}
    >
      <CardActionArea
        onClick={onClick}
        disabled={disabled}
        aria-label={`${title}: ${description}`}
        sx={{
          height: '100%',
          display: 'flex',
          alignItems: 'stretch',
          justifyContent: 'flex-start',
        }}
      >
        <CardContent
          sx={{
            width: '100%',
            p: 2,
            '&:last-child': { pb: 2 },
          }}
        >
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 2,
            }}
          >
            {/* Icon */}
            <Box
              sx={{
                width: 48,
                height: 48,
                borderRadius: 2,
                backgroundColor: 'primary.light',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'primary.main',
                flexShrink: 0,
              }}
            >
              {icon}
            </Box>

            {/* Text content */}
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography
                variant="subtitle1"
                fontWeight={600}
                sx={{
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                }}
              >
                {title}
              </Typography>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                }}
              >
                {description}
              </Typography>
            </Box>

            {/* Chevron indicator */}
            <ChevronRightIcon sx={{ color: 'text.secondary', flexShrink: 0 }} aria-hidden="true" />
          </Box>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}
