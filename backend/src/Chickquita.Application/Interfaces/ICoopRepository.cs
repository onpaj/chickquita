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
    /// <returns>A list of all coops</returns>
    Task<List<Coop>> GetAllAsync();

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
}
