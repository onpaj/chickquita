#!/usr/bin/env node

import sharp from 'sharp';
import { readFileSync, mkdirSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const publicDir = join(__dirname, '../public');
const iconsDir = join(publicDir, 'icons');
const svgPath = join(publicDir, 'icon-source.svg');

// Icon sizes to generate
const sizes = [
  { size: 72, name: 'icon-72x72.png' },
  { size: 192, name: 'icon-192x192.png' },
  { size: 512, name: 'icon-512x512.png' },
];

async function generateIcons() {
  try {
    // Create icons directory if it doesn't exist
    mkdirSync(iconsDir, { recursive: true });
    console.log('✓ Created icons directory');

    // Read SVG source
    const svgBuffer = readFileSync(svgPath);
    console.log('✓ Read SVG source file');

    // Generate each icon size
    for (const { size, name } of sizes) {
      const outputPath = join(iconsDir, name);

      await sharp(svgBuffer)
        .resize(size, size, {
          fit: 'contain',
          background: { r: 255, g: 107, b: 53, alpha: 1 } // #FF6B35
        })
        .png()
        .toFile(outputPath);

      console.log(`✓ Generated ${name} (${size}x${size}px)`);
    }

    // Generate favicon.ico (32x32 PNG is fine for modern browsers)
    const faviconPath = join(publicDir, 'favicon.png');
    await sharp(svgBuffer)
      .resize(32, 32, {
        fit: 'contain',
        background: { r: 255, g: 107, b: 53, alpha: 1 }
      })
      .png()
      .toFile(faviconPath);

    console.log('✓ Generated favicon.png (32x32px)');

    console.log('\n✅ All icons generated successfully!');
    console.log('\nNext steps:');
    console.log('1. Run: npm run build');
    console.log('2. Test: npx http-server dist -p 8080');
    console.log('3. Check: Chrome DevTools > Application > Manifest');
    console.log('4. Audit: Lighthouse PWA check\n');
  } catch (error) {
    console.error('❌ Error generating icons:', error.message);
    process.exit(1);
  }
}

generateIcons();
