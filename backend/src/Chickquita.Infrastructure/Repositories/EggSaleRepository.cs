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
    public async Task<EggSale> AddAsync(EggSale eggSale)
    {
        await _context.EggSales.AddAsync(eggSale);
        return eggSale;
    }

    /// <inheritdoc />
    public async Task<EggSale> UpdateAsync(EggSale eggSale)
    {
        _context.EggSales.Update(eggSale);
        return await Task.FromResult(eggSale);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        await _context.EggSales
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync();
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
    public async Task<IEnumerable<EggSale>> GetWithFiltersAsync(
        Guid tenantId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.EggSales
            .Where(e => e.TenantId == tenantId)
            .AsQueryable();

        if (dateFrom.HasValue)
        {
            var dateFromUtc = DateTime.SpecifyKind(dateFrom.Value.Date, DateTimeKind.Utc);
            query = query.Where(e => e.Date >= dateFromUtc);
        }

        if (dateTo.HasValue)
        {
            var dateToUtc = DateTime.SpecifyKind(dateTo.Value.Date, DateTimeKind.Utc);
            query = query.Where(e => e.Date <= dateToUtc);
        }

        return await query
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }
}
