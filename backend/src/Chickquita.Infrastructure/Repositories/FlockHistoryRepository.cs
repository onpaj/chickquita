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
    private readonly ICurrentUserService _currentUserService;

    public FlockHistoryRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    private Guid GetTenantId() =>
        _currentUserService.TenantId ?? throw new InvalidOperationException("No tenant context");

    /// <inheritdoc />
    public async Task<FlockHistory?> GetByIdAsync(Guid id)
    {
        var tenantId = GetTenantId();
        return await _context.FlockHistory
            .FirstOrDefaultAsync(h => h.Id == id && h.TenantId == tenantId);
    }

    /// <inheritdoc />
    public async Task<FlockHistory> UpdateAsync(FlockHistory historyEntry)
    {
        if (historyEntry == null)
        {
            throw new ArgumentNullException(nameof(historyEntry));
        }

        _context.FlockHistory.Update(historyEntry);
        return historyEntry;
    }
}
