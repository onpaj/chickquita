import { useState } from 'react';
import {
  Card,
  CardHeader,
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
  useTheme,
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import EditIcon from '@mui/icons-material/Edit';
import ArchiveIcon from '@mui/icons-material/Archive';
import HistoryIcon from '@mui/icons-material/History';
import PetsIcon from '@mui/icons-material/Pets';
import FemaleIcon from '@mui/icons-material/Female';
import MaleIcon from '@mui/icons-material/Male';
import EggIcon from '@mui/icons-material/Egg';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import type { Flock } from '../api/flocksApi';
import { MatureChicksModal } from './MatureChicksModal';

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
  const theme = useTheme();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [isMatureChicksModalOpen, setIsMatureChicksModalOpen] = useState(false);

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

  const handleMatureChicks = (event: React.MouseEvent) => {
    event.stopPropagation();
    handleMenuClose();
    setIsMatureChicksModalOpen(true);
  };

  return (
    <>
      <Card
        data-testid="flock-card"
        elevation={1}
        sx={{
          position: 'relative',
          cursor: 'pointer',
          borderRadius: 2,
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
        <CardHeader
          title={
            <Typography variant="subtitle1" component="h2" id={`flock-title-${flock.id}`}>
              {flock.identifier}
            </Typography>
          }
          subheader={
            <Typography
              variant="body2"
              color="text.secondary"
              aria-label={t('flocks.belongsToCoopAriaLabel', { coopName })}
            >
              {t('flocks.coop')}: {coopName}
            </Typography>
          }
          action={
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Chip
                label={flock.isActive ? t('flocks.active') : t('flocks.archived')}
                color={flock.isActive ? 'success' : 'default'}
                size="small"
              />
              <IconButton
                size="small"
                onClick={handleMenuClick}
                aria-label={t('common.more')}
                aria-expanded={menuOpen}
                aria-haspopup="true"
                aria-controls={menuOpen ? `flock-menu-${flock.id}` : undefined}
              >
                <MoreVertIcon />
              </IconButton>
            </Box>
          }
        />
        <CardContent sx={{ pt: 0 }}>

          {/* Composition */}
          <Stack spacing={1}>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <FemaleIcon
                  sx={{
                    fontSize: 18,
                    color: theme.palette.error.main,
                  }}
                />
                <Typography variant="body2" color="text.secondary">
                  {t('flocks.hens')}:
                </Typography>
              </Box>
              <Typography variant="body2" fontWeight="medium" data-testid="flock-hens">
                {flock.currentHens}
              </Typography>
            </Box>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <MaleIcon
                  sx={{
                    fontSize: 18,
                    color: theme.palette.info.main,
                  }}
                />
                <Typography variant="body2" color="text.secondary">
                  {t('flocks.roosters')}:
                </Typography>
              </Box>
              <Typography variant="body2" fontWeight="medium" data-testid="flock-roosters">
                {flock.currentRoosters}
              </Typography>
            </Box>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <EggIcon
                  sx={{
                    fontSize: 18,
                    color: theme.palette.warning.main,
                  }}
                />
                <Typography variant="body2" color="text.secondary">
                  {t('flocks.chicks')}:
                </Typography>
              </Box>
              <Typography variant="body2" fontWeight="medium" data-testid="flock-chicks">
                {flock.currentChicks}
              </Typography>
            </Box>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                pt: 1.5,
                mt: 0.5,
                borderTop: 2,
                borderColor: 'primary.main',
              }}
            >
              <Typography variant="body1" fontWeight="bold" color="primary">
                {t('flocks.total')}:
              </Typography>
              <Typography
                variant="h6"
                fontWeight="bold"
                color="primary"
                sx={{ lineHeight: 1 }}
                data-testid="flock-total"
              >
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
        <MenuItem
          onClick={handleMatureChicks}
          disabled={!flock.isActive || flock.currentChicks === 0}
          aria-label={t('flocks.matureChicks.action')}
        >
          <ListItemIcon>
            <PetsIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('flocks.matureChicks.action')}</ListItemText>
        </MenuItem>
      </Menu>

      {/* Mature Chicks Modal */}
      {isMatureChicksModalOpen && (
        <MatureChicksModal
          open={isMatureChicksModalOpen}
          onClose={() => setIsMatureChicksModalOpen(false)}
          flock={flock}
        />
      )}
    </>
  );
}
