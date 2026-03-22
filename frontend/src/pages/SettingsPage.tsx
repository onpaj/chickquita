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
  Avatar,
  Divider,
  FormControlLabel,
  Switch,
  FormHelperText,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import LanguageIcon from '@mui/icons-material/Language';
import CurrencyExchangeIcon from '@mui/icons-material/CurrencyExchange';
import LogoutIcon from '@mui/icons-material/Logout';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import { useTranslation } from 'react-i18next';
import { OrganizationProfile, useClerk, useOrganization, useUser } from '@clerk/clerk-react';
import { ConfirmationDialog } from '@/shared/components';
import { useUserSettings, useUpdateUserSettings } from '@/features/settings';
import { useCoops } from '@/features/coops/hooks/useCoops';

const appVersion = import.meta.env.VITE_APP_VERSION || 'dev';

export function SettingsPage() {
  const { t, i18n } = useTranslation();
  const { signOut } = useClerk();
  const { user } = useUser();
  const { organization } = useOrganization();
  const [signOutDialogOpen, setSignOutDialogOpen] = useState(false);
  const [isSigningOut, setIsSigningOut] = useState(false);

  const { data: settings } = useUserSettings();
  const { data: coops } = useCoops();
  const { updateSettings, isPending: isUpdatingSettings } = useUpdateUserSettings();

  const activeCoops = coops?.filter((c) => c.isActive) ?? [];
  const canToggleSingleCoopMode = activeCoops.length === 1;
  const singleCoopMode = settings?.singleCoopMode ?? true;
const revenueTrackingEnabled = settings?.revenueTrackingEnabled ?? true;
  const currency = settings?.currency ?? 'CZK';

  const handleLanguageChange = (event: SelectChangeEvent) => {
    i18n.changeLanguage(event.target.value);
  };

  const handleSingleCoopModeToggle = (event: React.ChangeEvent<HTMLInputElement>) => {
    // checked=true → enable multi-coop (singleCoopMode=false), always allowed
    // checked=false → revert to single-coop (singleCoopMode=true), requires exactly 1 coop
    if (!event.target.checked && !canToggleSingleCoopMode) return;
updateSettings({ singleCoopMode: !event.target.checked, revenueTrackingEnabled, currency });
  };

  const handleRevenueTrackingToggle = (event: React.ChangeEvent<HTMLInputElement>) => {
    updateSettings({ singleCoopMode, revenueTrackingEnabled: event.target.checked, currency });
  };

  const handleCurrencyChange = (event: SelectChangeEvent) => {
    updateSettings({ singleCoopMode, revenueTrackingEnabled, currency: event.target.value });
  };

  const handleSignOutConfirm = async () => {
    setIsSigningOut(true);
    await signOut();
  };

  return (
    <Container maxWidth="md" sx={{ py: 3 }}>
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

      {/* Currency section */}
      <Card elevation={1} sx={{ mt: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
            {t('settings.currency.label')}
          </Typography>
          <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 2 }}>
            {t('settings.currency.description')}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <CurrencyExchangeIcon color="action" />
            <FormControl fullWidth size="small">
              <InputLabel id="currency-select-label">
                {t('settings.currency.label')}
              </InputLabel>
              <Select
                labelId="currency-select-label"
                id="currency-select"
                value={currency}
                label={t('settings.currency.label')}
                onChange={handleCurrencyChange}
                disabled={isUpdatingSettings}
              >
                <MenuItem value="CZK">CZK – Česká koruna</MenuItem>
                <MenuItem value="EUR">EUR – Euro</MenuItem>
                <MenuItem value="USD">USD – US Dollar</MenuItem>
                <MenuItem value="GBP">GBP – British Pound</MenuItem>
                <MenuItem value="CHF">CHF – Swiss Franc</MenuItem>
                <MenuItem value="PLN">PLN – Polish Zloty</MenuItem>
                <MenuItem value="HUF">HUF – Hungarian Forint</MenuItem>
                <MenuItem value="RON">RON – Romanian Leu</MenuItem>
                <MenuItem value="BGN">BGN – Bulgarian Lev</MenuItem>
                <MenuItem value="HRK">HRK – Croatian Kuna</MenuItem>
                <MenuItem value="DKK">DKK – Danish Krone</MenuItem>
                <MenuItem value="SEK">SEK – Swedish Krona</MenuItem>
                <MenuItem value="NOK">NOK – Norwegian Krone</MenuItem>
                <MenuItem value="UAH">UAH – Ukrainian Hryvnia</MenuItem>
                <MenuItem value="RUB">RUB – Russian Ruble</MenuItem>
                <MenuItem value="TRY">TRY – Turkish Lira</MenuItem>
                <MenuItem value="CAD">CAD – Canadian Dollar</MenuItem>
                <MenuItem value="AUD">AUD – Australian Dollar</MenuItem>
                <MenuItem value="JPY">JPY – Japanese Yen</MenuItem>
                <MenuItem value="CNY">CNY – Chinese Yuan</MenuItem>
                <MenuItem value="INR">INR – Indian Rupee</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </CardContent>
      </Card>

      {/* Single-coop mode section */}
      <Card elevation={1} sx={{ mt: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
            {t('settings.singleCoopMode.label')}
          </Typography>
          <FormControlLabel
            control={
              <Switch
                checked={!singleCoopMode}
                onChange={handleSingleCoopModeToggle}
                disabled={(!singleCoopMode && !canToggleSingleCoopMode) || isUpdatingSettings}
              />
            }
            label={
              <Box>
                <Typography variant="body2">
                  {t('settings.singleCoopMode.label')}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {t('settings.singleCoopMode.description')}
                </Typography>
              </Box>
            }
          />
          {!singleCoopMode && !canToggleSingleCoopMode && (
            <FormHelperText sx={{ mt: 0.5, ml: 0 }}>
              {t('settings.singleCoopMode.onlyOneCoopHint')}
            </FormHelperText>
          )}
        </CardContent>
      </Card>

      {/* Revenue tracking section */}
      <Card elevation={1} sx={{ mt: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
            {t('settings.revenueTracking.label')}
          </Typography>
          <FormControlLabel
            control={
              <Switch
                checked={revenueTrackingEnabled}
                onChange={handleRevenueTrackingToggle}
                disabled={isUpdatingSettings}
              />
            }
            label={
              <Box>
                <Typography variant="body2">
                  {t('settings.revenueTracking.label')}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {t('settings.revenueTracking.description')}
                </Typography>
              </Box>
            }
          />
        </CardContent>
      </Card>

      {/* Members section - only show if user has an active organization */}
      {organization && (
        <Card elevation={1} sx={{ mt: 2 }}>
          <CardContent>
            <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2 }}>
              {t('settings.members')}
            </Typography>
            <OrganizationProfile />
          </CardContent>
        </Card>
      )}

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

          {/* User info row */}
          {user && (
            <>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, px: 2, pb: 2 }}>
                <Avatar
                  src={user.imageUrl}
                  alt={user.fullName ?? user.primaryEmailAddress?.emailAddress}
                  sx={{ width: 48, height: 48 }}
                >
                  {(user.fullName ?? user.primaryEmailAddress?.emailAddress ?? '?')[0].toUpperCase()}
                </Avatar>
                <Box>
                  {user.fullName && (
                    <Typography variant="body1" fontWeight={600}>
                      {user.fullName}
                    </Typography>
                  )}
                  {user.primaryEmailAddress && (
                    <Typography variant="body2" color="text.secondary">
                      {user.primaryEmailAddress.emailAddress}
                    </Typography>
                  )}
                </Box>
              </Box>
              <Divider />
            </>
          )}

          <ListItem
            component="button"
            aria-label={t('settings.signOut')}
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
              primary={t('settings.signOut')}
              primaryTypographyProps={{ variant: 'body1', color: 'error' }}
            />
          </ListItem>
        </CardContent>
      </Card>

      {/* About section */}
      <Card elevation={1} sx={{ mt: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2 }}>
            {t('settings.about')}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <InfoOutlinedIcon color="action" />
            <Typography variant="body2" color="text.secondary">
              {t('settings.version')}: <strong>{appVersion}</strong>
            </Typography>
          </Box>
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
