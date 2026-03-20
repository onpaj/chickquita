import { useState, useEffect, useRef } from 'react';
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
  Tabs,
  Tab,
  Box,
} from '@mui/material';
import type { TransitionProps } from '@mui/material/transitions';
import CloseIcon from '@mui/icons-material/Close';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { useCreateDailyRecord } from '../hooks/useDailyRecords';
import { useErrorHandler } from '../../../hooks/useErrorHandler';
import { processApiError, ErrorType } from '../../../lib/errors';
import { NumericStepper } from '@/shared/components/NumericStepper';
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
import { useCreateEggSale, useLastUsedEggPrice } from '@/features/eggSales';

const SlideUp = React.forwardRef(function SlideUp(
  props: TransitionProps & { children: React.ReactElement },
  ref: React.Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

interface QuickAddModalProps {
  open: boolean;
  onClose: () => void;
  flocks: Array<{ id: string; identifier: string; coopName: string }>;
  defaultFlockId?: string;
}

const LAST_FLOCK_KEY = 'chickquita_lastUsedFlockId';
const MAX_NOTES_LENGTH = 500;
const MAX_BUYER_NAME_LENGTH = 100;

/**
 * QuickAddModal Component
 *
 * Mobile-optimized modal with two tabs:
 * - "Záznam" tab: quick daily record entry
 * - "Prodej" tab: quick egg sale entry
 *
 * Target completion time: < 30 seconds per entry.
 *
 * @example
 * <QuickAddModal
 *   open={isOpen}
 *   onClose={handleClose}
 *   flocks={availableFlocks}
 * />
 */
export function QuickAddModal({
  open,
  onClose,
  flocks,
  defaultFlockId,
}: QuickAddModalProps) {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState(0);

  // ── Daily record form ───────────────────────────────────────────────────
  const { mutate: createDailyRecord, isPending: isDailyRecordPending } =
    useCreateDailyRecord();
  const { handleError } = useErrorHandler();

  const getInitialFlockId = (): string => {
    if (defaultFlockId) return defaultFlockId;
    const savedFlockId = localStorage.getItem(LAST_FLOCK_KEY);
    if (savedFlockId && flocks.some((f) => f.id === savedFlockId)) {
      return savedFlockId;
    }
    return flocks[0]?.id || '';
  };

  const getCurrentUtcTime = (): string => {
    const now = new Date();
    const hh = String(now.getUTCHours()).padStart(2, '0');
    const mm = String(now.getUTCMinutes()).padStart(2, '0');
    return `${hh}:${mm}`;
  };

  const [flockId, setFlockId] = useState<string>(getInitialFlockId());
  const [eggCount, setEggCount] = useState<number>(0);
  const [recordDate, setRecordDate] = useState<string>(
    new Date().toISOString().split('T')[0]
  );
  const [collectionTime, setCollectionTime] = useState<string>(getCurrentUtcTime());
  const [notesLength, setNotesLength] = useState<number>(0);
  const [flockIdError, setFlockIdError] = useState('');
  const [eggCountError, setEggCountError] = useState('');
  const [recordDateError, setRecordDateError] = useState('');
  const [notesError, setNotesError] = useState('');

  const eggCountRef = useRef<HTMLInputElement>(null);
  const notesRef = useRef<HTMLTextAreaElement>(null);

  // Auto-focus on egg count when daily record tab opens
  useEffect(() => {
    if (open && activeTab === 0) {
      const timer = setTimeout(() => {
        const activeEl = document.activeElement;
        const isTextareaFocused = activeEl?.tagName === 'TEXTAREA';
        if (!isTextareaFocused) {
          eggCountRef.current?.focus();
        }
      }, 100);
      return () => clearTimeout(timer);
    }
  }, [open, activeTab]);

  // Update flockId when modal opens
  useEffect(() => {
    if (open) {
      const savedFlockId = localStorage.getItem(LAST_FLOCK_KEY);
      const initialFlockId =
        defaultFlockId ||
        (savedFlockId && flocks.some((f) => f.id === savedFlockId)
          ? savedFlockId
          : null) ||
        flocks[0]?.id ||
        '';
      if (initialFlockId) {
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setFlockId(initialFlockId);
      }
    }
  }, [open, flocks, defaultFlockId]);

  const validateFlockId = (value: string): string => {
    if (!value) return t('validation.required');
    if (!flocks.some((f) => f.id === value)) return t('errors.notFound');
    return '';
  };

  const validateEggCount = (value: number): string => {
    if (value < 0) return t('validation.positiveNumber');
    return '';
  };

  const validateRecordDate = (value: string): string => {
    if (!value) return t('validation.required');
    const [year, month, day] = value.split('-').map(Number);
    const dateObj = new Date(year, month - 1, day);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (dateObj > today) return t('dailyRecords.dateFutureError');
    return '';
  };

  const validateNotes = (value: string): string => {
    if (value.length > MAX_NOTES_LENGTH)
      return t('validation.maxLength', { count: MAX_NOTES_LENGTH });
    return '';
  };

  const validateDailyRecord = (): boolean => {
    const flockErr = validateFlockId(flockId);
    const eggErr = validateEggCount(eggCount);
    const dateErr = validateRecordDate(recordDate);
    const notesErr = validateNotes(notesRef.current?.value ?? '');
    setFlockIdError(flockErr);
    setEggCountError(eggErr);
    setRecordDateError(dateErr);
    setNotesError(notesErr);
    return !flockErr && !eggErr && !dateErr && !notesErr;
  };

  const isDailyRecordFormValid = (): boolean => {
    return (
      flockId.length > 0 &&
      flocks.some((f) => f.id === flockId) &&
      eggCount >= 0 &&
      recordDate.length > 0 &&
      notesLength <= MAX_NOTES_LENGTH
    );
  };

  const submitDailyRecord = () => {
    if (!validateDailyRecord()) return;
    const notesValue = notesRef.current?.value ?? '';
    localStorage.setItem(LAST_FLOCK_KEY, flockId);
    createDailyRecord(
      {
        flockId,
        data: {
          recordDate,
          eggCount,
          notes: notesValue.trim() || undefined,
          collectionTime: collectionTime || undefined,
        },
      },
      {
        onSuccess: () => handleClose(),
        onError: (error: Error) => {
          const processedError = processApiError(error);
          if (
            processedError.type === ErrorType.VALIDATION &&
            processedError.fieldErrors
          ) {
            processedError.fieldErrors.forEach((fieldError) => {
              const field = fieldError.field.toLowerCase();
              if (field === 'flockid') setFlockIdError(fieldError.message);
              else if (field === 'eggcount') setEggCountError(fieldError.message);
              else if (field === 'recorddate') setRecordDateError(fieldError.message);
              else if (field === 'notes') setNotesError(fieldError.message);
            });
          } else {
            handleError(error, submitDailyRecord);
          }
        },
      }
    );
  };

  // ── Egg sale form ───────────────────────────────────────────────────────
  const { createEggSale, isCreating: isSalePending } = useCreateEggSale();
  const lastUsedPrice = useLastUsedEggPrice();

  const todayStr = new Date().toISOString().split('T')[0];

  const [saleDate, setSaleDate] = useState<string>(todayStr);
  const [saleQuantity, setSaleQuantity] = useState<number>(1);
  const [salePricePerUnit, setSalePricePerUnit] = useState<string>(
    lastUsedPrice !== undefined ? String(lastUsedPrice) : ''
  );
  const [saleBuyerName, setSaleBuyerName] = useState<string>('');
  const saleNotesRef = useRef<HTMLTextAreaElement>(null);
  const [saleNotesLength, setSaleNotesLength] = useState<number>(0);
  const [saleDateError, setSaleDateError] = useState('');
  const [saleQuantityError, setSaleQuantityError] = useState('');
  const [salePriceError, setSalePriceError] = useState('');
  const [saleBuyerNameError, setSaleBuyerNameError] = useState('');
  const [saleNotesError, setSaleNotesError] = useState('');

  // Pre-fill price from last used when modal opens and we have a value
  useEffect(() => {
    if (open && lastUsedPrice !== undefined) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setSalePricePerUnit(String(lastUsedPrice));
    }
  }, [open, lastUsedPrice]);

  const validateSaleDate = (value: string): string => {
    if (!value) return t('validation.required');
    const [year, month, day] = value.split('-').map(Number);
    const dateObj = new Date(year, month - 1, day);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (dateObj > today) return t('eggSales.form.dateFuture');
    return '';
  };

  const validateSaleQuantity = (value: number): string => {
    if (value < 1) return t('eggSales.form.quantityMin');
    return '';
  };

  const validateSalePrice = (value: number): string => {
    if (value < 0) return t('eggSales.form.priceMin');
    return '';
  };

  const validateSaleBuyerName = (value: string): string => {
    if (value.length > MAX_BUYER_NAME_LENGTH)
      return t('validation.maxLength', { count: MAX_BUYER_NAME_LENGTH });
    return '';
  };

  const validateSaleNotes = (value: string): string => {
    if (value.length > MAX_NOTES_LENGTH)
      return t('validation.maxLength', { count: MAX_NOTES_LENGTH });
    return '';
  };

  const validateSaleForm = (): boolean => {
    const dateErr = validateSaleDate(saleDate);
    const quantityErr = validateSaleQuantity(saleQuantity);
    const priceErr = validateSalePrice(parseFloat(salePricePerUnit) || 0);
    const buyerErr = validateSaleBuyerName(saleBuyerName);
    const notesErr = validateSaleNotes(saleNotesRef.current?.value ?? '');
    setSaleDateError(dateErr);
    setSaleQuantityError(quantityErr);
    setSalePriceError(priceErr);
    setSaleBuyerNameError(buyerErr);
    setSaleNotesError(notesErr);
    return !dateErr && !quantityErr && !priceErr && !buyerErr && !notesErr;
  };

  const isSaleFormValid = (): boolean => {
    return (
      saleDate.length > 0 &&
      saleQuantity >= 1 &&
      parseFloat(salePricePerUnit) >= 0 &&
      saleBuyerName.length <= MAX_BUYER_NAME_LENGTH &&
      saleNotesLength <= MAX_NOTES_LENGTH
    );
  };

  const submitSale = () => {
    if (!validateSaleForm()) return;
    const notesValue = saleNotesRef.current?.value ?? '';
    createEggSale(
      {
        date: saleDate,
        quantity: saleQuantity,
        pricePerUnit: parseFloat(salePricePerUnit) || 0,
        buyerName: saleBuyerName.trim() || null,
        notes: notesValue.trim() || null,
      },
      {
        onSuccess: () => handleClose(),
      }
    );
  };

  // ── Shared handlers ─────────────────────────────────────────────────────
  const resetDailyRecordForm = () => {
    setFlockId(getInitialFlockId());
    setEggCount(0);
    setRecordDate(new Date().toISOString().split('T')[0]);
    setCollectionTime(getCurrentUtcTime());
    if (notesRef.current) notesRef.current.value = '';
    setNotesLength(0);
    setFlockIdError('');
    setEggCountError('');
    setRecordDateError('');
    setNotesError('');
  };

  const resetSaleForm = () => {
    setSaleDate(new Date().toISOString().split('T')[0]);
    setSaleQuantity(1);
    setSalePricePerUnit(lastUsedPrice !== undefined ? String(lastUsedPrice) : '');
    setSaleBuyerName('');
    if (saleNotesRef.current) saleNotesRef.current.value = '';
    setSaleNotesLength(0);
    setSaleDateError('');
    setSaleQuantityError('');
    setSalePriceError('');
    setSaleBuyerNameError('');
    setSaleNotesError('');
  };

  const handleClose = () => {
    resetDailyRecordForm();
    resetSaleForm();
    setActiveTab(0);
    onClose();
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (activeTab === 0) submitDailyRecord();
    else submitSale();
  };

  const isPending = activeTab === 0 ? isDailyRecordPending : isSalePending;
  const isFormValid = activeTab === 0 ? isDailyRecordFormValid() : isSaleFormValid();

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
      <form
        onSubmit={handleSubmit}
        style={{ display: 'flex', flexDirection: 'column', height: '100%' }}
      >
        <DialogTitle sx={{ ...dialogTitleSx, pr: 6, pb: 0 }}>
          {t('dailyRecords.quickAdd.title')}
          <IconButton
            aria-label={t('common.close')}
            onClick={handleClose}
            disabled={isPending}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>

        <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}>
          <Tabs
            value={activeTab}
            onChange={(_e, newValue: number) => setActiveTab(newValue)}
            aria-label="quick add tabs"
          >
            <Tab label={t('dailyRecords.quickAdd.tabRecord')} disabled={isPending} />
            <Tab label={t('dailyRecords.quickAdd.tabSale')} disabled={isPending} />
          </Tabs>
        </Box>

        <DialogContent
          dividers
          sx={{
            ...dialogContentSx,
            overflowY: 'auto',
            flex: 1,
          }}
        >
          {/* Daily record tab */}
          {activeTab === 0 && (
            <Stack spacing={FORM_FIELD_SPACING}>
              <TextField
                select
                label={t('dailyRecords.flock')}
                value={flockId}
                onChange={(e) => {
                  setFlockId(e.target.value);
                  if (flockIdError) setFlockIdError(validateFlockId(e.target.value));
                }}
                onBlur={() => setFlockIdError(validateFlockId(flockId))}
                error={!!flockIdError}
                helperText={flockIdError}
                required
                fullWidth
                disabled={isPending || flocks.length === 0}
                inputProps={touchInputProps}
                SelectProps={{ native: true }}
              >
                {flocks.length === 0 ? (
                  <option value="" disabled>
                    {t('dailyRecords.noFlocks')}
                  </option>
                ) : (
                  flocks.map((flock) => (
                    <option
                      key={flock.id}
                      value={flock.id}
                      onClick={() => setFlockId(flock.id)}
                    >
                      {flock.identifier} ({flock.coopName})
                    </option>
                  ))
                )}
              </TextField>

              <TextField
                type="date"
                label={t('dailyRecords.date')}
                value={recordDate}
                onChange={(e) => {
                  setRecordDate(e.target.value);
                  if (recordDateError) setRecordDateError(validateRecordDate(e.target.value));
                }}
                onBlur={() => setRecordDateError(validateRecordDate(recordDate))}
                error={!!recordDateError}
                helperText={recordDateError}
                required
                fullWidth
                disabled={isPending}
                InputLabelProps={{ shrink: true }}
                inputProps={{
                  ...touchInputProps,
                  max: new Date().toISOString().split('T')[0],
                }}
              />

              <TextField
                type="time"
                label={t('dailyRecords.form.collectionTime')}
                value={collectionTime}
                onChange={(e) => setCollectionTime(e.target.value)}
                fullWidth
                disabled={isPending}
                InputLabelProps={{ shrink: true }}
                inputProps={{ ...touchInputProps, step: 60 }}
              />

              <NumericStepper
                label={t('dailyRecords.eggCount')}
                value={eggCount}
                onChange={(value) => {
                  setEggCount(value);
                  if (eggCountError) setEggCountError(validateEggCount(value));
                }}
                min={0}
                max={9999}
                disabled={isPending}
                error={!!eggCountError}
                helperText={eggCountError}
                aria-label="egg count"
                inputRef={eggCountRef}
              />

              <TextField
                label={t('dailyRecords.notes')}
                defaultValue=""
                onChange={(e) => {
                  setNotesLength(e.target.value.length);
                  if (notesError) setNotesError(validateNotes(e.target.value));
                }}
                onBlur={(e) => {
                  const value = e.target.value;
                  setNotesLength(value.length);
                  setNotesError(validateNotes(value));
                }}
                error={!!notesError}
                helperText={
                  notesError ||
                  `${notesLength}/${MAX_NOTES_LENGTH} ${t('common.characters')}`
                }
                fullWidth
                disabled={isPending}
                multiline
                rows={2}
                inputProps={{ ...touchInputProps }}
                inputRef={notesRef}
              />
            </Stack>
          )}

          {/* Egg sale tab */}
          {activeTab === 1 && (
            <Stack spacing={FORM_FIELD_SPACING}>
              <TextField
                type="date"
                label={t('eggSales.form.date')}
                value={saleDate}
                onChange={(e) => {
                  setSaleDate(e.target.value);
                  if (saleDateError) setSaleDateError(validateSaleDate(e.target.value));
                }}
                onBlur={() => setSaleDateError(validateSaleDate(saleDate))}
                error={!!saleDateError}
                helperText={saleDateError}
                required
                fullWidth
                disabled={isSalePending}
                InputLabelProps={{ shrink: true }}
                inputProps={{
                  ...touchInputProps,
                  max: new Date().toISOString().split('T')[0],
                }}
              />

              <NumericStepper
                label={t('eggSales.form.quantity')}
                value={saleQuantity}
                onChange={(value) => {
                  setSaleQuantity(value);
                  if (saleQuantityError) setSaleQuantityError(validateSaleQuantity(value));
                }}
                min={1}
                max={99999}
                disabled={isSalePending}
                error={!!saleQuantityError}
                helperText={saleQuantityError}
                aria-label="sale quantity"
              />

              <TextField
                type="number"
                label={t('eggSales.form.pricePerUnit')}
                value={salePricePerUnit}
                onChange={(e) => {
                  setSalePricePerUnit(e.target.value);
                  if (salePriceError) setSalePriceError(validateSalePrice(parseFloat(e.target.value) || 0));
                }}
                onBlur={() => setSalePriceError(validateSalePrice(parseFloat(salePricePerUnit) || 0))}
                error={!!salePriceError}
                helperText={salePriceError}
                required
                fullWidth
                disabled={isSalePending}
                inputProps={{ ...touchInputProps, min: 0, step: 0.01 }}
                InputLabelProps={{ shrink: true }}
              />

              <TextField
                label={t('eggSales.form.buyerName')}
                value={saleBuyerName}
                onChange={(e) => {
                  setSaleBuyerName(e.target.value);
                  if (saleBuyerNameError)
                    setSaleBuyerNameError(validateSaleBuyerName(e.target.value));
                }}
                onBlur={() => setSaleBuyerNameError(validateSaleBuyerName(saleBuyerName))}
                error={!!saleBuyerNameError}
                helperText={saleBuyerNameError}
                fullWidth
                disabled={isSalePending}
                inputProps={{ ...touchInputProps, maxLength: MAX_BUYER_NAME_LENGTH }}
              />

              <TextField
                label={t('eggSales.form.notes')}
                defaultValue=""
                onChange={(e) => {
                  setSaleNotesLength(e.target.value.length);
                  if (saleNotesError) setSaleNotesError(validateSaleNotes(e.target.value));
                }}
                onBlur={(e) => {
                  const value = e.target.value;
                  setSaleNotesLength(value.length);
                  setSaleNotesError(validateSaleNotes(value));
                }}
                error={!!saleNotesError}
                helperText={
                  saleNotesError ||
                  `${saleNotesLength}/${MAX_NOTES_LENGTH} ${t('common.characters')}`
                }
                fullWidth
                disabled={isSalePending}
                multiline
                rows={2}
                inputProps={{ ...touchInputProps }}
                inputRef={saleNotesRef}
              />
            </Stack>
          )}
        </DialogContent>

        <DialogActions sx={dialogActionsSx}>
          <Button
            variant="text"
            onClick={handleClose}
            disabled={isPending}
            sx={touchButtonSx}
          >
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isPending || !isFormValid}
            startIcon={
              isPending ? <CircularProgress size={20} color="inherit" /> : undefined
            }
            sx={touchButtonSx}
          >
            {isPending ? t('common.saving') : t('common.save')}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
