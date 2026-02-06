import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { ClerkProvider } from '@clerk/clerk-react'
import './index.css'
import './lib/i18n'
import App from './App.tsx'
import { clerkConfig } from './lib/clerkConfig'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ClerkProvider
      publishableKey={clerkConfig.publishableKey}
      localization={clerkConfig.localization}
      appearance={clerkConfig.appearance}
      afterSignInUrl="/dashboard" 
    >
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </ClerkProvider>
  </StrictMode>,
)
