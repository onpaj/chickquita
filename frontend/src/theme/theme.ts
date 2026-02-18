import { createTheme } from '@mui/material/styles';

/**
 * Chickquita Application Theme
 *
 * Primary color: Warm Orange (#FF6B35) - Represents energy, warmth, and egg yolk
 * Design system following Material Design 3 principles
 * Mobile-first approach with touch-friendly targets
 */

declare module '@mui/material/styles' {
  interface Palette {
    accent: Palette['primary'];
  }
  interface PaletteOptions {
    accent?: PaletteOptions['primary'];
  }
}

export const theme = createTheme({
  // Color Palette
  palette: {
    mode: 'light',
    primary: {
      main: '#FF6B35', // Warm orange
      light: '#FF9066',
      dark: '#E55420',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#4A5568', // Cool gray for balance
      light: '#718096',
      dark: '#2D3748',
      contrastText: '#FFFFFF',
    },
    error: {
      main: '#E53E3E',
      light: '#FC8181',
      dark: '#C53030',
      contrastText: '#FFFFFF',
    },
    warning: {
      main: '#DD6B20',
      light: '#F6AD55',
      dark: '#C05621',
      contrastText: '#FFFFFF',
    },
    info: {
      main: '#3182CE',
      light: '#63B3ED',
      dark: '#2C5282',
      contrastText: '#FFFFFF',
    },
    success: {
      main: '#38A169',
      light: '#68D391',
      dark: '#2F855A',
      contrastText: '#FFFFFF',
    },
    accent: {
      main: '#FFA94D', // Lighter orange accent
      light: '#FFB86C',
      dark: '#FF922B',
      contrastText: '#000000',
    },
    background: {
      default: '#F7FAFC',
      paper: '#FFFFFF',
    },
    text: {
      primary: '#1A202C',
      secondary: '#4A5568',
      disabled: '#A0AEC0',
    },
    divider: '#E2E8F0',
  },

  // Typography
  typography: {
    fontFamily: [
      'Roboto',
      '-apple-system',
      'BlinkMacSystemFont',
      '"Segoe UI"',
      '"Helvetica Neue"',
      'Arial',
      'sans-serif',
      '"Apple Color Emoji"',
      '"Segoe UI Emoji"',
      '"Segoe UI Symbol"',
    ].join(','),

    // Type scale
    h1: {
      fontSize: '2.5rem', // 40px
      fontWeight: 700,
      lineHeight: 1.2,
      letterSpacing: '-0.01562em',
    },
    h2: {
      fontSize: '2rem', // 32px
      fontWeight: 700,
      lineHeight: 1.3,
      letterSpacing: '-0.00833em',
    },
    h3: {
      fontSize: '1.75rem', // 28px
      fontWeight: 600,
      lineHeight: 1.4,
      letterSpacing: '0em',
    },
    h4: {
      fontSize: '1.5rem', // 24px
      fontWeight: 600,
      lineHeight: 1.4,
      letterSpacing: '0.00735em',
    },
    h5: {
      fontSize: '1.25rem', // 20px
      fontWeight: 600,
      lineHeight: 1.5,
      letterSpacing: '0em',
    },
    h6: {
      fontSize: '1.125rem', // 18px
      fontWeight: 600,
      lineHeight: 1.5,
      letterSpacing: '0.0075em',
    },
    subtitle1: {
      fontSize: '1rem', // 16px
      fontWeight: 500,
      lineHeight: 1.75,
      letterSpacing: '0.00938em',
    },
    subtitle2: {
      fontSize: '0.875rem', // 14px
      fontWeight: 500,
      lineHeight: 1.57,
      letterSpacing: '0.00714em',
    },
    body1: {
      fontSize: '1rem', // 16px
      fontWeight: 400,
      lineHeight: 1.5,
      letterSpacing: '0.00938em',
    },
    body2: {
      fontSize: '0.875rem', // 14px
      fontWeight: 400,
      lineHeight: 1.43,
      letterSpacing: '0.01071em',
    },
    button: {
      fontSize: '0.875rem', // 14px
      fontWeight: 500,
      lineHeight: 1.75,
      letterSpacing: '0.02857em',
      textTransform: 'uppercase',
    },
    caption: {
      fontSize: '0.75rem', // 12px
      fontWeight: 400,
      lineHeight: 1.66,
      letterSpacing: '0.03333em',
    },
    overline: {
      fontSize: '0.75rem', // 12px
      fontWeight: 500,
      lineHeight: 2.66,
      letterSpacing: '0.08333em',
      textTransform: 'uppercase',
    },
  },

  // Spacing - Base unit: 8px
  spacing: 8,

  // Shape
  shape: {
    borderRadius: 8,
  },

  // Shadows (Elevation Standards)
  shadows: [
    'none',
    '0px 1px 3px rgba(0, 0, 0, 0.12), 0px 1px 2px rgba(0, 0, 0, 0.24)', // 1
    '0px 3px 6px rgba(0, 0, 0, 0.15), 0px 2px 4px rgba(0, 0, 0, 0.12)', // 2
    '0px 6px 12px rgba(0, 0, 0, 0.15), 0px 4px 8px rgba(0, 0, 0, 0.12)', // 3
    '0px 10px 20px rgba(0, 0, 0, 0.15), 0px 6px 10px rgba(0, 0, 0, 0.12)', // 4
    '0px 15px 25px rgba(0, 0, 0, 0.15), 0px 8px 12px rgba(0, 0, 0, 0.12)', // 5
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 6
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 7
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 8
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 9
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 10
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 11
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 12
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 13
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 14
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 15
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 16
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 17
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 18
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 19
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 20
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 21
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 22
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 23
    '0px 20px 40px rgba(0, 0, 0, 0.2)', // 24
  ],

  // Component Overrides
  components: {
    // Button
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          padding: '10px 24px',
          minHeight: 48, // Touch-friendly target (iOS/Material Design standard)
          fontSize: '0.875rem',
          fontWeight: 500,
          textTransform: 'none',
          boxShadow: 'none',
          '&:hover': {
            boxShadow: '0px 2px 4px rgba(0, 0, 0, 0.2)',
          },
          '&:active': {
            boxShadow: 'none',
          },
          '&:focus-visible': {
            outline: '3px solid #FF6B35',
            outlineOffset: '2px',
          },
        },
        contained: {
          '&:hover': {
            boxShadow: '0px 4px 8px rgba(0, 0, 0, 0.2)',
          },
        },
        outlined: {
          borderWidth: 2,
          '&:hover': {
            borderWidth: 2,
          },
        },
        sizeLarge: {
          padding: '14px 28px',
          minHeight: 56,
          fontSize: '1rem',
        },
        sizeSmall: {
          padding: '6px 16px',
          minHeight: 40,
          fontSize: '0.8125rem',
        },
      },
      defaultProps: {
        disableElevation: true,
      },
    },

    // IconButton
    MuiIconButton: {
      styleOverrides: {
        root: {
          minWidth: 48, // Touch-friendly target
          minHeight: 48,
          padding: 12,
          borderRadius: 8,
          '&:hover': {
            backgroundColor: 'rgba(255, 107, 53, 0.08)',
          },
          '&:focus-visible': {
            outline: '3px solid #FF6B35',
            outlineOffset: '2px',
          },
        },
        sizeLarge: {
          minWidth: 56,
          minHeight: 56,
          padding: 16,
        },
        sizeSmall: {
          minWidth: 40,
          minHeight: 40,
          padding: 8,
        },
      },
    },

    // Floating Action Button
    MuiFab: {
      styleOverrides: {
        root: {
          minWidth: 56,
          minHeight: 56,
          borderRadius: 16,
          boxShadow: '0px 6px 12px rgba(0, 0, 0, 0.15), 0px 4px 8px rgba(0, 0, 0, 0.12)',
          '&:hover': {
            boxShadow: '0px 10px 20px rgba(0, 0, 0, 0.2), 0px 6px 10px rgba(0, 0, 0, 0.15)',
          },
          '&:active': {
            boxShadow: '0px 6px 12px rgba(0, 0, 0, 0.15), 0px 4px 8px rgba(0, 0, 0, 0.12)',
          },
          '&:focus-visible': {
            outline: '3px solid #FF6B35',
            outlineOffset: '2px',
          },
        },
      },
    },

    // Card
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0px 2px 8px rgba(0, 0, 0, 0.1)',
          transition: 'box-shadow 0.3s ease, transform 0.3s ease',
          '&:hover': {
            boxShadow: '0px 4px 16px rgba(0, 0, 0, 0.15)',
          },
          '&:focus-visible': {
            outline: '3px solid #FF6B35',
            outlineOffset: '2px',
          },
        },
      },
    },

    // Card Content
    MuiCardContent: {
      styleOverrides: {
        root: {
          padding: 16,
          '&:last-child': {
            paddingBottom: 16,
          },
        },
      },
    },

    // Card Actions
    MuiCardActions: {
      styleOverrides: {
        root: {
          padding: 16,
          paddingTop: 8,
        },
      },
    },

    // Paper
    MuiPaper: {
      styleOverrides: {
        root: {
          borderRadius: 12,
        },
        elevation1: {
          boxShadow: '0px 1px 3px rgba(0, 0, 0, 0.12), 0px 1px 2px rgba(0, 0, 0, 0.24)',
        },
        elevation2: {
          boxShadow: '0px 3px 6px rgba(0, 0, 0, 0.15), 0px 2px 4px rgba(0, 0, 0, 0.12)',
        },
        elevation3: {
          boxShadow: '0px 6px 12px rgba(0, 0, 0, 0.15), 0px 4px 8px rgba(0, 0, 0, 0.12)',
        },
      },
    },

    // TextField
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: 8,
            minHeight: 48, // Touch-friendly
            '&.Mui-focused': {
              '& .MuiOutlinedInput-notchedOutline': {
                borderWidth: 2,
                borderColor: '#FF6B35',
              },
            },
          },
        },
      },
    },

    // App Bar
    MuiAppBar: {
      styleOverrides: {
        root: {
          borderRadius: 0,
          boxShadow: '0px 1px 3px rgba(0, 0, 0, 0.12)',
        },
      },
    },

    // Chip
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          fontWeight: 500,
        },
      },
    },

    // Dialog
    MuiDialog: {
      styleOverrides: {
        paper: {
          borderRadius: 16,
        },
      },
    },

    // Bottom Navigation
    MuiBottomNavigation: {
      styleOverrides: {
        root: {
          height: 64,
          boxShadow: '0px -2px 4px rgba(0, 0, 0, 0.1)',
        },
      },
    },

    // Bottom Navigation Action
    MuiBottomNavigationAction: {
      styleOverrides: {
        root: {
          minWidth: 64,
          padding: '8px 12px',
          '&.Mui-selected': {
            fontSize: '0.75rem',
          },
        },
      },
    },
  },

  // Breakpoints for responsive design (mobile-first)
  breakpoints: {
    values: {
      xs: 0,     // Mobile portrait
      sm: 480,   // Mobile landscape
      md: 768,   // Tablet
      lg: 1024,  // Desktop
      xl: 1440,  // Large desktop
    },
  },
});

export default theme;
