import { useState } from 'react';
import {
  Container,
  Box,
  Typography,
  Paper,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  ToggleButtonGroup,
  ToggleButton,
  Alert,
  Skeleton,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import FilterListIcon from '@mui/icons-material/FilterList';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';
import 'dayjs/locale/cs';
import 'dayjs/locale/en';
import EggIcon from '@mui/icons-material/Egg';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import { StatCard } from '@/shared/components';
import { EggCostBreakdownChart } from '@/features/statistics/components/EggCostBreakdownChart';
import { ProductionTrendChart } from '@/features/statistics/components/ProductionTrendChart';
import { CostPerEggTrendChart } from '@/features/statistics/components/CostPerEggTrendChart';
import { FlockProductivityChart } from '@/features/statistics/components/FlockProductivityChart';
import { useStatistics } from '@/features/statistics/hooks/useStatistics';
import { useCoops } from '@/features/coops/hooks/useCoops';
import { useFlocks } from '@/features/flocks/hooks/useFlocks';
import { useUserSettingsContext } from '@/features/settings';

/**
 * Statistics Page Component
 *
 * Displays comprehensive statistics and charts for egg production, costs, and flock productivity.
 *
 * Features:
 * - Date range filters (7/30/90 days, custom)
 * - Egg cost breakdown by purchase type (pie chart)
 * - Production trend over time (line chart)
 * - Cost per egg trend (line chart)
 * - Flock productivity comparison (bar chart)
 */
export default function StatisticsPage() {
  const { t, i18n } = useTranslation();
  const { singleCoopMode } = useUserSettingsContext();
  const [dateRange, setDateRange] = useState<'7' | '30' | '90' | 'all' | 'custom'>('all');
  const [customStartDate, setCustomStartDate] = useState<Dayjs | null>(null);
  const [customEndDate, setCustomEndDate] = useState<Dayjs | null>(null);
  const [selectedCoopId, setSelectedCoopId] = useState<string>('');
  const [selectedFlockId, setSelectedFlockId] = useState<string>('');

  // Calculate date range based on selection
  const getDateRange = () => {
    if (dateRange === 'custom' && customStartDate && customEndDate) {
      return {
        startDate: customStartDate.format('YYYY-MM-DD'),
        endDate: customEndDate.format('YYYY-MM-DD'),
      };
    }

    if (dateRange === 'all') {
      return { startDate: undefined, endDate: undefined };
    }

    const endDate = dayjs();
    const startDate = endDate.subtract(parseInt(dateRange), 'day');

    return {
      startDate: startDate.format('YYYY-MM-DD'),
      endDate: endDate.format('YYYY-MM-DD'),
    };
  };

  const { startDate, endDate } = getDateRange();
  const { data: coops } = useCoops();
  const effectiveCoopId = singleCoopMode ? (coops?.[0]?.id ?? '') : selectedCoopId;
  const { data: flocks } = useFlocks(effectiveCoopId, true);
  const { data: stats, isLoading, error } = useStatistics(
    startDate,
    endDate,
    effectiveCoopId || undefined,
    selectedFlockId || undefined,
  );

  const handleDateRangeChange = (_event: React.MouseEvent<HTMLElement>, newRange: string | null) => {
    if (newRange !== null) {
      setDateRange(newRange as '7' | '30' | '90' | 'all' | 'custom');
    }
  };

  const handleCoopChange = (coopId: string) => {
    setSelectedCoopId(coopId);
    setSelectedFlockId('');
  };

  const activeDateLabel = (() => {
    if (dateRange === '7') return t('statistics.dateRange.last7Days');
    if (dateRange === '30') return t('statistics.dateRange.last30Days');
    if (dateRange === '90') return t('statistics.dateRange.last90Days');
    if (dateRange === 'all') return t('statistics.dateRange.allTime');
    return t('statistics.dateRange.custom');
  })();

  const activeFlockLabel = (() => {
    if (selectedFlockId) {
      const flock = flocks?.find((f) => f.id === selectedFlockId);
      return flock?.identifier ?? t('statistics.filters.allFlocks');
    }
    if (effectiveCoopId) {
      const coop = coops?.find((c) => c.id === effectiveCoopId);
      return coop?.name ?? t('statistics.filters.allCoops');
    }
    return t('statistics.filters.allFlocks');
  })();

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale={i18n.language}>
      <Container maxWidth="lg" sx={{ py: 3 }}>
        <Box>
          {/* Page Header */}
          <Typography variant="h4" component="h1" gutterBottom fontWeight="bold" sx={{ mb: 3 }}>
            {t('statistics.title')}
          </Typography>

          {/* Collapsible Filters */}
          <Accordion
            defaultExpanded={false}
            disableGutters
            elevation={1}
            sx={{ mb: 3, '&:before': { display: 'none' }, borderRadius: 1 }}
          >
            <AccordionSummary
              expandIcon={<ExpandMoreIcon />}
              aria-controls="statistics-filters-content"
              id="statistics-filters-header"
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <FilterListIcon fontSize="small" color="action" />
                <Typography variant="body2">
                  {t('statistics.filters.summary', {
                    range: activeDateLabel,
                    flock: activeFlockLabel,
                  })}
                </Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails sx={{ pt: 0 }}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                {/* Date Range */}
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <CalendarTodayIcon color="action" fontSize="small" />
                    <Typography variant="subtitle2" fontWeight={600}>
                      {t('statistics.dateRange.title')}
                    </Typography>
                  </Box>

                  <ToggleButtonGroup
                    value={dateRange}
                    exclusive
                    onChange={handleDateRangeChange}
                    aria-label={t('statistics.dateRange.ariaLabel')}
                    fullWidth
                    sx={{ '& .MuiToggleButton-root': { py: 1 } }}
                  >
                    <ToggleButton value="all" aria-label={t('statistics.dateRange.allTime')}>
                      {t('statistics.dateRange.allTime')}
                    </ToggleButton>
                    <ToggleButton value="7" aria-label={t('statistics.dateRange.last7Days')}>
                      {t('statistics.dateRange.last7Days')}
                    </ToggleButton>
                    <ToggleButton value="30" aria-label={t('statistics.dateRange.last30Days')}>
                      {t('statistics.dateRange.last30Days')}
                    </ToggleButton>
                    <ToggleButton value="90" aria-label={t('statistics.dateRange.last90Days')}>
                      {t('statistics.dateRange.last90Days')}
                    </ToggleButton>
                    <ToggleButton value="custom" aria-label={t('statistics.dateRange.custom')}>
                      {t('statistics.dateRange.custom')}
                    </ToggleButton>
                  </ToggleButtonGroup>

                  {/* Custom Date Range Pickers */}
                  {dateRange === 'custom' && (
                    <Box
                      sx={{
                        display: 'flex',
                        flexDirection: { xs: 'column', sm: 'row' },
                        gap: 2,
                      }}
                    >
                      <DatePicker
                        label={t('statistics.dateRange.startDate')}
                        value={customStartDate}
                        onChange={(newValue) => setCustomStartDate(newValue)}
                        maxDate={customEndDate || dayjs()}
                        slotProps={{ textField: { fullWidth: true } }}
                      />
                      <DatePicker
                        label={t('statistics.dateRange.endDate')}
                        value={customEndDate}
                        onChange={(newValue) => setCustomEndDate(newValue)}
                        minDate={customStartDate || undefined}
                        maxDate={dayjs()}
                        slotProps={{ textField: { fullWidth: true } }}
                      />
                    </Box>
                  )}
                </Box>

                {/* Coop / Flock Filters */}
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <FilterListIcon color="action" fontSize="small" />
                    <Typography variant="subtitle2" fontWeight={600}>
                      {t('statistics.filters.title')}
                    </Typography>
                  </Box>
                  <Box
                    sx={{
                      display: 'flex',
                      flexDirection: { xs: 'column', sm: 'row' },
                      gap: 2,
                    }}
                  >
                    {!singleCoopMode && (
                      <FormControl fullWidth>
                        <InputLabel>{t('statistics.filters.coop')}</InputLabel>
                        <Select
                          value={selectedCoopId}
                          label={t('statistics.filters.coop')}
                          onChange={(e) => handleCoopChange(e.target.value)}
                        >
                          <MenuItem value="">{t('statistics.filters.allCoops')}</MenuItem>
                          {coops?.filter((c) => c.isActive).map((coop) => (
                            <MenuItem key={coop.id} value={coop.id}>
                              {coop.name}
                            </MenuItem>
                          ))}
                        </Select>
                      </FormControl>
                    )}

                    <FormControl fullWidth disabled={!effectiveCoopId}>
                      <InputLabel>{t('statistics.filters.flock')}</InputLabel>
                      <Select
                        value={selectedFlockId}
                        label={t('statistics.filters.flock')}
                        onChange={(e) => setSelectedFlockId(e.target.value)}
                      >
                        <MenuItem value="">{t('statistics.filters.allFlocks')}</MenuItem>
                        {flocks?.map((flock) => (
                          <MenuItem key={flock.id} value={flock.id}>
                            {flock.identifier}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Box>
                </Box>
              </Box>
            </AccordionDetails>
          </Accordion>

          {/* Summary StatCards */}
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', sm: 'repeat(3, 1fr)' },
              gap: 2,
              mb: 3,
            }}
          >
            <StatCard
              icon={<EggIcon />}
              label={t('statistics.summary.totalEggs')}
              value={stats?.summary.totalEggs ?? 0}
              loading={isLoading}
              color="primary"
            />
            <StatCard
              icon={<AttachMoneyIcon />}
              label={t('statistics.summary.totalCost')}
              value={stats ? `${stats.summary.totalCost.toFixed(2)} Kč` : '—'}
              loading={isLoading}
              color="warning"
            />
            <StatCard
              icon={<TrendingDownIcon />}
              label={t('statistics.summary.avgCostPerEgg')}
              value={stats ? `${stats.summary.avgCostPerEgg.toFixed(2)} Kč` : '—'}
              loading={isLoading}
              color="info"
            />
          </Box>

          {/* Error State */}
          {error && (
            <Alert severity="error" sx={{ mb: 3 }}>
              {t('statistics.error.loadFailed')}
            </Alert>
          )}

          {/* Loading State */}
          {isLoading && (
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', md: 'repeat(2, 1fr)' },
                gap: 3,
              }}
            >
              {[1, 2, 3, 4].map((i) => (
                <Skeleton key={i} variant="rectangular" height={300} />
              ))}
            </Box>
          )}

          {/* Charts Grid */}
          {!isLoading && stats && (
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', md: 'repeat(2, 1fr)' },
                gap: 3,
              }}
            >
              {/* Egg Cost Breakdown */}
              <Paper sx={{ p: 2 }}>
                <EggCostBreakdownChart data={stats.costBreakdown} />
              </Paper>

              {/* Production Trend */}
              <Paper sx={{ p: 2 }}>
                <ProductionTrendChart data={stats.productionTrend} />
              </Paper>

              {/* Cost Per Egg Trend */}
              <Paper sx={{ p: 2 }}>
                <CostPerEggTrendChart data={stats.costPerEggTrend} />
              </Paper>

              {/* Flock Productivity Comparison */}
              <Paper sx={{ p: 2 }}>
                <FlockProductivityChart data={stats.flockProductivity} />
              </Paper>
            </Box>
          )}

          {/* Empty State */}
          {!isLoading && !error && stats && stats.productionTrend.length === 0 && (
            <Alert severity="info" sx={{ mt: 3 }}>
              {t('statistics.emptyState.noData')}
            </Alert>
          )}
        </Box>
      </Container>
    </LocalizationProvider>
  );
}
