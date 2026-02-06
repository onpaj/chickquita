import {
  Container,
  Paper,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import { useTranslation } from 'react-i18next';

export function SettingsPage() {
  const { t, i18n } = useTranslation();

  const handleLanguageChange = (event: SelectChangeEvent) => {
    const newLanguage = event.target.value;
    i18n.changeLanguage(newLanguage);
  };

  return (
    <Container maxWidth="md" sx={{ py: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {t('settings.title')}
      </Typography>

      <Paper sx={{ p: 3, mt: 3 }}>
        <Box>
          <FormControl fullWidth>
            <InputLabel id="language-select-label">
              {t('settings.language')}
            </InputLabel>
            <Select
              labelId="language-select-label"
              id="language-select"
              value={i18n.language}
              label={t('settings.language')}
              onChange={handleLanguageChange}
            >
              <MenuItem value="cs">Čeština</MenuItem>
              <MenuItem value="en">English</MenuItem>
            </Select>
          </FormControl>
        </Box>
      </Paper>
    </Container>
  );
}
