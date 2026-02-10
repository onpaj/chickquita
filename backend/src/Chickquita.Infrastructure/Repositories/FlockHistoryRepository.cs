using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IFlockHistoryRepository
/// </summary>
public class FlockHistoryRepository : IFlockHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public FlockHistoryRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<FlockHistory?> GetByIdAsync(Guid id)
    {
        return await _context.FlockHistory
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    /// <inheritdoc />
    public async Task<FlockHistory> UpdateAsync(FlockHistory historyEntry)
    {
        if (historyEntry == null)
        {
            throw new ArgumentNullException(nameof(historyEntry));
        }

        _context.FlockHistory.Update(historyEntry);
        await _context.SaveChangesAsync();

        return historyEntry;
    }
}
