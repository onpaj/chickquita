# UI Consistency Fix — Coops & Settings Pages

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make CoopsPage and SettingsPage visually consistent with the DashboardPage (reference design).

**Architecture:** Three targeted fixes — theme AppBar border-radius, CoopCard border-radius, and SettingsPage card layout pattern.

**Tech Stack:** React 18, MUI v5, TypeScript, Vite

---

## Root Cause Analysis

Comparing screenshots (Dashboard = OK reference, Coops/Settings = broken):

| Issue | Root Cause | File |
|-------|-----------|------|
| AppBar looks like a floating card | `MuiPaper.root: { borderRadius: 12 }` in theme applies to AppBar (which extends Paper). Rounded corners + light-gray page background (`#F7FAFC`) = card-like appearance | `frontend/src/theme/theme.ts:356-370` |
| CoopCard has rounder corners than Dashboard cards | `Card sx={{ borderRadius: 2 }}` = 16px (MUI 8px × 2) overrides the theme's 12px | `frontend/src/features/coops/components/CoopCard.tsx:122` |
| Settings page cards look like iOS settings lists | Uses `Paper elevation={2}` + `overline` labels + `List`/`ListItem` instead of the `Card` pattern used on Dashboard | `frontend/src/pages/SettingsPage.tsx:46-123` |

---

## Task 1: Fix AppBar border radius in theme

**Files:**
- Modify: `frontend/src/theme/theme.ts:392-398`

**What to do:**
Add `borderRadius: 0` to the `MuiAppBar` styleOverrides so the global `MuiPaper.root.borderRadius` does not make the AppBar appear rounded.

**Current code (lines 392-398):**
```ts
MuiAppBar: {
  styleOverrides: {
    root: {
      boxShadow: '0px 1px 3px rgba(0, 0, 0, 0.12)',
    },
  },
},
```

**Target code:**
```ts
MuiAppBar: {
  styleOverrides: {
    root: {
      borderRadius: 0,
      boxShadow: '0px 1px 3px rgba(0, 0, 0, 0.12)',
    },
  },
},
```

**How to verify:**
1. Run `npm run dev` in `frontend/`
2. Navigate to `/coops` — the "Chickquita" header should now span full width edge-to-edge (no rounded card shape, no gray gap above it)
3. Navigate to `/settings` — same check
4. Navigate to `/dashboard` — AppBar should look unchanged

**Commit:**
```bash
git add frontend/src/theme/theme.ts
git commit -m "fix: remove AppBar border radius from global Paper theme override"
```

---

## Task 2: Fix CoopCard border radius

**Files:**
- Modify: `frontend/src/features/coops/components/CoopCard.tsx:120-134`

**What to do:**
Remove `borderRadius: 2` from the Card's `sx` prop. The theme already sets `MuiCard.root.borderRadius: 12`. The explicit `borderRadius: 2` (= 16px in MUI spacing) overrides this and makes coop cards more rounded than dashboard cards.

**Current code (lines 120-134):**
```tsx
<Card
  data-testid="coop-card"
  elevation={1}
  sx={{
    position: 'relative',
    cursor: 'pointer',
    borderRadius: 2,
    transition: 'box-shadow 0.3s ease',
    '&:hover': {
      boxShadow: 4,
    },
  }}
```

**Target code:**
```tsx
<Card
  data-testid="coop-card"
  elevation={1}
  sx={{
    position: 'relative',
    cursor: 'pointer',
    transition: 'box-shadow 0.3s ease',
    '&:hover': {
      boxShadow: 4,
    },
  }}
```

**How to verify:**
1. Navigate to `/coops` — the coop card should have 12px border radius (matching Dashboard cards)
2. Visually compare with a Dashboard stat card — corner rounding should look the same

**Commit:**
```bash
git add frontend/src/features/coops/components/CoopCard.tsx
git commit -m "fix: remove explicit borderRadius from CoopCard, use theme default"
```

---

## Task 3: Redesign SettingsPage cards to match Dashboard card pattern

**Files:**
- Modify: `frontend/src/pages/SettingsPage.tsx`

**What to do:**
Replace the `Paper` + `overline` + `List`/`ListItem` layout with `Card` + `CardContent` to match the Dashboard card visual language. Keep all functionality (language selector, sign-out confirmation).

**Current structure:**
- `Paper elevation={2}` with `overline` section label on top, `List`/`ListItem` inside
- Two separate Paper blocks for Language and Profile sections

