# Empty State Illustrations

This directory contains custom SVG illustrations for empty states throughout the Chickquita application.

## Available Illustrations

### 1. EmptyCoopsIllustration
**File**: `EmptyCoops.svg`
**Usage**: Empty state for the Coops page
**Theme**: Chicken coop with fence and hay bales
**Size**: ~2.1 KB

Displays a charming chicken coop building with a red roof, door, window, fence posts, and decorative hay bales.

### 2. EmptyFlocksIllustration
**File**: `EmptyFlocks.svg`
**Usage**: Empty state for the Flocks page
**Theme**: Chickens, chick, and scattered feed
**Size**: ~4.2 KB

Features two chickens (white and beige), a small fluffy chick, an egg, and scattered feed on the ground.

### 3. EmptyDashboardIllustration
**File**: `EmptyDashboard.svg`
**Usage**: Empty state for the Dashboard page
**Theme**: Complete farm scene
**Size**: ~4.1 KB

Shows a complete farm landscape with a red barn, windmill, sun, clouds, fence, tree, and rolling hills.

## Design System Integration

All illustrations follow the Chickquita design system:

### Color Palette
- **Primary Orange**: `#FF6B35` (warm orange - brand color)
- **Accent Orange**: `#FFA94D`, `#FFB86C` (lighter orange accents)
- **Secondary Gray**: `#4A5568`, `#718096`, `#A0AEC0` (cool grays)
- **Background**: `#F7FAFC` (light background)
- **Accent Colors**:
  - Sky/Windows: `#63B3ED` (light blue)
  - Nature: `#68D391`, `#48BB78` (greens)
  - Red Accents: `#E55420`, `#E53E3E` (reds)

### Responsive Design
All illustrations are designed at 200×200px and scale responsively:
- **Mobile (xs)**: 120×120px
- **Desktop (sm+)**: 200×200px

## Usage

### Import Components
```tsx
import {
  EmptyCoopsIllustration,
  EmptyFlocksIllustration,
  EmptyDashboardIllustration
} from '@/assets/illustrations';
```

### Basic Usage
```tsx
<EmptyCoopsIllustration />
```

### With Custom Styling
```tsx
<EmptyCoopsIllustration
  sx={{ width: 250, height: 250 }}
  aria-label="Custom description"
/>
```

### In Empty State Components
```tsx
import { IllustratedEmptyState } from '@/shared/components/IllustratedEmptyState';
import { EmptyFlocksIllustration } from '@/assets/illustrations';

<IllustratedEmptyState
  illustration={<EmptyFlocksIllustration />}
  title="No flocks yet"
  description="Create your first flock to get started"
  actionLabel="Add Flock"
  onAction={handleAddFlock}
/>
```

## Accessibility

All illustrations include:
- **`role="img"`**: Identifies the element as an image
- **`aria-label`**: Provides descriptive text for screen readers
- **Customizable labels**: Can override default aria-label via props

Default ARIA labels:
- EmptyCoopsIllustration: "Empty chicken coop illustration"
- EmptyFlocksIllustration: "Empty flocks illustration with chickens"
- EmptyDashboardIllustration: "Empty dashboard farm scene illustration"

## Testing

To test illustrations in components, use mocks:

```tsx
// In your test file
vi.mock('@/assets/illustrations', () => ({
  EmptyFlocksIllustration: ({ 'aria-label': ariaLabel }: { 'aria-label'?: string }) => (
    <svg data-testid="empty-flocks-illustration" aria-label={ariaLabel} role="img">
      <title>Empty Flocks</title>
    </svg>
  ),
}));
```

## Technical Details

### SVG Import Configuration
SVGs are imported as React components using `vite-plugin-svgr`:

```tsx
import EmptyCoopsSvg from './EmptyCoops.svg?react';
```

This is configured in `vite.config.ts`:
```ts
import svgr from 'vite-plugin-svgr';

export default defineConfig({
  plugins: [
    svgr({
      svgrOptions: {
        icon: true,
      },
    }),
  ],
});
```

### TypeScript Support
SVG imports are typed via `vite-env.d.ts`:
```ts
/// <reference types="vite-plugin-svgr/client" />
```

## Optimization

All SVGs are optimized for web:
- ✅ File sizes < 10 KB (requirement met)
- ✅ Inline SVG (no HTTP requests)
- ✅ Gzip-friendly XML structure
- ✅ No external dependencies
- ✅ Pure vector graphics (scales perfectly)

## Maintenance

### Modifying Illustrations
1. Edit the `.svg` files directly
2. Maintain the color palette from the design system
3. Keep file sizes under 10 KB
4. Test across different screen sizes
5. Verify accessibility attributes remain intact

### Adding New Illustrations
1. Create SVG file in this directory
2. Follow naming convention: `Empty[Feature].svg`
3. Add export in `index.tsx`
4. Update this README
5. Add corresponding tests

## Browser Support

Works in all modern browsers that support:
- SVG 1.1
- ES6+ (via Vite transpilation)
- React 18+

Tested on:
- Chrome/Edge (latest)
- Firefox (latest)
- Safari (15+)
- Mobile browsers (iOS Safari, Chrome Mobile)
