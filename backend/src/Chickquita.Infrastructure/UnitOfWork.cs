using Chickquita.Application.Interfaces;
using Chickquita.Infrastructure.Data;

namespace Chickquita.Infrastructure;

/// <summary>
/// EF Core implementation of IUnitOfWork.
/// Delegates SaveChangesAsync to the shared ApplicationDbContext instance so that
/// all repository changes tracked within the same DI scope are committed atomically.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
