# Chickquita Component Library Documentation

**Version:** 1.0.0
**Last Updated:** 2026-02-07

## Table of Contents

1. [Overview](#overview)
2. [Theme Configuration](#theme-configuration)
3. [Shared Components](#shared-components)
   - [NumericStepper](#numericstepper)
   - [IllustratedEmptyState](#illustratedemptystate)
   - [StatCard](#statcard)
   - [ConfirmationDialog](#confirmationdialog)
   - [ProtectedRoute](#protectedroute)
4. [Skeleton Components](#skeleton-components)
   - [CoopCardSkeleton](#coopcardskeleton)
   - [FlockCardSkeleton](#flockcardskeleton)
   - [CoopDetailSkeleton](#coopdetailskeleton)
5. [Modal Configuration](#modal-configuration)
6. [Design System Principles](#design-system-principles)
7. [Usage Examples](#usage-examples)

---

## Overview

The Chickquita component library is built on Material-UI (MUI) with Material Design 3 principles, optimized for mobile-first Progressive Web App (PWA) experiences. All components follow accessibility standards (WCAG 2.1 AA) and are designed for offline-first functionality.

**Key Features:**
- Mobile-first responsive design
- Touch-friendly interactions (44x44px minimum touch targets)
- Comprehensive loading states with skeleton components
- i18n support (Czech primary, English secondary)
- Type-safe with TypeScript
- Consistent theming and spacing

---

## Theme Configuration

### Location
- **File:** `frontend/src/theme/theme.ts`
- **Export:** `frontend/src/theme/index.ts`

### Color Palette

```typescript
const theme = createTheme({
  palette: {
    primary: {
      main: '#FF6B35', // Warm Orange - energy and egg yolks
      light: '#FF8C61',
      dark: '#E55A2B',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#4A5568', // Cool Gray
      light: '#718096',
      dark: '#2D3748',
      contrastText: '#FFFFFF',
    },
    background: {
      default: '#F7FAFC',
      paper: '#FFFFFF',
    },
    text: {
      primary: '#1A202C',
      secondary: '#4A5568',
    },
  },
});
```

### Typography Scale

```typescript
typography: {
  fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
  h1: { fontSize: '2rem', fontWeight: 700, lineHeight: 1.2 },      // 32px
  h2: { fontSize: '1.75rem', fontWeight: 700, lineHeight: 1.2 },   // 28px
  h3: { fontSize: '1.5rem', fontWeight: 600, lineHeight: 1.3 },    // 24px
  h4: { fontSize: '1.25rem', fontWeight: 600, lineHeight: 1.4 },   // 20px
  h5: { fontSize: '1.125rem', fontWeight: 600, lineHeight: 1.4 },  // 18px
  h6: { fontSize: '1rem', fontWeight: 600, lineHeight: 1.5 },      // 16px
  body1: { fontSize: '1rem', lineHeight: 1.5 },                    // 16px
  body2: { fontSize: '0.875rem', lineHeight: 1.5 },                // 14px
  button: { fontSize: '0.875rem', fontWeight: 600, textTransform: 'none' },
  caption: { fontSize: '0.75rem', lineHeight: 1.5 },               // 12px
  overline: { fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.08em' },
}
```

### Spacing System

Base unit: **8px**

```typescript
spacing: 8, // Base spacing unit

// Common spacing values:
// spacing(1) = 8px
// spacing(2) = 16px
// spacing(3) = 24px
// spacing(4) = 32px
// spacing(6) = 48px
```

### Breakpoints

```typescript
breakpoints: {
  values: {
    xs: 0,       // Mobile portrait
    sm: 480,     // Mobile landscape
    md: 768,     // Tablet
    lg: 1024,    // Desktop
    xl: 1440,    // Large desktop
  },
}
```

### Border Radius

```typescript
shape: {
  borderRadius: 8, // Default: 8px
}

// Component-specific:
// Cards: 12px
// Dialogs: 16px
// FAB: 16px (56x56px circular-like)
```

### Elevation Levels

25 levels with custom shadow definitions optimized for mobile readability.

```typescript
shadows: [
  'none',
  '0px 1px 3px rgba(0, 0, 0, 0.12)',   // Level 1 - subtle
  '0px 2px 6px rgba(0, 0, 0, 0.16)',   // Level 2 - cards
  '0px 4px 12px rgba(0, 0, 0, 0.15)',  // Level 3 - raised elements
  // ... up to level 24
]
```

### Usage Example

```tsx
import { ThemeProvider } from '@mui/material/styles';
import { theme } from '@/theme';

function App() {
  return (
    <ThemeProvider theme={theme}>
      <YourAppComponents />
    </ThemeProvider>
  );
}
```

### Customizing Component Defaults

Theme includes overrides for:
- **Buttons:** 48px min-height, custom hover states
- **Text Fields:** 48px min-height, 2px focused border
- **FAB:** 56px, custom shadows
- **Cards:** 12px border-radius, elevation 2
- **App Bar:** 64px height
- **Bottom Navigation:** 64px height

---

## Shared Components

### NumericStepper

**File:** `frontend/src/shared/components/NumericStepper.tsx`

#### Description
A mobile-friendly numeric input component with increment (+) and decrement (-) buttons. Designed for touch-first interactions with large, accessible buttons.

#### Props

```typescript
interface NumericStepperProps {
  label: string;                    // Field label
  value: number;                    // Current numeric value
  onChange: (value: number) => void; // Change callback
  min?: number;                     // Minimum value (optional)
  max?: number;                     // Maximum value (optional)
  step?: number;                    // Step increment (default: 1)
  disabled?: boolean;               // Disable state
  error?: boolean;                  // Error state with red styling
  helperText?: string;              // Helper/error message
  'aria-label'?: string;            // Accessibility label (optional)
}
```

#### Features
- **Touch-Friendly:** 48x48px buttons (iOS standard)
- **Input Validation:** Respects min/max constraints
- **Error State:** Red border when `error={true}`
- **Centered Input:** 80px width for numeric display
- **Keyboard Accessible:** Full ARIA label support
- **CSS Fixes:** Removes native number input spinners

#### Usage Example

```tsx
import { NumericStepper } from '@/shared/components';

function FlockCompositionForm() {
  const [hens, setHens] = useState(0);

  return (
    <NumericStepper
      label="Number of Hens"
      value={hens}
      onChange={setHens}
      min={0}
      max={100}
      step={1}
      helperText="Total adult female chickens"
      aria-label="Number of hens"
    />
  );
}
```

#### Validation Example

```tsx
import { NumericStepper } from '@/shared/components';

function ValidatedForm() {
  const [value, setValue] = useState(0);
  const [error, setError] = useState(false);

  const handleChange = (newValue: number) => {
    setValue(newValue);
    setError(newValue < 1);
  };

  return (
    <NumericStepper
      label="Required Field"
      value={value}
      onChange={handleChange}
      min={1}
      error={error}
      helperText={error ? "Value must be at least 1" : ""}
    />
  );
}
```

---

### IllustratedEmptyState

**File:** `frontend/src/shared/components/IllustratedEmptyState.tsx`

#### Description
Displays empty state screens with custom illustrations, title, description, and optional call-to-action button. Used for empty lists, first-run experiences, or no-data scenarios.

#### Props

```typescript
interface IllustratedEmptyStateProps {
  illustration: React.ReactNode;     // SVG/Icon to display
  title: string;                     // Empty state title
  description: string;               // Context message
  actionLabel?: string;              // Button text (optional)
  onAction?: () => void;             // Button click handler (optional)
  actionIcon?: React.ReactNode;      // Icon for action button (optional)
}
```

#### Features
- **Centered Layout:** 300px min-height for visual balance
- **Responsive Icons:** 80px mobile, 120px desktop
- **Large CTA Button:** Prominent call-to-action
- **Max-Width Description:** 400px for readability
- **Flexible Content:** Works with SVGs, icons, or custom illustrations

#### Usage Example

```tsx
import { IllustratedEmptyState } from '@/shared/components';
import { AddCircleOutline } from '@mui/icons-material';
import ChickenIllustration from '@/assets/chicken-empty.svg';

function CoopsList() {
  const coops = useCoops();

  if (coops.length === 0) {
    return (
      <IllustratedEmptyState
        illustration={<img src={ChickenIllustration} alt="" />}
        title="No Coops Yet"
        description="Start tracking your chicken farming profitability by creating your first coop."
        actionLabel="Add First Coop"
        actionIcon={<AddCircleOutline />}
        onAction={() => navigate('/coops/new')}
      />
    );
  }

  return <CoopGrid coops={coops} />;
}
```

#### Icon-Only Example

```tsx
import { IllustratedEmptyState } from '@/shared/components';
import { SearchOff } from '@mui/icons-material';

function SearchResults({ query, results }) {
  if (results.length === 0) {
    return (
      <IllustratedEmptyState
        illustration={<SearchOff sx={{ fontSize: 80, color: 'text.secondary' }} />}
        title="No Results Found"
        description={`No results found for "${query}". Try a different search term.`}
      />
    );
  }

  return <ResultsList results={results} />;
}
```

---

### StatCard

**File:** `frontend/src/shared/components/StatCard.tsx`

#### Description
Displays key statistics on the dashboard with optional trend indicators. Includes built-in loading skeleton state and color theming.

#### Props

```typescript
interface StatCardProps {
  icon: React.ReactNode;             // Icon to display
  label: string;                     // Stat label (uppercase, colored)
  value: string | number;            // Main value (h4 typography)
  trend?: {
    value: number;                   // Trend percentage
    direction: 'up' | 'down' | 'neutral'; // Trend direction
  };
  loading?: boolean;                 // Loading skeleton state
  color?: 'primary' | 'secondary' | 'success' | 'error' | 'warning' | 'info';
}
```

#### Features
- **Colored Icon Box:** 40x40px with theme color background
- **Optional Trend Indicator:** Shows percentage change with arrows
- **Loading Skeleton:** Built-in loading state
- **Hover Effect:** Subtle lift animation (translateY -2px)
- **Color Themes:** Supports all MUI color options
- **Color-Coded Trends:** Green (up), Red (down), Gray (neutral)

#### Usage Example

```tsx
import { StatCard } from '@/shared/components';
import { Egg, TrendingUp } from '@mui/icons-material';

function DashboardStats() {
  const { data: stats, isLoading } = useDashboardStats();

  return (
    <Grid container spacing={2}>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          icon={<Egg />}
          label="Total Eggs"
          value={stats?.totalEggs || 0}
          trend={{
            value: 12.5,
            direction: 'up',
          }}
          color="primary"
          loading={isLoading}
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          icon={<TrendingUp />}
          label="Cost Per Egg"
          value={`${stats?.costPerEgg} Kč`}
          trend={{
            value: 5.2,
            direction: 'down', // Lower cost is good
          }}
          color="success"
          loading={isLoading}
        />
      </Grid>
    </Grid>
  );
}
```

#### Without Trend Example

```tsx
<StatCard
  icon={<Pets />}
  label="Active Flocks"
  value={15}
  color="secondary"
/>
```

---

### ConfirmationDialog

**File:** `frontend/src/shared/components/ConfirmationDialog.tsx`

#### Description
Standardized confirmation dialog for destructive actions (delete, remove, etc.). Includes loading state, custom button styling, and mobile fullscreen support.

#### Props

```typescript
interface ConfirmationDialogProps {
  open: boolean;                     // Dialog visibility
  onClose: () => void;               // Close handler
  onConfirm: () => void;             // Confirm action handler
  title: string;                     // Dialog title
  message: string;                   // Main message (supports bold entity name)
  secondaryMessage?: string;         // Additional context (optional)
  isPending?: boolean;               // Loading state with disabled buttons
  confirmText?: string;              // Confirm button label (default: "Confirm")
  cancelText?: string;               // Cancel button label (default: "Cancel")
  confirmColor?: 'error' | 'primary' | 'secondary' | 'success' | 'warning' | 'info';
  confirmVariant?: 'contained' | 'outlined' | 'text';
  cancelVariant?: 'contained' | 'outlined' | 'text';
}
```

#### Features
- **Mobile Fullscreen:** Fullscreen on `sm` breakpoint
- **MaxWidth:** `xs` for compact confirmations
- **Standard Padding:** Title: 16px, Content: 24px
- **Touch-Friendly Buttons:** 44px min-height
- **Loading Spinner:** Shows in confirm button when `isPending`
- **Prevents Closing:** Disables close while pending

#### Usage Example

```tsx
import { ConfirmationDialog } from '@/shared/components';
import { useState } from 'react';

function CoopCard({ coop }) {
  const [confirmOpen, setConfirmOpen] = useState(false);
  const deleteCoop = useDeleteCoop();

  const handleDelete = async () => {
    await deleteCoop.mutateAsync(coop.id);
    setConfirmOpen(false);
  };

  return (
    <>
      <Button onClick={() => setConfirmOpen(true)}>Delete</Button>

      <ConfirmationDialog
        open={confirmOpen}
        onClose={() => setConfirmOpen(false)}
        onConfirm={handleDelete}
        title="Delete Coop?"
        message={`Are you sure you want to delete "${coop.name}"?`}
        secondaryMessage="This action cannot be undone. All flocks and records will be permanently removed."
        isPending={deleteCoop.isPending}
        confirmText="Delete"
        cancelText="Cancel"
        confirmColor="error"
        confirmVariant="contained"
      />
    </>
  );
}
```

#### Custom Styling Example

```tsx
<ConfirmationDialog
  open={open}
  onClose={handleClose}
  onConfirm={handleArchive}
  title="Archive Flock?"
  message="This flock will be moved to archives."
  confirmText="Archive"
  confirmColor="warning"
  confirmVariant="outlined"
  cancelVariant="text"
/>
```

---

### ProtectedRoute

**File:** `frontend/src/shared/components/ProtectedRoute.tsx`

#### Description
Authentication wrapper component that guards routes based on Clerk authentication state. Redirects unauthenticated users to sign-in.

#### Props

```typescript
interface ProtectedRouteProps {
  children: React.ReactNode;
}
```

#### Usage Example

```tsx
import { ProtectedRoute } from '@/shared/components';
import { Routes, Route } from 'react-router-dom';

function AppRoutes() {
  return (
    <Routes>
      <Route path="/sign-in" element={<SignIn />} />
      <Route path="/sign-up" element={<SignUp />} />

      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        }
      />

      <Route
        path="/coops/*"
        element={
          <ProtectedRoute>
            <CoopsRoutes />
          </ProtectedRoute>
        }
      />
    </Routes>
  );
}
```

---

## Skeleton Components

Skeleton components provide loading states that match the layout of their corresponding data components.

### CoopCardSkeleton

**File:** `frontend/src/shared/components/CoopCardSkeleton.tsx`

#### Description
Loading skeleton that matches the `CoopCard` layout.

#### Features
- Elevation 2 card
- Min-height: 120px
- Includes: Title skeleton, location icon, date

#### Usage Example

```tsx
import { CoopCardSkeleton } from '@/shared/components';

function CoopsList() {
  const { data: coops, isLoading } = useCoops();

  if (isLoading) {
    return (
      <Grid container spacing={2}>
        {[1, 2, 3, 4].map((i) => (
          <Grid item xs={12} sm={6} md={4} key={i}>
            <CoopCardSkeleton />
          </Grid>
        ))}
      </Grid>
    );
  }

  return <CoopGrid coops={coops} />;
}
```

---

### FlockCardSkeleton

**File:** `frontend/src/shared/components/FlockCardSkeleton.tsx`

#### Description
Loading skeleton that matches the `FlockCard` layout.

#### Features
- Stack layout with border
- Includes: Title, chip, composition (hens/roosters/chicks), total
- Border separating total section

#### Usage Example

```tsx
import { FlockCardSkeleton } from '@/shared/components';

function FlocksList() {
  const { data: flocks, isLoading } = useFlocks(coopId);

  if (isLoading) {
    return (
      <>
        <FlockCardSkeleton />
        <FlockCardSkeleton />
        <FlockCardSkeleton />
      </>
    );
  }

  return <FlockCards flocks={flocks} />;
}
```

---

### CoopDetailSkeleton

**File:** `frontend/src/shared/components/CoopDetailSkeleton.tsx`

#### Description
Full page loading skeleton for detail/form pages.

#### Features
- Container maxWidth: `sm`
- Includes: Back button, header, form fields
- Multiple field groups with spacing

#### Usage Example

```tsx
import { CoopDetailSkeleton } from '@/shared/components';

function CoopDetail({ id }) {
  const { data: coop, isLoading } = useCoop(id);

  if (isLoading) {
    return <CoopDetailSkeleton />;
  }

  return <CoopDetailForm coop={coop} />;
}
```

---

## Modal Configuration

**File:** `frontend/src/shared/constants/modalConfig.ts`

### Constants

```typescript
export const DIALOG_CONFIG = {
  maxWidth: 'sm' as const,  // For form modals
};

export const CONFIRMATION_DIALOG_CONFIG = {
  maxWidth: 'xs' as const,  // For confirmations
};

export const MOBILE_BREAKPOINT = 480;
export const DIALOG_TITLE_PADDING = 2;      // 16px
export const DIALOG_CONTENT_PADDING = 3;    // 24px
export const FORM_FIELD_SPACING = 2;        // 16px
export const MIN_TOUCH_TARGET = '44px';     // iOS standard
```

### Style Props

```typescript
export const dialogTitleSx = {
  pb: DIALOG_TITLE_PADDING,
};

export const dialogContentSx = {
  pt: DIALOG_CONTENT_PADDING,
  pb: DIALOG_CONTENT_PADDING,
};

export const dialogActionsSx = {
  position: 'sticky',
  bottom: 0,
  backgroundColor: 'background.paper',
  borderTop: '1px solid',
  borderColor: 'divider',
  p: DIALOG_CONTENT_PADDING,
};

export const dialogActionsSimpleSx = {
  p: DIALOG_CONTENT_PADDING,
};

export const touchButtonSx = {
  minHeight: MIN_TOUCH_TARGET,
};

export const numberStepperButtonSx = {
  minWidth: MIN_TOUCH_TARGET,
  minHeight: MIN_TOUCH_TARGET,
};
```

### Usage Example

```tsx
import {
  DIALOG_CONFIG,
  dialogTitleSx,
  dialogContentSx,
  dialogActionsSx
} from '@/shared/constants/modalConfig';

function MyFormDialog() {
  return (
    <Dialog open={open} onClose={onClose} {...DIALOG_CONFIG}>
      <DialogTitle sx={dialogTitleSx}>Add New Coop</DialogTitle>
      <DialogContent sx={dialogContentSx}>
        <FormFields />
      </DialogContent>
      <DialogActions sx={dialogActionsSx}>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={onSubmit}>Save</Button>
      </DialogActions>
    </Dialog>
  );
}
```

---

## Design System Principles

### 1. Mobile-First
All components are designed and optimized for mobile devices first, with progressive enhancement for larger screens.

**Example:**
```tsx
<Box
  sx={{
    padding: 2,        // 16px on mobile
    md: { padding: 3 } // 24px on tablet+
  }}
>
```

### 2. Touch-Friendly
Minimum touch target size: **44x44px** (iOS standard) or **48x48px** (Material Design).

**Example:**
```tsx
<IconButton
  sx={{
    minWidth: 44,
    minHeight: 44
  }}
>
  <DeleteIcon />
</IconButton>
```

### 3. Consistent Spacing
All spacing uses the 8px base unit via `theme.spacing()`.

**Example:**
```tsx
<Stack spacing={2}>  {/* 16px gaps */}
  <TextField />
  <TextField />
</Stack>
```

### 4. Accessibility (WCAG 2.1 AA)
- Color contrast ratio ≥ 4.5:1 for text
- Focus indicators on all interactive elements
- ARIA labels on all icon buttons
- Keyboard navigation support
- Screen reader friendly

**Example:**
```tsx
<IconButton aria-label="Delete coop">
  <DeleteIcon />
</IconButton>
```

### 5. Loading States
All data-driven components include loading skeleton states.

**Example:**
```tsx
function CoopsList() {
  if (isLoading) return <CoopCardSkeleton />;
  return <CoopCards />;
}
```

### 6. Offline-First
Components handle offline scenarios gracefully with queued actions and sync indicators.

### 7. i18n Ready
All UI text uses translation keys via `react-i18next`.

**Example:**
```tsx
const { t } = useTranslation();
return <Typography>{t('coops.list.title')}</Typography>;
```

---

## Usage Examples

### Complete Form with All Components

```tsx
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  NumericStepper,
  IllustratedEmptyState,
  StatCard,
  ConfirmationDialog,
  FlockCardSkeleton,
} from '@/shared/components';
import { Button, Stack, Grid } from '@mui/material';
import { Egg, Pets } from '@mui/icons-material';

function FlockManagement({ coopId }) {
  const { t } = useTranslation();
  const { data: flocks, isLoading } = useFlocks(coopId);
  const [hens, setHens] = useState(0);
  const [roosters, setRoosters] = useState(0);
  const [chicks, setChicks] = useState(0);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const deleteFlock = useDeleteFlock();

  // Loading state
  if (isLoading) {
    return (
      <>
        <FlockCardSkeleton />
        <FlockCardSkeleton />
      </>
    );
  }

  // Empty state
  if (flocks.length === 0) {
    return (
      <IllustratedEmptyState
        illustration={<Pets sx={{ fontSize: 120 }} />}
        title={t('flocks.empty.title')}
        description={t('flocks.empty.description')}
        actionLabel={t('flocks.empty.action')}
        onAction={() => navigate('/flocks/new')}
      />
    );
  }

  return (
    <>
      {/* Dashboard Stats */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6}>
          <StatCard
            icon={<Egg />}
            label="Total Eggs Today"
            value={flocks.reduce((sum, f) => sum + f.eggsToday, 0)}
            color="primary"
          />
        </Grid>
        <Grid item xs={12} sm={6}>
          <StatCard
            icon={<Pets />}
            label="Total Chickens"
            value={flocks.reduce((sum, f) => sum + f.totalChickens, 0)}
            color="secondary"
          />
        </Grid>
      </Grid>

      {/* Add Flock Form */}
      <Stack spacing={2}>
        <NumericStepper
          label={t('flocks.form.hens')}
          value={hens}
          onChange={setHens}
          min={0}
          aria-label="Number of hens"
        />
        <NumericStepper
          label={t('flocks.form.roosters')}
          value={roosters}
          onChange={setRoosters}
          min={0}
          aria-label="Number of roosters"
        />
        <NumericStepper
          label={t('flocks.form.chicks')}
          value={chicks}
          onChange={setChicks}
          min={0}
          aria-label="Number of chicks"
        />
        <Button variant="contained" onClick={handleSubmit}>
          {t('flocks.form.submit')}
        </Button>
      </Stack>

      {/* Delete Confirmation */}
      <ConfirmationDialog
        open={deleteOpen}
        onClose={() => setDeleteOpen(false)}
        onConfirm={() => deleteFlock.mutate(selectedFlockId)}
        title={t('flocks.delete.title')}
        message={t('flocks.delete.message')}
        isPending={deleteFlock.isPending}
        confirmText={t('common.delete')}
        confirmColor="error"
      />
    </>
  );
}
```

### Responsive Dashboard Layout

```tsx
import { Grid, Container } from '@mui/material';
import { StatCard } from '@/shared/components';
import { Egg, AttachMoney, TrendingUp, Pets } from '@mui/icons-material';

function Dashboard() {
  const { data, isLoading } = useDashboardStats();

  return (
    <Container maxWidth="lg" sx={{ py: 3 }}>
      <Grid container spacing={2}>
        {/* Mobile: 1 column, Tablet: 2 columns, Desktop: 4 columns */}
        <Grid item xs={12} sm={6} lg={3}>
          <StatCard
            icon={<Egg />}
            label="Total Eggs"
            value={data?.totalEggs || 0}
            trend={{ value: 12.5, direction: 'up' }}
            color="primary"
            loading={isLoading}
          />
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <StatCard
            icon={<AttachMoney />}
            label="Cost Per Egg"
            value={`${data?.costPerEgg || 0} Kč`}
            trend={{ value: 5.2, direction: 'down' }}
            color="success"
            loading={isLoading}
          />
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <StatCard
            icon={<TrendingUp />}
            label="Monthly Revenue"
            value={`${data?.revenue || 0} Kč`}
            trend={{ value: 8.3, direction: 'up' }}
            color="info"
            loading={isLoading}
          />
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <StatCard
            icon={<Pets />}
            label="Active Flocks"
            value={data?.activeFlocks || 0}
            color="secondary"
            loading={isLoading}
          />
        </Grid>
      </Grid>
    </Container>
  );
}
```

---

## Additional Resources

- **Main Documentation:** `/docs/ui-layout-system.md`
- **Coding Standards:** `/docs/coding-standards.md`
- **Theme File:** `frontend/src/theme/theme.ts`
- **Modal Config:** `frontend/src/shared/constants/modalConfig.ts`

### Storybook Integration (Optional)

Storybook is not currently set up for this project but can be added for interactive component documentation. If you decide to implement Storybook:

1. Install Storybook for React + Vite:
```bash
npx storybook@latest init
```

2. Create stories for each component in the same directory:
```
shared/components/
├── NumericStepper.tsx
├── NumericStepper.stories.tsx
├── StatCard.tsx
└── StatCard.stories.tsx
```

3. Example story structure:
```tsx
import type { Meta, StoryObj } from '@storybook/react';
import { NumericStepper } from './NumericStepper';

const meta = {
  title: 'Shared/NumericStepper',
  component: NumericStepper,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
} satisfies Meta<typeof NumericStepper>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    label: 'Number of Hens',
    value: 10,
    onChange: (value) => console.log(value),
  },
};

export const WithError: Story = {
  args: {
    label: 'Required Field',
    value: 0,
    error: true,
    helperText: 'This field is required',
    onChange: (value) => console.log(value),
  },
};
```

4. Run Storybook:
```bash
npm run storybook
```

For now, this markdown documentation serves as the primary component reference.

---

## Component Checklist

When creating new components, ensure:

- [ ] Mobile-first responsive design
- [ ] Touch targets ≥ 44x44px
- [ ] Loading skeleton state
- [ ] Error state handling
- [ ] i18n translation keys (no hardcoded Czech text)
- [ ] TypeScript strict mode types
- [ ] ARIA labels on interactive elements
- [ ] Keyboard navigation support
- [ ] Color contrast ≥ 4.5:1
- [ ] Uses theme spacing/colors
- [ ] Documented in this file

---

**Last Updated:** 2026-02-07
**Maintainers:** Chickquita Development Team
