namespace Chickquita.Application.Interfaces;

/// <summary>
/// Represents a unit of work that coordinates writing out changes as a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the store</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
