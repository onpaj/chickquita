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
            .OrderByDescending(f => f.HatchDate)
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
            .OrderByDescending(f => f.HatchDate)
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

        _context.Flocks.Update(flock);
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
