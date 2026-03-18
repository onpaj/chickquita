using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for EggSale entity operations.
/// </summary>
public interface IEggSaleRepository
{
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

    /// <summary>
    /// Gets an egg sale by its ID.
    /// </summary>
    /// <param name="id">The egg sale ID</param>
    /// <returns>The egg sale if found, null otherwise</returns>
    Task<EggSale?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets egg sales for the specified tenant with optional date range filters.
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="dateFrom">Optional start date filter (inclusive)</param>
    /// <param name="dateTo">Optional end date filter (inclusive)</param>
    /// <returns>A collection of egg sales matching the filters, ordered by date descending</returns>
    Task<IEnumerable<EggSale>> GetWithFiltersAsync(Guid tenantId, DateTime? dateFrom = null, DateTime? dateTo = null);
}
