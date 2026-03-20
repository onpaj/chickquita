import { useEffect } from 'react'
import { Routes, Route, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { Box, Tabs, Tab } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { DailyRecordsListPage } from './DailyRecordsListPage'
import StatisticsPage from './StatisticsPage'
import { EggSalesListPage } from './EggSalesListPage'

export function RecordsPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const { t } = useTranslation()

  const getTab = () => {
    if (location.pathname.endsWith('/list')) return 'list'
    if (location.pathname.endsWith('/sales')) return 'sales'
    return 'stats'
  }

  const currentTab = getTab()

  useEffect(() => {
    // Redirect bare /records to /records/stats
    if (location.pathname === '/records' || location.pathname === '/records/') {
      navigate('/records/stats', { replace: true })
    }
  }, [location.pathname, navigate])

  return (
    <Box>
      <Box sx={{ borderBottom: 1, borderColor: 'divider', bgcolor: 'background.paper', position: 'sticky', top: 64, zIndex: 900 }}>
        <Tabs
          value={currentTab}
          onChange={(_, v: string) => navigate(`/records/${v}`)}
          variant="fullWidth"
        >
          <Tab label={t('navigation.statistics')} value="stats" />
          <Tab label={t('navigation.dailyRecords')} value="list" />
          <Tab label={t('navigation.eggSales')} value="sales" />
        </Tabs>
      </Box>
      <Routes>
        <Route path="list" element={<DailyRecordsListPage />} />
        <Route path="stats" element={<StatisticsPage />} />
        <Route path="sales" element={<EggSalesListPage />} />
        <Route index element={<Navigate to="stats" replace />} />
      </Routes>
    </Box>
  )
}
