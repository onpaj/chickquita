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
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import EditIcon from '@mui/icons-material/Edit';
import ArchiveIcon from '@mui/icons-material/Archive';
import DeleteIcon from '@mui/icons-material/Delete';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { cs, enUS } from 'date-fns/locale';
import { useArchiveCoop, useDeleteCoop } from '../hooks/useCoops';
import { EditCoopModal } from './EditCoopModal';
import { ArchiveCoopDialog } from './ArchiveCoopDialog';
import { DeleteCoopDialog } from './DeleteCoopDialog';
import { useToast } from '../../../hooks/useToast';
import { useErrorHandler } from '../../../hooks/useErrorHandler';
import { processApiError, ErrorType } from '../../../lib/errors';
import type { Coop } from '../api/coopsApi';

interface CoopCardProps {
  coop: Coop;
  onMenuClick?: (event: React.MouseEvent<HTMLButtonElement>, coop: Coop) => void;
}

export function CoopCard({ coop }: CoopCardProps) {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { mutate: archiveCoop, isPending: isArchiving } = useArchiveCoop();
  const { mutate: deleteCoop, isPending: isDeleting } = useDeleteCoop();
  const { showSuccess, showError } = useToast();
  const { handleError } = useErrorHandler();

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isArchiveDialogOpen, setIsArchiveDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  const menuOpen = Boolean(anchorEl);

  const handleCardClick = () => {
    navigate(`/coops/${coop.id}`);
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
    setIsEditModalOpen(true);
  };

  const handleArchive = (event: React.MouseEvent) => {
    event.stopPropagation();
    handleMenuClose();
    setIsArchiveDialogOpen(true);
  };

  const handleDelete = (event: React.MouseEvent) => {
    event.stopPropagation();
    handleMenuClose();
    setIsDeleteDialogOpen(true);
  };

  const handleConfirmArchive = () => {
    archiveCoop(coop.id, {
      onSuccess: () => {
        showSuccess(t('coops.archiveSuccess'));
        setIsArchiveDialogOpen(false);
      },
      onError: (error: Error) => {
        setIsArchiveDialogOpen(false);
        handleError(error, handleConfirmArchive);
      },
    });
  };

  const handleConfirmDelete = () => {
    deleteCoop(coop.id, {
      onSuccess: () => {
        showSuccess(t('coops.deleteSuccess'));
        setIsDeleteDialogOpen(false);
      },
      onError: (error: Error) => {
        setIsDeleteDialogOpen(false);
        const processedError = processApiError(error);
        if (processedError.type === ErrorType.VALIDATION) {
          showError(t('coops.deleteErrorHasFlocks'));
        } else {
          handleError(error, handleConfirmDelete);
        }
      },
    });
  };

  const dateLocale = i18n.language === 'cs' ? cs : enUS;
  const formattedDate = format(new Date(coop.createdAt), 'd. MMMM yyyy', { locale: dateLocale });

  return (
    <>
      <Card
        data-testid="coop-card"
        elevation={1}
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
        aria-label={t('coops.coopCardAriaLabel', { coopName: coop.name })}
      >
        <CardHeader
          title={
            <Typography variant="h6" component="h2" id={`coop-title-${coop.id}`}>
              {coop.name}
            </Typography>
          }
          action={
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Chip
                label={coop.isActive ? t('coops.active') : t('coops.archived')}
                color={coop.isActive ? 'success' : 'default'}
                size="small"
              />
              <IconButton
                size="small"
                onClick={handleMenuClick}
                aria-label={t('common.more')}
                aria-haspopup="true"
                aria-expanded={menuOpen}
                aria-controls={menuOpen ? `coop-menu-${coop.id}` : undefined}
              >
                <MoreVertIcon />
              </IconButton>
            </Box>
          }
        />
        <CardContent sx={{ pt: 0 }}>
          {coop.location && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1 }}>
              <LocationOnIcon fontSize="small" color="action" />
              <Typography variant="body2" color="text.secondary">
                {coop.location}
              </Typography>
            </Box>
          )}

          <Typography variant="body2" color="text.secondary">
            {t('coops.createdAt', { date: formattedDate })}
          </Typography>
        </CardContent>
      </Card>

      {/* Action Menu */}
      <Menu
        id={`coop-menu-${coop.id}`}
        anchorEl={anchorEl}
        open={menuOpen}
        onClose={handleMenuClose}
        onClick={(e) => e.stopPropagation()}
        MenuListProps={{
          'aria-labelledby': `coop-title-${coop.id}`,
        }}
      >
        <MenuItem onClick={handleEdit} disabled={!coop.isActive}>
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('common.edit')}</ListItemText>
        </MenuItem>
        <MenuItem onClick={handleArchive} disabled={!coop.isActive || isArchiving}>
          <ListItemIcon>
            <ArchiveIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('coops.archiveCoop')}</ListItemText>
        </MenuItem>
        <MenuItem onClick={handleDelete} disabled={!coop.isActive || isDeleting}>
          <ListItemIcon>
            <DeleteIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('common.delete')}</ListItemText>
        </MenuItem>
      </Menu>

      {/* Edit Modal */}
      <EditCoopModal
        open={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        coop={coop}
      />

      {/* Archive Dialog */}
      <ArchiveCoopDialog
        open={isArchiveDialogOpen}
        onClose={() => setIsArchiveDialogOpen(false)}
        onConfirm={handleConfirmArchive}
        coopName={coop.name}
        isPending={isArchiving}
      />

      {/* Delete Dialog */}
      <DeleteCoopDialog
        open={isDeleteDialogOpen}
        onClose={() => setIsDeleteDialogOpen(false)}
        onConfirm={handleConfirmDelete}
        coopName={coop.name}
        isPending={isDeleting}
      />
    </>
  );
}
