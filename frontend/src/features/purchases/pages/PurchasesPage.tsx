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
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
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

const PURCHASE_FORM_ID = 'purchase-form';

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

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPurchase, setEditingPurchase] = useState<PurchaseDto | null>(null);
  const [isFormValid, setIsFormValid] = useState(false);

  // CRUD hooks
  const { createPurchase, isCreating } = useCreatePurchase();
  const { updatePurchase, isUpdating } = useUpdatePurchase();
  const { data: coops } = useCoops();

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
    if ('id' in data) {
      // Update existing purchase
      updatePurchase(data, {
        onSuccess: () => {
          handleModalClose();
        },
      });
    } else {
      // Create new purchase
      createPurchase(data, {
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
    <Container sx={{ pb: 10, pt: 3 }}>
      {/* Page Header */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h4" component="h1">
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

      {/* Purchase List */}
      <PurchaseList onEdit={handleEditClick} />

      {/* Mobile FAB */}
      {isMobile && (
        <Fab
          color="primary"
          aria-label={t('purchases.addPurchase')}
          sx={{
            position: 'fixed',
            bottom: 80,
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
      >
        <DialogTitle sx={dialogTitleSx}>
          {editingPurchase
            ? t('purchases.editPurchase')
            : t('purchases.createPurchase')}
        </DialogTitle>
        <DialogContent sx={dialogContentSx}>
          <PurchaseForm
            initialData={editingPurchase || undefined}
            onSubmit={handleFormSubmit}
            isSubmitting={isSubmitting}
            coops={coopsForForm}
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
