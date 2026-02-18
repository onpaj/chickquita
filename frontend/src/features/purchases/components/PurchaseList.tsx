import { useState, useMemo } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Stack,
  TextField,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  IconButton,
  Chip,
  type SelectChangeEvent,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import LocalHospitalIcon from '@mui/icons-material/LocalHospital';
import MedicationIcon from '@mui/icons-material/Medication';
import ToyTrainIcon from '@mui/icons-material/Toys';
import BedIcon from '@mui/icons-material/Bed';
import MoreHorizIcon from '@mui/icons-material/MoreHoriz';
import { useTranslation } from 'react-i18next';
import { IllustratedEmptyState, ConfirmationDialog } from '../../../shared/components';
import { PurchaseListSkeleton } from './PurchaseListSkeleton';
import { usePurchases, useDeletePurchase } from '../hooks/usePurchases';
import { useCoops } from '../../coops/hooks/useCoops';
import { PurchaseType, QuantityUnit, type PurchaseDto } from '../types/purchase.types';

interface PurchaseListProps {
  onEdit?: (purchase: PurchaseDto) => void;
}

/**
 * PurchaseList component displays a filterable list of purchases.
 * Features:
 * - Filter by date range, type, and flock
 * - Summary card showing total spent this month
 * - Purchase cards with type icon, name, date, amount, quantity
 * - Edit and delete actions
 * - Empty state and loading skeleton
 */
