using Chickquita.Application.Interfaces;
using Chickquita.Infrastructure.Data;

namespace Chickquita.Infrastructure;

/// <summary>
/// EF Core implementation of IUnitOfWork that delegates to ApplicationDbContext.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
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
