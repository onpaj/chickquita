import { useState } from 'react';
import {
  Box,
  Typography,
  Fab,
  Card,
  CardContent,
  CircularProgress,
  Alert,
  Container,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { useTranslation } from 'react-i18next';
import { useCoops } from '../features/coops/hooks/useCoops';
import { CreateCoopModal } from '../features/coops/components/CreateCoopModal';

export default function CoopsPage() {
  const { t } = useTranslation();
  const { data: coops, isLoading, error } = useCoops();
  const [isModalOpen, setIsModalOpen] = useState(false);

  if (isLoading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '50vh',
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Container sx={{ mt: 2 }}>
        <Alert severity="error">{t('errors.generic')}</Alert>
      </Container>
    );
  }

  return (
    <Container sx={{ pb: 10 }}>
      <Box sx={{ py: 3 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          {t('coops.title')}
        </Typography>

        {coops && coops.length === 0 ? (
          <Box sx={{ textAlign: 'center', mt: 8 }}>
            <Typography variant="body1" color="text.secondary" gutterBottom>
              {t('coops.noCoops')}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {t('coops.addFirstCoop')}
            </Typography>
          </Box>
        ) : (
          <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
            {coops?.map((coop) => (
              <Card key={coop.id} elevation={2}>
                <CardContent>
                  <Typography variant="h6" component="h2">
                    {coop.name}
                  </Typography>
                  {coop.location && (
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                      {coop.location}
                    </Typography>
                  )}
                  <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                    {new Date(coop.createdAt).toLocaleDateString()}
                  </Typography>
                </CardContent>
              </Card>
            ))}
          </Box>
        )}
      </Box>

      <Fab
        color="primary"
        aria-label={t('coops.addCoop')}
        sx={{
          position: 'fixed',
          bottom: 80,
          right: 16,
        }}
        onClick={() => setIsModalOpen(true)}
      >
        <AddIcon />
      </Fab>

      <CreateCoopModal open={isModalOpen} onClose={() => setIsModalOpen(false)} />
    </Container>
  );
}
