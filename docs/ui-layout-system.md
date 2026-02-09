# UI Layout System

**Chickquita (Chickquita)** - Complete UI design system, layout patterns, and component specifications for mobile-first PWA.

**Version:** 1.0
**Date:** February 5, 2026
**Status:** Approved

**Living Document:** This design system reflects current implementation in `frontend/src/theme/theme.ts`. Last Verified: 2026-02-09

---

## Table of Contents

- [Design System Foundation](#design-system-foundation)
- [Responsive Layout System](#responsive-layout-system)
- [Navigation Structure](#navigation-structure)
- [Component Patterns](#component-patterns)
- [PWA-Specific UI Patterns](#pwa-specific-ui-patterns)
- [Form Patterns & Validation](#form-patterns--validation)
- [Accessibility & Touch Optimization](#accessibility--touch-optimization)

---

## Design System Foundation

### Color Palette (MUI Theme)

```typescript
// src/theme/theme.ts
import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    primary: {
      main: '#FF6B35',      // Warm orange (chicken/farm theme)
      light: '#FF9066',
      dark: '#E55420',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#4A5568',      // Cool gray for balance
      light: '#718096',
      dark: '#2D3748',
      contrastText: '#FFFFFF',
    },
    success: {
      main: '#38A169',      // Green (eggs, productivity)
      light: '#68D391',
      dark: '#2F855A',
    },
    warning: {
      main: '#DD6B20',      // Orange (alerts)
      light: '#F6AD55',
      dark: '#C05621',
    },
    error: {
      main: '#E53E3E',      // Red (offline, errors)
      light: '#FC8181',
      dark: '#C53030',
    },
    info: {
      main: '#3182CE',
      light: '#63B3ED',
      dark: '#2C5282',
    },
    background: {
      default: '#F7FAFC',   // Light gray
      paper: '#FFFFFF',
    },
    text: {
      primary: '#1A202C',   // Dark blue-gray
      secondary: '#4A5568', // Medium gray
      disabled: '#A0AEC0',
    },
  },
});
```

### Typography

**Font Family:** "Roboto" (primary) loaded via `@fontsource/roboto` package

```typescript
typography: {
  fontFamily: '"Roboto", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "Helvetica Neue", "Arial", sans-serif',
  h1: {
    fontSize: '2.5rem',     // 40px - Page titles
    fontWeight: 700,
    lineHeight: 1.2,
    letterSpacing: '-0.01562em',
  },
  h2: {
    fontSize: '2rem',       // 32px - Section headers
    fontWeight: 700,
    lineHeight: 1.3,
    letterSpacing: '-0.00833em',
  },
  h3: {
    fontSize: '1.75rem',    // 28px - Card titles
    fontWeight: 600,
    lineHeight: 1.4,
    letterSpacing: '0em',
  },
  h4: {
    fontSize: '1.5rem',     // 24px - Subsections
    fontWeight: 600,
    lineHeight: 1.4,
    letterSpacing: '0.00735em',
  },
  h5: {
    fontSize: '1.25rem',    // 20px
    fontWeight: 600,
    lineHeight: 1.5,
    letterSpacing: '0em',
  },
  h6: {
    fontSize: '1.125rem',   // 18px
    fontWeight: 600,
    lineHeight: 1.5,
    letterSpacing: '0.0075em',
  },
  body1: {
    fontSize: '1rem',       // 16px - Main text
    fontWeight: 400,
    lineHeight: 1.5,
    letterSpacing: '0.00938em',
  },
  body2: {
    fontSize: '0.875rem',   // 14px - Secondary text
    fontWeight: 400,
    lineHeight: 1.43,
    letterSpacing: '0.01071em',
  },
  button: {
    fontSize: '0.875rem',   // 14px
    fontWeight: 500,
    lineHeight: 1.75,
    letterSpacing: '0.02857em',
    textTransform: 'uppercase',  // Default (overridden at component level)
  },

  // Note: MuiButton component overrides textTransform to 'none' in components config
  caption: {
    fontSize: '0.75rem',    // 12px - Captions, hints
    fontWeight: 400,
    lineHeight: 1.66,
    letterSpacing: '0.03333em',
  },
},
```

### Spacing Scale

```typescript
spacing: 8, // Base unit: 8px

// Usage examples:
theme.spacing(0.5)  // 4px
theme.spacing(1)    // 8px
theme.spacing(1.5)  // 12px
theme.spacing(2)    // 16px
theme.spacing(3)    // 24px
theme.spacing(4)    // 32px
theme.spacing(6)    // 48px
theme.spacing(8)    // 64px
```

### Elevation (Shadows)

```typescript
// MUI provides 25 elevation levels (0-24)
// Common usage:
elevation={0}   // No shadow (flat)
elevation={1}   // Cards, normal state
elevation={2}   // App bar, raised cards
elevation={3}   // Modals, dialogs
elevation={4}   // FAB button
elevation={8}   // Drawer, navigation drawer
```

---

## Responsive Layout System

### Breakpoints (Mobile-First)

```typescript
breakpoints: {
  values: {
    xs: 0,      // Mobile portrait (320px+)
    sm: 480,    // Mobile landscape
    md: 768,    // Tablet
    lg: 1024,   // Desktop
    xl: 1440,   // Large desktop
  },
}

// Usage in components:
sx={{
  width: '100%',              // Mobile
  [theme.breakpoints.up('sm')]: {
    width: '50%',             // Tablet+
  },
  [theme.breakpoints.up('lg')]: {
    width: '33.33%',          // Desktop+
  },
}}
```

### Grid System

```typescript
// Mobile-first responsive grid
<Grid container spacing={2}>
  <Grid item xs={12} sm={6} md={4}>
    {/* Full width on mobile, half on tablet, third on desktop */}
  </Grid>
</Grid>

// Spacing values:
spacing={1}  // 8px gap
spacing={2}  // 16px gap (default)
spacing={3}  // 24px gap
```

### Container Max Widths

```typescript
// Automatic max-widths per breakpoint
<Container maxWidth="lg">
  {/* Content */}
</Container>

// Max widths:
// xs-sm: 100% width
// md: 768px max
// lg: 1024px max
// xl: 1280px max
```

### Layout Patterns

**Dashboard Layout (Mobile):**
```
┌─────────────────────────────┐
│ Header (64px fixed)         │
├─────────────────────────────┤
│                             │
│ Scrollable Content          │
│ - Widgets (full width)      │
│ - Cards (16px margin)       │
│                             │
│                             │
│                             │
├─────────────────────────────┤
│ Bottom Nav (64px fixed)     │
└─────────────────────────────┘
```

**Detail View Layout:**
```
┌─────────────────────────────┐
│ Header + Back Button        │
├─────────────────────────────┤
│ Hero Section (summary)      │
├─────────────────────────────┤
│ Tabs / Sections             │
│                             │
│ Scrollable Content          │
│                             │
│                             │
└─────────────────────────────┘
```

### Safe Areas (Mobile)

```css
/* Handle iOS notch and home indicator */
.app-header {
  padding-top: env(safe-area-inset-top);
  /* Additional: 44px for status bar */
}

.app-content {
  padding-bottom: env(safe-area-inset-bottom);
  /* Additional: 34px for home indicator on iOS */
}

/* In MUI components: */
sx={{
  pt: 'env(safe-area-inset-top)',
  pb: 'env(safe-area-inset-bottom)',
}}
```

---

## Navigation Structure

### Bottom Navigation (Primary - Mobile)

```typescript
// 5 main sections with icons
const navItems = [
  { path: '/', icon: HomeIcon, label: 'Dashboard' },
  { path: '/coops', icon: HomeWorkIcon, label: 'Kurníky' },
  { path: '/daily-records', icon: EditNoteIcon, label: 'Záznamy' },
  { path: '/statistics', icon: BarChartIcon, label: 'Statistiky' },
  { path: '/menu', icon: MenuIcon, label: 'Menu' },
];

<BottomNavigation
  value={currentPath}
  onChange={handleNavChange}
  sx={{
    position: 'fixed',
    bottom: 0,
    left: 0,
    right: 0,
    height: 64,  // Updated to match theme.ts implementation
    borderTop: '1px solid',
    borderColor: 'divider',
  }}
>
  {navItems.map((item) => (
    <BottomNavigationAction
      key={item.path}
      label={item.label}
      icon={<item.icon />}
      value={item.path}
      sx={{
        minWidth: 48,
        '&.Mui-selected': {
          color: 'primary.main',
        },
      }}
    />
  ))}
</BottomNavigation>
```

**Bottom Nav Specs:**
- Height: 64px (defined in theme.ts)
- Fixed position at bottom
- Active item: primary color with label
- Inactive: gray with icon only
- Touch target: 48x48px minimum
- Ripple effect on tap

### App Bar (Top - Mobile)

```typescript
<AppBar position="sticky" elevation={2}>
  <Toolbar>
    {/* Left: Logo + Title */}
    <Box display="flex" alignItems="center" gap={1}>
      <Avatar src="/logo.png" sx={{ width: 32, height: 32 }} />
      <Typography variant="h6">Chickquita</Typography>
    </Box>

    {/* Right: Actions */}
    <Box ml="auto" display="flex" gap={1}>
      <IconButton color="inherit" aria-label="notifications">
        <NotificationsIcon />
      </IconButton>
      <IconButton color="inherit" aria-label="menu">
        <MoreVertIcon />
      </IconButton>
    </Box>
  </Toolbar>
</AppBar>
```

**App Bar Specs:**
- Height: 64px
- Sticky on scroll (stays visible)
- Elevation: 2
- Background: primary color
- Text color: white

### Drawer Menu (Desktop - lg+)

```typescript
<Drawer
  variant="permanent"
  sx={{
    display: { xs: 'none', lg: 'block' },
    width: 240,
    '& .MuiDrawer-paper': {
      width: 240,
      boxSizing: 'border-box',
    },
  }}
>
  <List>
    {navItems.map((item) => (
      <ListItem key={item.path} button component={Link} to={item.path}>
        <ListItemIcon>
          <item.icon />
        </ListItemIcon>
        <ListItemText primary={item.label} />
      </ListItem>
    ))}
  </List>
</Drawer>
```

### Routing Structure

```typescript
// React Router setup
const routes = [
  { path: '/', element: <DashboardPage />, protected: true },
  { path: '/login', element: <LoginPage />, protected: false },
  { path: '/register', element: <RegisterPage />, protected: false },
  { path: '/coops', element: <CoopsPage />, protected: true },
  { path: '/coops/:id', element: <CoopDetailPage />, protected: true },
  { path: '/flocks/:id', element: <FlockDetailPage />, protected: true },
  { path: '/daily-records', element: <DailyRecordsPage />, protected: true },
  { path: '/purchases', element: <PurchasesPage />, protected: true },
  { path: '/statistics', element: <StatisticsPage />, protected: true },
  { path: '/statistics/egg-cost', element: <EggCostPage />, protected: true },
  { path: '/menu', element: <MenuPage />, protected: true },
];

// Protected route wrapper
const ProtectedRoute = ({ children }) => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return children;
};
```

---

## Component Patterns

### 1. Card Pattern (Primary Container)

```typescript
<Card
  elevation={1}
  sx={{
    borderRadius: 2,
    mb: 2,
    overflow: 'hidden',
  }}
>
  <CardHeader
    title="Hejno A - Hnědé 2024"
    subheader="15 slepic • 2 kohouti • 3 kuřata"
    action={
      <IconButton aria-label="actions">
        <MoreVertIcon />
      </IconButton>
    }
  />
  <CardContent>
    {/* Content */}
  </CardContent>
  <CardActions sx={{ justifyContent: 'flex-end' }}>
    <Button>Upravit</Button>
    <Button color="error">Smazat</Button>
  </CardActions>
</Card>
```

### 2. Floating Action Button (FAB)

```typescript
<Fab
  color="primary"
  aria-label="add"
  sx={{
    position: 'fixed',
    bottom: { xs: 80, sm: 16 }, // Above bottom nav (64px + 16px margin) on mobile
    right: 16,
    zIndex: 1000,
  }}
  onClick={handleQuickAdd}
>
  <AddIcon />
</Fab>
```

### 3. Modal/Dialog Pattern

```typescript
<Dialog
  fullScreen={isMobile}  // Full screen on mobile
  open={open}
  onClose={handleClose}
  TransitionComponent={Slide}
  TransitionProps={{ direction: 'up' }}
  PaperProps={{
    sx: {
      borderRadius: { xs: 0, sm: 2 },
    },
  }}
>
  <DialogTitle>
    Denní záznam
    <IconButton
      onClick={handleClose}
      sx={{
        position: 'absolute',
        right: 8,
        top: 8,
      }}
      aria-label="close"
    >
      <CloseIcon />
    </IconButton>
  </DialogTitle>

  <DialogContent dividers>
    {/* Form content */}
  </DialogContent>

  <DialogActions>
    <Button onClick={handleClose}>
      Zrušit
    </Button>
    <Button
      variant="contained"
      onClick={handleSubmit}
      disabled={!isValid}
    >
      Uložit
    </Button>
  </DialogActions>
</Dialog>
```

### 4. Number Stepper (Critical for Quick Entry)

**Note:** The actual implementation uses the `NumericStepper` component with 48x48px IconButtons (standard touch target size), not 56x56px as shown in this example. See `frontend/src/shared/components/NumericStepper.tsx` for the implemented component.

```typescript
// Example pattern - actual implementation may vary
<Box
  display="flex"
  alignItems="center"
  justifyContent="center"
  gap={2}
>
  <IconButton
    size="large"
    onClick={() => setValue(v => Math.max(0, v - 1))}
    sx={{
      width: 48,  // Standard touch target size (48x48px)
      height: 48,
      bgcolor: 'action.hover',
    }}
    aria-label="decrease"
  >
    <RemoveIcon fontSize="large" />
  </IconButton>

  <TextField
    value={value}
    onChange={(e) => setValue(parseInt(e.target.value) || 0)}
    type="number"
    inputProps={{
      style: {
        textAlign: 'center',
        fontSize: '2rem',
        fontWeight: 600,
      },
      min: 0,
    }}
    sx={{ width: 80 }}  // 80px input width in actual implementation
  />

  <IconButton
    size="large"
    onClick={() => setValue(v => v + 1)}
    sx={{
      width: 48,
      height: 48,
      bgcolor: 'action.hover',
    }}
    aria-label="increase"
  >
    <AddIcon fontSize="large" />
  </IconButton>
</Box>
```

### 5. List with Actions

```typescript
<List>
  {items.map((item) => (
    <ListItem
      key={item.id}
      secondaryAction={
        <IconButton edge="end" aria-label="delete">
          <DeleteIcon />
        </IconButton>
      }
      sx={{
        bgcolor: 'background.paper',
        mb: 1,
        borderRadius: 1,
      }}
    >
      <ListItemAvatar>
        <Avatar>
          <item.icon />
        </Avatar>
      </ListItemAvatar>
      <ListItemText
        primary={item.title}
        secondary={item.subtitle}
      />
    </ListItem>
  ))}
</List>
```

---

## PWA-Specific UI Patterns

### 1. Offline Banner (Persistent)

```typescript
<Snackbar
  open={!isOnline}
  anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
  sx={{ top: { xs: 64, sm: 24 } }} // Below app bar
>
  <Alert
    severity="warning"
    icon={<WifiOffIcon />}
    sx={{ width: '100%' }}
  >
    Jste offline - data se uloží lokálně
  </Alert>
</Snackbar>
```

### 2. Sync Indicator (Bottom Bar)

```typescript
{pendingItems > 0 && (
  <Paper
    elevation={3}
    sx={{
      position: 'fixed',
      bottom: { xs: 64, sm: 0 }, // Above bottom nav (64px) on mobile
      left: 0,
      right: 0,
      p: 1.5,
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      bgcolor: 'warning.light',
      zIndex: 1100,
    }}
  >
    <Typography variant="body2" fontWeight={500}>
      {pendingItems} neuložené záznamy
    </Typography>
    <Button
      size="small"
      variant="contained"
      startIcon={<SyncIcon />}
      onClick={handleSync}
      disabled={isSyncing}
    >
      {isSyncing ? 'Synchronizuji...' : 'Synchronizovat'}
    </Button>
  </Paper>
)}
```

### 3. Install Prompt (Custom)

```typescript
<Paper
  elevation={4}
  sx={{
    position: 'fixed',
    bottom: { xs: 80, sm: 16 }, // Above bottom nav (64px + 16px margin) on mobile
    left: 16,
    right: 16,
    p: 2,
    borderRadius: 2,
    zIndex: 1200,
  }}
>
  <Box display="flex" alignItems="center" gap={2} mb={2}>
    <Avatar sx={{ bgcolor: 'primary.main', width: 48, height: 48 }}>
      <GetAppIcon />
    </Avatar>
    <Box flex={1}>
      <Typography variant="subtitle1" fontWeight={600}>
        Přidat na plochu
      </Typography>
      <Typography variant="body2" color="text.secondary">
        Rychlý přístup i bez internetu!
      </Typography>
    </Box>
  </Box>

  <Stack direction="row" spacing={1}>
    <Button fullWidth onClick={handleDismiss}>
      Možná později
    </Button>
    <Button
      fullWidth
      variant="contained"
      onClick={handleInstall}
    >
      Přidat
    </Button>
  </Stack>
</Paper>
```

### 4. Pull-to-Refresh

**Status:** 🚧 Planned Feature - Not Yet Implemented

This pattern is documented for future implementation. Users can manually refresh by using standard browser refresh or the sync button in the UI.

```typescript
// Future implementation using touch events
const [isPulling, setIsPulling] = useState(false);
const [pullDistance, setPullDistance] = useState(0);

const handleTouchStart = (e: TouchEvent) => {
  if (window.scrollY === 0) {
    startY = e.touches[0].clientY;
  }
};

const handleTouchMove = (e: TouchEvent) => {
  if (startY === null) return;

  const currentY = e.touches[0].clientY;
  const distance = currentY - startY;

  if (distance > 0 && distance < 100) {
    setPullDistance(distance);
    setIsPulling(true);
  }
};

const handleTouchEnd = () => {
  if (pullDistance > 70) {
    // Trigger refresh
    refetch();
  }
  setPullDistance(0);
  setIsPulling(false);
  startY = null;
};

<Box
  sx={{
    overflowY: 'auto',
    WebkitOverflowScrolling: 'touch',
  }}
  onTouchStart={handleTouchStart}
  onTouchMove={handleTouchMove}
  onTouchEnd={handleTouchEnd}
>
  {isPulling && (
    <Box
      textAlign="center"
      py={2}
      sx={{
        transform: `translateY(${pullDistance}px)`,
        transition: 'transform 0.2s',
      }}
    >
      <CircularProgress size={24} />
    </Box>
  )}
  {children}
</Box>
```

### 5. Loading States (Skeleton)

```typescript
{isLoading ? (
  <Card>
    <CardContent>
      <Skeleton variant="text" width="60%" height={32} />
      <Skeleton variant="text" width="40%" />
      <Skeleton
        variant="rectangular"
        height={100}
        sx={{ mt: 2, borderRadius: 1 }}
      />
    </CardContent>
  </Card>
) : (
  <ActualContent />
)}
```

---

## Form Patterns & Validation

### Quick Add Form (< 30 Second Target)

```typescript
const schema = z.object({
  flockId: z.string().min(1, 'Vyberte hejno'),
  date: z.date(),
  eggCount: z.number().min(0, 'Musí být 0 nebo více'),
  notes: z.string().optional(),
});

const QuickAddModal = () => {
  const { register, handleSubmit, formState: { errors }, setValue } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      flockId: lastUsedFlockId, // Auto-select last used
      date: new Date(),          // Default today
      eggCount: 0,
    }
  });

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)}>
      {/* Flock selector */}
      <FormControl fullWidth sx={{ mb: 2 }}>
        <InputLabel>Hejno</InputLabel>
        <Select {...register('flockId')} error={!!errors.flockId}>
          {flocks.map(f => (
            <MenuItem key={f.id} value={f.id}>{f.identifier}</MenuItem>
          ))}
        </Select>
        {errors.flockId && (
          <FormHelperText error>{errors.flockId.message}</FormHelperText>
        )}
      </FormControl>

      {/* Date picker */}
      <DatePicker
        label="Datum"
        value={date}
        onChange={(newDate) => setValue('date', newDate)}
        slotProps={{
          textField: {
            fullWidth: true,
            sx: { mb: 2 },
            error: !!errors.date,
          }
        }}
      />

      {/* Egg count - AUTO-FOCUS HERE for fastest entry */}
      <Box sx={{ mb: 2 }}>
        <Typography variant="body2" mb={1} fontWeight={500}>
          Počet vajec
        </Typography>
        <NumberStepper
          value={eggCount}
          onChange={(value) => setValue('eggCount', value)}
          autoFocus  // Focus here for fastest entry
        />
        {errors.eggCount && (
          <FormHelperText error>{errors.eggCount.message}</FormHelperText>
        )}
      </Box>

      {/* Optional notes */}
      <TextField
        label="Poznámka (volitelně)"
        multiline
        rows={2}
        fullWidth
        {...register('notes')}
        sx={{ mb: 3 }}
      />

      {/* Sticky submit button */}
      <Button
        type="submit"
        variant="contained"
        fullWidth
        size="large"
        sx={{ py: 1.5 }}
      >
        Uložit ✓
      </Button>
    </Box>
  );
};
```

### Real-time Validation Feedback

```typescript
<TextField
  {...register('email')}
  error={!!errors.email}
  helperText={errors.email?.message}
  InputProps={{
    endAdornment: (
      !errors.email && isDirty && (
        <InputAdornment position="end">
          <CheckCircleIcon color="success" />
        </InputAdornment>
      )
    ),
  }}
/>
```

### Submit Button States

```typescript
<LoadingButton
  type="submit"
  variant="contained"
  loading={isSubmitting}
  loadingPosition="start"
  startIcon={<SaveIcon />}
  disabled={!isValid}
  fullWidth
>
  {isSubmitting ? 'Ukládám...' : 'Uložit'}
</LoadingButton>
```

---

## Accessibility & Touch Optimization

### Touch Targets

```typescript
// All interactive elements minimum 44x44px
const buttonSx = {
  minWidth: 44,
  minHeight: 44,
};

// Spacing between adjacent targets
<Stack spacing={1}> {/* 8px minimum spacing */}
  <Button sx={buttonSx}>Action 1</Button>
  <Button sx={buttonSx}>Action 2</Button>
</Stack>
```

### Color Contrast (WCAG 2.1 AA)

All text meets 4.5:1 contrast ratio (MUI default theme is compliant).

### Keyboard Navigation

```typescript
// All interactive elements accessible via keyboard
<Button
  onKeyDown={(e) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      handleAction();
    }
  }}
>
  Action
</Button>

// Tab order follows visual flow
// Use tabIndex={-1} to exclude from tab order
// Use tabIndex={0} to include in natural order
```

### Screen Reader Support

```typescript
// ARIA labels for icon-only buttons
<IconButton aria-label="Smazat záznam">
  <DeleteIcon />
</IconButton>

// ARIA live regions for dynamic content
<div role="status" aria-live="polite" aria-atomic="true">
  {syncStatus && `${syncStatus} záznamů synchronizováno`}
</div>

// Semantic HTML
<main>
  <article>
    <header>
      <Typography variant="h1" component="h1">Dashboard</Typography>
    </header>
    <section aria-labelledby="today-stats">
      <Typography variant="h2" id="today-stats" component="h2">
        Dnes
      </Typography>
      {/* Content */}
    </section>
  </article>
</main>
```

### Focus Management

```typescript
// Trap focus in modals
<Dialog open={open} disableEscapeKeyDown={false}>
  <DialogContent>
    {/* Focus automatically trapped */}
  </DialogContent>
</Dialog>

// Return focus after modal close
const buttonRef = useRef<HTMLButtonElement>(null);

const handleClose = () => {
  setOpen(false);
  // Focus returns to trigger button
  setTimeout(() => buttonRef.current?.focus(), 100);
};
```

### Reduced Motion

```typescript
// Respect user's motion preferences
const prefersReducedMotion = useMediaQuery('(prefers-reduced-motion: reduce)');

<Fade
  in={open}
  timeout={prefersReducedMotion ? 0 : 300}
>
  <Paper>{/* Content */}</Paper>
</Fade>
```

### Touch Gestures

- **Tap**: Primary action (48x48px ripple effect)
- **Long press**: Context menu (500ms threshold)
- **Swipe left**: Quick actions on cards (Phase 2)
- **Pull down**: Refresh lists
- **Pinch**: Zoom charts (Phase 3)

---

## Summary

This UI layout system provides:
- ✅ **Mobile-first** - Optimized for small screens, enhances for large
- ✅ **Accessible** - WCAG 2.1 AA compliant
- ✅ **Touch-optimized** - 44x44px targets, gesture support
- ✅ **PWA-ready** - Offline indicators, install prompts, sync status
- ✅ **Consistent** - Design system with clear patterns
- ✅ **Performance** - Skeleton loaders, optimistic updates

**Design Principles:**
- Simplicity over complexity
- Speed over features (especially for daily records)
- Mobile experience first, desktop second
- Offline capability is not an afterthought
