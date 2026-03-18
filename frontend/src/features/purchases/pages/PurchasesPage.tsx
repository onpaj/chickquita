import { useState } from 'react';
import {
  Box,
  Typography,
  Fab,
  Container,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  useMediaQuery,
  useTheme,
  Button,
  IconButton,
  Slide,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import type { TransitionProps } from '@mui/material/transitions';
import { forwardRef } from 'react';
import AddIcon from '@mui/icons-material/Add';
import CloseIcon from '@mui/icons-material/Close';
import { useTranslation } from 'react-i18next';
import { PurchaseList } from '../components/PurchaseList';
import { PurchaseForm } from '../components/PurchaseForm';
import { useCreatePurchase, useUpdatePurchase } from '../hooks/usePurchases';
import { useCoops } from '../../coops/hooks/useCoops';
import type { PurchaseDto, CreatePurchaseDto, UpdatePurchaseDto } from '../types/purchase.types';
import {
  DIALOG_CONFIG,
  dialogTitleSx,
  dialogContentSx,
  dialogActionsSx,
  touchButtonSx,
} from '@/shared/constants/modalConfig';
import { useUserSettingsContext } from '@/features/settings';

const PURCHASE_FORM_ID = 'purchase-form';

const SlideUp = forwardRef(function SlideUp(
  props: TransitionProps & { children: React.ReactElement },
  ref: React.Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

/**
 * PurchasesPage Component
 *
 * Main page for managing purchases with the following features:
 * - Title "Nákupy"
 * - Create button (desktop) / FAB (mobile)
 * - PurchaseList with filtering
 * - Create/Edit modal with PurchaseForm
 * - Responsive layout (mobile-first)
 * - Protected with authentication via route
 *
 * User flows:
 * 1. Create: Click FAB/button -> Modal opens -> Fill form -> Submit -> Modal closes
 * 2. Edit: Click edit icon on purchase card -> Modal opens with pre-filled data -> Submit -> Modal closes
 */
export function PurchasesPage() {
  const { t } = useTranslation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { singleCoopMode } = useUserSettingsContext();

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPurchase, setEditingPurchase] = useState<PurchaseDto | null>(null);
  const [isFormValid, setIsFormValid] = useState(false);

  // Coop filter state (only used in multi-coop mode)
  const [selectedCoopId, setSelectedCoopId] = useState<string>('');

  // CRUD hooks
  const { createPurchase, isCreating } = useCreatePurchase();
  const { updatePurchase, isUpdating } = useUpdatePurchase();
  const { data: coops } = useCoops();

  const effectiveCoopId = singleCoopMode ? coops?.[0]?.id : selectedCoopId;

  const isSubmitting = isCreating || isUpdating;

  // Handle create button click
  const handleCreateClick = () => {
    setEditingPurchase(null);
    setIsFormValid(false);
    setIsModalOpen(true);
  };

  // Handle edit button click
  const handleEditClick = (purchase: PurchaseDto) => {
    setEditingPurchase(purchase);
    setIsFormValid(false);
    setIsModalOpen(true);
  };

  // Handle modal close
  const handleModalClose = () => {
    if (!isSubmitting) {
      setIsModalOpen(false);
      setEditingPurchase(null);
    }
  };

  // Handle form submit
  const handleFormSubmit = (data: CreatePurchaseDto | UpdatePurchaseDto) => {
    const dataWithCoop = {
      ...data,
      coopId: data.coopId || effectiveCoopId || null,
    };
    if ('id' in dataWithCoop) {
      // Update existing purchase
      updatePurchase(dataWithCoop as UpdatePurchaseDto, {
        onSuccess: () => {
          handleModalClose();
        },
      });
    } else {
      // Create new purchase
      createPurchase(dataWithCoop as CreatePurchaseDto, {
        onSuccess: () => {
          handleModalClose();
        },
      });
    }
  };

  // Prepare coops data for form
  const coopsForForm = coops?.map((coop) => ({
    id: coop.id,
    name: coop.name,
  }));

  return (
    <Container maxWidth="lg" sx={{ pt: 3 }}>
      {/* Page Header */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h5" component="h1">
          {t('purchases.title')}
        </Typography>

        {/* Desktop Create Button */}
        {!isMobile && (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateClick}
            aria-label={t('purchases.addPurchase')}
            sx={{
              minHeight: 48,
              px: 3,
            }}
          >
            {t('purchases.addPurchase')}
          </Button>
        )}
      </Box>

      {/* Coop selector — hidden in single-coop mode */}
      {!singleCoopMode && (
        <FormControl fullWidth sx={{ mb: 3 }}>
          <InputLabel id="coop-filter-label">
            {t('purchases.filters.coop')}
          </InputLabel>
          <Select
            labelId="coop-filter-label"
            value={selectedCoopId}
            label={t('purchases.filters.coop')}
            onChange={(e) => setSelectedCoopId(e.target.value)}
          >
            <MenuItem value="">{t('purchases.filters.allCoops')}</MenuItem>
            {coops?.filter((c) => c.isActive).map((coop) => (
              <MenuItem key={coop.id} value={coop.id}>
                {coop.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      )}

      {/* Purchase List */}
      <PurchaseList onEdit={handleEditClick} />

      {/* Mobile FAB */}
      {isMobile && (
        <Fab
          color="primary"
          aria-label={t('purchases.addPurchase')}
          sx={{
            position: 'fixed',
            bottom: { xs: 'calc(env(safe-area-inset-bottom) + 80px)', sm: 24 },
            right: 16,
            minWidth: 56,
            minHeight: 56,
          }}
          onClick={handleCreateClick}
        >
          <AddIcon />
        </Fab>
      )}

      {/* Create/Edit Modal */}
      <Dialog
        open={isModalOpen}
        onClose={handleModalClose}
        {...DIALOG_CONFIG}
        fullScreen={isMobile}
        TransitionComponent={SlideUp}
      >
        <DialogTitle sx={{ ...dialogTitleSx, pr: 6 }}>
          {editingPurchase
            ? t('purchases.editPurchase')
            : t('purchases.createPurchase')}
          <IconButton
            onClick={handleModalClose}
            disabled={isSubmitting}
            aria-label={t('common.close')}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers sx={dialogContentSx}>
          <PurchaseForm
            initialData={editingPurchase || undefined}
            onSubmit={handleFormSubmit}
            onCancel={handleModalClose}
            isSubmitting={isSubmitting}
            coops={!singleCoopMode ? coopsForForm : undefined}
            formId={PURCHASE_FORM_ID}
            onValidityChange={setIsFormValid}
          />
        </DialogContent>
        <DialogActions sx={dialogActionsSx}>
          <Button
            onClick={handleModalClose}
            disabled={isSubmitting}
            sx={touchButtonSx}
          >
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            form={PURCHASE_FORM_ID}
            variant="contained"
            disabled={isSubmitting || !isFormValid}
            startIcon={isSubmitting ? <CircularProgress size={20} color="inherit" /> : undefined}
            sx={touchButtonSx}
          >
            {isSubmitting
              ? t('common.saving')
              : editingPurchase
              ? t('common.save')
              : t('common.create')}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}
