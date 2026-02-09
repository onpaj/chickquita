# Chickquita i18n Translation Keys Documentation

**Version:** 1.1.0
**Last Updated:** 2026-02-09
**Primary Language:** Czech (cs-CZ)
**Secondary Language:** English (en-US)

---

## Translation Key Summary

This document provides comprehensive translation key tables for all Chickquita features.

### Total Translation Keys by Feature

| Feature | Key Count | Status |
|---------|-----------|--------|
| Common | 25 | ✅ Complete |
| Auth | 6 | ✅ Complete |
| Navigation | 5 | ✅ Complete |
| Dashboard | 31 | ✅ Complete |
| Coops | 29 | ✅ Complete |
| Flocks | 49 | ✅ Complete |
| Purchases | 50 | ✅ Complete |
| Daily Records | 72 | ✅ Complete |
| Settings | 4 | ✅ Complete |
| Validation | 11 | ✅ Complete |
| Errors | 16 | ✅ Complete |
| **TOTAL** | **298** | ✅ Complete |

---

## Table of Contents

1. [Overview](#overview)
2. [Translation Structure](#translation-structure)
3. [Common Keys](#common-keys)
4. [Auth Keys](#auth-keys)
5. [Navigation Keys](#navigation-keys)
6. [Dashboard Keys](#dashboard-keys)
7. [Coops Feature Keys](#coops-feature-keys)
8. [Flocks Feature Keys](#flocks-feature-keys)
9. [Purchases Feature Keys](#purchases-feature-keys)
10. [Daily Records Feature Keys](#daily-records-feature-keys)
11. [Settings Keys](#settings-keys)
12. [Validation Keys](#validation-keys)
13. [Error Keys](#error-keys)
14. [Translation Key Naming Patterns](#translation-key-naming-patterns)
15. [Usage Examples](#usage-examples)
16. [Translation Guidelines](#translation-guidelines)
17. [Adding New Keys](#adding-new-keys)

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

## Common Keys

Shared UI elements used across the entire application.

### Basic Actions

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `common.appName` | Chickquita | Chickquita | Application name |
| `common.loading` | Načítání... | Loading... | Loading indicator |
| `common.save` | Uložit | Save | Save button |
| `common.saving` | Ukládám... | Saving... | Saving state |
| `common.create` | Vytvořit | Create | Create button |
| `common.cancel` | Zrušit | Cancel | Cancel button |
| `common.delete` | Smazat | Delete | Delete button |
| `common.deleting` | Mažu... | Deleting... | Deleting state |
| `common.edit` | Upravit | Edit | Edit button |
| `common.add` | Přidat | Add | Add button |
| `common.back` | Zpět | Back | Back button |
| `common.confirm` | Potvrdit | Confirm | Confirm button |
| `common.search` | Hledat | Search | Search input |
| `common.filter` | Filtrovat | Filter | Filter button |
| `common.close` | Zavřít | Close | Close button/dialog |
| `common.error` | Chyba | Error | Error label |
| `common.success` | Úspěch | Success | Success label |
| `common.more` | Více | More | More options button |
| `common.retry` | Zkusit znovu | Retry | Retry button |
| `common.all` | Vše | All | All filter option |
| `common.processing` | Zpracovávám... | Processing... | Processing state |
| `common.characters` | znaků | characters | Character count label |
| `common.locale` | cs-CZ | en-US | Current locale identifier |

**Total Keys:** 25

---

## Auth Keys

Authentication-related text for sign-in and sign-up flows.

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `auth.signIn` | Přihlásit se | Sign In | Sign in button |
| `auth.signUp` | Registrovat se | Sign Up | Sign up button |
| `auth.signOut` | Odhlásit se | Sign Out | Sign out button |
| `auth.email` | E-mail | Email | Email field label |
| `auth.password` | Heslo | Password | Password field label |
| `auth.welcome` | Vítejte v Chickquita | Welcome to Chickquita | Welcome message |

**Total Keys:** 6

---

## Navigation Keys

Bottom navigation menu labels.

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `navigation.dashboard` | Přehled | Dashboard | Dashboard tab |
| `navigation.coops` | Kurníky | Coops | Coops tab |
| `navigation.purchases` | Nákupy | Purchases | Purchases tab |
| `navigation.dailyRecords` | Denní záznamy | Daily Records | Daily records tab |
| `navigation.settings` | Nastavení | Settings | Settings tab |

**Total Keys:** 5

---

## Dashboard Keys

Dashboard page including widgets, quick actions, and empty state.

### Main Dashboard

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dashboard.title` | Přehled | Dashboard | Page title |
| `dashboard.totalEggs` | Celkem vajec | Total Eggs | Stat label |
| `dashboard.totalCosts` | Celkové náklady | Total Costs | Stat label |
| `dashboard.eggCost` | Cena za vejce | Egg Cost | Stat label |
| `dashboard.activeFlocks` | Aktivní hejna | Active Flocks | Stat label |

### Dashboard Widgets

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dashboard.widgets.todaySummary.title` | Dnešní přehled | Today's Summary | Widget title |
| `dashboard.widgets.todaySummary.eggsToday` | Vajec dnes | Eggs Today | Stat label |
| `dashboard.widgets.todaySummary.notAvailable` | Zatím nedostupné | Not available yet | Placeholder |
| `dashboard.widgets.weeklyProduction.title` | Týdenní produkce | Weekly Production | Widget title |
| `dashboard.widgets.weeklyProduction.eggsThisWeek` | Vajec tento týden | Eggs This Week | Stat label |
| `dashboard.widgets.weeklyProduction.notAvailable` | Zatím nedostupné | Not available yet | Placeholder |
| `dashboard.widgets.flockStatus.title` | Stav hejn | Flock Status | Widget title |
| `dashboard.widgets.flockStatus.totalHens` | Celkem slepic | Total Hens | Stat label |
| `dashboard.widgets.flockStatus.totalRoosters` | Celkem kohoutů | Total Roosters | Stat label |
| `dashboard.widgets.flockStatus.totalChicks` | Celkem kuřat | Total Chicks | Stat label |
| `dashboard.widgets.flockStatus.activeFlocks` | Aktivních hejn | Active Flocks | Stat label |
| `dashboard.widgets.eggCostCalc.title` | Kalkulace nákladů | Cost Calculation | Widget title |
| `dashboard.widgets.eggCostCalc.costPerEgg` | Cena za vejce | Cost Per Egg | Stat label |
| `dashboard.widgets.eggCostCalc.notAvailable` | Zatím nedostupné | Not available yet | Placeholder |

### Quick Actions

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dashboard.quickActions.title` | Rychlé akce | Quick Actions | Section title |
| `dashboard.quickActions.addDailyRecord` | Zaznamenat vajíčka | Log Eggs | Action label |
| `dashboard.quickActions.addDailyRecordDesc` | Přidejte dnešní produkci vajec | Add today's egg production | Action description |
| `dashboard.quickActions.manageCoops` | Spravovat kurníky | Manage Coops | Action label |
| `dashboard.quickActions.manageCoopsDesc` | Přidejte nebo upravte kurníky | Add or edit coops | Action description |
| `dashboard.quickActions.manageFlocks` | Spravovat hejna | Manage Flocks | Action label |
| `dashboard.quickActions.manageFlocksDesc` | Sledujte svá kuřecí hejna | Track your chicken flocks | Action description |
| `dashboard.quickActions.trackPurchases` | Sledovat nákupy | Track Purchases | Action label |
| `dashboard.quickActions.trackPurchasesDesc` | Zaznamenejte náklady na chov | Record farming costs | Action description |
| `dashboard.quickActions.addDailyRecordAriaLabel` | Přidat denní záznam | Add daily record | ARIA label |

### Empty State

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dashboard.emptyState.title` | Vítejte v Chickquita! | Welcome to Chickquita! | Empty state title |
| `dashboard.emptyState.message` | Začněte sledováním své kuřecí farmy | Start tracking your chicken farm | Empty state message |
| `dashboard.emptyState.createFirstCoop` | Vytvořte svůj první kurník | Create Your First Coop | Action button |
| `dashboard.emptyState.createFirstCoopDesc` | Nastavte kurník a začněte spravovat hejna | Set up a coop to start managing your flocks | Action description |

**Total Keys:** 31

---

## Coops Feature Keys

Chicken coop management including CRUD operations, archival, and empty states.

### Main Coops

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `coops.title` | Kurníky | Coops | Page title |
| `coops.addCoop` | Přidat kurník | Add Coop | Add button label |
| `coops.editCoop` | Upravit kurník | Edit Coop | Edit action |
| `coops.deleteCoop` | Smazat kurník | Delete Coop | Delete action |
| `coops.archiveCoop` | Archivovat kurník | Archive Coop | Archive action |
| `coops.archiving` | Archivuji... | Archiving... | Archiving state |
| `coops.coopName` | Název kurníku | Coop Name | Name field label |
| `coops.coopDescription` | Popis | Description | Description field |
| `coops.location` | Umístění | Location | Location field |
| `coops.noCoops` | Zatím nemáte žádné kurníky | You don't have any coops yet | Empty list message |
| `coops.addFirstCoop` | Klikněte na tlačítko + pro přidání vašeho prvního kurníku | Click the + button to add your first coop | Instruction text |
| `coops.duplicateName` | Kurník s tímto názvem už existuje | A coop with this name already exists | Validation error |
| `coops.active` | Aktivní | Active | Active status |
| `coops.archived` | Archivován | Archived | Archived status |
| `coops.status` | Stav | Status | Status label |
| `coops.createdAt` | Vytvořeno {{date}} | Created {{date}} | Created date with interpolation |
| `coops.updatedAt` | Aktualizováno {{date}} | Updated {{date}} | Updated date with interpolation |
| `coops.details` | Detail kurníku | Coop Details | Detail page title |
| `coops.coopNotFound` | Kurník nenalezen | Coop not found | Not found message |
| `coops.coopCardAriaLabel` | Kurník {{coopName}} | Coop {{coopName}} | ARIA label for card |

### Archive Operations

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `coops.archiveConfirmTitle` | Archivovat kurník? | Archive Coop? | Confirm dialog title |
| `coops.archiveConfirmMessage` | Tento kurník bude archivován a odstraněn z vašeho aktivního seznamu. V případě potřeby jej můžete později znovu aktivovat. | This coop will be archived and removed from your active list. You can reactivate it later if needed. | Confirm message |
| `coops.archiveSuccess` | Kurník byl úspěšně archivován | Coop archived successfully | Success toast |
| `coops.archiveError` | Nepodařilo se archivovat kurník | Failed to archive coop | Error toast |

### Delete Operations

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `coops.deleteConfirmTitle` | Smazat kurník? | Delete Coop? | Delete dialog title |
| `coops.deleteConfirmMessage` | Tento kurník bude trvale smazán. Tuto akci nelze vrátit zpět. | This coop will be permanently deleted. This action cannot be undone. | Delete warning |
| `coops.deleteSuccess` | Kurník byl úspěšně smazán | Coop deleted successfully | Success toast |
| `coops.deleteErrorHasFlocks` | Nelze smazat kurník s existujícími hejny. Nejprve prosím smažte nebo přesuňte všechna hejna. | Cannot delete coop with existing flocks. Please delete or move all flocks first. | Business rule error |

### Empty State

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `coops.emptyState.title` | Zatím tu nejsou žádné kurníky | No coops yet | Empty state title |
| `coops.emptyState.message` | Přidejte svůj první kurník, abyste mohli začít! | Add your first coop to get started! | Empty state message |

**Total Keys:** 29

---

## Flocks Feature Keys

Flock management including composition editing, chick maturation, and history tracking.

### Main Flocks

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `flocks.title` | Hejna | Flocks | Page title |
| `flocks.details` | Detail hejna | Flock Details | Detail page title |
| `flocks.addFlock` | Přidat hejno | Add Flock | Add button |
| `flocks.editFlock` | Upravit hejno | Edit Flock | Edit action |
| `flocks.deleteFlock` | Smazat hejno | Delete Flock | Delete action |
| `flocks.archiveFlock` | Archivovat hejno | Archive Flock | Archive action |
| `flocks.flockName` | Název hejna | Flock Name | Name field |
| `flocks.identifier` | Identifikátor | Identifier | Identifier field |
| `flocks.hens` | Slepice | Hens | Hens label |
| `flocks.roosters` | Kohouti | Roosters | Roosters label |
| `flocks.chicks` | Kuřata | Chicks | Chicks label |
| `flocks.total` | Celkem | Total | Total count label |
| `flocks.active` | Aktivní | Active | Active status |
| `flocks.archived` | Archivováno | Archived | Archived status |
| `flocks.status` | Stav | Status | Status label |
| `flocks.filterStatus` | Filtrovat podle stavu | Filter by status | Filter action |
| `flocks.coop` | Kurník | Coop | Coop label |
| `flocks.hatchDate` | Datum líhnutí | Hatch Date | Hatch date field |
| `flocks.currentComposition` | Aktuální složení | Current Composition | Composition section |
| `flocks.createdAt` | Vytvořeno {{date}} | Created {{date}} | Created date |
| `flocks.updatedAt` | Aktualizováno {{date}} | Updated {{date}} | Updated date |
| `flocks.flockNotFound` | Hejno nenalezeno | Flock not found | Not found message |
| `flocks.viewHistory` | Zobrazit historii | View History | View history action |
| `flocks.flockCardAriaLabel` | Hejno {{identifier}} v kurníku {{coopName}} | Flock {{identifier}} in coop {{coopName}} | ARIA label |
| `flocks.belongsToCoopAriaLabel` | Patří do kurníku {{coopName}} | Belongs to coop {{coopName}} | Coop relationship label |

### Form Fields

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `flocks.form.identifier` | Identifikátor hejna | Flock Identifier | Identifier field label |
| `flocks.form.hatchDate` | Datum líhnutí | Hatch Date | Hatch date field |
| `flocks.form.hatchDateFuture` | Datum nemůže být v budoucnosti | Date cannot be in the future | Validation error |
| `flocks.form.composition` | Složení hejna | Flock Composition | Composition section |
| `flocks.form.atLeastOne` | Alespoň jeden počet musí být větší než 0 | At least one count must be greater than 0 | Validation error |
| `flocks.form.increase` | Zvýšit | Increase | Increment button |
| `flocks.form.decrease` | Snížit | Decrease | Decrement button |

### Chick Maturation (Phase 2 Feature)

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `flocks.matureChicks.title` | Převést kuřata na dospělé | Mature Chicks to Adults | Action title |
| `flocks.matureChicks.chicksCount` | Počet kuřat | Number of Chicks | Chicks count field |
| `flocks.matureChicks.toHens` | Na slepice | To Hens | Convert to hens |
| `flocks.matureChicks.toRoosters` | Na kohouty | To Roosters | Convert to roosters |

### CRUD Operations

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `flocks.create.success` | Hejno bylo úspěšně vytvořeno | Flock created successfully | Create success toast |
| `flocks.create.error` | Nepodařilo se vytvořit hejno | Failed to create flock | Create error toast |
| `flocks.update.success` | Hejno bylo úspěšně aktualizováno | Flock updated successfully | Update success toast |
| `flocks.update.error` | Nepodařilo se aktualizovat hejno | Failed to update flock | Update error toast |
| `flocks.archive.success` | Hejno bylo úspěšně archivováno | Flock archived successfully | Archive success toast |
| `flocks.archive.error` | Nepodařilo se archivovat hejno | Failed to archive flock | Archive error toast |

### Archive Operations

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `flocks.archiving` | Archivuji... | Archiving... | Archiving state |
| `flocks.archiveSuccess` | Hejno bylo úspěšně archivováno | Flock archived successfully | Success toast (duplicate) |
| `flocks.archiveConfirmTitle` | Archivovat hejno? | Archive Flock? | Confirm dialog title |
| `flocks.archiveConfirmMessage` | Toto hejno bude archivováno a odstraněno z vašeho aktivního seznamu. V případě potřeby jej můžete později znovu aktivovat. | This flock will be archived and removed from your active list. You can reactivate it later if needed. | Confirm message |

### Empty State

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `flocks.emptyState.title` | Zatím tu nejsou žádná hejna | No flocks yet | Empty state title |
| `flocks.emptyState.message` | Přidejte své první hejno, abyste mohli začít sledovat slepice! | Add your first flock to start tracking your chickens! | Empty state message |

**Total Keys:** 49

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

## Daily Records Feature Keys

Daily egg production tracking with comprehensive validation messages.

### Main Daily Records

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.title` | Denní záznamy | Daily Records | Page title |
| `dailyRecords.addRecord` | Přidat záznam | Add Record | Add button |
| `dailyRecords.eggCount` | Počet vajec | Egg Count | Egg count field |
| `dailyRecords.eggsLabel` | vajec | eggs | Eggs label (lowercase) |
| `dailyRecords.date` | Datum | Date | Date field |
| `dailyRecords.notes` | Poznámky | Notes | Notes field |
| `dailyRecords.flock` | Hejno | Flock | Flock selector |
| `dailyRecords.noFlocks` | Žádná dostupná hejna | No flocks available | No flocks message |
| `dailyRecords.clearFilters` | Vymazat filtry | Clear Filters | Clear filters button |
| `dailyRecords.dateFutureError` | Datum záznamu nemůže být v budoucnosti | Record date cannot be in the future | Validation error |

### Record Count (with Pluralization)

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.recordsCount` | {{count}} záznamů | {{count}} records | Records count (plural) |
| `dailyRecords.recordsCount_one` | {{count}} záznam | {{count}} record | Records count (singular) |
| `dailyRecords.recordsCount_few` | {{count}} záznamy | (not used) | Records count (Czech 2-4) |
| `dailyRecords.recordsCount_many` | {{count}} záznamů | (not used) | Records count (Czech 5+) |

### Filters

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.filters.today` | Dnes | Today | Today filter |
| `dailyRecords.filters.lastWeek` | Poslední týden | Last Week | Last week filter |
| `dailyRecords.filters.lastMonth` | Poslední měsíc | Last Month | Last month filter |
| `dailyRecords.filters.startDate` | Od data | Start Date | Start date field |
| `dailyRecords.filters.endDate` | Do data | End Date | End date field |

### Quick Add

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.quickAdd.title` | Rychlý záznam vajec | Quick Add Eggs | Quick add modal title |

### Edit Operations

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.edit.title` | Upravit denní záznam | Edit Daily Record | Edit modal title |
| `dailyRecords.edit.dateNotEditable` | Datum záznamu nelze změnit | Record date cannot be changed | Field restriction |
| `dailyRecords.edit.flockNotEditable` | Hejno nelze změnit | Flock cannot be changed | Field restriction |
| `dailyRecords.edit.sameDayRestriction` | Záznamy lze upravovat pouze v den vytvoření | Records can only be edited on the day they were created | Business rule |

### Empty State

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.emptyState.title` | Zatím tu nejsou žádné záznamy | No records yet | Empty state title |
| `dailyRecords.emptyState.noRecords` | Začněte zaznamenávat denní produkci vajec | Start recording your daily egg production | Empty state message |
| `dailyRecords.emptyState.noRecordsFiltered` | Žádné záznamy nevyhovují zvoleným filtrům | No records match the selected filters | Filtered empty state |

### CRUD Operations

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.create.success` | Denní záznam byl úspěšně vytvořen | Daily record created successfully | Create success toast |
| `dailyRecords.create.error` | Nepodařilo se vytvořit denní záznam | Failed to create daily record | Create error toast |
| `dailyRecords.update.success` | Denní záznam byl úspěšně aktualizován | Daily record updated successfully | Update success toast |
| `dailyRecords.update.error` | Nepodařilo se aktualizovat denní záznam | Failed to update daily record | Update error toast |

### Delete Operations

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.delete.title` | Smazat denní záznam | Delete Daily Record | Delete dialog title |
| `dailyRecords.delete.message` | Opravdu chcete smazat denní záznam z data | Are you sure you want to delete the daily record from | Delete message |
| `dailyRecords.delete.flockInfo` | Hejno: {{flock}} | Flock: {{flock}} | Flock info in delete dialog |
| `dailyRecords.delete.success` | Denní záznam byl úspěšně smazán | Daily record deleted successfully | Delete success toast |
| `dailyRecords.delete.error` | Nepodařilo se smazat denní záznam | Failed to delete daily record | Delete error toast |

### Comprehensive Validation Messages

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `dailyRecords.validation.flockIdRequired` | ID hejna je povinné | Flock ID is required | Required field |
| `dailyRecords.validation.flockIdString` | ID hejna musí být textový řetězec | Flock ID must be a string | Type validation |
| `dailyRecords.validation.flockIdUuid` | ID hejna musí být platný UUID formát | Flock ID must be a valid UUID format | Format validation |
| `dailyRecords.validation.recordDateRequired` | Datum záznamu je povinné | Record date is required | Required field |
| `dailyRecords.validation.recordDateString` | Datum záznamu musí být textový řetězec | Record date must be a string | Type validation |
| `dailyRecords.validation.recordDateFormat` | Datum záznamu musí být ve formátu YYYY-MM-DD | Record date must be in YYYY-MM-DD format | Format validation |
| `dailyRecords.validation.recordDateFuture` | Datum záznamu nemůže být v budoucnosti | Record date cannot be in the future | Business rule |
| `dailyRecords.validation.eggCountNumber` | Počet vajec musí být číslo | Egg count must be a number | Type validation |
| `dailyRecords.validation.eggCountInteger` | Počet vajec musí být celé číslo | Egg count must be an integer | Type validation |
| `dailyRecords.validation.eggCountNonnegative` | Počet vajec nemůže být záporný | Egg count cannot be negative | Range validation |
| `dailyRecords.validation.notesString` | Poznámky musí být textový řetězec | Notes must be a string | Type validation |
| `dailyRecords.validation.notesMaxLength` | Poznámky nesmí překročit 500 znaků | Notes cannot exceed 500 characters | Length validation |
| `dailyRecords.validation.recordIdRequired` | ID denního záznamu je povinné | Daily record ID is required | Required field |
| `dailyRecords.validation.recordIdString` | ID denního záznamu musí být textový řetězec | Daily record ID must be a string | Type validation |
| `dailyRecords.validation.recordIdUuid` | ID denního záznamu musí být platný UUID formát | Daily record ID must be a valid UUID format | Format validation |
| `dailyRecords.validation.startDateString` | Datum začátku musí být textový řetězec | Start date must be a string | Type validation |
| `dailyRecords.validation.startDateFormat` | Datum začátku musí být ve formátu YYYY-MM-DD | Start date must be in YYYY-MM-DD format | Format validation |
| `dailyRecords.validation.endDateString` | Datum konce musí být textový řetězec | End date must be a string | Type validation |
| `dailyRecords.validation.endDateFormat` | Datum konce musí být ve formátu YYYY-MM-DD | End date must be in YYYY-MM-DD format | Format validation |
| `dailyRecords.validation.endDateAfterStart` | Datum konce musí být stejné nebo pozdější než datum začátku | End date must be the same or later than start date | Range validation |

**Total Keys:** 72

**Note:** Daily Records has the most comprehensive validation messages (20 validation keys) due to complex business rules around date restrictions, flock associations, and edit restrictions.

---

## Settings Keys

Application settings including language and profile.

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `settings.title` | Nastavení | Settings | Page title |
| `settings.language` | Jazyk | Language | Language setting |
| `settings.profile` | Profil | Profile | Profile setting |
| `settings.theme` | Vzhled | Theme | Theme setting |

**Total Keys:** 4

---

## Validation Keys

Reusable form validation messages used across features.

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `validation.required` | Toto pole je povinné | This field is required | Required field |
| `validation.invalidEmail` | Neplatná e-mailová adresa | Invalid email address | Email validation |
| `validation.minLength` | Minimální délka je {{count}} znaků | Minimum length is {{count}} characters | Min length |
| `validation.maxLength` | Maximální délka je {{count}} znaků | Maximum length is {{count}} characters | Max length |
| `validation.min` | Minimální hodnota je {{count}} | Minimum value is {{count}} | Min value |
| `validation.max` | Maximální hodnota je {{count}} | Maximum value is {{count}} | Max value |
| `validation.positiveNumber` | Musí být kladné číslo | Must be a positive number | Positive number |
| `validation.invalidNumber` | Musí být platné číslo | Must be a valid number | Number format |
| `validation.invalidType` | Neplatný typ | Invalid type | Type validation |
| `validation.maxAmount` | Maximální částka je 999999.99 Kč | Maximum amount is 999999.99 CZK | Amount limit |
| `validation.maxQuantity` | Maximální množství je 999999.99 | Maximum quantity is 999999.99 | Quantity limit |

**Total Keys:** 11

---

## Error Keys

Error messages for HTTP errors, validation, and resource not found.

### Generic Errors

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `errors.generic` | Něco se pokazilo | Something went wrong | Generic error |
| `errors.unknown` | Došlo k neočekávané chybě | An unexpected error occurred | Unknown error |
| `errors.networkError` | Chyba připojení. Zkontrolujte prosím připojení k internetu. | Connection error. Please check your internet connection. | Network error |
| `errors.validationError` | Formulář obsahuje chyby | The form contains errors | Form validation error |

### HTTP Status Errors

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `errors.unauthorized` | Nejste přihlášeni. Přihlaste se prosím znovu. | You are not signed in. Please sign in again. | 401 Unauthorized |
| `errors.forbidden` | Nemáte oprávnění k provedení této akce | You do not have permission to perform this action | 403 Forbidden |
| `errors.conflict` | Konflikt - prostředek již existuje | Conflict - resource already exists | 409 Conflict |
| `errors.serverError` | Chyba serveru. Zkuste to prosím později. | Server error. Please try again later. | 500 Server Error |

### Not Found Errors

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `errors.notFound` | Požadovaný prostředek nebyl nalezen | The requested resource was not found | 404 Not Found |
| `errors.notFoundTitle` | Stránka nenalezena | Page Not Found | 404 page title |
| `errors.notFoundDescription` | Omlouváme se, ale stránka, kterou hledáte, neexistuje. | Sorry, the page you are looking for does not exist. | 404 page description |
| `errors.resourceNotFoundDescription` | {{resource}} nebyl nalezen nebo již neexistuje. | The {{resource}} was not found or no longer exists. | Resource not found with interpolation |
| `errors.backToDashboard` | Zpět na přehled | Back to Dashboard | Navigation button |
| `errors.backToList` | Zpět na seznam | Back to List | Navigation button |

### Business Rule Errors

| Key | Czech (cs) | English (en) | Description |
|-----|-----------|--------------|-------------|
| `errors.duplicateName` | Položka s tímto názvem již existuje | An item with this name already exists | Duplicate name error |
| `errors.missingCoopId` | Chybí ID kurníku | Missing coop ID | Missing required ID |

**Total Keys:** 16

---

## Translation Key Naming Patterns

### Consistent Naming Conventions Across Features

After analyzing all 298 translation keys, the following patterns emerge:

#### 1. Hierarchical Structure
```
feature.section.key
```
**Examples:**
- `dashboard.widgets.todaySummary.title`
- `flocks.matureChicks.title`
- `purchases.filters.fromDate`

#### 2. CamelCase for Multi-Word Keys
```
feature.multiWordKey
```
**Examples:**
- `coops.addCoop` (not `add_coop`)
- `dashboard.quickActions` (not `quick_actions`)
- `dailyRecords.eggCount` (not `egg_count`)

#### 3. CRUD Operation Pattern
```
feature.operation.result
```
**Examples:**
- `flocks.create.success`
- `purchases.update.error`
- `dailyRecords.delete.success`

#### 4. Empty State Pattern
```
feature.emptyState.property
```
**Examples:**
- `coops.emptyState.title`
- `flocks.emptyState.message`
- `purchases.emptyState.description`

#### 5. Form Field Pattern
```
feature.form.fieldName
```
**Examples:**
- `flocks.form.identifier`
- `purchases.form.purchaseDate`
- `coops.coopName` (simpler pattern for basic forms)

#### 6. Validation Pattern
```
feature.validation.rule
dailyRecords.validation.fieldNameRule
```
**Examples:**
- `validation.required` (shared)
- `dailyRecords.validation.eggCountNonnegative` (feature-specific)
- `flocks.form.hatchDateFuture` (inline form validation)

#### 7. Dialog/Confirmation Pattern
```
feature.action.dialogProperty
```
**Examples:**
- `coops.delete.title`
- `flocks.archiveConfirmTitle`
- `purchases.delete.warning`

#### 8. ARIA Label Pattern
```
feature.componentAriaLabel
```
**Examples:**
- `coops.coopCardAriaLabel`
- `flocks.flockCardAriaLabel`
- `dashboard.quickActions.addDailyRecordAriaLabel`

#### 9. Enum Translation Pattern
```
feature.enumName.value
```
**Examples:**
- `purchases.types.feed`
- `purchases.units.kg`
- `flocks.status` (for status display)

#### 10. Interpolation Naming
Use descriptive parameter names in double curly braces:
```
{{paramName}}
```
**Examples:**
- `{{count}}` for numbers
- `{{date}}` for dates
- `{{name}}`, `{{identifier}}`, `{{coopName}}`, `{{flock}}` for entity references

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

## Related Documentation

- [CLAUDE.md](../CLAUDE.md) - Project overview and internationalization guidelines
- [i18n-validation-flocks.md](./i18n-validation-flocks.md) - Historical i18n validation report for Flocks feature
- [API_SPEC_PURCHASES.md](./API_SPEC_PURCHASES.md) - Purchases API specification
- [COMPONENT_LIBRARY.md](./COMPONENT_LIBRARY.md) - Component library reference

---

**Maintainers:** Chickquita Development Team
**Last Updated:** 2026-02-09
**Translation Files:**
- Czech: `frontend/src/locales/cs/translation.json`
- English: `frontend/src/locales/en/translation.json`
