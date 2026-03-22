import { Card, CardContent, Typography, Box, Chip, IconButton } from '@mui/material';
import { ShoppingCart as SaleIcon, Edit as EditIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { formatDate } from '@/lib/dateFormat';
import type { EggSaleDto } from '../types/eggSale.types';

interface EggSaleCardProps {
  sale: EggSaleDto;
  onEdit?: (sale: EggSaleDto) => void;
}

export function EggSaleCard({ sale, onEdit }: EggSaleCardProps) {
  const { t } = useTranslation();
  const formattedDate = formatDate(sale.date);
  const total = sale.quantity * sale.pricePerUnit;

  return (
    <Card
      elevation={1}
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        position: 'relative',
        transition: 'box-shadow 0.2s',
        '&:hover': { boxShadow: 3 },
      }}
    >
      <CardContent sx={{ flexGrow: 1, pb: 2 }}>
        {/* Header: date + edit button */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            mb: 2,
          }}
        >
          <Typography variant="h6" component="h3" sx={{ fontWeight: 600 }}>
            {formattedDate}
          </Typography>
          {onEdit && (
            <IconButton
              size="small"
              onClick={() => onEdit(sale)}
              sx={{ color: 'primary.main', '&:hover': { bgcolor: 'action.hover' } }}
              aria-label={t('eggSales.editSale')}
            >
              <EditIcon />
            </IconButton>
          )}
        </Box>

        {/* Revenue summary — prominent */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            mb: 2,
            p: 2,
            bgcolor: 'action.hover',
            borderRadius: 2,
          }}
        >
          <SaleIcon sx={{ fontSize: 32, color: 'success.main' }} />
          <Box>
            <Typography variant="h5" component="div" sx={{ fontWeight: 700, color: 'success.main' }}>
              {total.toLocaleString('cs-CZ', { minimumFractionDigits: 0, maximumFractionDigits: 2 })} {t('eggSales.currency')}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {sale.quantity} ks × {sale.pricePerUnit} {t('eggSales.currency')}
            </Typography>
          </Box>
        </Box>

        {/* Buyer */}
        {sale.buyerName && (
          <Box sx={{ mb: 1.5 }}>
            <Chip
              label={sale.buyerName}
              size="small"
              color="secondary"
              sx={{ fontWeight: 500 }}
            />
          </Box>
        )}

        {/* Notes */}
        {sale.notes && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              mt: 1,
              fontStyle: 'italic',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
            }}
          >
            {sale.notes}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}
