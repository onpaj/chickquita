import { useRegisterSW } from 'virtual:pwa-register/react'
import { Snackbar, Button, Alert } from '@mui/material'
import SystemUpdateAltIcon from '@mui/icons-material/SystemUpdateAlt'
import { useTranslation } from 'react-i18next'

export function PwaUpdatePrompt() {
  const { t } = useTranslation()
  const {
    needRefresh: [needRefresh, setNeedRefresh],
    updateServiceWorker,
  } = useRegisterSW()

  const handleUpdate = () => {
    void updateServiceWorker(true)
  }

  const handleClose = () => {
    setNeedRefresh(false)
  }

  return (
    <Snackbar
      open={needRefresh}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      sx={{ bottom: { xs: 'calc(64px + env(safe-area-inset-bottom) + 16px)', sm: 16 } }}
    >
      <Alert
        severity="info"
        icon={<SystemUpdateAltIcon fontSize="inherit" />}
        onClose={handleClose}
        action={
          <Button
            color="inherit"
            size="small"
            onClick={handleUpdate}
            sx={{ fontWeight: 700, whiteSpace: 'nowrap' }}
          >
            {t('pwa.update.action')}
          </Button>
        }
        sx={{ width: '100%', alignItems: 'center' }}
      >
        {t('pwa.update.message')}
      </Alert>
    </Snackbar>
  )
}
