using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of ICoopRepository
/// </summary>
public class CoopRepository : ICoopRepository
{
    private readonly ApplicationDbContext _context;

    public CoopRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<Coop>> GetAllAsync(bool includeArchived = false)
    {
        var query = _context.Coops.AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Coop?> GetByIdAsync(Guid id)
    {
        return await _context.Coops
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc />
    public async Task<Coop> AddAsync(Coop coop)
    {
        if (coop == null)
        {
            throw new ArgumentNullException(nameof(coop));
        }

        await _context.Coops.AddAsync(coop);
        await _context.SaveChangesAsync();

        return coop;
    }

    /// <inheritdoc />
    public async Task<Coop> UpdateAsync(Coop coop)
    {
        if (coop == null)
        {
            throw new ArgumentNullException(nameof(coop));
        }

        _context.Coops.Update(coop);
        await _context.SaveChangesAsync();

        return coop;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var coop = await _context.Coops.FindAsync(id);
        if (coop != null)
        {
            _context.Coops.Remove(coop);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return await _context.Coops
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }
}
