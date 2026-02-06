using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for Flock entity operations.
/// </summary>
public interface IFlockRepository
{
    /// <summary>
    /// Gets all flocks for the current tenant.
    /// </summary>
    /// <param name="includeArchived">Whether to include archived (inactive) flocks. Defaults to false.</param>
    /// <returns>A list of all flocks ordered by HatchDate descending</returns>
    Task<List<Flock>> GetAllAsync(bool includeArchived = false);

    /// <summary>
    /// Gets all flocks for a specific coop.
    /// </summary>
    /// <param name="coopId">The coop ID</param>
    /// <param name="includeArchived">Whether to include archived (inactive) flocks. Defaults to false.</param>
    /// <returns>A list of flocks for the specified coop ordered by HatchDate descending</returns>
    Task<List<Flock>> GetByCoopIdAsync(Guid coopId, bool includeArchived = false);

    /// <summary>
    /// Gets a flock by its ID with history loaded.
    /// </summary>
    /// <param name="id">The flock ID</param>
    /// <returns>The flock with history if found, null otherwise</returns>
    Task<Flock?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a flock by its ID without loading navigation properties.
    /// Use this when you don't need the history data.
    /// </summary>
    /// <param name="id">The flock ID</param>
    /// <returns>The flock if found, null otherwise</returns>
    Task<Flock?> GetByIdWithoutHistoryAsync(Guid id);

    /// <summary>
    /// Adds a new flock.
    /// </summary>
    /// <param name="flock">The flock to add</param>
    /// <returns>The added flock</returns>
    Task<Flock> AddAsync(Flock flock);

    /// <summary>
    /// Updates an existing flock.
    /// </summary>
    /// <param name="flock">The flock to update</param>
    /// <returns>The updated flock</returns>
    Task<Flock> UpdateAsync(Flock flock);

    /// <summary>
    /// Deletes a flock by its ID.
    /// </summary>
    /// <param name="id">The flock ID</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a flock with the specified identifier already exists within a coop for the current tenant.
    /// </summary>
    /// <param name="coopId">The coop ID</param>
    /// <param name="identifier">The flock identifier to check</param>
    /// <returns>True if a flock with this identifier exists in the coop, false otherwise</returns>
    Task<bool> ExistsByIdentifierInCoopAsync(Guid coopId, string identifier);

    /// <summary>
    /// Checks if a flock with the specified identifier already exists within a coop, excluding a specific flock ID.
    /// Useful for update operations where you want to check uniqueness but exclude the current flock.
    /// </summary>
    /// <param name="coopId">The coop ID</param>
    /// <param name="identifier">The flock identifier to check</param>
    /// <param name="excludeFlockId">The flock ID to exclude from the check</param>
    /// <returns>True if a flock with this identifier exists in the coop (excluding the specified flock), false otherwise</returns>
    Task<bool> ExistsByIdentifierInCoopAsync(Guid coopId, string identifier, Guid excludeFlockId);

    /// <summary>
    /// Gets the count of flocks in a specific coop (includes both active and archived flocks).
    /// </summary>
    /// <param name="coopId">The coop ID</param>
    /// <returns>The number of flocks in the coop</returns>
    Task<int> GetCountByCoopIdAsync(Guid coopId);
}
