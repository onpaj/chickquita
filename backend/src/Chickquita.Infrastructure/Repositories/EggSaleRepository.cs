using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IEggSaleRepository.
/// </summary>
public class EggSaleRepository : IEggSaleRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public EggSaleRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    private Guid GetTenantId() =>
        _currentUserService.TenantId ?? throw new InvalidOperationException("No tenant context");

    /// <inheritdoc />
    public async Task<List<EggSale>> GetAllAsync()
    {
        var tenantId = GetTenantId();
        return await _context.EggSales
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<EggSale>> GetWithFiltersAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var tenantId = GetTenantId();
        var query = _context.EggSales
            .Where(e => e.TenantId == tenantId)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(e => e.Date >= fromDateUtc);
        }

        if (toDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(toDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(e => e.Date <= toDateUtc);
        }

        return await query
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<EggSale?> GetByIdAsync(Guid id)
    {
        var tenantId = GetTenantId();
        return await _context.EggSales
            .Where(e => e.TenantId == tenantId)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc />
    public async Task<EggSale> AddAsync(EggSale eggSale)
    {
        await _context.EggSales.AddAsync(eggSale);
        return eggSale;
    }

    /// <inheritdoc />
    public async Task<EggSale> UpdateAsync(EggSale eggSale)
    {
        _context.EggSales.Update(eggSale);
        return eggSale;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        await _context.EggSales
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync();
    }
}
