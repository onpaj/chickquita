import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { ClerkProvider } from '@clerk/clerk-react'
import { QueryClientProvider } from '@tanstack/react-query'
import { ThemeProvider } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import './index.css'
import './lib/i18n'
import App from './App.tsx'
import { clerkConfig } from './lib/clerkConfig'
import { queryClient } from './lib/queryClient'
import { ToastProvider } from './components/ToastProvider'
import { theme } from './theme/theme'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <ClerkProvider
        publishableKey={clerkConfig.publishableKey}
        localization={clerkConfig.localization}
        appearance={clerkConfig.appearance}
        afterSignInUrl="/dashboard"
      >
        <QueryClientProvider client={queryClient}>
          <BrowserRouter>
            <ToastProvider>
              <App />
            </ToastProvider>
          </BrowserRouter>
        </QueryClientProvider>
      </ClerkProvider>
    </ThemeProvider>
  </StrictMode>,
)
