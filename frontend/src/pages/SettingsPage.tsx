import { useState } from 'react';
import {
  Container,
  Card,
  CardContent,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  ListItem,
  ListItemIcon,
  ListItemText,
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
    i18n.changeLanguage(event.target.value);
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

      {/* Language section */}
      <Card elevation={1} sx={{ mt: 3 }}>
        <CardContent>
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2 }}>
            {t('settings.language')}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <LanguageIcon color="action" />
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
          </Box>
        </CardContent>
      </Card>

      {/* Profile / sign-out section */}
      <Card elevation={1} sx={{ mt: 2 }}>
        <CardContent sx={{ p: 0, '&:last-child': { pb: 0 } }}>
          <Typography
            variant="subtitle2"
            color="text.secondary"
            sx={{ px: 2, pt: 2, pb: 1 }}
          >
            {t('settings.profile')}
          </Typography>
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
        </CardContent>
      </Card>

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
