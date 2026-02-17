import { useEffect, useState } from 'react';
import { Box, Alert, Button, Chip, IconButton, Collapse } from '@mui/material';
import {
  CloudOff as CloudOffIcon,
  CloudDone as CloudDoneIcon,
  Sync as SyncIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { db, type SyncStatus } from '@/lib/db';
import { manualSync } from '@/lib/syncManager';

interface OfflineBannerProps {
  onVisibilityChange?: (visible: boolean) => void;
}

/**
 * Offline banner component that displays:
 * - Offline/online status
 * - Number of pending syncs
 * - Manual sync button
 * - Sync progress indicator
 *
 * Appears at the top of the screen when offline or when there are pending syncs.
 */
export function OfflineBanner({ onVisibilityChange }: OfflineBannerProps = {}) {
  const { t } = useTranslation();
  const [isOnline, setIsOnline] = useState(navigator.onLine);
  const [syncStatus, setSyncStatus] = useState<SyncStatus | null>(null);
  const [isSyncing, setIsSyncing] = useState(false);
  const [isDismissed, setIsDismissed] = useState(false);

  // Listen for online/offline events
  useEffect(() => {
    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => {
      setIsOnline(false);
      setIsDismissed(false); // Show banner when going offline
    };

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  // Poll sync status from IndexedDB
  useEffect(() => {
    const updateSyncStatus = async () => {
      const status = await db.getSyncStatus();
      setSyncStatus(status);
      setIsSyncing(status.status === 'syncing');
    };

    // Initial load
    updateSyncStatus();

    // Poll every 2 seconds
    const interval = setInterval(updateSyncStatus, 2000);

    return () => clearInterval(interval);
  }, []);

  const handleManualSync = async () => {
    setIsSyncing(true);
    try {
      const success = await manualSync();
      if (success) {
        // Auto-dismiss after successful sync
        setTimeout(() => setIsDismissed(true), 3000);
      }
    } finally {
      setIsSyncing(false);
    }
  };

  const handleDismiss = () => {
    setIsDismissed(true);
  };

  const shouldShow =
    !(isDismissed && isOnline && (!syncStatus || syncStatus.pendingCount === 0)) &&
    !(isOnline && (!syncStatus || syncStatus.pendingCount === 0));

  // Notify parent when visibility changes
  useEffect(() => {
    onVisibilityChange?.(shouldShow);
  }, [shouldShow, onVisibilityChange]);

  if (!shouldShow) {
    return null;
  }

  const severity = !isOnline ? 'warning' : syncStatus?.status === 'error' ? 'error' : 'info';
  const icon = !isOnline ? <CloudOffIcon /> : syncStatus?.status === 'success' ? <CloudDoneIcon /> : <SyncIcon />;

  return (
    <Collapse in={!isDismissed}>
      <Box
        sx={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          zIndex: 1300,
          boxShadow: 2,
        }}
      >
        <Alert
          severity={severity}
          icon={icon}
          action={
            <Box display="flex" alignItems="center" gap={1}>
              {/* Pending count chip */}
              {syncStatus && syncStatus.pendingCount > 0 && (
                <Chip
                  size="small"
                  label={`${syncStatus.pendingCount} ${t('offline.pending', 'pending')}`}
                  color={severity === 'error' ? 'error' : 'default'}
                  sx={{ fontWeight: 600 }}
                />
              )}

              {/* Manual sync button */}
              {isOnline && syncStatus && syncStatus.pendingCount > 0 && (
                <Button
                  size="small"
                  variant="outlined"
                  startIcon={<SyncIcon />}
                  onClick={handleManualSync}
                  disabled={isSyncing}
                  sx={{
                    minWidth: 48,
                    minHeight: 32,
                    animation: isSyncing ? 'spin 1s linear infinite' : 'none',
                    '@keyframes spin': {
                      '0%': { transform: 'rotate(0deg)' },
                      '100%': { transform: 'rotate(360deg)' },
                    },
                  }}
                >
                  {t('offline.sync', 'Sync')}
                </Button>
              )}

              {/* Dismiss button (only when online) */}
              {isOnline && (
                <IconButton
                  size="small"
                  onClick={handleDismiss}
                  aria-label={t('common.close', 'Close')}
                  sx={{ minWidth: 48, minHeight: 48 }}
                >
                  <CloseIcon />
                </IconButton>
              )}
            </Box>
          }
          sx={{
            borderRadius: 0,
            '& .MuiAlert-message': {
              width: '100%',
            },
          }}
        >
          <Box>
            {!isOnline ? (
              <>
                <strong>{t('offline.title', 'Offline Mode')}</strong>
                <br />
                {t('offline.message', 'Changes will be saved and synced when connection is restored')}
              </>
            ) : syncStatus?.status === 'syncing' ? (
              <>
                <strong>{t('offline.syncing', 'Syncing...')}</strong>
                <br />
                {t('offline.syncingMessage', 'Uploading pending changes')}
              </>
            ) : syncStatus?.status === 'success' ? (
              <>
                <strong>{t('offline.synced', 'All changes synced')}</strong>
                <br />
                {t('offline.syncedMessage', 'Your data is up to date')}
              </>
            ) : syncStatus?.status === 'error' ? (
              <>
                <strong>{t('offline.syncError', 'Sync failed')}</strong>
                <br />
                {syncStatus.errorMessage || t('offline.syncErrorMessage', 'Some changes could not be synced')}
              </>
            ) : null}
          </Box>
        </Alert>
      </Box>
    </Collapse>
  );
}
