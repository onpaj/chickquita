namespace Chickquita.Application.Interfaces;

/// <summary>
/// Represents a unit of work that can commit all tracked changes to the database in a single transaction.
/// Handlers call SaveChangesAsync once after completing all repository operations,
/// which guarantees that multiple aggregate changes are committed atomically.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes tracked by the current context to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
