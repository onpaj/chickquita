# Feature Inventory: Milestones 2-5

This document contains a structured inventory of all features from PRD Milestones 2-5 for E2E test validation.

## M2: Coop Management

| Feature ID | Feature Name | Description | PRD Line Reference |
|------------|--------------|-------------|-------------------|
| M2-F1 | Create Coop | User can create a coop (name, optional location) | 1764 |
| M2-F2 | List Coops | User sees list of their coops on Coops page | 1765 |
| M2-F3 | Edit Coop | User can edit coop details (name, location) | 1766 |
| M2-F4 | Archive Coop | User can archive a coop (soft delete) | 1767 |
| M2-F5 | Delete Coop | User can delete empty coops (validation: no active flocks) | 1768 |

**Additional Acceptance Criteria:**
- Tenant isolation verified (users only see their own coops)
- Mobile-responsive forms and list view
- Form validation (name required, max length)

---

## M3: Basic Flock Creation

| Feature ID | Feature Name | Description | PRD Line Reference |
|------------|--------------|-------------|-------------------|
| M3-F1 | Create Flock | User can create a flock (identifier, hatch date, coop, initial counts) | 1806 |
| M3-F2 | List Flocks | User sees list of flocks within a coop | 1807 |
| M3-F3 | View Flock Details | User can view flock details (current composition) | 1808 |
| M3-F4 | Edit Basic Flock Info | User can edit basic flock info (identifier, hatch date) | 1809 |
| M3-F5 | Archive Flock | User can archive a flock | 1810 |
| M3-F6 | Initial Flock History | Initial flock history record created automatically | 1811 |

**Validation Rules:**
- At least one animal type > 0
- Identifier unique within coop

---

## M4: Daily Egg Records

| Feature ID | Feature Name | Description | PRD Line Reference |
|------------|--------------|-------------|-------------------|
| M4-F1 | Create Daily Record | User can create a daily record (flock, date, egg count) | 1848 |
| M4-F2 | Quick-Add via FAB | User can access quick-add via FAB button on dashboard | 1849 |
| M4-F3 | View Daily Records List | User can view daily records list (filtered by flock/date range) | 1850 |
| M4-F4 | Edit Daily Record | User can edit daily record (same day only) | 1851 |
| M4-F5 | Delete Daily Record | User can delete daily record | 1852 |

**Validation Rules:**
- One record per flock per day (1853)
- Cannot create future-dated records (1854)
- Egg count >= 0 (1855)
- Mobile-optimized quick-add flow (< 30 seconds) (1856)

---

## M5: Purchase Tracking

| Feature ID | Feature Name | Description | PRD Line Reference |
|------------|--------------|-------------|-------------------|
| M5-F1 | Create Purchase | User can create a purchase (type, name, date, price, quantity, unit) | 1891 |
| M5-F2 | List Purchases with Filters | User sees list of purchases (with filters: type, date range) | 1892 |
| M5-F3 | Edit Purchase | User can edit purchase details | 1893 |
| M5-F4 | Delete Purchase | User can delete a purchase | 1894 |
| M5-F5 | Purchase Name Autocomplete | Purchase name autocomplete from history | 1895 |
| M5-F6 | Purchase Types | Purchase types: Feed, Vitamins, Bedding, Toys, Veterinary, Other | 1896 |
| M5-F7 | Quantity Units | Units: kg, pcs, l, package, other | 1897 |
| M5-F8 | Optional Flock/Coop Assignment | Optional flock/coop assignment | 1898 |

---

## Summary Statistics

- **Total Features:** 27
- **M2 Features:** 5
- **M3 Features:** 6
- **M4 Features:** 5 (+ 3 validation rules)
- **M5 Features:** 8

---

## Notes

- All features are marked with ✅ in the PRD, indicating they are planned for implementation
- Each milestone includes specific technical scope (Frontend, Backend, Database)
- Dependencies are clearly defined (M2 → M3 → M4, M5)
- Out of scope items are explicitly documented for each milestone
