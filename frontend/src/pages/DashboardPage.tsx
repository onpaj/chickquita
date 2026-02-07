import { Container, Box, Typography, Alert, Fab, Tooltip } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import AddIcon from '@mui/icons-material/Add';
import EggIcon from '@mui/icons-material/Egg';
import HomeIcon from '@mui/icons-material/Home';
import PetsIcon from '@mui/icons-material/Pets';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import { useDashboardStats } from '@/features/dashboard/hooks/useDashboardStats';
import { DashboardEmptyState } from '@/features/dashboard/components/DashboardEmptyState';
import { TodaySummaryWidget } from '@/features/dashboard/components/TodaySummaryWidget';
import { WeeklyProductionWidget } from '@/features/dashboard/components/WeeklyProductionWidget';
import { FlockStatusWidget } from '@/features/dashboard/components/FlockStatusWidget';
import { EggCostWidget } from '@/features/dashboard/components/EggCostWidget';
import { QuickActionCard } from '@/features/dashboard/components/QuickActionCard';

/**
 * Dashboard Page Component
 *
 * Main landing page after authentication.
 * Displays farm statistics and quick action cards for common tasks.
 *
 * Layout:
 * - Statistics widgets at top (responsive grid)
 * - Quick action cards below
 * - Empty state when no data available
 */
export default function DashboardPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { data: stats, isLoading, error } = useDashboardStats();

  // Check if user has any data
  const hasData = stats && stats.flockStats.activeFlocks > 0;

  // Quick actions configuration
  const quickActions = [
    {
      title: t('dashboard.quickActions.addDailyRecord'),
      description: t('dashboard.quickActions.addDailyRecordDesc'),
      icon: <EggIcon />,
      onClick: () => {
        // TODO: Implement when M4 (Daily Records) is ready
        console.log('Add Daily Record - Coming in M4');
      },
      disabled: true, // Will be enabled in M4
    },
    {
      title: t('dashboard.quickActions.manageCoops'),
      description: t('dashboard.quickActions.manageCoopsDesc'),
      icon: <HomeIcon />,
      onClick: () => navigate('/coops'),
      disabled: false,
    },
    {
      title: t('dashboard.quickActions.manageFlocks'),
      description: t('dashboard.quickActions.manageFlocksDesc'),
      icon: <PetsIcon />,
      onClick: () => navigate('/coops'),
      disabled: false,
    },
    {
      title: t('dashboard.quickActions.trackPurchases'),
      description: t('dashboard.quickActions.trackPurchasesDesc'),
      icon: <ShoppingCartIcon />,
      onClick: () => {
        // TODO: Implement when purchases feature is ready
        console.log('Track Purchases - Coming soon');
      },
      disabled: true, // Will be enabled when purchases are implemented
    },
  ];

  const handleAddDailyRecord = () => {
    // TODO: Implement when M4 (Daily Records) is ready
    console.log('Add Daily Record FAB clicked - Coming in M4');
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 3, pb: 10 }}>
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

        {/* Show dashboard content if has data or is loading */}
        {(isLoading || hasData) && (
          <>
            {/* Statistics Widgets Section */}
            <Box sx={{ mb: 4 }}>
              <Typography variant="h6" gutterBottom fontWeight={600} sx={{ mb: 2 }}>
                {t('dashboard.title')}
              </Typography>

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
                {/* Today's Summary */}
                <TodaySummaryWidget
                  eggsToday={stats?.productionStats?.eggsToday}
                  loading={isLoading}
                />

                {/* Weekly Production */}
                <WeeklyProductionWidget
                  eggsThisWeek={stats?.productionStats?.eggsThisWeek}
                  loading={isLoading}
                />

                {/* Egg Cost Calculation */}
                <EggCostWidget
                  costPerEgg={stats?.costStats?.costPerEgg}
                  loading={isLoading}
                />

                {/* Flock Status */}
                <FlockStatusWidget
                  totalHens={stats?.flockStats.totalHens ?? 0}
                  totalRoosters={stats?.flockStats.totalRoosters ?? 0}
                  totalChicks={stats?.flockStats.totalChicks ?? 0}
                  activeFlocks={stats?.flockStats.activeFlocks ?? 0}
                  loading={isLoading}
                />
              </Box>
            </Box>

            {/* Quick Actions Section */}
            <Box>
              <Typography variant="h6" gutterBottom fontWeight={600} sx={{ mb: 2 }}>
                {t('dashboard.quickActions.title')}
              </Typography>

              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: {
                    xs: '1fr',
                    sm: 'repeat(2, 1fr)',
                    lg: 'repeat(4, 1fr)',
                  },
                  gap: 2,
                }}
              >
                {quickActions.map((action) => (
                  <QuickActionCard
                    key={action.title}
                    title={action.title}
                    description={action.description}
                    icon={action.icon}
                    onClick={action.onClick}
                    disabled={action.disabled}
                  />
                ))}
              </Box>
            </Box>
          </>
        )}

        {/* Floating Action Button - Add Daily Record (M4) */}
        {/* Only show if user has data (flocks exist) */}
        {hasData && (
          <Tooltip title={t('dashboard.quickActions.addDailyRecord')} placement="left">
            <Fab
              color="primary"
              aria-label={t('dashboard.quickActions.addDailyRecordAriaLabel')}
              onClick={handleAddDailyRecord}
              disabled={true} // Will be enabled in M4
              sx={{
                position: 'fixed',
                bottom: 88, // Above bottom navigation (56px height + 32px spacing)
                right: 16,
                opacity: 0.6, // Indicate disabled state
              }}
            >
              <AddIcon />
            </Fab>
          </Tooltip>
        )}
      </Box>
    </Container>
  );
}
