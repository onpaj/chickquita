import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  IconButton,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import GetAppIcon from '@mui/icons-material/GetApp';
import PhoneAndroidIcon from '@mui/icons-material/PhoneAndroid';
import { useTranslation } from 'react-i18next';

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

/**
 * PWA Install Prompt Component
 *
 * Displays an installation prompt for Progressive Web App on Android/Chrome
 * when the browser's beforeinstallprompt event is triggered.
 *
 * Features:
 * - Auto-shows after 2nd visit or 5 minutes of usage
 * - Uses native browser install prompt
 * - Persists user dismissal to avoid annoyance
 * - Mobile-friendly dialog design
 *
 * @example
 * ```tsx
 * <PwaInstallPrompt />
 * ```
 */
export function PwaInstallPrompt() {
  const { t } = useTranslation();
  const [deferredPrompt, setDeferredPrompt] = useState<BeforeInstallPromptEvent | null>(null);
  const [showPrompt, setShowPrompt] = useState(false);

  useEffect(() => {
    // Check if user has already dismissed the prompt
    const dismissed = localStorage.getItem('pwa-install-dismissed');
    if (dismissed === 'true') {
      return;
    }

    // Listen for beforeinstallprompt event
    const handleBeforeInstallPrompt = (e: Event) => {
      // Prevent the default mini-infobar from appearing
      e.preventDefault();

      // Save the event so it can be triggered later
      setDeferredPrompt(e as BeforeInstallPromptEvent);

      // Check engagement criteria
      const visitCount = parseInt(localStorage.getItem('pwa-visit-count') || '0', 10);
      const firstVisit = parseInt(localStorage.getItem('pwa-first-visit') || '0', 10);
      const now = Date.now();

      // Show after 2nd visit OR after 5 minutes of first visit
      if (visitCount >= 2 || (firstVisit && now - firstVisit >= 5 * 60 * 1000)) {
        // Delay showing prompt by 3 seconds to avoid immediate popup
        setTimeout(() => {
          setShowPrompt(true);
        }, 3000);
      }
    };

    // Track visits
    const visitCount = parseInt(localStorage.getItem('pwa-visit-count') || '0', 10);
    localStorage.setItem('pwa-visit-count', (visitCount + 1).toString());

    if (!localStorage.getItem('pwa-first-visit')) {
      localStorage.setItem('pwa-first-visit', Date.now().toString());
    }

    window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt);

    return () => {
      window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
    };
  }, []);

  const handleInstall = async () => {
    if (!deferredPrompt) return;

    // Show the native install prompt
    await deferredPrompt.prompt();

    // Wait for the user's response
    const choiceResult = await deferredPrompt.userChoice;

    if (choiceResult.outcome === 'accepted') {
      console.log('User accepted the PWA install prompt');
    } else {
      console.log('User dismissed the PWA install prompt');
    }

    // Clear the deferred prompt
    setDeferredPrompt(null);
    setShowPrompt(false);
  };

  const handleDismiss = () => {
    // Mark as dismissed in localStorage to prevent future prompts
    localStorage.setItem('pwa-install-dismissed', 'true');
    setShowPrompt(false);
  };

  const handleClose = () => {
    // Just close for now, allow showing again on next visit
    setShowPrompt(false);
  };

  if (!showPrompt || !deferredPrompt) {
    return null;
  }

  return (
    <Dialog
      open={showPrompt}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: {
          borderRadius: 2,
          m: 2,
        },
      }}
    >
      <DialogTitle sx={{ pr: 6 }}>
        <Box display="flex" alignItems="center" gap={1}>
          <PhoneAndroidIcon color="primary" />
          <Typography variant="h6" component="span">
            {t('pwa.install.title')}
          </Typography>
        </Box>
        <IconButton
          aria-label="close"
          onClick={handleClose}
          sx={{
            position: 'absolute',
            right: 8,
            top: 8,
            color: 'text.secondary',
          }}
        >
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent>
        <Typography variant="body1" paragraph>
          {t('pwa.install.description')}
        </Typography>

        <Box
          component="ul"
          sx={{
            pl: 2,
            '& li': {
              mb: 1,
              color: 'text.secondary',
            },
          }}
        >
          <li>
            <Typography variant="body2">{t('pwa.install.benefit1')}</Typography>
          </li>
          <li>
            <Typography variant="body2">{t('pwa.install.benefit2')}</Typography>
          </li>
          <li>
            <Typography variant="body2">{t('pwa.install.benefit3')}</Typography>
          </li>
          <li>
            <Typography variant="body2">{t('pwa.install.benefit4')}</Typography>
          </li>
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={handleDismiss} color="inherit">
          {t('pwa.install.dismiss')}
        </Button>
        <Button
          onClick={handleInstall}
          variant="contained"
          startIcon={<GetAppIcon />}
          size="large"
          sx={{ minWidth: 140 }}
        >
          {t('pwa.install.install')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
