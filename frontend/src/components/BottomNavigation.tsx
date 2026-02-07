import { BottomNavigation as MuiBottomNavigation, BottomNavigationAction, Paper } from '@mui/material';
import {
  Dashboard as DashboardIcon,
  HomeWork as CoopsIcon,
  Assignment as DailyRecordsIcon,
  Settings as SettingsIcon
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

export function BottomNavigation() {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const getCurrentTab = () => {
    if (location.pathname.startsWith('/coops')) return 'coops';
    if (location.pathname.startsWith('/daily-records')) return 'daily-records';
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
      case 'daily-records':
        // Placeholder for M4 - Daily Records not yet implemented
        // navigate('/daily-records');
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
      <MuiBottomNavigation
        value={getCurrentTab()}
        onChange={handleChange}
        showLabels
        sx={{
          height: 64, // Ensures minimum 48px touch target with padding
          '& .MuiBottomNavigationAction-root': {
            minHeight: 56, // Touch-friendly minimum height
            paddingTop: 1,
            paddingBottom: 1,
            transition: 'color 0.2s ease-in-out, transform 0.2s ease-in-out',
            '&:active': {
              transform: 'scale(0.95)',
            },
            '&.Mui-selected': {
              paddingTop: 1,
            },
          },
        }}
      >
        <BottomNavigationAction
          label={t('navigation.dashboard')}
          value="dashboard"
          icon={<DashboardIcon />}
        />
        <BottomNavigationAction
          label={t('navigation.coops')}
          value="coops"
          icon={<CoopsIcon />}
        />
        <BottomNavigationAction
          label={t('navigation.dailyRecords')}
          value="daily-records"
          icon={<DailyRecordsIcon />}
          disabled
          sx={{
            opacity: 0.4,
            '&.Mui-disabled': {
              opacity: 0.4,
            },
          }}
        />
        <BottomNavigationAction
          label={t('navigation.settings')}
          value="settings"
          icon={<SettingsIcon />}
        />
      </MuiBottomNavigation>
    </Paper>
  );
}
