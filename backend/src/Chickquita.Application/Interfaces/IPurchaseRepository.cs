using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for Purchase entity operations.
/// </summary>
public interface IPurchaseRepository
{
    /// <summary>
    /// Gets all purchases for the current tenant.
    /// </summary>
    /// <returns>A list of all purchases ordered by PurchaseDate descending</returns>
    Task<List<Purchase>> GetAllAsync();

    /// <summary>
    /// Gets a purchase by its ID with navigation properties loaded.
    /// </summary>
    /// <param name="id">The purchase ID</param>
    /// <returns>The purchase if found, null otherwise</returns>
    Task<Purchase?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets purchases within a date range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>A list of purchases within the date range ordered by PurchaseDate descending</returns>
    Task<List<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets purchases by type.
    /// </summary>
    /// <param name="type">The purchase type</param>
    /// <returns>A list of purchases of the specified type ordered by PurchaseDate descending</returns>
    Task<List<Purchase>> GetByTypeAsync(PurchaseType type);

    /// <summary>
    /// Gets distinct purchase names for autocomplete functionality.
    /// </summary>
    /// <returns>A list of distinct purchase names</returns>
    Task<List<string>> GetDistinctNamesAsync();

    /// <summary>
    /// Adds a new purchase.
    /// </summary>
    /// <param name="purchase">The purchase to add</param>
    /// <returns>The added purchase</returns>
    Task<Purchase> AddAsync(Purchase purchase);

    /// <summary>
    /// Updates an existing purchase.
    /// </summary>
    /// <param name="purchase">The purchase to update</param>
    /// <returns>The updated purchase</returns>
    Task<Purchase> UpdateAsync(Purchase purchase);

    /// <summary>
    /// Deletes a purchase by its ID.
    /// </summary>
    /// <param name="id">The purchase ID</param>
    Task DeleteAsync(Guid id);
}
