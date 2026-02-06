using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for Coop entity operations.
/// </summary>
public interface ICoopRepository
{
    /// <summary>
    /// Gets all coops for the current tenant.
    /// </summary>
    /// <param name="includeArchived">Whether to include archived (inactive) coops. Defaults to false.</param>
    /// <returns>A list of all coops ordered by CreatedAt descending</returns>
    Task<List<Coop>> GetAllAsync(bool includeArchived = false);

    /// <summary>
    /// Gets a coop by its ID.
    /// </summary>
    /// <param name="id">The coop ID</param>
    /// <returns>The coop if found, null otherwise</returns>
    Task<Coop?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new coop.
    /// </summary>
    /// <param name="coop">The coop to add</param>
    /// <returns>The added coop</returns>
    Task<Coop> AddAsync(Coop coop);

    /// <summary>
    /// Updates an existing coop.
    /// </summary>
    /// <param name="coop">The coop to update</param>
    /// <returns>The updated coop</returns>
    Task<Coop> UpdateAsync(Coop coop);

    /// <summary>
    /// Deletes a coop by its ID.
    /// </summary>
    /// <param name="id">The coop ID</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a coop with the specified name already exists for the current tenant.
    /// </summary>
    /// <param name="name">The coop name to check</param>
    /// <returns>True if a coop with this name exists, false otherwise</returns>
    Task<bool> ExistsByNameAsync(string name);

    /// <summary>
    /// Checks if a coop has any associated flocks.
    /// </summary>
    /// <param name="coopId">The coop ID to check</param>
    /// <returns>True if the coop has flocks, false otherwise</returns>
    Task<bool> HasFlocksAsync(Guid coopId);

    /// <summary>
    /// Gets the count of flocks associated with a coop (includes both active and archived flocks).
    /// </summary>
    /// <param name="coopId">The coop ID</param>
    /// <returns>The number of flocks associated with the coop</returns>
    Task<int> GetFlocksCountAsync(Guid coopId);
}
