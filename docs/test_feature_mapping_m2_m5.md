# E2E Test to Feature Mapping: Milestones 2-5

This document maps PRD features from Milestones 2-5 to their corresponding Playwright E2E test files and test cases.

**Generated:** 2026-02-09
**Test Directory:** `/frontend/e2e`
**Total Features Mapped:** 27

---

## M2: Coop Management (5 Features)

### M2-F1: Create Coop
- **PRD Reference:** Line 1764
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/coops.spec.ts`
- **Test Cases:**
  - `test.describe('Create Coop')` (lines 160-233)
    - `should create a coop with name only` (lines 161-179)
    - `should create a coop with name and location` (lines 181-193)
    - `should show validation error when name is empty` (lines 195-205)
    - `should cancel coop creation` (lines 207-219)
    - `should enforce name max length validation` (lines 221-232)

### M2-F2: List Coops
- **PRD Reference:** Line 1765
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/coops.spec.ts`
- **Test Cases:**
  - `test.describe('List Coops')` (lines 235-277)
    - `should display empty state when no coops exist` (lines 236-243)
    - `should display list of coops` (lines 245-267)
    - `should navigate to coops page from dashboard` (lines 269-276)

### M2-F3: Edit Coop
- **PRD Reference:** Line 1766
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/coops.spec.ts`
- **Test Cases:**
  - `test.describe('Edit Coop')` (lines 279-341)
    - `should edit coop name` (lines 290-305)
    - `should edit coop location` (lines 307-317)
    - `should cancel edit` (lines 319-329)
    - `should show validation error when editing to empty name` (lines 331-340)

### M2-F4: Archive Coop
- **PRD Reference:** Line 1767
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/coops.spec.ts`
- **Test Cases:**
  - `test.describe('Archive Coop')` (lines 343-388)
    - `should archive a coop` (lines 354-372)
    - `should cancel archive` (lines 374-387)

### M2-F5: Delete Coop
- **PRD Reference:** Line 1768
- **Test Status:** ✅ Exists (partial validation pending)
- **Test File:** `/frontend/e2e/coops.spec.ts`
- **Test Cases:**
  - `test.describe('Delete Coop')` (lines 390-432)
    - `should delete an empty coop` (lines 401-416)
    - `should cancel delete` (lines 418-429)
    - **Note:** Validation rule "cannot delete coop with active flocks" requires M3 implementation (line 431)

---

## M3: Basic Flock Creation (6 Features)

### M3-F1: Create Flock
- **PRD Reference:** Line 1806
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/flocks.spec.ts`
- **Test Cases:**
  - `test.describe('Create Flock')` (lines 94-198)
    - `should create a flock with valid data` (lines 95-120)
    - `should show validation error for empty identifier` (lines 122-135)
    - `should show validation error for future hatch date` (lines 137-157)
    - `should show validation error when all counts are zero` (lines 159-176)
    - `should cancel flock creation` (lines 178-197)

### M3-F2: List Flocks
- **PRD Reference:** Line 1807
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/flocks.spec.ts`
- **Test Cases:**
  - `test.describe('View Flocks List')` (lines 200-261)
    - `should display empty state when no flocks exist` (lines 201-207)
    - `should display populated list of flocks` (lines 209-260)

### M3-F3: View Flock Details
- **PRD Reference:** Line 1808
- **Test Status:** ✅ Exists (implicit)
- **Test File:** `/frontend/e2e/flocks.spec.ts`
- **Test Cases:**
  - Tested in `should create a flock with valid data` (lines 111-119)
  - Composition details verified via `getFlockComposition()` helper
  - Status verified via `getFlockStatus()` helper

