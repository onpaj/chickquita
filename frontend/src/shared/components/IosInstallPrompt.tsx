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
  Step,
  Stepper,
  StepLabel,
  StepContent,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import AppleIcon from '@mui/icons-material/Apple';
import IosShareIcon from '@mui/icons-material/IosShare';
import AddBoxIcon from '@mui/icons-material/AddBox';
import { useTranslation } from 'react-i18next';
import { DIALOG_CONFIG } from '@/shared/constants/modalConfig';

const DISMISS_EXPIRY_MS = 90 * 24 * 60 * 60 * 1000; // 90 days

function isDismissed(): boolean {
  try {
    const raw = localStorage.getItem('ios-install-dismissed');
    if (!raw) return false;
    const parsed = JSON.parse(raw);
    if (parsed.dismissed && parsed.expiresAt) {
      if (Date.now() < parsed.expiresAt) return true;
    }
    // Legacy or expired — clear
    localStorage.removeItem('ios-install-dismissed');
    return false;
  } catch {
    return false;
  }
}

function setDismissed(): void {
  localStorage.setItem(
    'ios-install-dismissed',
    JSON.stringify({ dismissed: true, expiresAt: Date.now() + DISMISS_EXPIRY_MS })
  );
}

/**
 * iOS PWA Installation Instructions Component
 *
 * Displays step-by-step instructions for adding the app to Home Screen on iOS Safari.
 * Since iOS doesn't support the beforeinstallprompt API, we need to provide manual instructions.
 *
 * Features:
 * - Auto-detects iOS Safari
 * - Shows after 2nd visit or 5 minutes of usage
 * - Sequential step-by-step guide (one step at a time)
 * - Persists user dismissal for 90 days
 *
 * @example
 * ```tsx
 * <IosInstallPrompt />
 * ```
 */
