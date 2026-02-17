import { useState } from 'react';
import {
  Container,
  Paper,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  Button,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import LogoutIcon from '@mui/icons-material/Logout';
import { useTranslation } from 'react-i18next';
import { useClerk } from '@clerk/clerk-react';
import { ConfirmationDialog } from '@/shared/components';

export function SettingsPage() {
  const { t, i18n } = useTranslation();
  const { signOut } = useClerk();
  const [signOutDialogOpen, setSignOutDialogOpen] = useState(false);
  const [isSigningOut, setIsSigningOut] = useState(false);

  const handleLanguageChange = (event: SelectChangeEvent) => {
    const newLanguage = event.target.value;
    i18n.changeLanguage(newLanguage);
  };

  const handleSignOutConfirm = async () => {
    setIsSigningOut(true);
    await signOut();
  };

  return (
    <Container maxWidth="md" sx={{ py: 3, pb: 10 }}>
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

      <Box sx={{ mt: 4 }}>
        <Button
          variant="outlined"
          color="error"
          size="large"
          fullWidth
          startIcon={<LogoutIcon />}
          onClick={() => setSignOutDialogOpen(true)}
          sx={{ minHeight: 48 }}
        >
          {t('settings.signOut')}
        </Button>
      </Box>

      <ConfirmationDialog
        open={signOutDialogOpen}
        onClose={() => setSignOutDialogOpen(false)}
        onConfirm={handleSignOutConfirm}
        title={t('settings.signOutConfirmTitle')}
        message={t('settings.signOutConfirmMessage')}
        confirmText={t('settings.signOutConfirmButton')}
        confirmColor="error"
        isPending={isSigningOut}
      />
    </Container>
  );
}
