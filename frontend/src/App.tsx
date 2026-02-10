import { useEffect } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
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
import { OfflineBanner } from './shared/components/OfflineBanner'
import { useApiClient } from './lib/useApiClient'
import { useAuth } from '@clerk/clerk-react'
import { startAutoSync } from './lib/syncManager'
import { Box } from '@mui/material'

function App() {
  // Initialize API client with Clerk authentication
  useApiClient()
  const { isSignedIn } = useAuth()

  // Start offline sync manager
  useEffect(() => {
    startAutoSync()
  }, [])

  return (
    <>
      {/* Offline detection banner */}
      {isSignedIn && <OfflineBanner />}

      <Box sx={{ pb: isSignedIn ? 8 : 0, pt: isSignedIn ? 0 : 0 }}>
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
