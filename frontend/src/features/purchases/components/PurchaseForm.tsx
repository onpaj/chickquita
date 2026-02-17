import { useEffect, useRef, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  Box,
  TextField,
  Button,
  Stack,
  CircularProgress,
  Autocomplete,
  MenuItem,
  InputAdornment,
} from '@mui/material';
import GrainIcon from '@mui/icons-material/Grain';
import VaccinesIcon from '@mui/icons-material/Vaccines';
import BedIcon from '@mui/icons-material/Bed';
import ToysIcon from '@mui/icons-material/Toys';
import LocalHospitalIcon from '@mui/icons-material/LocalHospital';
import MoreHorizIcon from '@mui/icons-material/MoreHoriz';
import { useTranslation } from 'react-i18next';
import { PurchaseType, QuantityUnit, type CreatePurchaseDto, type UpdatePurchaseDto, type PurchaseDto } from '../types';
import { usePurchaseAutocomplete } from '../hooks/usePurchaseAutocomplete';
import { NumericStepper } from '@/shared/components';
import {
  FORM_FIELD_SPACING,
  touchButtonSx,
  touchInputProps,
} from '@/shared/constants/modalConfig';

/**
 * Zod validation schema for purchase form.
 * Validates all form fields according to business rules.
 */
const purchaseFormSchema = z.object({
  type: z.nativeEnum(PurchaseType, {
    error: 'validation.required',
  }),
  name: z
    .string({
      error: 'validation.required',
    })
    .min(1, 'validation.required')
    .max(100, { message: 'validation.maxLength' }),
  purchaseDate: z
    .string({
      error: 'validation.required',
    })
    .min(1, 'validation.required')
    .refine(
      (date) => {
        const selectedDate = new Date(date);
        const today = new Date();
        today.setHours(23, 59, 59, 999);
        return selectedDate <= today;
      },
      { message: 'purchases.form.purchaseDateFuture' }
    ),
  amount: z
    .number({
      error: 'validation.required',
    })
    .positive('validation.positiveNumber')
    .max(999999.99, { message: 'validation.maxAmount' }),
  quantity: z
    .number({
      error: 'validation.required',
    })
    .positive('validation.positiveNumber')
    .max(999999.99, { message: 'validation.maxQuantity' }),
  unit: z.nativeEnum(QuantityUnit, {
    error: 'validation.required',
  }),
  consumedDate: z
    .string()
    .optional()
    .nullable()
    .refine(
      (date) => {
        if (!date) return true;
        const selectedDate = new Date(date);
        const today = new Date();
        today.setHours(23, 59, 59, 999);
        return selectedDate <= today;
      },
      { message: 'purchases.form.consumedDateFuture' }
    ),
  notes: z.string().max(500, { message: 'validation.maxLength' }).optional().nullable(),
  coopId: z.string().optional().nullable(),
});

type PurchaseFormData = z.infer<typeof purchaseFormSchema>;

/**
 * Props for PurchaseForm component
 */
export interface PurchaseFormProps {
  /**
   * Initial data for edit mode (omit for create mode)
   */
  initialData?: PurchaseDto;
  /**
   * Callback when form is submitted with valid data
   */
  onSubmit: (data: CreatePurchaseDto | UpdatePurchaseDto) => void;
  /**
   * Whether the form is currently submitting
   */
  isSubmitting?: boolean;
  /**
   * Available coops for selection (optional - if provided, shows coop selector)
   */
  coops?: Array<{ id: string; name: string }>;
  /**
   * Callback when cancel button is clicked
   */
  onCancel?: () => void;
  /**
   * Optional form id — when provided, action buttons are NOT rendered inside the form.
   * Use this to render action buttons in DialogActions externally via form={formId}.
   */
  formId?: string;
  /**
   * Called whenever form validity changes. Used by parent to disable/enable an external submit button.
   */
  onValidityChange?: (isValid: boolean) => void;
}

