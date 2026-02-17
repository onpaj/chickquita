import { useState } from 'react';
import {
  Container,
  Box,
  Typography,
  Paper,
  ToggleButtonGroup,
  ToggleButton,
  Alert,
  Skeleton,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
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
  const [dateRange, setDateRange] = useState<'7' | '30' | '90' | 'custom'>('30');
  const [customStartDate, setCustomStartDate] = useState<Dayjs | null>(null);
  const [customEndDate, setCustomEndDate] = useState<Dayjs | null>(null);

  // Calculate date range based on selection
  const getDateRange = () => {
    if (dateRange === 'custom' && customStartDate && customEndDate) {
      return {
        startDate: customStartDate.format('YYYY-MM-DD'),
        endDate: customEndDate.format('YYYY-MM-DD'),
      };
    }

    const endDate = dayjs();
    const startDate = endDate.subtract(parseInt(dateRange), 'day');

    return {
      startDate: startDate.format('YYYY-MM-DD'),
      endDate: endDate.format('YYYY-MM-DD'),
    };
  };

  const { startDate, endDate } = getDateRange();
  const { data: stats, isLoading, error } = useStatistics(startDate, endDate);

  const handleDateRangeChange = (_event: React.MouseEvent<HTMLElement>, newRange: string | null) => {
    if (newRange !== null) {
      setDateRange(newRange as '7' | '30' | '90' | 'custom');
    }
  };

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale={i18n.language}>
      <Container maxWidth="lg">
        <Box sx={{ py: 3, pb: 10 }}>
          {/* Page Header */}
          <Typography variant="h4" component="h1" gutterBottom fontWeight="bold" sx={{ mb: 3 }}>
            {t('statistics.title')}
          </Typography>

          {/* Date Range Filters */}
          <Paper sx={{ p: 2, mb: 3 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <CalendarTodayIcon color="action" />
                <Typography variant="subtitle1" fontWeight={600}>
                  {t('statistics.dateRange.title')}
                </Typography>
              </Box>

              <ToggleButtonGroup
                value={dateRange}
                exclusive
                onChange={handleDateRangeChange}
                aria-label={t('statistics.dateRange.ariaLabel')}
                fullWidth
                sx={{
                  '& .MuiToggleButton-root': {
                    py: 1,
                  },
                }}
              >
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
                    slotProps={{
                      textField: {
                        fullWidth: true,
                      },
                    }}
                  />
                  <DatePicker
                    label={t('statistics.dateRange.endDate')}
                    value={customEndDate}
                    onChange={(newValue) => setCustomEndDate(newValue)}
                    minDate={customStartDate || undefined}
                    maxDate={dayjs()}
                    slotProps={{
                      textField: {
                        fullWidth: true,
                      },
                    }}
                  />
                </Box>
              )}
            </Box>
          </Paper>

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
