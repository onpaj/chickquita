import { useState } from 'react';
import { Alert, Button, IconButton, Box } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import GetAppIcon from '@mui/icons-material/GetApp';
import { useTranslation } from 'react-i18next';
import { usePWAInstall } from '@/hooks/usePWAInstall';

export function InstallBanner() {
  const { t } = useTranslation();
  const { isInstallable, isInstalled, promptInstall } = usePWAInstall();
  const [dismissed, setDismissed] = useState(false);
  const [showBanner, setShowBanner] = useState(() => {
    const visitCount = parseInt(localStorage.getItem('pwa_visit_count') || '0');
    const firstVisit = localStorage.getItem('pwa_first_visit');

    if (!firstVisit) {
      localStorage.setItem('pwa_first_visit', Date.now().toString());
      localStorage.setItem('pwa_visit_count', '1');
      return false;
    }

    localStorage.setItem('pwa_visit_count', (visitCount + 1).toString());

    const timeSinceFirstVisit = Date.now() - parseInt(firstVisit);
    const fiveMinutes = 5 * 60 * 1000;

    if (visitCount >= 2 || timeSinceFirstVisit > fiveMinutes) {
      return localStorage.getItem('pwa_install_dismissed') !== 'true';
    }

    return false;
  });

  const handleInstall = async () => {
    const accepted = await promptInstall();
    if (accepted) {
      setShowBanner(false);
      localStorage.setItem('pwa_install_dismissed', 'true');
    }
  };

  const handleDismiss = () => {
    setDismissed(true);
    setShowBanner(false);
    localStorage.setItem('pwa_install_dismissed', 'true');
  };

  if (!isInstallable || isInstalled || dismissed || !showBanner) {
    return null;
  }

  return (
    <Alert
      severity="info"
      icon={<GetAppIcon />}
      sx={{
        position: 'fixed',
        bottom: 16,
        left: 16,
        right: 16,
        zIndex: 1300,
        maxWidth: 600,
        margin: '0 auto',
      }}
      action={
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <Button
            color="inherit"
            size="small"
            onClick={handleInstall}
            variant="outlined"
          >
            {t('pwa.install.action')}
          </Button>
          <IconButton
            size="small"
            aria-label={t('pwa.install.dismiss')}
            onClick={handleDismiss}
            color="inherit"
          >
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>
      }
    >
      {t('pwa.install.message')}
    </Alert>
  );
}
