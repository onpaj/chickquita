import { Card, CardContent, Typography, Box, Chip } from '@mui/material';
import { Egg as EggIcon } from '@mui/icons-material';
import { format } from 'date-fns';
import { cs } from 'date-fns/locale';
import type { DailyRecordDto } from '../api/dailyRecordsApi';

interface DailyRecordCardProps {
  record: DailyRecordDto;
  flockIdentifier?: string;
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
export function DailyRecordCard({ record, flockIdentifier }: DailyRecordCardProps) {
  const formattedDate = format(new Date(record.recordDate), 'dd. MM. yyyy', {
    locale: cs,
  });

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
        {/* Header with date */}
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
              vajec
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
