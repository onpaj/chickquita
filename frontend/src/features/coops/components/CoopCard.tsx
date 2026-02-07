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
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import EditIcon from '@mui/icons-material/Edit';
import ArchiveIcon from '@mui/icons-material/Archive';
import DeleteIcon from '@mui/icons-material/Delete';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
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
  const { t } = useTranslation();
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

  const formattedDate = new Date(coop.createdAt).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });

  return (
    <>
      <Card
        data-testid="coop-card"
        elevation={2}
        sx={{
          position: 'relative',
          cursor: 'pointer',
          minHeight: 120,
          transition: 'box-shadow 0.3s ease',
          '&:hover': {
            boxShadow: 4,
          },
        }}
        onClick={handleCardClick}
        role="article"
        aria-label={t('coops.coopCardAriaLabel', { coopName: coop.name })}
      >
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
            <Typography variant="h6" component="h2" sx={{ flexGrow: 1, pr: 1 }} id={`coop-title-${coop.id}`}>
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
                aria-haspopup="true"
                aria-expanded={menuOpen}
                aria-controls={menuOpen ? `coop-menu-${coop.id}` : undefined}
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
