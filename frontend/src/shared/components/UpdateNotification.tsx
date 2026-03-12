import { useRegisterSW } from 'virtual:pwa-register/react'
import { Snackbar, Alert, Button } from '@mui/material'
import { useTranslation } from 'react-i18next'

export function UpdateNotification() {
  const { t } = useTranslation()
  const {
    needRefresh: [needRefresh],
    updateServiceWorker,
  } = useRegisterSW()

  return (
    <Snackbar
      open={needRefresh}
      anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      sx={{ top: { xs: 72, sm: 80 } }}
    >
      <Alert
        severity="info"
        variant="filled"
        action={
          <Button
            color="inherit"
            size="small"
            onClick={() => updateServiceWorker(true)}
          >
            {t('pwa.update.reload')}
          </Button>
        }
        sx={{ width: '100%' }}
      >
        {t('pwa.update.message')}
      </Alert>
    </Snackbar>
  )
}
