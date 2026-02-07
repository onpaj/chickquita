/// <reference types="vite/client" />
/// <reference types="vite-plugin-svgr/client" />

interface ImportMetaEnv {
  // Clerk Authentication
  readonly VITE_CLERK_PUBLISHABLE_KEY: string

  // API Configuration
  readonly VITE_API_BASE_URL: string

  // Application Configuration
  readonly VITE_APP_NAME: string
  readonly VITE_APP_VERSION: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
