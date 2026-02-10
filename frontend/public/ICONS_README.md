# PWA App Icons Generation

This directory should contain the PWA app icons for the Chickquita application.

## Required Icons

The following icons are needed for proper PWA functionality:

- `icons/icon-72x72.png` - Small devices (72x72 pixels)
- `icons/icon-192x192.png` - Standard Android (192x192 pixels)
- `icons/icon-512x512.png` - High-res, splash screens (512x512 pixels)

## Icon Design

The icons should follow these specifications:
- **Subject**: Chicken silhouette with egg (representing chicken farming)
- **Background**: Solid color #FF6B35 (theme primary color)
- **Format**: PNG with transparency
- **Purpose**: Both "any" and "maskable" (safe zone: 80% of canvas)

## Source File

A source SVG file is provided: `icon-source.svg`

This SVG contains:
- Orange background (#FF6B35) - brand color
- White chicken silhouette
- Egg icon (symbolizing egg production)
- Designed for 512x512px canvas

## Generating Icons

### Option 1: Online Tool (Recommended)

1. Visit https://www.pwabuilder.com/imageGenerator
2. Upload `icon-source.svg`
3. Generate all required sizes
4. Download the icon package
5. Extract PNG files to `frontend/public/icons/` directory

### Option 2: ImageMagick (Command Line)

If you have ImageMagick installed:

```bash
# Navigate to frontend/public directory
cd frontend/public

# Create icons directory
mkdir -p icons

# Generate icons from SVG
convert icon-source.svg -resize 72x72 icons/icon-72x72.png
convert icon-source.svg -resize 192x192 icons/icon-192x192.png
convert icon-source.svg -resize 512x512 icons/icon-512x512.png
```

### Option 3: Inkscape (GUI)

1. Open `icon-source.svg` in Inkscape
2. For each size:
   - File > Export PNG Image
   - Set width and height to target size
   - Export as `icon-{size}.png` in `icons/` directory

### Option 4: Online SVG to PNG Converter

1. Visit https://svgtopng.com/ or similar service
2. Upload `icon-source.svg`
3. Export at 72x72, 192x192, and 512x512 pixels
4. Save files to `icons/` directory

## Verification

After generating icons, verify them:

1. **File sizes**:
   - 72x72: ~2-5 KB
   - 192x192: ~8-15 KB
   - 512x512: ~20-50 KB

2. **Visual quality**:
   - Icons should be sharp and clear
   - Background color should be #FF6B35
   - Chicken and egg should be visible

3. **Maskable compliance**:
   - Important content should be within 80% safe zone
   - Icon should look good when cropped to circle

## Testing Icons

After generating icons, test them:

1. Build the app: `npm run build`
2. Serve the build: `npx http-server dist -p 8080`
3. Open Chrome DevTools > Application > Manifest
4. Verify all icons are listed and display correctly

## Lighthouse PWA Check

Run Lighthouse audit to verify PWA compliance:

```bash
# In Chrome DevTools
1. Open http://localhost:8080 (built app)
2. DevTools > Lighthouse
3. Select "Progressive Web App"
4. Run audit
5. Verify "Installable" criteria are met
```

## Current Status

⚠️ **ICONS NOT YET GENERATED**

The `icons/` directory needs to be created and populated with the three required PNG files before the PWA can be properly installed.

## Next Steps

1. Generate icons using one of the methods above
2. Place icons in `frontend/public/icons/` directory
3. Rebuild the app: `npm run build`
4. Test installation on mobile devices
5. Run Lighthouse PWA audit

---

**Note**: The SVG source file (`icon-source.svg`) is provided as a starting point. Feel free to improve the design or create a custom icon that better represents your brand.