export function IosInstallPrompt() {
  const { t } = useTranslation();
  const [showPrompt, setShowPrompt] = useState(false);
  const [activeStep, setActiveStep] = useState(0);

  useEffect(() => {
    // Detect iOS Safari
    const userAgent = window.navigator.userAgent.toLowerCase();
    const isIosDevice = /iphone|ipad|ipod/.test(userAgent);
    const isSafari = /safari/.test(userAgent) && !/crios|fxios|edgios/.test(userAgent);
    const isInStandaloneMode = window.matchMedia('(display-mode: standalone)').matches;

    // Don't show if not iOS Safari or already in standalone mode
    if (!isIosDevice || !isSafari || isInStandaloneMode) {
      return;
    }

    // Check if user has already dismissed the prompt
    if (isDismissed()) {
      return;
    }

    // Track visits
    const visitCount = parseInt(localStorage.getItem('ios-visit-count') || '0', 10);
    localStorage.setItem('ios-visit-count', (visitCount + 1).toString());

    if (!localStorage.getItem('ios-first-visit')) {
      localStorage.setItem('ios-first-visit', Date.now().toString());
    }

    // Check engagement criteria
    const firstVisit = parseInt(localStorage.getItem('ios-first-visit') || '0', 10);
    const now = Date.now();

    // Show after 2nd visit OR after 5 minutes of first visit
    if (visitCount >= 2 || (firstVisit && now - firstVisit >= 5 * 60 * 1000)) {
      // Delay showing prompt by 3 seconds to avoid immediate popup
      setTimeout(() => {
        setShowPrompt(true);
      }, 3000);
    }
  }, []);

  const handleDismiss = () => {
    setDismissed();
    setShowPrompt(false);
  };

  const handleClose = () => {
    // Just close for now, allow showing again on next visit
    setShowPrompt(false);
  };

  const handleNext = () => {
    setActiveStep((prev) => Math.min(prev + 1, 2));
  };

  const handleBack = () => {
    setActiveStep((prev) => Math.max(prev - 1, 0));
  };

  const isLastStep = activeStep === 2;

  // Don't render if prompt is not showing
  if (!showPrompt) {
    return null;
  }

  return (
    <Dialog
      open={showPrompt}
      onClose={handleClose}
      {...DIALOG_CONFIG}
      PaperProps={{
        sx: {
          borderRadius: 2,
          m: 2,
        },
      }}
    >
      <DialogTitle sx={{ pr: 6 }}>
        <Box display="flex" alignItems="center" gap={1}>
          <AppleIcon color="primary" />
          <Typography variant="h6" component="span">
            {t('pwa.ios.title')}
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
          {t('pwa.ios.description')}
        </Typography>

        <Stepper activeStep={activeStep} orientation="vertical" sx={{ mt: 2 }}>
          <Step>
            <StepLabel
              icon={
                <Box
                  sx={{
                    bgcolor: 'primary.main',
                    color: 'white',
                    borderRadius: '50%',
                    width: 24,
                    height: 24,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '0.875rem',
                    fontWeight: 'bold',
                  }}
                >
                  1
                </Box>
              }
            >
              <Typography variant="subtitle2">{t('pwa.ios.step1.title')}</Typography>
            </StepLabel>
            <StepContent>
              <Box display="flex" alignItems="center" gap={1} sx={{ py: 1 }}>
                <Typography variant="body2" color="text.secondary">
                  {t('pwa.ios.step1.description')}
                </Typography>
                <IosShareIcon
                  sx={{
                    color: 'primary.main',
                    fontSize: 28,
                    border: '2px solid',
                    borderColor: 'primary.main',
                    borderRadius: 1,
                    p: 0.5,
                  }}
                />
              </Box>
              <Box sx={{ mt: 1 }}>
                <Button variant="contained" size="small" onClick={handleNext}>
                  {t('common.next')}
                </Button>
              </Box>
            </StepContent>
          </Step>

          <Step>
            <StepLabel
              icon={
                <Box
                  sx={{
                    bgcolor: 'primary.main',
                    color: 'white',
                    borderRadius: '50%',
                    width: 24,
                    height: 24,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '0.875rem',
                    fontWeight: 'bold',
                  }}
                >
                  2
                </Box>
              }
            >
              <Typography variant="subtitle2">{t('pwa.ios.step2.title')}</Typography>
            </StepLabel>
            <StepContent>
              <Box display="flex" alignItems="center" gap={1} sx={{ py: 1 }}>
                <Typography variant="body2" color="text.secondary">
                  {t('pwa.ios.step2.description')}
                </Typography>
                <AddBoxIcon
                  sx={{
                    color: 'primary.main',
                    fontSize: 28,
                  }}
                />
              </Box>
              <Box sx={{ mt: 1, display: 'flex', gap: 1 }}>
                <Button size="small" onClick={handleBack}>
                  {t('common.back')}
                </Button>
                <Button variant="contained" size="small" onClick={handleNext}>
                  {t('common.next')}
                </Button>
              </Box>
            </StepContent>
          </Step>

          <Step>
            <StepLabel
              icon={
                <Box
                  sx={{
                    bgcolor: 'primary.main',
                    color: 'white',
                    borderRadius: '50%',
                    width: 24,
                    height: 24,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '0.875rem',
                    fontWeight: 'bold',
                  }}
                >
                  3
                </Box>
              }
            >
              <Typography variant="subtitle2">{t('pwa.ios.step3.title')}</Typography>
            </StepLabel>
            <StepContent>
              <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
                {t('pwa.ios.step3.description')}
              </Typography>
              <Box sx={{ mt: 1, display: 'flex', gap: 1 }}>
                <Button size="small" onClick={handleBack}>
                  {t('common.back')}
                </Button>
              </Box>
            </StepContent>
          </Step>
        </Stepper>

        {isLastStep && (
          <Box
            sx={{
              mt: 3,
              p: 2,
              bgcolor: 'success.light',
              borderRadius: 1,
              border: '1px solid',
              borderColor: 'success.main',
            }}
          >
            <Typography variant="body2" color="success.dark" fontWeight="medium">
              {t('pwa.ios.benefit')}
            </Typography>
          </Box>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={handleDismiss} color="inherit">
          {t('pwa.ios.dismiss')}
        </Button>
        <Button onClick={handleClose} variant="contained">
          {t('pwa.ios.gotIt')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
