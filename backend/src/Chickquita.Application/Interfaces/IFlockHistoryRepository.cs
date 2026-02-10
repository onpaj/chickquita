using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for FlockHistory entity operations.
/// </summary>
public interface IFlockHistoryRepository
{
    /// <summary>
    /// Gets a flock history entry by its ID.
    /// </summary>
    /// <param name="id">The history entry ID</param>
    /// <returns>The history entry if found, null otherwise</returns>
    Task<FlockHistory?> GetByIdAsync(Guid id);

    /// <summary>
    /// Updates an existing flock history entry.
    /// </summary>
    /// <param name="historyEntry">The history entry to update</param>
    /// <returns>The updated history entry</returns>
    Task<FlockHistory> UpdateAsync(FlockHistory historyEntry);
}
