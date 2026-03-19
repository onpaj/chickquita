using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for EggSale entity operations.
/// </summary>
public interface IEggSaleRepository
{
    /// <summary>
    /// Gets all egg sales for the current tenant.
    /// </summary>
    /// <returns>A list of all egg sales ordered by Date descending</returns>
    Task<List<EggSale>> GetAllAsync();

    /// <summary>
    /// Gets egg sales with optional date range filters.
    /// </summary>
    /// <param name="fromDate">Optional start date filter (inclusive)</param>
    /// <param name="toDate">Optional end date filter (inclusive)</param>
    /// <returns>A list of filtered egg sales ordered by Date descending</returns>
    Task<List<EggSale>> GetWithFiltersAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Gets an egg sale by its ID.
    /// </summary>
    /// <param name="id">The egg sale ID</param>
    /// <returns>The egg sale if found, null otherwise</returns>
    Task<EggSale?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new egg sale.
    /// </summary>
    /// <param name="eggSale">The egg sale to add</param>
    /// <returns>The added egg sale</returns>
    Task<EggSale> AddAsync(EggSale eggSale);

    /// <summary>
    /// Updates an existing egg sale.
    /// </summary>
    /// <param name="eggSale">The egg sale to update</param>
    /// <returns>The updated egg sale</returns>
    Task<EggSale> UpdateAsync(EggSale eggSale);

    /// <summary>
    /// Deletes an egg sale by its ID.
    /// </summary>
    /// <param name="id">The egg sale ID</param>
    Task DeleteAsync(Guid id);
}
