import * as fs from 'fs';
import * as path from 'path';

/**
 * TESTING-002: Screenshot Reporter Utility
 *
 * Provides utilities for capturing, organizing, and reporting screenshots
 * across different browsers, devices, and breakpoints.
 *
 * Features:
 * - Organized directory structure by browser/device
 * - Comparison report generation
 * - HTML report with visual diff
 * - JSON metadata for CI integration
 */

export interface ScreenshotMetadata {
  name: string;
  browser: string;
  device?: string;
  viewport: { width: number; height: number };
  breakpoint?: string;
  timestamp: string;
  path: string;
  testFile: string;
  testTitle: string;
}

export interface ScreenshotReport {
  generatedAt: string;
  totalScreenshots: number;
  byBrowser: Record<string, number>;
  byDevice: Record<string, number>;
  byBreakpoint: Record<string, number>;
  screenshots: ScreenshotMetadata[];
  failures: {
    screenshot: string;
    error: string;
  }[];
}

/**
 * Directory structure for organized screenshot storage
 */
export const SCREENSHOT_DIRS = {
  baseline: 'screenshots/baseline',
  current: 'screenshots/current',
  diff: 'screenshots/diff',
  reports: 'screenshots/reports',
};

/**
 * Breakpoint definitions with human-readable names
 */
export const BREAKPOINT_LABELS: Record<string, string> = {
  '320': 'Mobile XS (iPhone SE)',
  '375': 'Mobile SM (iPhone SE 3rd)',
  '390': 'Mobile MD (iPhone 14)',
  '412': 'Mobile Android (Galaxy A52)',
  '480': 'Mobile Landscape',
  '768': 'Tablet (iPad)',
  '1024': 'Desktop Small',
  '1920': 'Desktop Full HD',
};

/**
 * Get breakpoint label from viewport width
 */
export function getBreakpointLabel(width: number): string {
  const widthStr = String(width);
  return BREAKPOINT_LABELS[widthStr] || `${width}px`;
}

/**
 * Generate screenshot filename with metadata
 */
export function generateScreenshotName(
  testName: string,
  browser: string,
  viewport: { width: number; height: number },
  device?: string
): string {
  const sanitizedTestName = testName
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '');

  const deviceStr = device ? `-${device.toLowerCase().replace(/\s+/g, '-')}` : '';
  const viewportStr = `${viewport.width}x${viewport.height}`;

  return `${sanitizedTestName}-${browser}${deviceStr}-${viewportStr}.png`;
}

/**
 * Ensure directory exists
 */
export function ensureDir(dirPath: string): void {
  if (!fs.existsSync(dirPath)) {
    fs.mkdirSync(dirPath, { recursive: true });
  }
}

/**
 * Initialize screenshot directories
 */
export function initScreenshotDirs(baseDir: string): void {
  Object.values(SCREENSHOT_DIRS).forEach((dir) => {
    ensureDir(path.join(baseDir, dir));
  });
}

/**
 * Save screenshot metadata to JSON
 */
export function saveScreenshotMetadata(
  baseDir: string,
  metadata: ScreenshotMetadata
): void {
  const metadataDir = path.join(baseDir, SCREENSHOT_DIRS.reports, 'metadata');
  ensureDir(metadataDir);

  const metadataFile = path.join(
    metadataDir,
    `${path.basename(metadata.path, '.png')}.json`
  );

  fs.writeFileSync(metadataFile, JSON.stringify(metadata, null, 2));
}

/**
 * Generate comprehensive screenshot report
 */
