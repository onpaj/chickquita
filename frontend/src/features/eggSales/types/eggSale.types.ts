/**
 * TypeScript types for EggSale feature.
 * Matches backend DTOs and domain entities.
 */

/**
 * Data Transfer Object for EggSale entity.
 * Represents an egg sale transaction.
 * Maps to backend DTO: Chickquita.Application.DTOs.EggSaleDto
 */
export interface EggSaleDto {
  /** Unique identifier for the egg sale */
  id: string;
  /** The tenant that owns this egg sale */
  tenantId: string;
  /** The date of the sale (ISO 8601 string) */
  date: string;
  /** Number of eggs sold */
  quantity: number;
  /** Price per egg unit */
  pricePerUnit: number;
  /** Optional buyer name */
  buyerName: string | null;
  /** Optional notes */
  notes: string | null;
  /** Timestamp when the record was created (ISO 8601 string) */
  createdAt: string;
  /** Timestamp when the record was last updated (ISO 8601 string) */
  updatedAt: string;
}

/**
 * Request payload for creating a new egg sale.
 * Maps to backend command: Chickquita.Application.Features.EggSales.Commands.Create.CreateEggSaleCommand
 */
export interface CreateEggSaleDto {
  /** The date of the sale (ISO 8601 string) */
  date: string;
  /** Number of eggs sold */
  quantity: number;
  /** Price per egg unit */
  pricePerUnit: number;
  /** Optional buyer name */
  buyerName?: string | null;
  /** Optional notes */
  notes?: string | null;
}

/**
 * Request payload for updating an existing egg sale.
 * Maps to backend command: Chickquita.Application.Features.EggSales.Commands.Update.UpdateEggSaleCommand
 */
export interface UpdateEggSaleDto {
  /** The ID of the egg sale to update */
  id: string;
  /** The date of the sale (ISO 8601 string) */
  date: string;
  /** Number of eggs sold */
  quantity: number;
  /** Price per egg unit */
  pricePerUnit: number;
  /** Optional buyer name */
  buyerName?: string | null;
  /** Optional notes */
  notes?: string | null;
}

/**
 * Query parameters for filtering egg sales.
 * Maps to backend query: Chickquita.Application.Features.EggSales.Queries.GetEggSalesQuery
 */
export interface EggSaleFilterParams {
  /** Start date for filtering egg sales (inclusive, ISO 8601 string, optional) */
  fromDate?: string | null;
  /** End date for filtering egg sales (inclusive, ISO 8601 string, optional) */
  toDate?: string | null;
}
