# Chickquita i18n Translation Keys Documentation

**Version:** 1.0.0
**Last Updated:** 2026-02-08
**Primary Language:** Czech (cs-CZ)
**Secondary Language:** English (en-US)

---

## Table of Contents

1. [Overview](#overview)
2. [Translation Structure](#translation-structure)
3. [Purchases Feature Keys](#purchases-feature-keys)
4. [Usage Examples](#usage-examples)
5. [Translation Guidelines](#translation-guidelines)
6. [Adding New Keys](#adding-new-keys)

---

## Overview

Chickquita uses `react-i18next` for internationalization. All UI text is defined in translation JSON files located in:
- **Czech (Primary):** `frontend/src/locales/cs/translation.json`
- **English (Secondary):** `frontend/src/locales/en/translation.json`

**Important:** All code, comments, and documentation must be in English. Only UI-facing text is translated to Czech.

---

## Translation Structure

### File Location
```
frontend/src/locales/
├── cs/
│   └── translation.json  # Czech translations
└── en/
    └── translation.json  # English translations
```

### Top-Level Namespaces
```json
{
  "common": { ... },           // Shared UI elements
  "auth": { ... },             // Authentication
  "navigation": { ... },       // Navigation menu
  "dashboard": { ... },        // Dashboard
  "coops": { ... },            // Coops feature
  "flocks": { ... },           // Flocks feature
  "purchases": { ... },        // Purchases feature
  "dailyRecords": { ... },     // Daily records feature
  "settings": { ... },         // Settings
  "validation": { ... },       // Form validation messages
  "errors": { ... }            // Error messages
}
```

---

## Purchases Feature Keys

### Top-Level Keys

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.title` | Nákupy | Purchases | Feature title |
| `purchases.addPurchase` | Přidat nákup | Add Purchase | Button label for adding |
| `purchases.createPurchase` | Vytvořit nákup | Create Purchase | Create form title |
| `purchases.editPurchase` | Upravit nákup | Edit Purchase | Edit form title |
| `purchases.category` | Kategorie | Category | Category label |
| `purchases.amount` | Částka | Amount | Amount label |
| `purchases.date` | Datum | Date | Date label |
| `purchases.description` | Popis | Description | Description label |
| `purchases.currency` | Kč | CZK | Currency symbol |

### Purchase Types

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.types.feed` | Krmivo | Feed | Feed type |
| `purchases.types.vitamins` | Vitamíny | Vitamins | Vitamins type |
| `purchases.types.bedding` | Podestýlka | Bedding | Bedding type |
| `purchases.types.toys` | Hračky | Toys | Toys type |
| `purchases.types.veterinary` | Veterinární péče | Veterinary | Veterinary type |
| `purchases.types.other` | Ostatní | Other | Other type |

### Quantity Units

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.units.kg` | kg | kg | Kilograms |
| `purchases.units.pcs` | ks | pcs | Pieces |
| `purchases.units.l` | l | L | Liters |
| `purchases.units.package` | balení | package | Package |
| `purchases.units.other` | jiné | other | Other unit |

### Filters

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.filters.title` | Filtry | Filters | Filters section title |
| `purchases.filters.fromDate` | Od data | From Date | Start date filter |
| `purchases.filters.toDate` | Do data | To Date | End date filter |
| `purchases.filters.type` | Typ | Type | Type filter |
| `purchases.filters.flock` | Hejno | Flock | Flock filter |

### Summary

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.summary.thisMonth` | Celkem utraceno tento měsíc | Total Spent This Month | Monthly summary label |

### List View

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.list.type` | Typ | Type | Type column |
| `purchases.list.amount` | Částka | Amount | Amount column |
| `purchases.list.quantity` | Množství | Quantity | Quantity column |
| `purchases.list.flock` | Hejno | Flock | Flock column |
| `purchases.list.unknownFlock` | Neznámé hejno | Unknown Flock | Unknown flock label |

### Empty State

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.emptyState.title` | Zatím žádné nákupy | No purchases yet | Empty state title |
| `purchases.emptyState.description` | Začněte sledovat náklady na chov přidáním prvního nákupu. | Start tracking your farming costs by adding your first purchase. | Empty state description |

### Delete Confirmation

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.delete.title` | Smazat nákup? | Delete Purchase? | Delete dialog title |
| `purchases.delete.message` | Opravdu chcete smazat nákup "{{name}}"? | Are you sure you want to delete purchase "{{name}}"? | Delete message with interpolation |
| `purchases.delete.warning` | Tuto akci nelze vrátit zpět. | This action cannot be undone. | Warning message |
| `purchases.delete.success` | Nákup byl úspěšně smazán | Purchase deleted successfully | Success toast |
| `purchases.delete.error` | Nepodařilo se smazat nákup | Failed to delete purchase | Error toast |

### Accessibility

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.purchaseCardAriaLabel` | Nákup {{name}} | Purchase {{name}} | ARIA label for purchase card |

### CRUD Operations

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.create.success` | Nákup byl úspěšně vytvořen | Purchase created successfully | Create success message |
| `purchases.create.error` | Nepodařilo se vytvořit nákup | Failed to create purchase | Create error message |
| `purchases.update.success` | Nákup byl úspěšně aktualizován | Purchase updated successfully | Update success message |
| `purchases.update.error` | Nepodařilo se aktualizovat nákup | Failed to update purchase | Update error message |

### Form Fields

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `purchases.form.type` | Typ nákupu | Purchase Type | Type field label |
| `purchases.form.name` | Název | Name | Name field label |
| `purchases.form.purchaseDate` | Datum nákupu | Purchase Date | Purchase date field |
| `purchases.form.amount` | Částka (Kč) | Amount (CZK) | Amount field with currency |
| `purchases.form.quantity` | Množství | Quantity | Quantity field |
| `purchases.form.unit` | Jednotka | Unit | Unit field |
| `purchases.form.consumedDate` | Datum spotřeby | Consumed Date | Consumed date field |
| `purchases.form.notes` | Poznámky | Notes | Notes field |
| `purchases.form.coop` | Kurník | Coop | Coop field |
| `purchases.form.noCoop` | Bez kurníku | No Coop | No coop option |
| `purchases.form.purchaseDateFuture` | Datum nákupu nemůže být v budoucnosti | Purchase date cannot be in the future | Validation error |
| `purchases.form.consumedDateFuture` | Datum spotřeby nemůže být v budoucnosti | Consumed date cannot be in the future | Validation error |

---

## Usage Examples

### Basic Translation

```tsx
import { useTranslation } from 'react-i18next';

function PurchasesList() {
  const { t } = useTranslation();

  return (
    <Box>
      <Typography variant="h4">{t('purchases.title')}</Typography>
      <Button>{t('purchases.addPurchase')}</Button>
    </Box>
  );
}
```

### With Interpolation

```tsx
import { useTranslation } from 'react-i18next';

function DeleteConfirmation({ purchase }) {
  const { t } = useTranslation();

  return (
    <Dialog>
      <DialogTitle>{t('purchases.delete.title')}</DialogTitle>
      <DialogContent>
        <Typography>
          {t('purchases.delete.message', { name: purchase.name })}
        </Typography>
      </DialogContent>
    </Dialog>
  );
}
```

### Enum Translation (Purchase Types)

```tsx
import { useTranslation } from 'react-i18next';

function PurchaseTypeSelect() {
  const { t } = useTranslation();

  const purchaseTypes = [
    { value: 0, label: t('purchases.types.feed') },
    { value: 1, label: t('purchases.types.vitamins') },
    { value: 2, label: t('purchases.types.bedding') },
    { value: 3, label: t('purchases.types.toys') },
    { value: 4, label: t('purchases.types.veterinary') },
    { value: 5, label: t('purchases.types.other') },
  ];

  return (
    <Select>
      {purchaseTypes.map((type) => (
        <MenuItem key={type.value} value={type.value}>
          {type.label}
        </MenuItem>
      ))}
    </Select>
  );
}
```

### Quantity Units Translation

```tsx
import { useTranslation } from 'react-i18next';

function QuantityUnitSelect() {
  const { t } = useTranslation();

  const units = [
    { value: 0, label: t('purchases.units.kg') },
    { value: 1, label: t('purchases.units.pcs') },
    { value: 2, label: t('purchases.units.l') },
    { value: 3, label: t('purchases.units.package') },
    { value: 4, label: t('purchases.units.other') },
  ];

  return (
    <Select>
      {units.map((unit) => (
        <MenuItem key={unit.value} value={unit.value}>
          {unit.label}
        </MenuItem>
      ))}
    </Select>
  );
}
```

### Toast Notifications

```tsx
import { useTranslation } from 'react-i18next';
import { toast } from 'react-toastify';

function PurchaseForm() {
  const { t } = useTranslation();
  const createPurchase = useCreatePurchase();

  const handleSubmit = async (data) => {
    try {
      await createPurchase.mutateAsync(data);
      toast.success(t('purchases.create.success'));
    } catch (error) {
      toast.error(t('purchases.create.error'));
    }
  };

  return <form onSubmit={handleSubmit}>...</form>;
}
```

### Empty State with Translation

```tsx
import { useTranslation } from 'react-i18next';
import { IllustratedEmptyState } from '@/shared/components';
import { ShoppingCart } from '@mui/icons-material';

function PurchasesList({ purchases }) {
  const { t } = useTranslation();

  if (purchases.length === 0) {
    return (
      <IllustratedEmptyState
        illustration={<ShoppingCart sx={{ fontSize: 80 }} />}
        title={t('purchases.emptyState.title')}
        description={t('purchases.emptyState.description')}
        actionLabel={t('purchases.addPurchase')}
        onAction={() => navigate('/purchases/new')}
      />
    );
  }

  return <PurchaseGrid purchases={purchases} />;
}
```

---

## Translation Guidelines

### 1. Key Naming Convention

- Use dot notation for nested keys: `feature.section.key`
- Use camelCase for multi-word keys: `emptyState`, `addPurchase`
- Keep keys descriptive and self-explanatory

**Example:**
```json
{
  "purchases": {
    "form": {
      "purchaseDate": "Purchase Date"
    }
  }
}
```

### 2. Pluralization

Use `react-i18next` pluralization for count-based strings:

```json
{
  "dailyRecords": {
    "recordsCount": "{{count}} records",
    "recordsCount_one": "{{count}} record"
  }
}
```

### 3. Interpolation

Use double curly braces for variable interpolation:

```json
{
  "coops": {
    "createdAt": "Created {{date}}",
    "deleteConfirmMessage": "Delete \"{{name}}\"?"
  }
}
```

### 4. Context-Specific Keys

Create separate keys for different contexts even if text is similar:

```json
{
  "common": {
    "save": "Save",
    "saving": "Saving..."
  }
}
```

### 5. Validation Messages

Keep validation messages in the `validation` namespace:

```json
{
  "validation": {
    "required": "This field is required",
    "maxAmount": "Maximum amount is 999999.99 CZK"
  }
}
```

---

## Adding New Keys

### Step 1: Add to Translation Files

Add the key to **both** Czech and English translation files.

**`frontend/src/locales/cs/translation.json`:**
```json
{
  "purchases": {
    "newFeature": {
      "title": "Nový název",
      "description": "Popis nové funkce"
    }
  }
}
```

**`frontend/src/locales/en/translation.json`:**
```json
{
  "purchases": {
    "newFeature": {
      "title": "New Title",
      "description": "Description of new feature"
    }
  }
}
```

### Step 2: Use in Component

```tsx
import { useTranslation } from 'react-i18next';

function NewFeature() {
  const { t } = useTranslation();

  return (
    <Box>
      <Typography variant="h4">{t('purchases.newFeature.title')}</Typography>
      <Typography>{t('purchases.newFeature.description')}</Typography>
    </Box>
  );
}
```

### Step 3: Document in This File

Add the new keys to the appropriate table in this documentation file.

---

## Common Translation Patterns

### Enum-Based Translations

For backend enums (PurchaseType, QuantityUnit), create a mapping object:

```tsx
const purchaseTypeLabels = {
  0: t('purchases.types.feed'),
  1: t('purchases.types.vitamins'),
  2: t('purchases.types.bedding'),
  3: t('purchases.types.toys'),
  4: t('purchases.types.veterinary'),
  5: t('purchases.types.other'),
};

// Usage
<Typography>{purchaseTypeLabels[purchase.type]}</Typography>
```

### Date Formatting

Use `react-i18next` with date-fns for date formatting:

```tsx
import { useTranslation } from 'react-i18next';
import { format } from 'date-fns';
import { cs, enUS } from 'date-fns/locale';

function PurchaseDate({ date }) {
  const { i18n } = useTranslation();
  const locale = i18n.language === 'cs' ? cs : enUS;

  return (
    <Typography>
      {format(new Date(date), 'P', { locale })}
    </Typography>
  );
}
```

### Currency Formatting

```tsx
import { useTranslation } from 'react-i18next';

function PurchaseAmount({ amount }) {
  const { t } = useTranslation();

  return (
    <Typography>
      {amount.toFixed(2)} {t('purchases.currency')}
    </Typography>
  );
}
```

---

## Validation Messages

### Reusable Validation Keys

```json
{
  "validation": {
    "required": "This field is required",
    "maxLength": "Maximum length is {{count}} characters",
    "positiveNumber": "Must be a positive number",
    "maxAmount": "Maximum amount is 999999.99 CZK",
    "maxQuantity": "Maximum quantity is 999999.99"
  }
}
```

### Usage with React Hook Form

```tsx
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';

function PurchaseForm() {
  const { t } = useTranslation();
  const { register, formState: { errors } } = useForm();

  return (
    <TextField
      {...register('name', {
        required: t('validation.required'),
        maxLength: {
          value: 100,
          message: t('validation.maxLength', { count: 100 }),
        },
      })}
      error={!!errors.name}
      helperText={errors.name?.message}
    />
  );
}
```

---

## Related Documentation

- [CLAUDE.md](../CLAUDE.md) - Project overview and internationalization guidelines
- [API_SPEC_PURCHASES.md](./API_SPEC_PURCHASES.md) - Purchases API specification
- [COMPONENT_LIBRARY.md](./COMPONENT_LIBRARY.md) - Component library reference

---

## Translation Coverage Checklist

When implementing a new feature, ensure:

- [ ] All UI text uses translation keys (no hardcoded strings)
- [ ] Keys added to both `cs` and `en` translation files
- [ ] Enum values have translation mappings
- [ ] Form validation messages use translation keys
- [ ] Success/error toast messages are translated
- [ ] Empty states are translated
- [ ] ARIA labels are translated
- [ ] Confirmation dialogs are translated
- [ ] Help text and tooltips are translated
- [ ] This documentation file is updated

---

**Maintainers:** Chickquita Development Team
**Last Updated:** 2026-02-08
