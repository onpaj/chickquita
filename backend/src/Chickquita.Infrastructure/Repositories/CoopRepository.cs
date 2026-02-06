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
    public async Task<List<Coop>> GetAllAsync()
    {
        return await _context.Coops
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
}
