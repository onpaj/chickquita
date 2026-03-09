import { useState } from 'react';
import { Container, Box, Typography, Alert, Fab, Tooltip } from '@mui/material';
import { useTranslation } from 'react-i18next';
import AddIcon from '@mui/icons-material/Add';
import { useDashboardStats } from '@/features/dashboard/hooks/useDashboardStats';
import { DashboardEmptyState } from '@/features/dashboard/components/DashboardEmptyState';
import { TodaySummaryWidget } from '@/features/dashboard/components/TodaySummaryWidget';
import { WeeklyProductionWidget } from '@/features/dashboard/components/WeeklyProductionWidget';
import { FlockStatusWidget } from '@/features/dashboard/components/FlockStatusWidget';
import { EggCostWidget } from '@/features/dashboard/components/EggCostWidget';
import { QuickAddModal } from '@/features/dailyRecords/components/QuickAddModal';
import { useAllFlocks } from '@/features/flocks/hooks/useAllFlocks';

/**
 * Dashboard Page Component
 *
 * Main landing page after authentication.
 * Displays farm statistics.
 *
 * Layout:
 * - Statistics widgets at top (responsive grid)
 * - FAB for quick add daily record
 * - Empty state when no data available
 */
export default function DashboardPage() {
  const { t } = useTranslation();
  const { data: stats, isLoading, error } = useDashboardStats();
  const { data: flocks = [] } = useAllFlocks();
  const [isQuickAddOpen, setIsQuickAddOpen] = useState(false);

  // Check if user has any data
  const hasData = stats && stats.activeFlocks > 0;

  const handleAddDailyRecord = () => {
    setIsQuickAddOpen(true);
  };

  const handleCloseQuickAdd = () => {
    setIsQuickAddOpen(false);
  };

  return (
    <Container maxWidth="lg" sx={{ py: 3 }}>
      <Box>
        {/* Page Header */}
        <Typography variant="h4" component="h1" gutterBottom fontWeight="bold" sx={{ mb: 3 }}>
          {t('dashboard.title')}
        </Typography>

        {/* Error state */}
        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error.message}
          </Alert>
        )}

        {/* Show empty state if no data and not loading */}
        {!isLoading && !hasData && <DashboardEmptyState />}

        {/* Show stat widgets when has data or is loading */}
        {(isLoading || hasData) && (
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
            <TodaySummaryWidget
              eggsToday={stats?.todayEggs}
              loading={isLoading}
            />

            <WeeklyProductionWidget
              eggsThisWeek={stats?.thisWeekEggs}
              loading={isLoading}
            />

            <EggCostWidget
              costPerEgg={stats?.costPerEgg ?? undefined}
              loading={isLoading}
            />

            <FlockStatusWidget
              totalHens={stats?.totalHens ?? 0}
              totalRoosters={stats?.totalRoosters ?? 0}
              totalChicks={stats?.totalChicks ?? 0}
              activeFlocks={stats?.activeFlocks ?? 0}
              loading={isLoading}
            />
          </Box>
        )}

        {/* Floating Action Button - Add Daily Record */}
        {hasData && (
          <Tooltip title={t('dashboard.quickActions.addDailyRecord')} placement="left">
            <span>
              <Fab
                color="primary"
                aria-label={t('dashboard.quickActions.addDailyRecordAriaLabel')}
                onClick={handleAddDailyRecord}
                disabled={flocks.length === 0}
                sx={{
                  position: 'fixed',
                  bottom: { xs: 'calc(env(safe-area-inset-bottom) + 80px)', sm: 24 },
                  right: 16,
                }}
              >
                <AddIcon />
              </Fab>
            </span>
          </Tooltip>
        )}

        {/* Quick Add Modal */}
        <QuickAddModal
          open={isQuickAddOpen}
          onClose={handleCloseQuickAdd}
          flocks={flocks}
        />
      </Box>
    </Container>
  );
}
