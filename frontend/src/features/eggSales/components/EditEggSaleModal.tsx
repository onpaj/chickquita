import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  CircularProgress,
  TextField,
  IconButton,
  Slide,
} from '@mui/material';
import type { TransitionProps } from '@mui/material/transitions';
import { Delete as DeleteIcon } from '@mui/icons-material';
import CloseIcon from '@mui/icons-material/Close';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { useUpdateEggSale } from '../hooks/useEggSales';
import {
  DIALOG_CONFIG,
  isMobileViewport,
  dialogTitleSx,
  dialogContentSx,
  dialogActionsSx,
  touchButtonSx,
  touchInputProps,
  FORM_FIELD_SPACING,
} from '@/shared/constants/modalConfig';
import type { EggSaleDto } from '../types/eggSale.types';
import { DeleteEggSaleDialog } from './DeleteEggSaleDialog';

const SlideUp = React.forwardRef(function SlideUp(
  props: TransitionProps & { children: React.ReactElement },
  ref: React.Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

interface EditEggSaleModalProps {
  open: boolean;
  onClose: () => void;
  sale: EggSaleDto | null;
}

const MAX_NOTES_LENGTH = 500;
const MAX_BUYER_NAME_LENGTH = 100;

export function EditEggSaleModal({ open, onClose, sale }: EditEggSaleModalProps) {
  const { t } = useTranslation();
  const { updateEggSale, isUpdating } = useUpdateEggSale();

  const [date, setDate] = useState('');
  const [quantity, setQuantity] = useState('');
  const [pricePerUnit, setPricePerUnit] = useState('');
  const [buyerName, setBuyerName] = useState('');
  const [notes, setNotes] = useState('');

  const [dateError, setDateError] = useState('');
  const [quantityError, setQuantityError] = useState('');
  const [priceError, setPriceError] = useState('');
  const [notesError, setNotesError] = useState('');

  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  // Pre-fill form when sale changes
  useEffect(() => {
    if (sale && open) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setDate(sale.date);
      setQuantity(String(sale.quantity));
      setPricePerUnit(String(sale.pricePerUnit));
      setBuyerName(sale.buyerName || '');
      setNotes(sale.notes || '');
      setDateError('');
      setQuantityError('');
      setPriceError('');
      setNotesError('');
    }
  }, [sale, open]);

  const handleClose = () => {
    setIsDeleteDialogOpen(false);
    onClose();
  };

  const validateDate = (value: string): string => {
    if (!value) return t('validation.required');
    const today = new Date();
    today.setHours(23, 59, 59, 999);
    if (new Date(value) > today) return t('eggSales.form.dateFuture');
    return '';
  };

  const validateQuantity = (value: string): string => {
    const num = parseInt(value, 10);
    if (isNaN(num) || num < 1) return t('eggSales.form.quantityMin');
    return '';
  };

  const validatePrice = (value: string): string => {
    const num = parseFloat(value);
    if (isNaN(num) || num < 0) return t('eggSales.form.priceMin');
    return '';
  };

  const validateNotes = (value: string): string => {
    if (value.length > MAX_NOTES_LENGTH) return t('validation.maxLength', { count: MAX_NOTES_LENGTH });
    return '';
  };

  const validate = (): boolean => {
    const dErr = validateDate(date);
    const qErr = validateQuantity(quantity);
    const pErr = validatePrice(pricePerUnit);
    const nErr = validateNotes(notes);

    setDateError(dErr);
    setQuantityError(qErr);
    setPriceError(pErr);
    setNotesError(nErr);

    return !dErr && !qErr && !pErr && !nErr;
  };

  const isFormValid = (): boolean => {
    return (
      !!date &&
      parseInt(quantity, 10) >= 1 &&
      parseFloat(pricePerUnit) >= 0 &&
      notes.length <= MAX_NOTES_LENGTH
    );
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!sale || !validate()) return;

    updateEggSale(
      {
        id: sale.id,
        date,
        quantity: parseInt(quantity, 10),
        pricePerUnit: parseFloat(pricePerUnit),
        buyerName: buyerName.trim() || undefined,
        notes: notes.trim() || undefined,
      },
      {
        onSuccess: () => {
          handleClose();
        },
      }
    );
  };

  if (!sale) return null;

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth={DIALOG_CONFIG.maxWidth}
      fullWidth={DIALOG_CONFIG.fullWidth}
      fullScreen={isMobileViewport()}
      TransitionComponent={SlideUp}
      sx={{
        '& .MuiDialog-paper': {
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '100vh',
        },
      }}
    >
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <DialogTitle sx={{ ...dialogTitleSx, pr: 6 }}>
          {t('eggSales.editSale')}
          <IconButton
            aria-label={t('common.close')}
            onClick={handleClose}
            disabled={isUpdating}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>

        <DialogContent
          dividers
          sx={{ ...dialogContentSx, overflowY: 'auto', flex: 1 }}
        >
          <Stack spacing={FORM_FIELD_SPACING}>
            {/* Date */}
            <TextField
              type="date"
              label={t('eggSales.form.date')}
              value={date}
              onChange={(e) => {
                setDate(e.target.value);
                if (dateError) setDateError(validateDate(e.target.value));
              }}
              onBlur={() => setDateError(validateDate(date))}
              error={!!dateError}
              helperText={dateError}
              fullWidth
              disabled={isUpdating}
              InputLabelProps={{ shrink: true }}
              inputProps={{ ...touchInputProps, max: new Date().toISOString().split('T')[0] }}
            />

            {/* Quantity */}
            <TextField
              type="number"
              label={t('eggSales.form.quantity')}
              value={quantity}
              onChange={(e) => {
                setQuantity(e.target.value);
                if (quantityError) setQuantityError(validateQuantity(e.target.value));
              }}
              onBlur={() => setQuantityError(validateQuantity(quantity))}
              error={!!quantityError}
              helperText={quantityError}
              fullWidth
              disabled={isUpdating}
              inputProps={{ ...touchInputProps, min: 1, step: 1 }}
            />

            {/* Price per unit */}
            <TextField
              type="number"
              label={t('eggSales.form.pricePerUnit')}
              value={pricePerUnit}
              onChange={(e) => {
                setPricePerUnit(e.target.value);
                if (priceError) setPriceError(validatePrice(e.target.value));
              }}
              onBlur={() => setPriceError(validatePrice(pricePerUnit))}
              error={!!priceError}
              helperText={priceError}
              fullWidth
              disabled={isUpdating}
              inputProps={{ ...touchInputProps, min: 0, step: 0.01 }}
            />

            {/* Buyer name (optional) */}
            <TextField
              label={t('eggSales.form.buyerName')}
              value={buyerName}
              onChange={(e) => setBuyerName(e.target.value.slice(0, MAX_BUYER_NAME_LENGTH))}
              fullWidth
              disabled={isUpdating}
              inputProps={touchInputProps}
            />

            {/* Notes (optional) */}
            <TextField
              label={t('eggSales.form.notes')}
              value={notes}
              onChange={(e) => {
                setNotes(e.target.value);
                if (notesError) setNotesError(validateNotes(e.target.value));
              }}
              onBlur={() => setNotesError(validateNotes(notes))}
              error={!!notesError}
              helperText={
                notesError ||
                `${notes.length}/${MAX_NOTES_LENGTH} ${t('common.characters')}`
              }
              fullWidth
              disabled={isUpdating}
              multiline
              rows={2}
              inputProps={touchInputProps}
            />
          </Stack>
        </DialogContent>

        <DialogActions sx={dialogActionsSx}>
          <Button
            onClick={() => setIsDeleteDialogOpen(true)}
            disabled={isUpdating}
            color="error"
            startIcon={<DeleteIcon />}
            sx={{ ...touchButtonSx, mr: 'auto' }}
          >
            {t('common.delete')}
          </Button>
          <Button
            variant="text"
            onClick={handleClose}
            disabled={isUpdating}
            sx={touchButtonSx}
          >
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isUpdating || !isFormValid()}
            startIcon={
              isUpdating ? <CircularProgress size={20} color="inherit" /> : undefined
            }
            sx={touchButtonSx}
          >
            {isUpdating ? t('common.saving') : t('common.save')}
          </Button>
        </DialogActions>
      </form>

      <DeleteEggSaleDialog
        open={isDeleteDialogOpen}
        onClose={() => setIsDeleteDialogOpen(false)}
        sale={sale}
        onSuccess={handleClose}
      />
    </Dialog>
  );
}
