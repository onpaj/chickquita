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
    private readonly ICurrentUserService _currentUserService;

    public PurchaseRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    private Guid GetTenantId() =>
        _currentUserService.TenantId ?? throw new InvalidOperationException("No tenant context");

    /// <inheritdoc />
    public async Task<List<Purchase>> GetAllAsync()
    {
        var tenantId = GetTenantId();
        return await _context.Purchases
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Coop)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Purchase>> GetWithFiltersAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        PurchaseType? type = null,
        Guid? coopId = null)
    {
        var tenantId = GetTenantId();
        var query = _context.Purchases
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Coop)
            .AsQueryable();

        // Apply date range filter
        if (fromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(p => p.PurchaseDate >= fromDateUtc);
        }

        if (toDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(toDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(p => p.PurchaseDate <= toDateUtc);
        }

        // Apply type filter
        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type.Value);
        }

        // Apply coop filter
        if (coopId.HasValue)
        {
            query = query.Where(p => p.CoopId == coopId.Value);
        }

        return await query
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Purchase?> GetByIdAsync(Guid id)
    {
        var tenantId = GetTenantId();
        return await _context.Purchases
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Coop)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<string>> GetDistinctNamesAsync()
    {
        var tenantId = GetTenantId();
        return await _context.Purchases
            .Where(p => p.TenantId == tenantId)
            .Select(p => p.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<string>> GetDistinctNamesByQueryAsync(string query, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<string>();
        }

        var lowerQuery = query.ToLower();
        var tenantId = GetTenantId();

        return await _context.Purchases
            .Where(p => p.TenantId == tenantId && p.Name.ToLower().Contains(lowerQuery))
            .Select(p => p.Name)
            .Distinct()
            .OrderBy(n => n)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Purchase> AddAsync(Purchase purchase)
    {
        await _context.Purchases.AddAsync(purchase);
        return purchase;
    }

    /// <inheritdoc />
    public async Task<Purchase> UpdateAsync(Purchase purchase)
    {
        _context.Purchases.Update(purchase);
        return purchase;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        await _context.Purchases
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();
    }
}
