import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  IconButton,
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import { useTranslation } from 'react-i18next';
import type { Coop } from '../api/coopsApi';

interface CoopCardProps {
  coop: Coop;
  onMenuClick?: (event: React.MouseEvent<HTMLButtonElement>, coop: Coop) => void;
}

export function CoopCard({ coop, onMenuClick }: CoopCardProps) {
  const { t } = useTranslation();

  const handleMenuClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    event.stopPropagation();
    onMenuClick?.(event, coop);
  };

  const formattedDate = new Date(coop.createdAt).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });

  return (
    <Card elevation={2} sx={{ position: 'relative' }}>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
          <Typography variant="h6" component="h2" sx={{ flexGrow: 1, pr: 1 }}>
            {coop.name}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Chip
              label={t('coops.active')}
              color="success"
              size="small"
              sx={{ height: 24 }}
            />
            <IconButton
              size="small"
              onClick={handleMenuClick}
              aria-label={t('common.more')}
            >
              <MoreVertIcon />
            </IconButton>
          </Box>
        </Box>

        {coop.location && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1 }}>
            <LocationOnIcon fontSize="small" color="action" />
            <Typography variant="body2" color="text.secondary">
              {coop.location}
            </Typography>
          </Box>
        )}

        <Typography variant="caption" color="text.secondary">
          {t('coops.createdAt', { date: formattedDate })}
        </Typography>
      </CardContent>
    </Card>
  );
}
