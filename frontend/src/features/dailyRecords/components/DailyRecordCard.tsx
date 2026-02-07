import { Card, CardContent, Typography, Box, Chip, IconButton } from '@mui/material';
import { Egg as EggIcon, Edit as EditIcon } from '@mui/icons-material';
import { format } from 'date-fns';
import { cs } from 'date-fns/locale';
import { useTranslation } from 'react-i18next';
import type { DailyRecordDto } from '../api/dailyRecordsApi';

interface DailyRecordCardProps {
  record: DailyRecordDto;
  flockIdentifier?: string;
  onEdit?: (record: DailyRecordDto) => void;
}

/**
 * DailyRecordCard Component
 *
 * Displays a single daily record with egg count, date, flock info, and optional notes.
 * Used in the daily records list page to show all records.
 *
 * @example
 * <DailyRecordCard
 *   record={dailyRecord}
 *   flockIdentifier="Hejno A"
 * />
 */
export function DailyRecordCard({ record, flockIdentifier, onEdit }: DailyRecordCardProps) {
  const { t } = useTranslation();
  const formattedDate = format(new Date(record.recordDate), 'dd. MM. yyyy', {
    locale: cs,
  });

  // Check if record can be edited (same-day restriction)
  const canEdit = (() => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const createdDate = new Date(record.createdAt);
    createdDate.setHours(0, 0, 0, 0);

    return createdDate.getTime() === today.getTime();
  })();

  return (
    <Card
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        position: 'relative',
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-2px)',
          boxShadow: 3,
        },
      }}
    >
      <CardContent sx={{ flexGrow: 1, pb: 2 }}>
        {/* Header with date and edit button */}
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
          {canEdit && onEdit && (
            <IconButton
              size="small"
              onClick={() => onEdit(record)}
              sx={{
                color: 'primary.main',
                '&:hover': {
                  bgcolor: 'primary.50',
                },
              }}
              aria-label="edit record"
            >
              <EditIcon />
            </IconButton>
          )}
        </Box>

        {/* Egg count - prominent display */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            mb: 2,
            p: 2,
            bgcolor: 'primary.50',
            borderRadius: 2,
          }}
        >
          <EggIcon sx={{ fontSize: 32, color: 'primary.main' }} />
          <Box>
            <Typography variant="h4" component="div" sx={{ fontWeight: 700 }}>
              {record.eggCount}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {t('dailyRecords.eggsLabel')}
            </Typography>
          </Box>
        </Box>

        {/* Flock identifier */}
        {flockIdentifier && (
          <Box sx={{ mb: 1.5 }}>
            <Chip
              label={flockIdentifier}
              size="small"
              color="secondary"
              sx={{
                fontWeight: 500,
              }}
            />
          </Box>
        )}

        {/* Notes */}
        {record.notes && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              mt: 1.5,
              fontStyle: 'italic',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
            }}
          >
            {record.notes}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}
