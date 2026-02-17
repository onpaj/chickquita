import { useEffect } from 'react'
import { Routes, Route, Navigate, useNavigate } from 'react-router-dom'
import SignUpPage from './pages/SignUpPage'
import SignInPage from './pages/SignInPage'
import DashboardPage from './pages/DashboardPage'
import StatisticsPage from './pages/StatisticsPage'
import CoopsPage from './pages/CoopsPage'
import { CoopDetailPage } from './pages/CoopDetailPage'
import FlocksPage from './pages/FlocksPage'
import { FlockDetailPage } from './pages/FlockDetailPage'
import { FlockHistoryPage } from './features/flocks/components/FlockHistoryPage'
import { DailyRecordsListPage } from './pages/DailyRecordsListPage'
import { PurchasesPage } from './features/purchases/pages/PurchasesPage'
import { SettingsPage } from './pages/SettingsPage'
import NotFoundPage from './pages/NotFoundPage'
import ProtectedRoute from './components/ProtectedRoute'
import { BottomNavigation } from './components/BottomNavigation'
import { OfflineBanner, PwaInstallPrompt, IosInstallPrompt } from './shared/components'
import { useApiClient } from './lib/useApiClient'
import { useAuth } from '@clerk/clerk-react'
import { startAutoSync } from './lib/syncManager'
import { Box, AppBar, Toolbar, Typography, IconButton } from '@mui/material'
import AccountCircleIcon from '@mui/icons-material/AccountCircle'

function App() {
  // Initialize API client with Clerk authentication
  useApiClient()
  const { isSignedIn } = useAuth()
  const navigate = useNavigate()

  // Start offline sync manager
  useEffect(() => {
    startAutoSync()
  }, [])

  return (
    <>
      {/* App Bar — only shown when signed in */}
      {isSignedIn && (
        <AppBar
          position="sticky"
          color="default"
          elevation={1}
          sx={{ top: 0, zIndex: 1100, height: 56 }}
        >
          <Toolbar sx={{ minHeight: '56px !important', px: 2 }}>
            <Typography
              variant="h6"
              component="div"
              sx={{ flexGrow: 1, fontWeight: 700, color: 'primary.main', letterSpacing: 0.5 }}
            >
              Chickquita
            </Typography>
            <IconButton
              onClick={() => navigate('/settings')}
              aria-label="settings"
              size="medium"
              sx={{ p: 1 }}
            >
              <AccountCircleIcon />
            </IconButton>
          </Toolbar>
        </AppBar>
      )}

      {/* Offline detection banner */}
      {isSignedIn && <OfflineBanner />}

      {/* PWA Install prompts (auto-detect platform) */}
      {isSignedIn && (
        <>
          <PwaInstallPrompt />
          <IosInstallPrompt />
        </>
      )}

      <Box sx={{ pb: isSignedIn ? 8 : 0 }}>
        <Routes>
          <Route path="/" element={<Navigate to="/sign-up" replace />} />
          <Route path="/sign-up/*" element={<SignUpPage />} />
          <Route path="/sign-in/*" element={<SignInPage />} />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/statistics"
            element={
              <ProtectedRoute>
                <StatisticsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/coops"
            element={
              <ProtectedRoute>
                <CoopsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/coops/:id"
            element={
              <ProtectedRoute>
                <CoopDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/coops/:coopId/flocks"
            element={
              <ProtectedRoute>
                <FlocksPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/coops/:coopId/flocks/:flockId"
            element={
              <ProtectedRoute>
                <FlockDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/coops/:coopId/flocks/:flockId/history"
            element={
              <ProtectedRoute>
                <FlockHistoryPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/daily-records"
            element={
              <ProtectedRoute>
                <DailyRecordsListPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/purchases"
            element={
              <ProtectedRoute>
                <PurchasesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/settings"
            element={
              <ProtectedRoute>
                <SettingsPage />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </Box>
      {isSignedIn && <BottomNavigation />}
    </>
  )
}

export default App
