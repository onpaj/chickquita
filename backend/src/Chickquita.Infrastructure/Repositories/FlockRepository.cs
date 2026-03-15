using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IFlockRepository
/// </summary>
public class FlockRepository : IFlockRepository
{
    private readonly ApplicationDbContext _context;

    public FlockRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<Flock>> GetAllAsync(bool includeArchived = false)
    {
        var query = _context.Flocks
            .Include(f => f.History.OrderByDescending(h => h.ChangeDate))
            .AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(f => f.IsActive);
        }

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Flock>> GetByCoopIdAsync(Guid coopId, bool includeArchived = false)
    {
        var query = _context.Flocks
            .Include(f => f.History.OrderByDescending(h => h.ChangeDate))
            .Where(f => f.CoopId == coopId);

        if (!includeArchived)
        {
            query = query.Where(f => f.IsActive);
        }

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Flock?> GetByIdAsync(Guid id)
    {
        return await _context.Flocks
            .Include(f => f.History.OrderByDescending(h => h.ChangeDate))
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    /// <inheritdoc />
    public async Task<Flock?> GetByIdWithoutHistoryAsync(Guid id)
    {
        return await _context.Flocks
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    /// <inheritdoc />
    public async Task<Flock> AddAsync(Flock flock)
    {
        if (flock == null)
        {
            throw new ArgumentNullException(nameof(flock));
        }

        await _context.Flocks.AddAsync(flock);
        await _context.SaveChangesAsync();

        return flock;
    }

    /// <inheritdoc />
    public async Task<Flock> UpdateAsync(Flock flock)
    {
        if (flock == null)
        {
            throw new ArgumentNullException(nameof(flock));
        }

        var entry = _context.Entry(flock);
        if (entry.State == EntityState.Detached)
        {
            // Entity was not loaded by this context (e.g. came from a previous scope).
            // Calling Update() walks the entity graph via TrackGraph and marks everything
            // as Modified so SaveChanges generates the correct UPDATE statements.
            _context.Flocks.Update(flock);
        }
        else
        {
            // Entity is already tracked by this context (loaded via GetByIdWithoutHistoryAsync).
            // EF Core's snapshot change tracker detects property mutations automatically.
            //
            // When a new FlockHistory entry is added via UpdateComposition, EF Core's internal
            // observable collection for History immediately fires a CollectionChanged event.
            // The fix-up handler tracks the new entity — but because it has a non-default GUID
            // key, EF Core assumes the entity already exists in the DB and marks it Modified
            // rather than Added. SaveChanges then generates UPDATE instead of INSERT, which
            // affects 0 rows (the row doesn't exist yet) and throws DbUpdateConcurrencyException.
            //
            // Fix: correct the state from Modified to Added for each entry currently in
            // flock.History. These are all new entities added since the flock was loaded
            // (GetByIdWithoutHistoryAsync loads without history, so the collection starts empty).
            foreach (var historyItem in flock.History)
            {
                var historyEntry = _context.Entry(historyItem);
                if (historyEntry.State == EntityState.Modified)
                {
                    historyEntry.State = EntityState.Added;
                }
            }
        }

        await _context.SaveChangesAsync();

        return flock;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var flock = await _context.Flocks.FindAsync(id);
        if (flock != null)
        {
            _context.Flocks.Remove(flock);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByIdentifierInCoopAsync(Guid coopId, string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return false;
        }

        return await _context.Flocks
            .AnyAsync(f => f.CoopId == coopId && f.Identifier.ToLower() == identifier.ToLower());
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByIdentifierInCoopAsync(Guid coopId, string identifier, Guid excludeFlockId)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return false;
        }

        return await _context.Flocks
            .AnyAsync(f => f.CoopId == coopId
                && f.Identifier.ToLower() == identifier.ToLower()
                && f.Id != excludeFlockId);
    }

    /// <inheritdoc />
    public async Task<int> GetCountByCoopIdAsync(Guid coopId)
    {
        return await _context.Flocks
            .CountAsync(f => f.CoopId == coopId);
    }
}
