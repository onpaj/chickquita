import { useState, useMemo } from 'react';
import {
  Box,
  Typography,
  Container,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Button,
  Chip,
  Card,
  CardContent,
} from '@mui/material';
import {
  Egg as EggIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
} from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { format, subDays } from 'date-fns';
import { useDailyRecords } from '../features/dailyRecords/hooks/useDailyRecords';
import { useCoops } from '../features/coops/hooks/useCoops';
import { DailyRecordCard } from '../features/dailyRecords/components/DailyRecordCard';
import { EditDailyRecordModal } from '../features/dailyRecords/components/EditDailyRecordModal';
import { IllustratedEmptyState, DailyRecordCardSkeleton } from '../shared/components';
import type { GetDailyRecordsParams, DailyRecordDto } from '../features/dailyRecords/api/dailyRecordsApi';

export function DailyRecordsListPage() {
  const { t } = useTranslation();
  const [filters, setFilters] = useState<GetDailyRecordsParams>({});
  const [editingRecord, setEditingRecord] = useState<DailyRecordDto | null>(null);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const { isLoading: isLoadingCoops } = useCoops();
  const { data: dailyRecords, isLoading: isLoadingRecords } = useDailyRecords(filters);

  // Create a map of flock IDs to display labels (flockName + coopName) for filter dropdown
  const flockMap = useMemo(() => {
    const map = new Map<string, string>();
    if (dailyRecords) {
      dailyRecords.forEach((record) => {
        if (!map.has(record.flockId)) {
          const label = record.flockCoopName
            ? `${record.flockName} (${record.flockCoopName})`
            : record.flockName || record.flockId;
          map.set(record.flockId, label);
        }
      });
    }
    return map;
  }, [dailyRecords]);

  // Get unique flock IDs from records for filter dropdown
  const availableFlockIds = useMemo(() => {
    if (!dailyRecords) return [];
    const flockIds = new Set(dailyRecords.map((r) => r.flockId));
    return Array.from(flockIds);
  }, [dailyRecords]);

  const handleFlockChange = (flockId: string) => {
    setFilters((prev) => ({
      ...prev,
      flockId: flockId || undefined,
    }));
  };

  const handleStartDateChange = (date: string) => {
    setFilters((prev) => ({
      ...prev,
      startDate: date || undefined,
    }));
  };

  const handleEndDateChange = (date: string) => {
    setFilters((prev) => ({
      ...prev,
      endDate: date || undefined,
    }));
  };

  const handleClearFilters = () => {
    setFilters({});
  };

  const handleEditRecord = (record: DailyRecordDto) => {
    setEditingRecord(record);
    setIsEditModalOpen(true);
  };

  const handleCloseEditModal = () => {
    setIsEditModalOpen(false);
    setEditingRecord(null);
  };

  const hasActiveFilters = filters.flockId || filters.startDate || filters.endDate;

  // Quick filter presets
  const handleQuickFilter = (preset: 'today' | 'week' | 'month') => {
    const today = format(new Date(), 'yyyy-MM-dd');
    switch (preset) {
      case 'today':
        setFilters({ startDate: today, endDate: today });
        break;
      case 'week':
        setFilters({
          startDate: format(subDays(new Date(), 7), 'yyyy-MM-dd'),
          endDate: today,
        });
        break;
      case 'month':
        setFilters({
          startDate: format(subDays(new Date(), 30), 'yyyy-MM-dd'),
          endDate: today,
        });
        break;
    }
  };

  const isLoading = isLoadingCoops || isLoadingRecords;

  return (
    <Container maxWidth="lg" sx={{ py: 3, pb: 10 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {t('dailyRecords.title')}
      </Typography>

      {/* Filters Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              mb: 2,
            }}
          >
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
                filters.startDate === format(new Date(), 'yyyy-MM-dd') &&
                filters.endDate === format(new Date(), 'yyyy-MM-dd')
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
              gridTemplateColumns: {
                xs: '1fr',
                sm: 'repeat(2, 1fr)',
                md: 'repeat(4, 1fr)',
              },
              gap: 2,
            }}
          >
            {/* Flock Filter */}
            <Box sx={{ gridColumn: { xs: '1', sm: '1 / 3', md: '1 / 3' } }}>
              <FormControl fullWidth>
                <InputLabel id="flock-filter-label">
                  {t('dailyRecords.flock')}
                </InputLabel>
                <Select
                  labelId="flock-filter-label"
                  id="flock-filter"
                  value={filters.flockId || ''}
                  label={t('dailyRecords.flock')}
                  onChange={(e) => handleFlockChange(e.target.value)}
                >
                  <MenuItem value="">
                    <em>{t('common.all')}</em>
                  </MenuItem>
                  {availableFlockIds.map((flockId) => (
                    <MenuItem key={flockId} value={flockId}>
                      {flockMap.get(flockId) || flockId}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            {/* Start Date Filter */}
            <TextField
              fullWidth
              type="date"
              label={t('dailyRecords.filters.startDate')}
              value={filters.startDate || ''}
              onChange={(e) => handleStartDateChange(e.target.value)}
              InputLabelProps={{
                shrink: true,
              }}
            />

            {/* End Date Filter */}
            <TextField
              fullWidth
              type="date"
              label={t('dailyRecords.filters.endDate')}
              value={filters.endDate || ''}
              onChange={(e) => handleEndDateChange(e.target.value)}
              InputLabelProps={{
                shrink: true,
              }}
            />
          </Box>
        </CardContent>
      </Card>

      {/* Records List */}
      {isLoading ? (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: '1fr',
              sm: 'repeat(2, 1fr)',
              md: 'repeat(3, 1fr)',
            },
            gap: 2,
          }}
        >
          {[1, 2, 3, 4, 5, 6].map((index) => (
            <DailyRecordCardSkeleton key={index} />
          ))}
        </Box>
      ) : !dailyRecords || dailyRecords.length === 0 ? (
        <IllustratedEmptyState
          illustration={<EggIcon sx={{ fontSize: 80, color: 'text.secondary' }} />}
          title={t('dailyRecords.emptyState.title')}
          description={
            hasActiveFilters
              ? t('dailyRecords.emptyState.noRecordsFiltered')
              : t('dailyRecords.emptyState.noRecords')
          }
        />
      ) : (
        <>
          {/* Results count */}
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            {t('dailyRecords.recordsCount', { count: dailyRecords.length })}
          </Typography>

          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: {
                xs: '1fr',
                sm: 'repeat(2, 1fr)',
                md: 'repeat(3, 1fr)',
              },
              gap: 2,
            }}
          >
            {dailyRecords.map((record) => (
              <DailyRecordCard
                key={record.id}
                record={record}
                flockIdentifier={flockMap.get(record.flockId)}
                onEdit={handleEditRecord}
              />
            ))}
          </Box>
        </>
      )}

      {/* Edit Modal */}
      <EditDailyRecordModal
        open={isEditModalOpen}
        onClose={handleCloseEditModal}
        record={editingRecord}
        flockIdentifier={editingRecord ? flockMap.get(editingRecord.flockId) : undefined}
      />
    </Container>
  );
}