### M3-F4: Edit Basic Flock Info
- **PRD Reference:** Line 1809
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/flocks.spec.ts`
- **Test Cases:**
  - `test.describe('Edit Flock')` (lines 263-346)
    - `should edit flock information` (lines 264-299)
    - `should cancel flock edit` (lines 301-323)
    - `should show validation errors on invalid edit data` (lines 325-345)

### M3-F5: Archive Flock
- **PRD Reference:** Line 1810
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/flocks.spec.ts`
- **Test Cases:**
  - `test.describe('Archive Flock')` (lines 348-407)
    - `should archive a flock after confirmation` (lines 349-386)
    - `should cancel flock archive` (lines 388-406)
  - `test.describe('Filter Flocks')` (lines 409-451)
    - `should filter archived flocks` (lines 410-450)

### M3-F6: Initial Flock History
- **PRD Reference:** Line 1811
- **Test Status:** ⚠️ MISSING
- **Notes:** No explicit E2E test verifying that initial flock history record is created automatically. This is likely tested at the API/integration level but not in E2E tests.

---

## M4: Daily Egg Records (8 Features including validations)

### M4-F1: Create Daily Record
- **PRD Reference:** Line 1848
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/daily-records-full-workflow.spec.ts`
- **Test Cases:**
  - `should complete full CRUD workflow on desktop` (lines 148-361) - CREATE section (lines 151-209)
  - Test File: `/frontend/e2e/daily-records-quick-add.spec.ts`
    - `should submit form successfully and close modal` (lines 257-301)

### M4-F2: Quick-Add via FAB
- **PRD Reference:** Line 1849
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/daily-records-quick-add.spec.ts`
- **Test Cases:**
  - `should open Quick Add modal from dashboard` (lines 116-131)
  - `should display all form fields correctly` (lines 133-152)
  - `should have today as default date` (lines 154-166)
  - `should increment and decrement egg count` (lines 168-196)
  - `should submit form successfully and close modal` (lines 257-301)
  - `should complete full workflow in less than 30 seconds` (lines 375-414)

### M4-F3: View Daily Records List
- **PRD Reference:** Line 1850
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/daily-records-full-workflow.spec.ts`
- **Test Cases:**
  - READ section in `should complete full CRUD workflow on desktop` (lines 211-236)
- **Additional File:** `/frontend/e2e/daily-records-list.spec.ts` (file name found, not read in detail)

### M4-F4: Edit Daily Record (Same Day Only)
- **PRD Reference:** Line 1851
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/daily-records-edit.spec.ts`
- **Test Cases:**
  - `should show edit button only for same-day records` (lines 22-39)
  - `should open edit modal with pre-filled data when edit button clicked` (lines 41-106)
  - `should display date field as read-only in edit modal` (lines 108-126)
  - `should display flock field as read-only in edit modal` (lines 128-144)
  - `should allow editing egg count and notes` (lines 146-171)
  - `should successfully update record when save is clicked` (lines 173-207)
- **Also in:** `/frontend/e2e/daily-records-full-workflow.spec.ts` - UPDATE section (lines 238-296)

### M4-F5: Delete Daily Record
- **PRD Reference:** Line 1852
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/daily-records-full-workflow.spec.ts`
- **Test Cases:**
  - DELETE section in `should complete full CRUD workflow on desktop` (lines 299-353)

### M4-V1: One Record Per Flock Per Day
- **PRD Reference:** Line 1853
- **Test Status:** ⚠️ MISSING
- **Notes:** No explicit E2E test attempting to create duplicate record for same flock on same day. This validation rule is not covered in E2E tests.

### M4-V2: Cannot Create Future-Dated Records
- **PRD Reference:** Line 1854
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/daily-records-quick-add.spec.ts`
- **Test Cases:**
  - `should validate date cannot be in future` (lines 198-217)
- **Also in:** `/frontend/e2e/daily-records-full-workflow.spec.ts`
  - `should handle validation errors gracefully` (lines 659-686)

