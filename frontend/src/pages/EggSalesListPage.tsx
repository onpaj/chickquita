import { useState } from 'react';
import {
  Box,
  Typography,
  Container,
  TextField,
  Button,
  Chip,
  Card,
  CardContent,
  Fab,
  Tooltip,
} from '@mui/material';
import {
  ShoppingCart as SaleIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
  Add as AddIcon,
} from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { format, subDays } from 'date-fns';
import { useEggSales } from '../features/eggSales/hooks/useEggSales';
import { EggSaleCard } from '../features/eggSales/components/EggSaleCard';
import { EditEggSaleModal } from '../features/eggSales/components/EditEggSaleModal';
import { QuickAddModal } from '../features/dailyRecords/components/QuickAddModal';
import { IllustratedEmptyState, DailyRecordCardSkeleton } from '../shared/components';
import { useAllFlocks } from '../features/flocks/hooks/useAllFlocks';
import type { EggSaleDto, EggSaleFilterParams } from '../features/eggSales/types/eggSale.types';

export function EggSalesListPage() {
  const { t } = useTranslation();

  const [filters, setFilters] = useState<EggSaleFilterParams>({});
  const [editingSale, setEditingSale] = useState<EggSaleDto | null>(null);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isQuickAddOpen, setIsQuickAddOpen] = useState(false);

  const { eggSales, isLoading } = useEggSales(filters);
  const { data: flocks = [] } = useAllFlocks();

  const hasActiveFilters = !!(filters.fromDate || filters.toDate);

  const handleClearFilters = () => setFilters({});

  const handleQuickFilter = (preset: 'today' | 'week' | 'month') => {
    const today = format(new Date(), 'yyyy-MM-dd');
    switch (preset) {
      case 'today':
        setFilters({ fromDate: today, toDate: today });
        break;
      case 'week':
        setFilters({ fromDate: format(subDays(new Date(), 7), 'yyyy-MM-dd'), toDate: today });
        break;
      case 'month':
        setFilters({ fromDate: format(subDays(new Date(), 30), 'yyyy-MM-dd'), toDate: today });
        break;
    }
  };

  const handleEditSale = (sale: EggSaleDto) => {
    setEditingSale(sale);
    setIsEditModalOpen(true);
  };

  const handleCloseEditModal = () => {
    setIsEditModalOpen(false);
    setEditingSale(null);
  };

  const totalRevenue = eggSales.reduce(
    (sum, sale) => sum + sale.quantity * sale.pricePerUnit,
    0,
  );

  return (
    <Container maxWidth="lg" sx={{ py: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {t('eggSales.title')}
      </Typography>

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
            <FilterIcon color="primary" />
            <Typography variant="h6">{t('common.filter')}</Typography>
            {hasActiveFilters && (
              <Button
                size="small"
                startIcon={<ClearIcon />}
                onClick={handleClearFilters}
                sx={{ ml: 'auto' }}
              >
                {t('dailyRecords.clearFilters')}
              </Button>
            )}
          </Box>

          {/* Quick filter chips */}
          <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
            <Chip
              label={t('dailyRecords.filters.today')}
              onClick={() => handleQuickFilter('today')}
              color={
                filters.fromDate === format(new Date(), 'yyyy-MM-dd') &&
                filters.toDate === format(new Date(), 'yyyy-MM-dd')
                  ? 'primary'
                  : 'default'
              }
            />
            <Chip
              label={t('dailyRecords.filters.lastWeek')}
              onClick={() => handleQuickFilter('week')}
              color="default"
            />
            <Chip
              label={t('dailyRecords.filters.lastMonth')}
              onClick={() => handleQuickFilter('month')}
              color="default"
            />
          </Box>

          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)' },
              gap: 2,
            }}
          >
            <TextField
              fullWidth
              type="date"
              label={t('eggSales.filters.fromDate')}
              value={filters.fromDate || ''}
              onChange={(e) =>
                setFilters((prev) => ({ ...prev, fromDate: e.target.value || undefined }))
              }
              InputLabelProps={{ shrink: true }}
            />
            <TextField
              fullWidth
              type="date"
              label={t('eggSales.filters.toDate')}
              value={filters.toDate || ''}
              onChange={(e) =>
                setFilters((prev) => ({ ...prev, toDate: e.target.value || undefined }))
              }
              InputLabelProps={{ shrink: true }}
            />
          </Box>
        </CardContent>
      </Card>

      {/* Content */}
      {isLoading ? (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(3, 1fr)' },
            gap: 2,
          }}
        >
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <DailyRecordCardSkeleton key={i} />
          ))}
        </Box>
      ) : eggSales.length === 0 ? (
        <IllustratedEmptyState
          illustration={<SaleIcon sx={{ fontSize: 80, color: 'text.secondary' }} />}
          title={t('eggSales.emptyState.title')}
          description={
            hasActiveFilters
              ? t('eggSales.emptyState.description')
              : t('eggSales.emptyState.description')
          }
        />
      ) : (
        <>
          {/* Summary */}
          <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            <Typography variant="body2" color="text.secondary">
              {eggSales.length} {eggSales.length === 1 ? t('eggSales.list.date') : t('eggSales.title').toLowerCase()}
            </Typography>
            <Typography variant="body1" sx={{ fontWeight: 600, color: 'success.main' }}>
              {t('eggSales.totalRevenue')}:{' '}
              {totalRevenue.toLocaleString('cs-CZ', { minimumFractionDigits: 0, maximumFractionDigits: 2 })}{' '}
              {t('eggSales.currency')}
            </Typography>
          </Box>

          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(3, 1fr)' },
              gap: 2,
            }}
          >
            {eggSales.map((sale) => (
              <EggSaleCard key={sale.id} sale={sale} onEdit={handleEditSale} />
            ))}
          </Box>
        </>
      )}

      {/* FAB */}
      <Tooltip title={t('eggSales.addSale')} placement="left">
        <Fab
          color="primary"
          aria-label={t('eggSales.addSale')}
          onClick={() => setIsQuickAddOpen(true)}
          sx={{
            position: 'fixed',
            bottom: { xs: 'calc(env(safe-area-inset-bottom) + 80px)', sm: 24 },
            right: 16,
          }}
        >
          <AddIcon />
        </Fab>
      </Tooltip>

      <QuickAddModal
        open={isQuickAddOpen}
        onClose={() => setIsQuickAddOpen(false)}
        flocks={flocks}
      />

      <EditEggSaleModal
        open={isEditModalOpen}
        onClose={handleCloseEditModal}
        sale={editingSale}
      />
    </Container>
  );
}
