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