### M4-V3: Egg Count >= 0
- **PRD Reference:** Line 1855
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/daily-records-edit.spec.ts`
- **Test Cases:**
  - `should validate egg count is not negative` (lines 227-252)

---

## M5: Purchase Tracking (8 Features)

### M5-F1: Create Purchase
- **PRD Reference:** Line 1891
- **Test Status:** ✅ Exists
- **Test Files:**
  - `/frontend/e2e/purchases-crud.spec.ts`
    - `test.describe('Scenario 1: Create Purchase')` (lines 36-191)
      - `should create a new purchase with all required fields` (lines 37-65)
      - `should create purchase with minimal required fields` (lines 67-83)
      - `should show validation error for empty name` (lines 85-101)
      - `should show validation error for zero amount` (lines 103-115)
      - `should show validation error for future purchase date` (lines 117-140)
      - `should cancel purchase creation` (lines 142-165)
      - `should create purchases with different types` (lines 167-190)
  - `/frontend/e2e/purchases.spec.ts` (API-level)
    - `should create a new purchase successfully` (lines 29-65)

### M5-F2: List Purchases with Filters
- **PRD Reference:** Line 1892
- **Test Status:** ✅ Exists
- **Test File:** `/frontend/e2e/purchases-crud.spec.ts`
- **Test Cases:**
  - `test.describe('Scenario 2: Filter Purchases')` (lines 193-280)
    - `should filter purchases by date range` (lines 231-240)
    - `should filter purchases by type` (lines 242-252)
    - `should clear type filter and show all purchases` (lines 254-266)
    - `should combine date and type filters` (lines 268-279)

### M5-F3: Edit Purchase
- **PRD Reference:** Line 1893
- **Test Status:** ✅ Exists
- **Test Files:**
  - `/frontend/e2e/purchases-crud.spec.ts`
    - `test.describe('Scenario 3: Edit Purchase')` (lines 282-399)
      - `should edit purchase name` (lines 299-319)
      - `should edit purchase amount and quantity` (lines 321-335)
      - `should edit purchase type` (lines 337-350)
      - `should cancel purchase edit` (lines 352-369)
      - `should show validation errors on invalid edit data` (lines 371-380)
      - `should edit multiple fields at once` (lines 382-398)
  - `/frontend/e2e/purchases.spec.ts` (API-level)
    - `should update the purchase successfully` (lines 100-131)

### M5-F4: Delete Purchase
- **PRD Reference:** Line 1894
- **Test Status:** ✅ Exists
- **Test Files:**
  - `/frontend/e2e/purchases-crud.spec.ts`
    - `test.describe('Scenario 4: Delete Purchase')` (lines 401-474)
      - `should delete purchase after confirmation` (lines 418-440)
      - `should cancel purchase deletion` (lines 442-458)
      - `should display confirmation dialog with purchase details` (lines 460-473)
  - `/frontend/e2e/purchases.spec.ts` (API-level)
    - `should delete the purchase successfully` (lines 133-140)

### M5-F5: Purchase Name Autocomplete
- **PRD Reference:** Line 1895
- **Test Status:** ✅ Exists
- **Test Files:**
  - `/frontend/e2e/purchases-crud.spec.ts`
    - `test.describe('Scenario 5: Autocomplete Functionality')` (lines 476-567)
      - `should show autocomplete suggestions based on existing purchases` (lines 477-511)
      - `should allow free text entry when no suggestions match` (lines 513-529)
      - `should populate name field when selecting autocomplete suggestion` (lines 531-566)
  - `/frontend/e2e/purchases.spec.ts` (API-level)
    - `test.describe('Purchase Names Endpoint')` (lines 213-235)

### M5-F6: Purchase Types (Feed, Vitamins, Bedding, Toys, Veterinary, Other)
- **PRD Reference:** Line 1896
- **Test Status:** ✅ Exists (implicit)
- **Test File:** `/frontend/e2e/purchases-crud.spec.ts`
- **Test Cases:**
  - `should create purchases with different types` (lines 167-190)
    - Covers: Krmivo (Feed), Vitamíny (Vitamins), Podestýlka (Bedding)
  - **Note:** Not all 6 types explicitly tested (Toys, Veterinary, Other are missing)

### M5-F7: Quantity Units (kg, pcs, l, package, other)
- **PRD Reference:** Line 1897
- **Test Status:** 🔶 Incomplete
- **Test File:** `/frontend/e2e/purchases-crud.spec.ts`
- **Coverage:**
  - Units tested: `kg` (kilograms), `ks` (pieces/pcs)
  - Units NOT tested: `l` (liters), `package`, `other`

### M5-F8: Optional Flock/Coop Assignment
- **PRD Reference:** Line 1898
- **Test Status:** ⚠️ MISSING
- **Notes:** No E2E test explicitly verifying that purchases can be created without flock/coop assignment OR with specific flock/coop assignment. This feature is not covered in E2E tests.

---

## Summary Statistics

### Coverage by Milestone

| Milestone | Total Features | Tests Exist | Tests Missing | Incomplete |
|-----------|---------------|-------------|---------------|------------|
| M2: Coop Management | 5 | 5 (100%) | 0 | 0 |
| M3: Basic Flock Creation | 6 | 5 (83%) | 1 (17%) | 0 |
| M4: Daily Egg Records | 8 | 6 (75%) | 2 (25%) | 0 |
| M5: Purchase Tracking | 8 | 6 (75%) | 1 (12.5%) | 1 (12.5%) |
| **TOTAL** | **27** | **22 (81%)** | **4 (15%)** | **1 (4%)** |

### Test Gap Analysis

#### Missing Tests (⚠️)
1. **M3-F6:** Initial Flock History - No E2E test verifying automatic history record creation
2. **M4-V1:** One Record Per Flock Per Day - No E2E test attempting duplicate record creation
3. **M5-F8:** Optional Flock/Coop Assignment - No E2E test for purchase assignment scenarios

#### Incomplete Tests (🔶)
1. **M5-F7:** Quantity Units - Only 2 out of 5 units tested (`kg`, `ks`)
2. **M5-F6:** Purchase Types - Only 3 out of 6 types explicitly tested

#### Covered Tests (✅)
- **22 features** have comprehensive E2E test coverage
- Includes validation rules, mobile responsiveness, and API integration tests

---

## Additional Test Files Found

The following E2E test files exist but were not directly mapped to PRD features (they test cross-cutting concerns):

1. `/frontend/e2e/daily-records-accessibility.spec.ts` - Accessibility testing
2. `/frontend/e2e/daily-records-mobile-ui.spec.ts` - Mobile UI testing
3. `/frontend/e2e/daily-records-performance.spec.ts` - Performance testing
4. `/frontend/e2e/daily-records-quick-add-performance.spec.ts` - Quick-add performance
5. `/frontend/e2e/dashboard-fab.spec.ts` - Dashboard FAB functionality
6. `/frontend/e2e/flocks-i18n.spec.ts` - Internationalization testing
7. `/frontend/e2e/flocks-performance.spec.ts` - Performance testing
8. `/frontend/e2e/purchases-form.spec.ts` - Purchase form testing
9. `/frontend/e2e/purchases-page.spec.ts` - Purchase page testing
10. `/frontend/e2e/crossbrowser/*.spec.ts` - Cross-browser testing

---

## Next Steps

1. **Create Missing Tests:**
   - M3-F6: Initial Flock History verification
   - M4-V1: Duplicate record validation
   - M5-F8: Purchase assignment scenarios

2. **Complete Incomplete Tests:**
   - M5-F7: Add tests for `l`, `package`, `other` units
   - M5-F6: Add tests for `Toys`, `Veterinary`, `Other` types

3. **Test Execution:**
   - Run all mapped tests to verify execution status
   - Document pass/fail results
   - Create validation report

---

**Document Version:** 1.0
**Last Updated:** 2026-02-09
**Status:** Ready for TASK-003 (Verify authentication setup)
