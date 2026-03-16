using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IDailyRecordRepository.
/// </summary>
public class DailyRecordRepository : IDailyRecordRepository
{
    private readonly ApplicationDbContext _context;

    public DailyRecordRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<DailyRecord>> GetAllAsync()
    {
        return await _context.DailyRecords
            .Include(d => d.Flock)
                .ThenInclude(f => f.Coop)
            .OrderByDescending(d => d.RecordDate)
            .ThenByDescending(d => d.CollectionTime)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<DailyRecord>> GetByFlockIdAsync(Guid flockId)
    {
        return await _context.DailyRecords
            .Include(d => d.Flock)
                .ThenInclude(f => f.Coop)
            .Where(d => d.FlockId == flockId)
            .OrderByDescending(d => d.RecordDate)
            .ThenByDescending(d => d.CollectionTime)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<DailyRecord>> GetByFlockIdAndDateRangeAsync(Guid flockId, DateTime startDate, DateTime endDate)
    {
        // Normalize dates to UTC date only
        var startDateUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

        return await _context.DailyRecords
            .Include(d => d.Flock)
                .ThenInclude(f => f.Coop)
            .Where(d => d.FlockId == flockId
                && d.RecordDate >= startDateUtc
                && d.RecordDate <= endDateUtc)
            .OrderByDescending(d => d.RecordDate)
            .ThenByDescending(d => d.CollectionTime)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<DailyRecord?> GetByIdAsync(Guid id)
    {
        return await _context.DailyRecords
            .Include(d => d.Flock)
                .ThenInclude(f => f.Coop)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <inheritdoc />
    public async Task<DailyRecord?> GetByIdWithoutNavigationAsync(Guid id)
    {
        return await _context.DailyRecords
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <inheritdoc />
    public async Task<DailyRecord> AddAsync(DailyRecord dailyRecord)
    {
        if (dailyRecord == null)
        {
            throw new ArgumentNullException(nameof(dailyRecord));
        }

        await _context.DailyRecords.AddAsync(dailyRecord);
        await _context.SaveChangesAsync();

        return dailyRecord;
    }

    /// <inheritdoc />
    public async Task<DailyRecord> UpdateAsync(DailyRecord dailyRecord)
    {
        if (dailyRecord == null)
        {
            throw new ArgumentNullException(nameof(dailyRecord));
        }

        _context.DailyRecords.Update(dailyRecord);
        await _context.SaveChangesAsync();

        return dailyRecord;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        await _context.DailyRecords
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetCountByFlockIdAsync(Guid flockId)
    {
        return await _context.DailyRecords
            .CountAsync(d => d.FlockId == flockId);
    }

    /// <inheritdoc />
    public async Task<int> GetTotalEggCountByFlockIdAsync(Guid flockId)
    {
        return await _context.DailyRecords
            .Where(d => d.FlockId == flockId)
            .SumAsync(d => d.EggCount);
    }
}
