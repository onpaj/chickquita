import { BottomNavigation as MuiBottomNavigation, BottomNavigationAction, Paper } from '@mui/material';
import {
  Dashboard as DashboardIcon,
  HomeWork as CoopsIcon,
  Assignment as RecordsIcon,
  ShoppingCart as PurchasesIcon,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useUserSettingsContext } from '@/features/settings';
import { useCoops, useEnsureDefaultCoop } from '@/features/coops/hooks/useCoops';

export function BottomNavigation() {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();
  const { singleCoopMode } = useUserSettingsContext();
  const { data: coops } = useCoops();
  const { mutate: ensureDefaultCoop } = useEnsureDefaultCoop();

  const getCurrentTab = () => {
    if (location.pathname.startsWith('/coops')) return 'coops';
    if (location.pathname.startsWith('/records')) return 'records';
    if (location.pathname.startsWith('/daily-records')) return 'records';
    if (location.pathname.startsWith('/statistics')) return 'records';
    if (location.pathname.startsWith('/purchases')) return 'purchases';
    return 'dashboard';
  };

  const handleChange = (_event: React.SyntheticEvent, newValue: string) => {
    switch (newValue) {
      case 'dashboard':
        navigate('/dashboard');
        break;
      case 'coops':
        if (singleCoopMode && coops?.[0]) {
          navigate(`/coops/${coops[0].id}/flocks`);
        } else if (singleCoopMode) {
          // No coop yet — ensure default exists then go straight to add flock
          ensureDefaultCoop(undefined, {
            onSuccess: (coop) => navigate(`/coops/${coop.id}/flocks?addFlock=true`),
          });
        } else {
          navigate('/coops');
        }
        break;
      case 'records':
        navigate('/records/stats');
        break;
      case 'purchases':
        navigate('/purchases');
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
        paddingBottom: 'env(safe-area-inset-bottom)',
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
          label={singleCoopMode ? t('navigation.flocks') : t('navigation.coops')}
          value="coops"
          icon={<CoopsIcon />}
        />
        <BottomNavigationAction
          label={t('navigation.records')}
          value="records"
          icon={<RecordsIcon />}
        />
        <BottomNavigationAction
          label={t('navigation.purchases')}
          value="purchases"
          icon={<PurchasesIcon />}
        />
      </MuiBottomNavigation>
    </Paper>
  );
}
