/**
 * TypeScript types and enums for Purchase feature.
 * Matches backend DTOs and domain entities.
 */

/**
 * Represents the type of purchase for chicken farming.
 * Maps to backend enum: Chickquita.Domain.Entities.PurchaseType
 */
export const PurchaseType = {
  /** Chicken feed purchase */
  Feed: 0,
  /** Vitamins and supplements purchase */
  Vitamins: 1,
  /** Bedding material purchase */
  Bedding: 2,
  /** Toys and enrichment items purchase */
  Toys: 3,
  /** Veterinary care and medication purchase */
  Veterinary: 4,
  /** Other miscellaneous purchases */
  Other: 5,
} as const;

export type PurchaseType = (typeof PurchaseType)[keyof typeof PurchaseType];

/**
 * Represents the unit of quantity for purchased items.
 * Maps to backend enum: Chickquita.Domain.Entities.QuantityUnit
 */
export const QuantityUnit = {
  /** Kilograms */
  Kg: 0,
  /** Pieces */
  Pcs: 1,
  /** Liters */
  L: 2,
  /** Package (unspecified unit) */
  Package: 3,
  /** Other unit not listed */
  Other: 4,
} as const;

export type QuantityUnit = (typeof QuantityUnit)[keyof typeof QuantityUnit];

/**
 * Data Transfer Object for Purchase entity.
 * Represents a purchase made for chicken farming.
 * Maps to backend DTO: Chickquita.Application.DTOs.PurchaseDto
 */
export interface PurchaseDto {
  /** Unique identifier for the purchase */
  id: string;
  /** The tenant that owns this purchase */
  tenantId: string;
  /** The coop this purchase is associated with (optional) */
  coopId: string | null;
  /** Name or description of the purchased item */
  name: string;
  /** Type of the purchase (Feed, Vitamins, Bedding, etc.) */
  type: PurchaseType;
  /** The amount paid for the purchase */
  amount: number;
  /** The quantity purchased */
  quantity: number;
  /** The unit of the quantity (Kg, Pcs, L, Package, Other) */
  unit: QuantityUnit;
  /** The date when the purchase was made (ISO 8601 string) */
  purchaseDate: string;
  /** The date when the item was consumed or used (ISO 8601 string, optional) */
  consumedDate: string | null;
  /** Optional notes about the purchase */
  notes: string | null;
  /** Timestamp when the purchase was created (ISO 8601 string) */
  createdAt: string;
  /** Timestamp when the purchase was last updated (ISO 8601 string) */
  updatedAt: string;
}

/**
 * Request payload for creating a new purchase.
 * Maps to backend command: Chickquita.Application.Features.Purchases.Commands.Create.CreatePurchaseCommand
 */
export interface CreatePurchaseDto {
  /** The coop this purchase is associated with (optional) */
  coopId?: string | null;
  /** Name or description of the purchased item */
  name: string;
  /** Type of the purchase */
  type: PurchaseType;
  /** The amount paid for the purchase */
  amount: number;
  /** The quantity purchased */
  quantity: number;
  /** The unit of the quantity */
  unit: QuantityUnit;
  /** The date when the purchase was made (ISO 8601 string) */
  purchaseDate: string;
  /** The date when the item was consumed or used (ISO 8601 string, optional) */
  consumedDate?: string | null;
  /** Optional notes about the purchase */
  notes?: string | null;
}

/**
 * Request payload for updating an existing purchase.
 * Maps to backend command: Chickquita.Application.Features.Purchases.Commands.Update.UpdatePurchaseCommand
 */
export interface UpdatePurchaseDto {
  /** The ID of the purchase to update */
  id: string;
  /** The coop this purchase is associated with (optional) */
  coopId?: string | null;
  /** Name or description of the purchased item */
  name: string;
  /** Type of the purchase */
  type: PurchaseType;
  /** The amount paid for the purchase */
  amount: number;
  /** The quantity purchased */
  quantity: number;
  /** The unit of the quantity */
  unit: QuantityUnit;
  /** The date when the purchase was made (ISO 8601 string) */
  purchaseDate: string;
  /** The date when the item was consumed or used (ISO 8601 string, optional) */
  consumedDate?: string | null;
  /** Optional notes about the purchase */
  notes?: string | null;
}

/**
 * Query parameters for filtering purchases.
 * Maps to backend query: Chickquita.Application.Features.Purchases.Queries.GetPurchasesQuery
 */
export interface PurchaseFilterParams {
  /** Start date for filtering purchases (inclusive, ISO 8601 string, optional) */
  fromDate?: string | null;
  /** End date for filtering purchases (inclusive, ISO 8601 string, optional) */
  toDate?: string | null;
  /** Purchase type filter (optional) */
  type?: PurchaseType | null;
  /** Flock ID filter (optional) - matches purchases for the coop containing this flock */
  flockId?: string | null;
}

/**
 * Type guard to check if an unknown object is a PurchaseDto.
 * Validates the structure and required properties of a PurchaseDto.
 *
 * @param obj - The object to check
 * @returns true if obj is a valid PurchaseDto
 */
export function isPurchaseDto(obj: unknown): obj is PurchaseDto {
  if (typeof obj !== 'object' || obj === null) {
    return false;
  }

  const purchase = obj as Record<string, unknown>;

  // Check required string properties (must be non-empty strings)
  if (
    typeof purchase.id !== 'string' ||
    purchase.id.trim() === '' ||
    typeof purchase.tenantId !== 'string' ||
    typeof purchase.name !== 'string' ||
    purchase.name.trim() === '' ||
    typeof purchase.purchaseDate !== 'string' ||
    typeof purchase.createdAt !== 'string' ||
    typeof purchase.updatedAt !== 'string'
  ) {
    return false;
  }

  // Check nullable string properties
  if (
    purchase.coopId !== null && typeof purchase.coopId !== 'string'
  ) {
    return false;
  }

  if (
    purchase.consumedDate !== null && typeof purchase.consumedDate !== 'string'
  ) {
    return false;
  }

  if (
    purchase.notes !== null && typeof purchase.notes !== 'string'
  ) {
    return false;
  }

  // Check number properties (must be valid numbers, not NaN)
  if (
    typeof purchase.amount !== 'number' ||
    Number.isNaN(purchase.amount) ||
    typeof purchase.quantity !== 'number' ||
    Number.isNaN(purchase.quantity)
  ) {
    return false;
  }

  // Check enum properties (PurchaseType and QuantityUnit are numbers)
  if (
    typeof purchase.type !== 'number' ||
    !Object.values(PurchaseType).includes(purchase.type as PurchaseType)
  ) {
    return false;
  }

  if (
    typeof purchase.unit !== 'number' ||
    !Object.values(QuantityUnit).includes(purchase.unit as QuantityUnit)
  ) {
    return false;
  }

  return true;
}