**Target structure:**
- `Card` with a section header `Typography variant="subtitle2"` and `CardContent` for each section
- Language section: globe icon + language `Select` inside `CardContent`
- Profile section: sign-out `ListItem`-style row inside `CardContent`

**Target code for SettingsPage.tsx:**
```tsx
import { useState } from 'react';
import {
  Container,
  Card,
  CardContent,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import LanguageIcon from '@mui/icons-material/Language';
import LogoutIcon from '@mui/icons-material/Logout';
import { useTranslation } from 'react-i18next';
import { useClerk } from '@clerk/clerk-react';
import { ConfirmationDialog } from '@/shared/components';

export function SettingsPage() {
  const { t, i18n } = useTranslation();
  const { signOut } = useClerk();
  const [signOutDialogOpen, setSignOutDialogOpen] = useState(false);
  const [isSigningOut, setIsSigningOut] = useState(false);

  const handleLanguageChange = (event: SelectChangeEvent) => {
    i18n.changeLanguage(event.target.value);
  };

  const handleSignOutConfirm = async () => {
    setIsSigningOut(true);
    await signOut();
  };

  return (
    <Container maxWidth="md" sx={{ py: 3, pb: 10 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {t('settings.title')}
      </Typography>

      {/* Language section */}
      <Card elevation={1} sx={{ mt: 3 }}>
        <CardContent>
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2 }}>
            {t('settings.language')}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <LanguageIcon color="action" />
            <FormControl fullWidth size="small">
              <InputLabel id="language-select-label">
                {t('settings.language')}
              </InputLabel>
              <Select
                labelId="language-select-label"
                id="language-select"
                value={i18n.language}
                label={t('settings.language')}
                onChange={handleLanguageChange}
              >
                <MenuItem value="cs">Čeština</MenuItem>
                <MenuItem value="en">English</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </CardContent>
      </Card>

      {/* Profile / sign-out section */}
      <Card elevation={1} sx={{ mt: 2 }}>
        <CardContent sx={{ p: 0, '&:last-child': { pb: 0 } }}>
          <Typography
            variant="subtitle2"
            color="text.secondary"
            sx={{ px: 2, pt: 2, pb: 1 }}
          >
            {t('settings.profile')}
          </Typography>
          <ListItem
            component="button"
            onClick={() => setSignOutDialogOpen(true)}
            sx={{
              py: 1.5,
              minHeight: 56,
              width: '100%',
              border: 'none',
              bgcolor: 'transparent',
              cursor: 'pointer',
              textAlign: 'left',
              color: 'error.main',
              '&:hover': {
                bgcolor: 'error.light',
                opacity: 0.9,
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 44, color: 'error.main' }}>
              <LogoutIcon />
            </ListItemIcon>
            <ListItemText
              primary={
                <Typography variant="body1" color="error">
                  {t('settings.signOut')}
                </Typography>
              }
            />
          </ListItem>
        </CardContent>
      </Card>

      <ConfirmationDialog
        open={signOutDialogOpen}
        onClose={() => setSignOutDialogOpen(false)}
        onConfirm={handleSignOutConfirm}
        title={t('settings.signOutConfirmTitle')}
        message={t('settings.signOutConfirmMessage')}
        confirmText={t('settings.signOutConfirmButton')}
        confirmColor="error"
        isPending={isSigningOut}
      />
    </Container>
  );
}
```

**How to verify:**
1. Navigate to `/settings`
2. Language card: uses Card with rounded corners + subtle shadow, `subtitle2` label (not uppercase overline), globe icon inline with the select
3. Profile card: uses same Card style, sign-out button takes full width
4. Both cards visually match the style of Dashboard's stat cards

**Commit:**
```bash
git add frontend/src/pages/SettingsPage.tsx
git commit -m "fix: redesign SettingsPage cards to match Dashboard Card pattern"
```

---

## Verification Checklist

After all three tasks are done:

- [ ] Navigate to `/dashboard` — AppBar looks identical to before (full-width, no visual change)
- [ ] Navigate to `/coops` — AppBar is full-width edge-to-edge (no gray gap above it, no rounded card shape)
- [ ] Navigate to `/coops` — CoopCard corners match Dashboard card corners visually
- [ ] Navigate to `/settings` — AppBar is full-width edge-to-edge
- [ ] Navigate to `/settings` — Language and Profile sections use Card components, not iOS-style lists
- [ ] Run `npm run type-check` — no TypeScript errors
- [ ] Run `npm run lint` — no lint errors
