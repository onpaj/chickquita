# M12: PWA Installation - Implementation Guide

## Overview
This document provides a complete implementation guide for M12 (PWA Installation), enabling users to install Chickquita on their home screen like a native app.

## Current Status
- ✅ VitePWA plugin configured in `vite.config.ts`
- ✅ Service worker implemented (M10)
- ✅ Basic manifest configuration
- ❌ App icons missing
- ❌ Install prompt handler not implemented
- ❌ Custom install banner not implemented
- ❌ iOS instructions modal not implemented

## Implementation Tasks

### 1. Create App Icons

#### Required Icon Sizes
- 72x72px (small devices)
- 192x192px (standard, Android)
- 512x512px (high-res, splash screen)

#### Icon Design Guidelines
- **Subject**: Chicken silhouette or egg icon
- **Background**: Solid color (#FF6B35 - theme primary color)
- **Format**: PNG with transparency
- **Purpose**: Both "any" and "maskable" (safe zone: 80% of canvas)

#### Steps to Create Icons
1. Create SVG source icon at 512x512px
2. Use online tool (e.g., https://realfavicongenerator.net/) or ImageMagick to generate PNG icons
3. Save icons to `frontend/public/icons/`:
   - `icon-72x72.png`
   - `icon-192x192.png`
   - `icon-512x512.png`

#### Example ImageMagick Commands
```bash
# If you have SVG source (icon.svg)
convert icon.svg -resize 72x72 frontend/public/icons/icon-72x72.png
convert icon.svg -resize 192x192 frontend/public/icons/icon-192x192.png
convert icon.svg -resize 512x512 frontend/public/icons/icon-512x512.png
```

#### Alternative: Use Online Icon Generator
- Upload logo to https://www.pwabuilder.com/imageGenerator
- Generate all required sizes
- Download and extract to `frontend/public/icons/`

### 2. Update vite.config.ts

Add 72x72 icon to the manifest configuration:

```typescript
// vite.config.ts
icons: [
  {
    src: '/icons/icon-72x72.png',
    sizes: '72x72',
    type: 'image/png',
    purpose: 'any maskable'
  },
  {
    src: '/icons/icon-192x192.png',
    sizes: '192x192',
    type: 'image/png',
    purpose: 'any maskable'
  },
  {
    src: '/icons/icon-512x512.png',
    sizes: '512x512',
    type: 'image/png',
    purpose: 'any maskable'
  }
]
```

### 3. Create Install Prompt Hook

Create a React hook to handle the install prompt:

**File**: `frontend/src/hooks/usePWAInstall.ts`

```typescript
import { useState, useEffect } from 'react';

interface BeforeInstallPromptEvent extends Event {
  readonly platforms: string[];
  readonly userChoice: Promise<{
    outcome: 'accepted' | 'dismissed';
    platform: string;
  }>;
  prompt(): Promise<void>;
}

export function usePWAInstall() {
  const [installPrompt, setInstallPrompt] = useState<BeforeInstallPromptEvent | null>(null);
  const [isInstallable, setIsInstallable] = useState(false);
  const [isInstalled, setIsInstalled] = useState(false);

  useEffect(() => {
    // Check if already installed
    const checkInstalled = () => {
      const isStandalone = window.matchMedia('(display-mode: standalone)').matches;
      const isIOSStandalone = (window.navigator as any).standalone === true;
      setIsInstalled(isStandalone || isIOSStandalone);
    };

    checkInstalled();

    // Listen for beforeinstallprompt event
    const handleBeforeInstallPrompt = (e: Event) => {
      e.preventDefault();
      const promptEvent = e as BeforeInstallPromptEvent;
      setInstallPrompt(promptEvent);
      setIsInstallable(true);
    };

    // Listen for appinstalled event
    const handleAppInstalled = () => {
      setIsInstalled(true);
      setIsInstallable(false);
      setInstallPrompt(null);
    };

    window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
    window.addEventListener('appinstalled', handleAppInstalled);

    return () => {
      window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
      window.removeEventListener('appinstalled', handleAppInstalled);
    };
  }, []);

  const promptInstall = async () => {
    if (!installPrompt) return false;

    await installPrompt.prompt();
    const { outcome } = await installPrompt.userChoice;

    if (outcome === 'accepted') {
      setIsInstallable(false);
      setInstallPrompt(null);
      return true;
    }

    return false;
  };

  const isIOS = /iPhone|iPad|iPod/.test(navigator.userAgent);
  const isIOSChrome = isIOS && /CriOS/.test(navigator.userAgent);
  const showIOSInstructions = isIOS && !isInstalled && !isIOSChrome;

  return {
    isInstallable,
    isInstalled,
    promptInstall,
    isIOS,
    showIOSInstructions,
  };
}
```

### 4. Create Install Banner Component

**File**: `frontend/src/features/pwa/components/InstallBanner.tsx`

```typescript
import { useState, useEffect } from 'react';
import { Alert, Button, IconButton, Box } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import GetAppIcon from '@mui/icons-material/GetApp';
import { useTranslation } from 'react-i18next';
import { usePWAInstall } from '@/hooks/usePWAInstall';

export function InstallBanner() {
  const { t } = useTranslation();
  const { isInstallable, isInstalled, promptInstall } = usePWAInstall();
  const [dismissed, setDismissed] = useState(false);
  const [showBanner, setShowBanner] = useState(false);

  useEffect(() => {
    // Show banner after 2nd visit or 5 minutes usage
    const visitCount = parseInt(localStorage.getItem('pwa_visit_count') || '0');
    const firstVisit = localStorage.getItem('pwa_first_visit');

    if (!firstVisit) {
      localStorage.setItem('pwa_first_visit', Date.now().toString());
      localStorage.setItem('pwa_visit_count', '1');
      return;
    }

    localStorage.setItem('pwa_visit_count', (visitCount + 1).toString());

    const timeSinceFirstVisit = Date.now() - parseInt(firstVisit);
    const fiveMinutes = 5 * 60 * 1000;

    if (visitCount >= 2 || timeSinceFirstVisit > fiveMinutes) {
      const dismissed = localStorage.getItem('pwa_install_dismissed') === 'true';
      if (!dismissed) {
        setShowBanner(true);
      }
    }
  }, []);

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
```

### 5. Create iOS Install Instructions Modal

**File**: `frontend/src/features/pwa/components/IOSInstallModal.tsx`

```typescript
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import IosShareIcon from '@mui/icons-material/IosShare';
import AddBoxIcon from '@mui/icons-material/AddBox';
import { useTranslation } from 'react-i18next';

interface IOSInstallModalProps {
  open: boolean;
  onClose: () => void;
}

export function IOSInstallModal({ open, onClose }: IOSInstallModalProps) {
  const { t } = useTranslation();

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{t('pwa.ios.title')}</DialogTitle>
      <DialogContent>
        <Typography variant="body1" gutterBottom>
          {t('pwa.ios.description')}
        </Typography>
        <List>
          <ListItem>
            <ListItemIcon>
              <Box
                sx={{
                  width: 40,
                  height: 40,
                  borderRadius: '50%',
                  bgcolor: 'primary.main',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'white',
                }}
              >
                1
              </Box>
            </ListItemIcon>
            <ListItemText
              primary={t('pwa.ios.step1')}
              secondary={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                  {t('pwa.ios.step1_detail')}
                  <IosShareIcon fontSize="small" />
                </Box>
              }
            />
          </ListItem>
          <ListItem>
            <ListItemIcon>
              <Box
                sx={{
                  width: 40,
                  height: 40,
                  borderRadius: '50%',
                  bgcolor: 'primary.main',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'white',
                }}
              >
                2
              </Box>
            </ListItemIcon>
            <ListItemText
              primary={t('pwa.ios.step2')}
              secondary={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                  {t('pwa.ios.step2_detail')}
                  <AddBoxIcon fontSize="small" />
                </Box>
              }
            />
          </ListItem>
          <ListItem>
            <ListItemIcon>
              <Box
                sx={{
                  width: 40,
                  height: 40,
                  borderRadius: '50%',
                  bgcolor: 'primary.main',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'white',
                }}
              >
                3
              </Box>
            </ListItemIcon>
            <ListItemText
              primary={t('pwa.ios.step3')}
              secondary={t('pwa.ios.step3_detail')}
            />
          </ListItem>
        </List>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="primary" variant="contained">
          {t('common.close')}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
```

### 6. Add Translations

**Czech** (`frontend/src/locales/cs/translation.json`):

```json
{
  "pwa": {
    "install": {
      "message": "Nainstalujte si Chickquita na domovskou obrazovku pro rychlý přístup",
      "action": "Instalovat",
      "dismiss": "Zavřít"
    },
    "ios": {
      "title": "Přidání na domovskou obrazovku",
      "description": "Pro instalaci aplikace na iPhone nebo iPad postupujte podle následujících kroků:",
      "step1": "Klepněte na tlačítko Sdílet",
      "step1_detail": "Ve spodní nabídce Safari",
      "step2": "Vyberte \"Přidat na plochu\"",
      "step2_detail": "V menu možností",
      "step3": "Potvrďte přidání",
      "step3_detail": "Klepněte na \"Přidat\" v pravém horním rohu"
    }
  }
}
```

**English** (`frontend/src/locales/en/translation.json`):

```json
{
  "pwa": {
    "install": {
      "message": "Install Chickquita on your home screen for quick access",
      "action": "Install",
      "dismiss": "Dismiss"
    },
    "ios": {
      "title": "Add to Home Screen",
      "description": "To install the app on iPhone or iPad, follow these steps:",
      "step1": "Tap the Share button",
      "step1_detail": "In the Safari bottom menu",
      "step2": "Select \"Add to Home Screen\"",
      "step2_detail": "From the options menu",
      "step3": "Confirm addition",
      "step3_detail": "Tap \"Add\" in the top right corner"
    }
  }
}
```

### 7. Integrate Components in App

**File**: `frontend/src/App.tsx`

Add the PWA components to the main app:

```typescript
import { InstallBanner } from '@/features/pwa/components/InstallBanner';
import { IOSInstallModal } from '@/features/pwa/components/IOSInstallModal';
import { usePWAInstall } from '@/hooks/usePWAInstall';
import { useState, useEffect } from 'react';

function App() {
  const { showIOSInstructions } = usePWAInstall();
  const [showIOSModal, setShowIOSModal] = useState(false);

  useEffect(() => {
    if (showIOSInstructions) {
      // Show iOS instructions after 5 seconds
      const timer = setTimeout(() => {
        const dismissed = localStorage.getItem('pwa_ios_dismissed') === 'true';
        if (!dismissed) {
          setShowIOSModal(true);
        }
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [showIOSInstructions]);

  const handleCloseIOSModal = () => {
    setShowIOSModal(false);
    localStorage.setItem('pwa_ios_dismissed', 'true');
  };

  return (
    <>
      {/* Your existing app content */}

      {/* PWA Install components */}
      <InstallBanner />
      <IOSInstallModal open={showIOSModal} onClose={handleCloseIOSModal} />
    </>
  );
}
```

### 8. Testing Checklist

#### Desktop Testing
- [ ] Open app in Chrome/Edge
- [ ] Check DevTools > Application > Manifest (verify all icons)
- [ ] Trigger install prompt (appears after criteria met)
- [ ] Install app and verify it opens in standalone mode
- [ ] Check that app icon appears correctly

#### Android Testing
- [ ] Open app in Chrome on Android device
- [ ] Wait for install banner to appear (after 2nd visit or 5 min)
- [ ] Install app via banner or Chrome menu
- [ ] Verify app opens in standalone mode (no browser chrome)
- [ ] Check app icon on home screen

#### iOS Testing
- [ ] Open app in Safari on iPhone/iPad
- [ ] Wait for iOS instructions modal to appear (after 5 sec)
- [ ] Follow instructions to add to home screen
- [ ] Verify app opens in standalone mode
- [ ] Check app icon on home screen
- [ ] Test offline functionality (airplane mode)

#### Lighthouse PWA Audit
```bash
cd frontend
npm run build
npx http-server dist -p 8080

# In Chrome DevTools:
# 1. Open http://localhost:8080
# 2. DevTools > Lighthouse
# 3. Select "Progressive Web App"
# 4. Run audit
# 5. Verify score > 90
```

## Architecture Decisions

### Why Show Install Prompt After 2nd Visit?
- Avoids annoying first-time visitors
- Users more likely to install after understanding value
- Industry best practice (Google recommends)

### Why Separate iOS Instructions?
- iOS doesn't support `beforeinstallprompt` event
- Manual instructions required for iOS Safari
- Different UX pattern needed

### Why Dismiss State in LocalStorage?
- Respects user choice (don't nag)
- Persists across sessions
- Lightweight solution (no backend needed)

### Why Modal Delay (5 seconds)?
- Gives user time to explore app first
- Less intrusive than immediate popup
- Better UX engagement

## Success Metrics

- **Install Rate**: Track installs via Google Analytics events
- **Lighthouse PWA Score**: Target > 90
- **Retention**: Compare standalone vs browser retention
- **Engagement**: Measure daily active users (DAU)

## Future Enhancements

1. **Push Notifications**: Remind users to log daily records
2. **Background Sync**: Queue offline actions (purchases, edits)
3. **Web Share API**: Share statistics with other farmers
4. **Install Prompt Customization**: Add app screenshots
5. **App Store Submission**: Publish to Google Play Store (TWA)

## References

- [PWA Install Patterns](https://web.dev/install-criteria/)
- [VitePWA Documentation](https://vite-pwa-org.netlify.app/)
- [iOS Add to Home Screen](https://support.apple.com/guide/iphone/bookmark-favorite-webpages-iph42ab2f3a7/ios)
- [Workbox Runtime Caching](https://developer.chrome.com/docs/workbox/modules/workbox-strategies/)

---

**Status**: 📋 Implementation guide ready
**Date**: 2024-02-10
**Milestone**: M12 - PWA Installation
