using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IPurchaseRepository.
/// </summary>
public class PurchaseRepository : IPurchaseRepository
{
    private readonly ApplicationDbContext _context;

    public PurchaseRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<Purchase>> GetAllAsync()
    {
        return await _context.Purchases
            .Include(p => p.Coop)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Purchase?> GetByIdAsync(Guid id)
    {
        return await _context.Purchases
            .Include(p => p.Coop)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        // Normalize dates to UTC date only
        var startDateUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

        return await _context.Purchases
            .Include(p => p.Coop)
            .Where(p => p.PurchaseDate >= startDateUtc && p.PurchaseDate <= endDateUtc)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Purchase>> GetByTypeAsync(PurchaseType type)
    {
        return await _context.Purchases
            .Include(p => p.Coop)
            .Where(p => p.Type == type)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetDistinctNamesAsync()
    {
        return await _context.Purchases
            .Select(p => p.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Purchase> AddAsync(Purchase purchase)
    {
        if (purchase == null)
        {
            throw new ArgumentNullException(nameof(purchase));
        }

        await _context.Purchases.AddAsync(purchase);
        await _context.SaveChangesAsync();

        return purchase;
    }

    /// <inheritdoc />
    public async Task<Purchase> UpdateAsync(Purchase purchase)
    {
        if (purchase == null)
        {
            throw new ArgumentNullException(nameof(purchase));
        }

        _context.Purchases.Update(purchase);
        await _context.SaveChangesAsync();

        return purchase;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var purchase = await _context.Purchases.FindAsync(id);
        if (purchase != null)
        {
            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
        }
    }
}
