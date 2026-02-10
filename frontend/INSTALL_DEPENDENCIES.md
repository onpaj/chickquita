# Dependencies to Install

For the Statistics feature (M11) to work, the following npm packages need to be installed:

```bash
cd frontend
npm install @mui/x-date-pickers dayjs
```

## Required Packages:
- `@mui/x-date-pickers` - Material-UI date picker components for custom date range selection
- `dayjs` - Date utility library (lighter alternative to moment.js)

## Already Installed:
- `recharts` - Charting library for all statistics visualizations (check package.json)

After installing these packages, run:
```bash
npm run dev
```

To test the statistics page, navigate to `/statistics` after signing in.