/**
 * Get icon for purchase type
 */
const getPurchaseTypeIcon = (type: PurchaseType) => {
  switch (type) {
    case PurchaseType.Feed:
      return <GrainIcon />;
    case PurchaseType.Vitamins:
      return <VaccinesIcon />;
    case PurchaseType.Bedding:
      return <BedIcon />;
    case PurchaseType.Toys:
      return <ToysIcon />;
    case PurchaseType.Veterinary:
      return <LocalHospitalIcon />;
    case PurchaseType.Other:
      return <MoreHorizIcon />;
    default:
      return <MoreHorizIcon />;
  }
};

/**
 * PurchaseForm Component
 *
 * Form component for creating and editing purchases using React Hook Form and Zod validation.
 * Supports autocomplete for purchase names, type icons, and mobile-responsive design.
 *
 * @example
 * // Create mode
 * <PurchaseForm
 *   onSubmit={handleCreate}
 *   isSubmitting={isPending}
 *   coops={availableCoops}
 * />
 *
 * @example
 * // Edit mode
 * <PurchaseForm
 *   initialData={existingPurchase}
 *   onSubmit={handleUpdate}
 *   isSubmitting={isPending}
 * />
 */
export function PurchaseForm({
  initialData,
  onSubmit,
  isSubmitting = false,
  coops,
  onCancel,
  formId,
  onValidityChange,
}: PurchaseFormProps) {
  const { t } = useTranslation();
  const [nameInput, setNameInput] = useState('');
  const { suggestions, isLoading: isLoadingSuggestions } = usePurchaseAutocomplete(nameInput);

  const isEditMode = !!initialData;

  const {
    control,
    handleSubmit,
    watch,
    formState: { errors, isValid },
    setValue,
  } = useForm<PurchaseFormData>({
    resolver: zodResolver(purchaseFormSchema),
    mode: 'onChange',
    defaultValues: initialData
      ? {
          type: initialData.type,
          name: initialData.name,
          purchaseDate: initialData.purchaseDate.split('T')[0],
          amount: initialData.amount,
          quantity: initialData.quantity,
          unit: initialData.unit,
          consumedDate: initialData.consumedDate
            ? initialData.consumedDate.split('T')[0]
            : null,
          notes: initialData.notes || null,
          coopId: initialData.coopId || null,
        }
      : {
          type: PurchaseType.Feed,
          name: '',
          purchaseDate: new Date().toISOString().split('T')[0],
          amount: 0,
          quantity: 0,
          unit: QuantityUnit.Kg,
          consumedDate: null,
          notes: null,
          coopId: null,
        },
  });

  const purchaseDate = watch('purchaseDate');
  const consumedDate = watch('consumedDate');

  // Set initial name input in edit mode
  useEffect(() => {
    if (initialData?.name) {
      setNameInput(initialData.name);
    }
  }, [initialData]);

  // Validate consumed date is after purchase date
  useEffect(() => {
    if (consumedDate && purchaseDate) {
      const purchase = new Date(purchaseDate);
      const consumed = new Date(consumedDate);
      if (consumed < purchase) {
        // Clear consumed date if it's before purchase date
        setValue('consumedDate', null);
      }
    }
  }, [consumedDate, purchaseDate, setValue]);

  // Notify parent of validity changes (used to enable/disable external submit button)
  const onValidityChangeRef = useRef(onValidityChange);
  onValidityChangeRef.current = onValidityChange;
  useEffect(() => {
    onValidityChangeRef.current?.(isValid);
  }, [isValid]);

  const handleFormSubmit = (data: PurchaseFormData) => {
    if (isEditMode && initialData) {
      const updateData: UpdatePurchaseDto = {
        id: initialData.id,
        type: data.type,
        name: data.name.trim(),
        purchaseDate: data.purchaseDate,
        amount: data.amount,
        quantity: data.quantity,
        unit: data.unit,
        consumedDate: data.consumedDate || null,
        notes: data.notes?.trim() || null,
        coopId: data.coopId || null,
      };
      onSubmit(updateData);
    } else {
      const createData: CreatePurchaseDto = {
        type: data.type,
        name: data.name.trim(),
        purchaseDate: data.purchaseDate,
        amount: data.amount,
        quantity: data.quantity,
        unit: data.unit,
        consumedDate: data.consumedDate || null,
        notes: data.notes?.trim() || null,
        coopId: data.coopId || null,
      };
      onSubmit(createData);
    }
  };

  const getTodayDate = (): string => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  };

  const getMinConsumedDate = (): string => {
    return purchaseDate || getTodayDate();
  };

  return (
    <Box
      component="form"
      id={formId}
      onSubmit={handleSubmit(handleFormSubmit)}
      noValidate
      sx={{ width: '100%' }}
    >
      <Stack spacing={FORM_FIELD_SPACING}>
        {/* Purchase Type */}
        <Controller
          name="type"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              select
              label={t('purchases.form.type')}
              error={!!errors.type}
              helperText={errors.type?.message ? t(errors.type.message) : ''}
              required
              fullWidth
              disabled={isSubmitting}
              inputProps={{
                ...touchInputProps,
                'aria-label': t('purchases.form.type'),
              }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    {getPurchaseTypeIcon(field.value)}
                  </InputAdornment>
                ),
              }}
            >
              {Object.entries(PurchaseType)
                .filter(([key]) => isNaN(Number(key)))
                .map(([key, value]) => (
                  <MenuItem key={value} value={value}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      {getPurchaseTypeIcon(value as PurchaseType)}
                      {t(`purchases.types.${key.toLowerCase()}`)}
                    </Box>
                  </MenuItem>
                ))}
            </TextField>
          )}
        />

        {/* Purchase Name with Autocomplete */}
        <Controller
          name="name"
          control={control}
          render={({ field: { onChange, value, onBlur } }) => (
            <Autocomplete
              freeSolo
              options={suggestions}
              value={value}
              inputValue={nameInput}
              onInputChange={(_event, newInputValue) => {
                setNameInput(newInputValue);
                onChange(newInputValue);
              }}
              onChange={(_event, newValue) => {
                if (typeof newValue === 'string') {
                  onChange(newValue);
                  setNameInput(newValue);
                }
              }}
              onBlur={onBlur}
              loading={isLoadingSuggestions}
              disabled={isSubmitting}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label={t('purchases.form.name')}
                  error={!!errors.name}
                  helperText={errors.name?.message ? t(errors.name.message) : ''}
                  required
                  inputProps={{
                    ...params.inputProps,
                    ...touchInputProps,
                    'aria-label': t('purchases.form.name'),
                  }}
                />
              )}
            />
          )}
        />

        {/* Purchase Date */}
        <Controller
          name="purchaseDate"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              type="date"
              label={t('purchases.form.purchaseDate')}
              error={!!errors.purchaseDate}
              helperText={
                errors.purchaseDate?.message ? t(errors.purchaseDate.message) : ''
              }
              required
              fullWidth
              disabled={isSubmitting}
              InputLabelProps={{
                shrink: true,
              }}
              inputProps={{
                max: getTodayDate(),
                ...touchInputProps,
                'aria-label': t('purchases.form.purchaseDate'),
              }}
            />
          )}
        />

        {/* Amount */}
        <Controller
          name="amount"
          control={control}
          render={({ field }) => (
            <NumericStepper
              label={t('purchases.form.amount')}
              value={field.value}
              onChange={field.onChange}
              min={1}
              max={999999}
              step={1}
              disabled={isSubmitting}
              error={!!errors.amount}
              helperText={errors.amount?.message ? t(errors.amount.message) : ''}
              aria-label={t('purchases.form.amount')}
            />
          )}
        />

        {/* Quantity */}
        <Controller
          name="quantity"
          control={control}
          render={({ field }) => (
            <NumericStepper
              label={t('purchases.form.quantity')}
              value={field.value}
              onChange={field.onChange}
              min={0.01}
              max={999999}
              step={0.5}
              disabled={isSubmitting}
              error={!!errors.quantity}
              helperText={errors.quantity?.message ? t(errors.quantity.message) : ''}
              aria-label={t('purchases.form.quantity')}
            />
          )}
        />

        {/* Unit */}
        <Controller
          name="unit"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              select
              label={t('purchases.form.unit')}
              error={!!errors.unit}
              helperText={errors.unit?.message ? t(errors.unit.message) : ''}
              required
              fullWidth
              disabled={isSubmitting}
              inputProps={{
                ...touchInputProps,
                'aria-label': t('purchases.form.unit'),
              }}
            >
              {Object.entries(QuantityUnit)
                .filter(([key]) => isNaN(Number(key)))
                .map(([key, value]) => (
                  <MenuItem key={value} value={value}>
                    {t(`purchases.units.${key.toLowerCase()}`)}
                  </MenuItem>
                ))}
            </TextField>
          )}
        />

        {/* Consumed Date (Optional) */}
        <Controller
          name="consumedDate"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              value={field.value || ''}
              type="date"
              label={t('purchases.form.consumedDate')}
              error={!!errors.consumedDate}
              helperText={
                errors.consumedDate?.message ? t(errors.consumedDate.message) : ''
              }
              fullWidth
              disabled={isSubmitting}
              InputLabelProps={{
                shrink: true,
              }}
              inputProps={{
                min: getMinConsumedDate(),
                max: getTodayDate(),
                ...touchInputProps,
                'aria-label': t('purchases.form.consumedDate'),
              }}
            />
          )}
        />

        {/* Coop Selection (if coops provided) */}
        {coops && coops.length > 0 && (
          <Controller
            name="coopId"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value || ''}
                select
                label={t('purchases.form.coop')}
                error={!!errors.coopId}
                helperText={errors.coopId?.message ? t(errors.coopId.message as string) : ''}
                fullWidth
                disabled={isSubmitting}
                inputProps={{
                  ...touchInputProps,
                  'aria-label': t('purchases.form.coop'),
                }}
              >
                <MenuItem value="">
                  <em>{t('purchases.form.noCoop')}</em>
                </MenuItem>
                {coops.map((coop) => (
                  <MenuItem key={coop.id} value={coop.id}>
                    {coop.name}
                  </MenuItem>
                ))}
              </TextField>
            )}
          />
        )}

        {/* Notes (Optional) */}
        <Controller
          name="notes"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              value={field.value || ''}
              label={t('purchases.form.notes')}
              error={!!errors.notes}
              helperText={errors.notes?.message ? t(errors.notes.message) : ''}
              fullWidth
              multiline
              rows={3}
              disabled={isSubmitting}
              inputProps={{
                maxLength: 500,
                'aria-label': t('purchases.form.notes'),
              }}
            />
          )}
        />

        {/* Form Actions — only rendered when formId is NOT provided */}
        {!formId && (
          <Stack direction="row" spacing={2} justifyContent="flex-end">
            {onCancel && (
              <Button
                onClick={onCancel}
                disabled={isSubmitting}
                sx={touchButtonSx}
                aria-label={t('common.cancel')}
              >
                {t('common.cancel')}
              </Button>
            )}
            <Button
              type="submit"
              variant="contained"
              disabled={isSubmitting || !isValid}
              startIcon={isSubmitting ? <CircularProgress size={20} color="inherit" /> : undefined}
              sx={touchButtonSx}
              aria-label={isEditMode ? t('common.save') : t('common.create')}
            >
              {isSubmitting
                ? t('common.saving')
                : isEditMode
                ? t('common.save')
                : t('common.create')}
            </Button>
          </Stack>
        )}
      </Stack>
    </Box>
  );
}