export function generateScreenshotReport(baseDir: string): ScreenshotReport {
  const metadataDir = path.join(baseDir, SCREENSHOT_DIRS.reports, 'metadata');
  const screenshots: ScreenshotMetadata[] = [];

  if (fs.existsSync(metadataDir)) {
    const files = fs.readdirSync(metadataDir).filter((f) => f.endsWith('.json'));

    for (const file of files) {
      const content = fs.readFileSync(path.join(metadataDir, file), 'utf-8');
      screenshots.push(JSON.parse(content));
    }
  }

  // Aggregate by browser
  const byBrowser: Record<string, number> = {};
  const byDevice: Record<string, number> = {};
  const byBreakpoint: Record<string, number> = {};

  for (const ss of screenshots) {
    byBrowser[ss.browser] = (byBrowser[ss.browser] || 0) + 1;

    if (ss.device) {
      byDevice[ss.device] = (byDevice[ss.device] || 0) + 1;
    }

    if (ss.breakpoint) {
      byBreakpoint[ss.breakpoint] = (byBreakpoint[ss.breakpoint] || 0) + 1;
    }
  }

  const report: ScreenshotReport = {
    generatedAt: new Date().toISOString(),
    totalScreenshots: screenshots.length,
    byBrowser,
    byDevice,
    byBreakpoint,
    screenshots,
    failures: [],
  };

  // Save report
  const reportPath = path.join(baseDir, SCREENSHOT_DIRS.reports, 'report.json');
  fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));

  return report;
}

/**
 * Generate HTML report for visual comparison
 */
