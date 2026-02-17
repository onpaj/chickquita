import type { Appearance } from '@clerk/types';
import { csCZ } from '@clerk/localizations';

/**
 * Clerk Configuration
 *
 * This file contains the configuration for Clerk authentication,
 * including appearance settings and localization.
 */

// Get Clerk Publishable Key from environment variables
const clerkPublishableKey = import.meta.env.VITE_CLERK_PUBLISHABLE_KEY;

// Temporary debug logging - remove after fixing
console.log('Clerk Key (first 20 chars):', clerkPublishableKey?.substring(0, 20));
console.log('Clerk Key length:', clerkPublishableKey?.length);

if (!clerkPublishableKey) {
  throw new Error('Missing VITE_CLERK_PUBLISHABLE_KEY environment variable');
}

/**
 * Appearance Configuration
 * Customizes the look and feel of Clerk components to match Chickquita branding
 */
export const clerkAppearance: Appearance = {
  // Base theme
  baseTheme: undefined, // Use Clerk's default theme as base

  // Variables for consistent styling
  variables: {
      // Color scheme
      colorPrimary: '#FF6B35', // Chickquita brand primary orange
      colorBackground: '#ffffff',
      colorText: '#000000',
      colorTextSecondary: '#666666',
      colorDanger: '#d32f2f', // Material-UI error red
      colorSuccess: '#2e7d32', // Material-UI success green

      // Typography
      fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
      fontSize: '14px',
      fontWeight: {
        normal: 400,
        medium: 500,
        bold: 600,
      },

      // Border radius (rounded corners per UX-016)
      borderRadius: '12px',

      // Spacing
      spacingUnit: '8px',
    },

  // Layout configuration
  layout: {
      // Responsive design
      socialButtonsVariant: 'iconButton', // Compact social buttons
      socialButtonsPlacement: 'bottom', // Place social buttons below form

      // Logo configuration
      logoImageUrl: undefined, // No custom logo for MVP
      logoPlacement: 'inside', // Logo inside the card

      // Privacy policy and terms
      privacyPageUrl: undefined, // Add in future
      termsPageUrl: undefined, // Add in future

      // Help links
      helpPageUrl: undefined, // Add in future

      // Show branding
      showOptionalFields: true, // Show optional fields like username
    },

  // Element-specific styling
  elements: {
      // Card container
      card: {
        boxShadow: '0px 2px 4px rgba(0, 0, 0, 0.1), 0px 4px 8px rgba(0, 0, 0, 0.1)',
      },

      // Form container
      formButtonPrimary: {
        fontSize: '14px',
        textTransform: 'none',
        fontWeight: 500,
        height: '36px',
      },

      // Input fields
      formFieldInput: {
        borderRadius: '4px',
        fontSize: '14px',
      },

      // Links
      footerActionLink: {
        color: '#FF6B35',
        textDecoration: 'none',
        '&:hover': {
          textDecoration: 'underline',
        },
      },

      // Social buttons
      socialButtonsIconButton: {
        border: '1px solid #e0e0e0',
      },

      // Error messages
      formFieldErrorText: {
        fontSize: '12px',
      },

      // Header
      headerTitle: {
        fontSize: '24px',
        fontWeight: 600,
      },

      headerSubtitle: {
        fontSize: '14px',
        color: '#666666',
      },
    },
};

/**
 * Export Clerk Publishable Key
 * Used to initialize Clerk in the application
 */
export const CLERK_PUBLISHABLE_KEY = clerkPublishableKey;

/**
 * Export Czech Localization
 * Used in ClerkProvider to enable Czech language
 */
export const CLERK_LOCALIZATION = csCZ;

/**
 * Clerk Configuration Object
 * Contains all configuration needed for Clerk initialization
 */
export const clerkConfig = {
  publishableKey: CLERK_PUBLISHABLE_KEY,
  localization: CLERK_LOCALIZATION,
  appearance: clerkAppearance,
} as const;

export default clerkConfig;
