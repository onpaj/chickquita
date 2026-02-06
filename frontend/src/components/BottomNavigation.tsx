import { BottomNavigation as MuiBottomNavigation, BottomNavigationAction, Paper } from '@mui/material';
import { Home as HomeIcon, Egg as EggIcon, Settings as SettingsIcon } from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

export function BottomNavigation() {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const getCurrentTab = () => {
    if (location.pathname.startsWith('/coops')) return 'coops';
    if (location.pathname.startsWith('/settings')) return 'settings';
    return 'dashboard';
  };

  const handleChange = (_event: React.SyntheticEvent, newValue: string) => {
    switch (newValue) {
      case 'dashboard':
        navigate('/dashboard');
        break;
      case 'coops':
        navigate('/coops');
        break;
      case 'settings':
        navigate('/settings');
        break;
    }
  };

  return (
    <Paper
      sx={{
        position: 'fixed',
        bottom: 0,
        left: 0,
        right: 0,
        zIndex: 1000,
      }}
      elevation={3}
    >
      <MuiBottomNavigation value={getCurrentTab()} onChange={handleChange}>
        <BottomNavigationAction
          label={t('dashboard.title')}
          value="dashboard"
          icon={<HomeIcon />}
        />
        <BottomNavigationAction
          label={t('coops.title')}
          value="coops"
          icon={<EggIcon />}
        />
        <BottomNavigationAction
          label={t('settings.title')}
          value="settings"
          icon={<SettingsIcon />}
        />
      </MuiBottomNavigation>
    </Paper>
  );
}