export function generateHtmlReport(baseDir: string): string {
  const report = generateScreenshotReport(baseDir);

  const html = `
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>TESTING-002 Cross-Browser Screenshot Report</title>
  <style>
    :root {
      --primary: #FF6B35;
      --bg: #fafafa;
      --card-bg: #ffffff;
      --text: #333333;
      --text-secondary: #666666;
      --border: #e0e0e0;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      background: var(--bg);
      color: var(--text);
      line-height: 1.6;
    }
    .header {
      background: var(--primary);
      color: white;
      padding: 24px;
      text-align: center;
    }
    .header h1 { font-size: 24px; margin-bottom: 8px; }
    .header p { opacity: 0.9; }
    .container { max-width: 1400px; margin: 0 auto; padding: 24px; }
    .summary {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 32px;
    }
    .summary-card {
      background: var(--card-bg);
      border-radius: 8px;
      padding: 20px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .summary-card h3 { color: var(--text-secondary); font-size: 14px; margin-bottom: 8px; }
    .summary-card .value { font-size: 32px; font-weight: bold; color: var(--primary); }
    .section { margin-bottom: 32px; }
    .section h2 { margin-bottom: 16px; padding-bottom: 8px; border-bottom: 2px solid var(--primary); }
    .breakdown {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }
    .breakdown-item {
      background: var(--card-bg);
      padding: 8px 16px;
      border-radius: 20px;
      border: 1px solid var(--border);
    }
    .breakdown-item .label { font-weight: 500; }
    .breakdown-item .count { color: var(--primary); margin-left: 8px; }
    .screenshots-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }
    .screenshot-card {
      background: var(--card-bg);
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .screenshot-card img {
      width: 100%;
      height: 200px;
      object-fit: cover;
      object-position: top;
      border-bottom: 1px solid var(--border);
    }
    .screenshot-card .info { padding: 16px; }
    .screenshot-card .title { font-weight: 600; margin-bottom: 8px; }
    .screenshot-card .meta { font-size: 12px; color: var(--text-secondary); }
    .screenshot-card .meta span { margin-right: 12px; }
    .filters {
      display: flex;
      gap: 16px;
      margin-bottom: 24px;
      flex-wrap: wrap;
    }
    .filter-group label { display: block; font-size: 12px; color: var(--text-secondary); margin-bottom: 4px; }
    .filter-group select {
      padding: 8px 12px;
      border: 1px solid var(--border);
      border-radius: 4px;
      min-width: 150px;
    }
    .timestamp { text-align: center; color: var(--text-secondary); font-size: 12px; margin-top: 24px; }
  </style>
</head>
<body>
  <div class="header">
    <h1>Cross-Browser & Device Testing Report</h1>
    <p>TESTING-002 Visual Regression Results</p>
  </div>

  <div class="container">
    <div class="summary">
      <div class="summary-card">
        <h3>Total Screenshots</h3>
        <div class="value">${report.totalScreenshots}</div>
      </div>
      <div class="summary-card">
        <h3>Browsers Tested</h3>
        <div class="value">${Object.keys(report.byBrowser).length}</div>
      </div>
      <div class="summary-card">
        <h3>Devices Tested</h3>
        <div class="value">${Object.keys(report.byDevice).length}</div>
      </div>
      <div class="summary-card">
        <h3>Breakpoints</h3>
        <div class="value">${Object.keys(report.byBreakpoint).length}</div>
      </div>
    </div>

    <div class="section">
      <h2>By Browser</h2>
      <div class="breakdown">
        ${Object.entries(report.byBrowser)
          .map(
            ([browser, count]) =>
              `<div class="breakdown-item"><span class="label">${browser}</span><span class="count">${count}</span></div>`
          )
          .join('')}
      </div>
    </div>

    <div class="section">
      <h2>By Device</h2>
      <div class="breakdown">
        ${Object.entries(report.byDevice)
          .map(
            ([device, count]) =>
              `<div class="breakdown-item"><span class="label">${device}</span><span class="count">${count}</span></div>`
          )
          .join('')}
      </div>
    </div>

    <div class="section">
      <h2>By Breakpoint</h2>
      <div class="breakdown">
        ${Object.entries(report.byBreakpoint)
          .map(
            ([bp, count]) =>
              `<div class="breakdown-item"><span class="label">${bp}</span><span class="count">${count}</span></div>`
          )
          .join('')}
      </div>
    </div>

    <div class="section">
      <h2>Screenshots</h2>
      <div class="filters">
        <div class="filter-group">
          <label>Browser</label>
          <select id="filter-browser" onchange="filterScreenshots()">
            <option value="">All Browsers</option>
            ${Object.keys(report.byBrowser)
              .map((b) => `<option value="${b}">${b}</option>`)
              .join('')}
          </select>
        </div>
        <div class="filter-group">
          <label>Device</label>
          <select id="filter-device" onchange="filterScreenshots()">
            <option value="">All Devices</option>
            ${Object.keys(report.byDevice)
              .map((d) => `<option value="${d}">${d}</option>`)
              .join('')}
          </select>
        </div>
      </div>
      <div class="screenshots-grid" id="screenshots-grid">
        ${report.screenshots
          .map(
            (ss) => `
          <div class="screenshot-card" data-browser="${ss.browser}" data-device="${ss.device || ''}">
            <img src="../current/${ss.name}" alt="${ss.name}" onerror="this.style.display='none'">
            <div class="info">
              <div class="title">${ss.testTitle}</div>
              <div class="meta">
                <span>Browser: ${ss.browser}</span>
                ${ss.device ? `<span>Device: ${ss.device}</span>` : ''}
                <span>Viewport: ${ss.viewport.width}x${ss.viewport.height}</span>
              </div>
            </div>
          </div>
        `
          )
          .join('')}
      </div>
    </div>

    <div class="timestamp">
      Report generated: ${report.generatedAt}
    </div>
  </div>

  <script>
    function filterScreenshots() {
      const browser = document.getElementById('filter-browser').value;
      const device = document.getElementById('filter-device').value;
      const cards = document.querySelectorAll('.screenshot-card');

      cards.forEach(card => {
        const matchBrowser = !browser || card.dataset.browser === browser;
        const matchDevice = !device || card.dataset.device === device;
        card.style.display = matchBrowser && matchDevice ? 'block' : 'none';
      });
    }
  </script>
</body>
</html>
`;

  const reportPath = path.join(baseDir, SCREENSHOT_DIRS.reports, 'report.html');
  fs.writeFileSync(reportPath, html);

  return reportPath;
}

/**
 * Export test results summary for CI
 */
export function exportCISummary(baseDir: string): string {
  const report = generateScreenshotReport(baseDir);

  const summary = {
    status: report.failures.length === 0 ? 'passed' : 'failed',
    totalTests: report.totalScreenshots,
    passed: report.totalScreenshots - report.failures.length,
    failed: report.failures.length,
    browsers: Object.keys(report.byBrowser),
    devices: Object.keys(report.byDevice),
    breakpoints: Object.keys(report.byBreakpoint),
    timestamp: report.generatedAt,
  };

  const summaryPath = path.join(baseDir, SCREENSHOT_DIRS.reports, 'ci-summary.json');
  fs.writeFileSync(summaryPath, JSON.stringify(summary, null, 2));

  return summaryPath;
}