export function PurchaseList({ onEdit }: PurchaseListProps) {
  const { t } = useTranslation();

  // Filter state
  const [fromDate, setFromDate] = useState<string>('');
  const [toDate, setToDate] = useState<string>('');
  const [typeFilter, setTypeFilter] = useState<PurchaseType | ''>('');
  const [flockIdFilter, setFlockIdFilter] = useState<string>('');

  // Delete confirmation state
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [purchaseToDelete, setPurchaseToDelete] = useState<PurchaseDto | null>(null);

  // Fetch data
  const { purchases, isLoading } = usePurchases({
    fromDate: fromDate || undefined,
    toDate: toDate || undefined,
    type: typeFilter === '' ? undefined : typeFilter,
    flockId: flockIdFilter || undefined,
  });

  const { data: coops } = useCoops();
  const { deletePurchase, isDeleting } = useDeletePurchase();

  // Build flock map (flockId -> flock name)
  const flockMap = useMemo(() => {
    const map = new Map<string, string>();
    // Future enhancement: fetch flocks for each coop and build map
    // Currently using placeholder until flock integration is needed
    if (coops) {
      // Placeholder for future implementation
    }
    return map;
  }, [coops]);

  // Calculate monthly summary
  const monthlySummary = useMemo(() => {
    if (!purchases) return 0;

    const now = new Date();
    const currentMonth = now.getMonth();
    const currentYear = now.getFullYear();

    return purchases
      .filter((p) => {
        const purchaseDate = new Date(p.purchaseDate);
        return (
          purchaseDate.getMonth() === currentMonth &&
          purchaseDate.getFullYear() === currentYear
        );
      })
      .reduce((sum, p) => sum + p.amount, 0);
  }, [purchases]);

  // Get icon for purchase type
  const getTypeIcon = (type: PurchaseType) => {
    switch (type) {
      case PurchaseType.Feed:
        return <ShoppingCartIcon />;
      case PurchaseType.Vitamins:
        return <MedicationIcon />;
      case PurchaseType.Bedding:
        return <BedIcon />;
      case PurchaseType.Toys:
        return <ToyTrainIcon />;
      case PurchaseType.Veterinary:
        return <LocalHospitalIcon />;
      case PurchaseType.Other:
        return <MoreHorizIcon />;
      default:
        return <ShoppingCartIcon />;
    }
  };

  // Get label for purchase type
  const getTypeLabel = (type: PurchaseType) => {
    switch (type) {
      case PurchaseType.Feed:
        return t('purchases.types.feed');
      case PurchaseType.Vitamins:
        return t('purchases.types.vitamins');
      case PurchaseType.Bedding:
        return t('purchases.types.bedding');
      case PurchaseType.Toys:
        return t('purchases.types.toys');
      case PurchaseType.Veterinary:
        return t('purchases.types.veterinary');
      case PurchaseType.Other:
        return t('purchases.types.other');
      default:
        return t('purchases.types.other');
    }
  };

  // Get label for quantity unit
  const getUnitLabel = (unit: QuantityUnit) => {
    switch (unit) {
      case QuantityUnit.Kg:
        return t('purchases.units.kg');
      case QuantityUnit.Pcs:
        return t('purchases.units.pcs');
      case QuantityUnit.L:
        return t('purchases.units.l');
      case QuantityUnit.Package:
        return t('purchases.units.package');
      case QuantityUnit.Other:
        return t('purchases.units.other');
      default:
        return '';
    }
  };

  // Handle filter changes
  const handleTypeFilterChange = (event: SelectChangeEvent<string>) => {
    const value = event.target.value;
    setTypeFilter(value === '' ? '' : Number(value) as PurchaseType);
  };

  const handleFlockFilterChange = (event: SelectChangeEvent<string>) => {
    setFlockIdFilter(event.target.value);
  };

  // Handle delete
  const handleDeleteClick = (purchase: PurchaseDto) => {
    setPurchaseToDelete(purchase);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = () => {
    if (purchaseToDelete) {
      deletePurchase(purchaseToDelete.id);
      setDeleteDialogOpen(false);
      setPurchaseToDelete(null);
    }
  };

  const handleDeleteCancel = () => {
    setDeleteDialogOpen(false);
    setPurchaseToDelete(null);
  };

  // Format date for display
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat(t('common.locale'), {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    }).format(date);
  };

  // Loading state
  if (isLoading) {
    return <PurchaseListSkeleton />;
  }

  // Empty state
  if (!purchases || purchases.length === 0) {
    return (
      <Box>
        {/* Filters */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Stack spacing={2}>
              <Typography variant="h6" component="h2">
                {t('purchases.filters.title')}
              </Typography>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField
                  label={t('purchases.filters.fromDate')}
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                  InputLabelProps={{ shrink: true }}
                  fullWidth
                  aria-label={t('purchases.filters.fromDate')}
                />
                <TextField
                  label={t('purchases.filters.toDate')}
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                  InputLabelProps={{ shrink: true }}
                  fullWidth
                  aria-label={t('purchases.filters.toDate')}
                />
              </Stack>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <FormControl fullWidth>
                  <InputLabel id="type-filter-label">
                    {t('purchases.filters.type')}
                  </InputLabel>
                  <Select
                    labelId="type-filter-label"
                    value={typeFilter === '' ? '' : String(typeFilter)}
                    onChange={handleTypeFilterChange}
                    label={t('purchases.filters.type')}
                    aria-label={t('purchases.filters.type')}
                  >
                    <MenuItem value="">
                      {t('common.all')}
                    </MenuItem>
                    <MenuItem value={String(PurchaseType.Feed)}>
                      {t('purchases.types.feed')}
                    </MenuItem>
                    <MenuItem value={String(PurchaseType.Vitamins)}>
                      {t('purchases.types.vitamins')}
                    </MenuItem>
                    <MenuItem value={String(PurchaseType.Bedding)}>
                      {t('purchases.types.bedding')}
                    </MenuItem>
                    <MenuItem value={String(PurchaseType.Toys)}>
                      {t('purchases.types.toys')}
                    </MenuItem>
                    <MenuItem value={String(PurchaseType.Veterinary)}>
                      {t('purchases.types.veterinary')}
                    </MenuItem>
                    <MenuItem value={String(PurchaseType.Other)}>
                      {t('purchases.types.other')}
                    </MenuItem>
                  </Select>
                </FormControl>
                <FormControl fullWidth>
                  <InputLabel id="flock-filter-label">
                    {t('purchases.filters.flock')}
                  </InputLabel>
                  <Select
                    labelId="flock-filter-label"
                    value={flockIdFilter}
                    onChange={handleFlockFilterChange}
                    label={t('purchases.filters.flock')}
                    disabled
                    aria-label={t('purchases.filters.flock')}
                  >
                    <MenuItem value="">
                      {t('common.all')}
                    </MenuItem>
                  </Select>
                </FormControl>
              </Stack>
            </Stack>
          </CardContent>
        </Card>

        <IllustratedEmptyState
          illustration={<ShoppingCartIcon sx={{ fontSize: 80, color: 'text.secondary' }} />}
          title={t('purchases.emptyState.title')}
          description={t('purchases.emptyState.description')}
        />
      </Box>
    );
  }

  return (
    <Box>
      {/* Monthly Summary Card */}
      <Card sx={{ mb: 3, backgroundColor: 'primary.light', color: 'primary.contrastText' }}>
        <CardContent>
          <Typography variant="overline" sx={{ opacity: 0.9 }}>
            {t('purchases.summary.thisMonth')}
          </Typography>
          <Typography variant="h4" fontWeight="bold">
            {monthlySummary.toFixed(2)} {t('purchases.currency')}
          </Typography>
        </CardContent>
      </Card>

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Stack spacing={2}>
            <Typography variant="h6" component="h2">
              {t('purchases.filters.title')}
            </Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                label={t('purchases.filters.fromDate')}
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                InputLabelProps={{ shrink: true }}
                fullWidth
                aria-label={t('purchases.filters.fromDate')}
              />
              <TextField
                label={t('purchases.filters.toDate')}
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                InputLabelProps={{ shrink: true }}
                fullWidth
                aria-label={t('purchases.filters.toDate')}
              />
            </Stack>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <FormControl fullWidth>
                <InputLabel id="type-filter-label">
                  {t('purchases.filters.type')}
                </InputLabel>
                <Select
                  labelId="type-filter-label"
                  value={typeFilter === '' ? '' : String(typeFilter)}
                  onChange={handleTypeFilterChange}
                  label={t('purchases.filters.type')}
                  aria-label={t('purchases.filters.type')}
                >
                  <MenuItem value="">
                    {t('common.all')}
                  </MenuItem>
                  <MenuItem value={String(PurchaseType.Feed)}>
                    {t('purchases.types.feed')}
                  </MenuItem>
                  <MenuItem value={String(PurchaseType.Vitamins)}>
                    {t('purchases.types.vitamins')}
                  </MenuItem>
                  <MenuItem value={String(PurchaseType.Bedding)}>
                    {t('purchases.types.bedding')}
                  </MenuItem>
                  <MenuItem value={String(PurchaseType.Toys)}>
                    {t('purchases.types.toys')}
                  </MenuItem>
                  <MenuItem value={String(PurchaseType.Veterinary)}>
                    {t('purchases.types.veterinary')}
                  </MenuItem>
                  <MenuItem value={String(PurchaseType.Other)}>
                    {t('purchases.types.other')}
                  </MenuItem>
                </Select>
              </FormControl>
              <FormControl fullWidth>
                <InputLabel id="flock-filter-label">
                  {t('purchases.filters.flock')}
                </InputLabel>
                <Select
                  labelId="flock-filter-label"
                  value={flockIdFilter}
                  onChange={handleFlockFilterChange}
                  label={t('purchases.filters.flock')}
                  disabled
                  aria-label={t('purchases.filters.flock')}
                >
                  <MenuItem value="">
                    {t('common.all')}
                  </MenuItem>
                </Select>
              </FormControl>
            </Stack>
          </Stack>
        </CardContent>
      </Card>

      {/* Purchase Cards */}
      <Stack spacing={2}>
        {purchases.map((purchase) => (
          <Card
            key={purchase.id}
            elevation={2}
            sx={{
              transition: 'box-shadow 0.3s ease',
              '&:hover': {
                boxShadow: 4,
              },
            }}
            role="article"
            aria-label={t('purchases.purchaseCardAriaLabel', { name: purchase.name })}
          >
            <CardContent>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'flex-start',
                  mb: 2,
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexGrow: 1 }}>
                  <Box
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      width: 40,
                      height: 40,
                      borderRadius: 1,
                      backgroundColor: 'primary.main',
                      color: 'primary.contrastText',
                    }}
                  >
                    {getTypeIcon(purchase.type)}
                  </Box>
                  <Box sx={{ flexGrow: 1 }}>
                    <Typography variant="h6" component="h3">
                      {purchase.name}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {formatDate(purchase.purchaseDate)}
                    </Typography>
                  </Box>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <IconButton
                    onClick={() => onEdit?.(purchase)}
                    aria-label={t('common.edit')}
                    sx={{
                      minWidth: 44,
                      minHeight: 44,
                    }}
                  >
                    <EditIcon />
                  </IconButton>
                  <IconButton
                    onClick={() => handleDeleteClick(purchase)}
                    aria-label={t('common.delete')}
                    color="error"
                    sx={{
                      minWidth: 44,
                      minHeight: 44,
                    }}
                  >
                    <DeleteIcon />
                  </IconButton>
                </Box>
              </Box>

              <Stack spacing={1}>
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    {t('purchases.list.type')}:
                  </Typography>
                  <Chip
                    label={getTypeLabel(purchase.type)}
                    size="small"
                    sx={{ height: 24 }}
                  />
                </Box>
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    {t('purchases.list.amount')}:
                  </Typography>
                  <Typography variant="h6" fontWeight="bold" color="primary">
                    <span>{purchase.amount.toFixed(2)}</span>{' '}
                    {t('purchases.currency')}
                  </Typography>
                </Box>
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    {t('purchases.list.quantity')}:
                  </Typography>
                  <Typography variant="body2" fontWeight="medium">
                    <span>{purchase.quantity}</span>{' '}
                    {getUnitLabel(purchase.unit)}
                  </Typography>
                </Box>
                {purchase.coopId && (
                  <Box
                    sx={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                    }}
                  >
                    <Typography variant="body2" color="text.secondary">
                      {t('purchases.list.flock')}:
                    </Typography>
                    <Typography variant="body2" fontWeight="medium">
                      {flockMap.get(purchase.coopId) || t('purchases.list.unknownFlock')}
                    </Typography>
                  </Box>
                )}
              </Stack>
            </CardContent>
          </Card>
        ))}
      </Stack>

      {/* Delete Confirmation Dialog */}
      <ConfirmationDialog
        open={deleteDialogOpen}
        onClose={handleDeleteCancel}
        onConfirm={handleDeleteConfirm}
        title={t('purchases.delete.title')}
        message={t('purchases.delete.message', { name: purchaseToDelete?.name })}
        secondaryMessage={t('purchases.delete.warning')}
        isPending={isDeleting}
        confirmText={t('common.delete')}
        cancelText={t('common.cancel')}
        confirmColor="error"
        confirmVariant="contained"
      />
    </Box>
  );
}
