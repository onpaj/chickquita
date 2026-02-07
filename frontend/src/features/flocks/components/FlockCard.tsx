import { useState } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  IconButton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Stack,
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import EditIcon from '@mui/icons-material/Edit';
import ArchiveIcon from '@mui/icons-material/Archive';
import HistoryIcon from '@mui/icons-material/History';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { Flock } from '../api/flocksApi';

interface FlockCardProps {
  flock: Flock;
  coopName: string;
  onEdit?: (flock: Flock) => void;
  onArchive?: (flock: Flock) => void;
  onViewHistory?: (flock: Flock) => void;
}

export function FlockCard({
  flock,
  coopName,
  onEdit,
  onArchive,
  onViewHistory,
}: FlockCardProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const menuOpen = Boolean(anchorEl);

  const totalAnimals = flock.currentHens + flock.currentRoosters + flock.currentChicks;

  const handleCardClick = () => {
    navigate(`/coops/${flock.coopId}/flocks/${flock.id}`);
  };

  const handleMenuClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    event.stopPropagation();
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleEdit = (event: React.MouseEvent) => {
    event.stopPropagation();
    handleMenuClose();
    onEdit?.(flock);
  };

  const handleArchive = (event: React.MouseEvent) => {
    event.stopPropagation();
    handleMenuClose();
    onArchive?.(flock);
  };

  const handleViewHistory = (event: React.MouseEvent) => {
    event.stopPropagation();
    handleMenuClose();
    onViewHistory?.(flock);
  };

  return (
    <>
      <Card
        data-testid="flock-card"
        elevation={2}
        sx={{
          position: 'relative',
          cursor: 'pointer',
          transition: 'box-shadow 0.3s ease',
          '&:hover': {
            boxShadow: 4,
          },
        }}
        onClick={handleCardClick}
        role="article"
        aria-label={t('flocks.flockCardAriaLabel', {
          identifier: flock.identifier,
          coopName,
        })}
      >
        <CardContent>
          {/* Header with title and status/menu */}
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'flex-start',
              mb: 2,
            }}
          >
            <Typography
              variant="h6"
              component="h2"
              sx={{ flexGrow: 1, pr: 1 }}
              id={`flock-title-${flock.id}`}
            >
              {flock.identifier}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Chip
                label={flock.isActive ? t('flocks.active') : t('flocks.archived')}
                color={flock.isActive ? 'success' : 'default'}
                size="small"
                sx={{ height: 24 }}
              />
              <IconButton
                onClick={handleMenuClick}
                aria-label={t('common.more')}
                aria-expanded={menuOpen}
                aria-haspopup="true"
                aria-controls={menuOpen ? `flock-menu-${flock.id}` : undefined}
                sx={{
                  p: 1.5,
                  minWidth: 44,
                  minHeight: 44,
                }}
              >
                <MoreVertIcon />
              </IconButton>
            </Box>
          </Box>

          {/* Coop name */}
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ mb: 2 }}
            aria-label={t('flocks.belongsToCoopAriaLabel', { coopName })}
          >
            {t('flocks.coop')}: {coopName}
          </Typography>

          {/* Composition */}
          <Stack spacing={1}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="body2" color="text.secondary">
                {t('flocks.hens')}:
              </Typography>
              <Typography variant="body2" fontWeight="medium">
                {flock.currentHens}
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="body2" color="text.secondary">
                {t('flocks.roosters')}:
              </Typography>
              <Typography variant="body2" fontWeight="medium">
                {flock.currentRoosters}
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="body2" color="text.secondary">
                {t('flocks.chicks')}:
              </Typography>
              <Typography variant="body2" fontWeight="medium">
                {flock.currentChicks}
              </Typography>
            </Box>
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
              <Typography variant="body2" fontWeight="medium">
                {t('flocks.total')}:
              </Typography>
              <Typography variant="body2" fontWeight="bold">
                {totalAnimals}
              </Typography>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      {/* Action Menu */}
      <Menu
        id={`flock-menu-${flock.id}`}
        anchorEl={anchorEl}
        open={menuOpen}
        onClose={handleMenuClose}
        onClick={(e) => e.stopPropagation()}
        MenuListProps={{
          'aria-labelledby': `flock-title-${flock.id}`,
        }}
      >
        <MenuItem
          onClick={handleEdit}
          disabled={!flock.isActive || !onEdit}
          aria-label={t('common.edit')}
        >
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('common.edit')}</ListItemText>
        </MenuItem>
        <MenuItem
          onClick={handleArchive}
          disabled={!flock.isActive || !onArchive}
          aria-label={t('flocks.archiveFlock')}
        >
          <ListItemIcon>
            <ArchiveIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('flocks.archiveFlock')}</ListItemText>
        </MenuItem>
        <MenuItem
          onClick={handleViewHistory}
          disabled={!onViewHistory}
          aria-label={t('flocks.viewHistory')}
        >
          <ListItemIcon>
            <HistoryIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('flocks.viewHistory')}</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
}
