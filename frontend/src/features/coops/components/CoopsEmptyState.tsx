import { Box, Typography, Button } from '@mui/material';
import HouseIcon from '@mui/icons-material/House';
import AddIcon from '@mui/icons-material/Add';
import { useTranslation } from 'react-i18next';

interface CoopsEmptyStateProps {
  onAddClick: () => void;
}

export function CoopsEmptyState({ onAddClick }: CoopsEmptyStateProps) {
  const { t } = useTranslation();

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '60vh',
        textAlign: 'center',
        py: 4,
        px: 2,
      }}
    >
      <Box
        sx={{
          width: 120,
          height: 120,
          borderRadius: '50%',
          backgroundColor: 'primary.light',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          mb: 3,
          opacity: 0.9,
        }}
      >
        <HouseIcon
          sx={{
            fontSize: 64,
            color: 'primary.main',
          }}
        />
      </Box>

      <Typography variant="h6" color="text.primary" gutterBottom sx={{ fontWeight: 500 }}>
        {t('coops.emptyState.title')}
      </Typography>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 4, maxWidth: 400 }}>
        {t('coops.emptyState.message')}
      </Typography>

      <Button
        variant="contained"
        color="primary"
        size="large"
        startIcon={<AddIcon />}
        onClick={onAddClick}
        sx={{
          minWidth: 200,
          py: 1.5,
          fontSize: '1rem',
          fontWeight: 600,
          boxShadow: 2,
          '&:hover': {
            boxShadow: 4,
          },
        }}
      >
        {t('coops.addCoop')}
      </Button>
    </Box>
  );
}
