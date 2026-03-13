import { Snackbar, Alert, Button } from '@mui/material';
import SystemUpdateAltIcon from '@mui/icons-material/SystemUpdateAlt';
import { useTranslation } from 'react-i18next';
import { useRegisterSW } from 'virtual:pwa-register/react';

/**
 * Shows a Snackbar when a new version of the PWA is available.
 * The user can click "Reload" to activate the new service worker and refresh.
 */
export function PwaUpdatePrompt() {
  const { t } = useTranslation();
  const {
    needRefresh: [needRefresh],
    updateServiceWorker,
  } = useRegisterSW();

  const handleReload = () => {
    updateServiceWorker(true);
  };

  return (
    <Snackbar
      open={needRefresh}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      sx={{ bottom: { xs: 'calc(64px + env(safe-area-inset-bottom) + 16px)', sm: 24 } }}
    >
      <Alert
        severity="info"
        icon={<SystemUpdateAltIcon />}
        action={
          <Button
            color="inherit"
            size="small"
            onClick={handleReload}
            sx={{ fontWeight: 700, whiteSpace: 'nowrap' }}
          >
            {t('pwa.update.reload')}
          </Button>
        }
        sx={{ width: '100%' }}
      >
        {t('pwa.update.message')}
      </Alert>
    </Snackbar>
  );
}
