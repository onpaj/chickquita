import { useState } from 'react';
import {
  Container,
  Paper,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import LanguageIcon from '@mui/icons-material/Language';
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

      {/* Language & Display section */}
      <Paper elevation={2} sx={{ mt: 3 }}>
        <Typography
          variant="overline"
          color="text.secondary"
          sx={{ display: 'block', px: 2, pt: 2, pb: 0.5 }}
        >
          {t('settings.language')}
        </Typography>
        <List disablePadding>
          <ListItem sx={{ py: 1.5 }}>
            <ListItemIcon sx={{ minWidth: 44 }}>
              <LanguageIcon color="action" />
            </ListItemIcon>
            <ListItemText
              primary={
                <FormControl fullWidth size="small">
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
              }
            />
          </ListItem>
        </List>
      </Paper>

      {/* Account / danger zone section */}
      <Paper elevation={2} sx={{ mt: 3 }}>
        <Typography
          variant="overline"
          color="text.secondary"
          sx={{ display: 'block', px: 2, pt: 2, pb: 0.5 }}
        >
          {t('settings.profile')}
        </Typography>
        <Divider />
        <List disablePadding>
          <ListItem
            component="button"
            onClick={() => setSignOutDialogOpen(true)}
            sx={{
              py: 1.5,
              minHeight: 56,
              width: '100%',
              border: 'none',
              bgcolor: 'transparent',
              cursor: 'pointer',
              textAlign: 'left',
              color: 'error.main',
              '&:hover': {
                bgcolor: 'error.light',
                opacity: 0.9,
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 44, color: 'error.main' }}>
              <LogoutIcon />
            </ListItemIcon>
            <ListItemText
              primary={
                <Typography variant="body1" color="error">
                  {t('settings.signOut')}
                </Typography>
              }
            />
          </ListItem>
        </List>
      </Paper>

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
