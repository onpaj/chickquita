import { useTranslation } from 'react-i18next';
import { Box, Card, CardContent, Typography, Skeleton } from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import PointOfSaleIcon from '@mui/icons-material/PointOfSale';

interface RevenuePnlWidgetProps {
  totalRevenue?: number | null;
  profitLoss?: number | null;
  loading?: boolean;
}

const czk = (value: number) =>
  new Intl.NumberFormat('cs-CZ', { style: 'currency', currency: 'CZK' }).format(value);

/**
 * Revenue and Profit/Loss widget
 * Shows all-time egg sale revenue and the resulting profit or loss against costs.
 */
export function RevenuePnlWidget({
  totalRevenue,
  profitLoss,
  loading = false,
}: RevenuePnlWidgetProps) {
  const { t } = useTranslation();

  const revenueText =
    totalRevenue != null
      ? czk(totalRevenue)
      : t('dashboard.widgets.revenuePnl.notAvailable');

  const hasPnl = profitLoss != null;
  const isProfit = hasPnl && profitLoss! >= 0;

  const pnlText = hasPnl
    ? `${isProfit ? '+' : ''}${czk(profitLoss!)}`
    : null;

  return (
    <Card
      elevation={2}
      sx={{
        height: '100%',
        transition: 'transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 4,
        },
      }}
    >
      <CardContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          {/* Title row */}
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            {loading ? (
              <Skeleton variant="text" width="60%" height={24} />
            ) : (
              <Typography variant="body2" color="text.secondary" fontWeight={500}>
                {t('dashboard.widgets.revenuePnl.title')}
              </Typography>
            )}
            {!loading && (
              <Box sx={{ color: 'primary.main', opacity: 0.7 }}>
                <PointOfSaleIcon />
              </Box>
            )}
          </Box>

          {/* Revenue value */}
          {loading ? (
            <Skeleton variant="text" width="80%" height={48} />
          ) : (
            <Typography variant="h3" fontWeight="bold" color="primary.main">
              {revenueText}
            </Typography>
          )}

          {/* P&L row */}
          {loading ? (
            <Skeleton variant="text" width="50%" height={20} />
          ) : pnlText ? (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              {isProfit ? (
                <TrendingUpIcon fontSize="small" sx={{ color: 'success.main' }} />
              ) : (
                <TrendingDownIcon fontSize="small" sx={{ color: 'error.main' }} />
              )}
              <Typography
                variant="caption"
                sx={{ color: isProfit ? 'success.main' : 'error.main', fontWeight: 600 }}
              >
                {pnlText}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {t('dashboard.widgets.revenuePnl.profitLossLabel')}
              </Typography>
            </Box>
          ) : (
            <Typography variant="caption" color="text.secondary">
              {t('dashboard.widgets.revenuePnl.noCosts')}
            </Typography>
          )}
        </Box>
      </CardContent>
    </Card>
  );
}
