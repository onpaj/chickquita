using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for DailyRecord entity operations.
/// </summary>
public interface IDailyRecordRepository
{
    /// <summary>
    /// Gets all daily records for the current tenant.
    /// </summary>
    /// <returns>A list of all daily records ordered by RecordDate descending</returns>
    Task<List<DailyRecord>> GetAllAsync();

    /// <summary>
    /// Gets all daily records for a specific flock.
    /// </summary>
    /// <param name="flockId">The flock ID</param>
    /// <returns>A list of daily records for the specified flock ordered by RecordDate descending</returns>
    Task<List<DailyRecord>> GetByFlockIdAsync(Guid flockId);

    /// <summary>
    /// Gets daily records for a specific flock within a date range.
    /// </summary>
    /// <param name="flockId">The flock ID</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>A list of daily records within the date range ordered by RecordDate descending</returns>
    Task<List<DailyRecord>> GetByFlockIdAndDateRangeAsync(Guid flockId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets a daily record by its ID with flock navigation property loaded.
    /// </summary>
    /// <param name="id">The daily record ID</param>
    /// <returns>The daily record if found, null otherwise</returns>
    Task<DailyRecord?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a daily record by its ID without loading navigation properties.
    /// Use this when you don't need the flock data.
    /// </summary>
    /// <param name="id">The daily record ID</param>
    /// <returns>The daily record if found, null otherwise</returns>
    Task<DailyRecord?> GetByIdWithoutNavigationAsync(Guid id);

    /// <summary>
    /// Gets a daily record for a specific flock and date.
    /// </summary>
    /// <param name="flockId">The flock ID</param>
    /// <param name="recordDate">The record date</param>
    /// <returns>The daily record if found, null otherwise</returns>
    Task<DailyRecord?> GetByFlockIdAndDateAsync(Guid flockId, DateTime recordDate);

    /// <summary>
    /// Adds a new daily record.
    /// </summary>
    /// <param name="dailyRecord">The daily record to add</param>
    /// <returns>The added daily record</returns>
    Task<DailyRecord> AddAsync(DailyRecord dailyRecord);

    /// <summary>
    /// Updates an existing daily record.
    /// </summary>
    /// <param name="dailyRecord">The daily record to update</param>
    /// <returns>The updated daily record</returns>
    Task<DailyRecord> UpdateAsync(DailyRecord dailyRecord);

    /// <summary>
    /// Deletes a daily record by its ID.
    /// </summary>
    /// <param name="id">The daily record ID</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a daily record exists for a specific flock and date.
    /// </summary>
    /// <param name="flockId">The flock ID</param>
    /// <param name="recordDate">The record date</param>
    /// <returns>True if a record exists for the flock and date, false otherwise</returns>
    Task<bool> ExistsForFlockAndDateAsync(Guid flockId, DateTime recordDate);

    /// <summary>
    /// Checks if a daily record exists for a specific flock and date, excluding a specific record ID.
    /// Useful for update operations where you want to check uniqueness but exclude the current record.
    /// </summary>
    /// <param name="flockId">The flock ID</param>
    /// <param name="recordDate">The record date</param>
    /// <param name="excludeRecordId">The record ID to exclude from the check</param>
    /// <returns>True if a record exists for the flock and date (excluding the specified record), false otherwise</returns>
    Task<bool> ExistsForFlockAndDateAsync(Guid flockId, DateTime recordDate, Guid excludeRecordId);

    /// <summary>
    /// Gets the count of daily records for a specific flock.
    /// </summary>
    /// <param name="flockId">The flock ID</param>
    /// <returns>The number of daily records for the flock</returns>
    Task<int> GetCountByFlockIdAsync(Guid flockId);

    /// <summary>
    /// Gets the total egg count for a specific flock.
    /// </summary>
    /// <param name="flockId">The flock ID</param>
    /// <returns>The total number of eggs collected for the flock</returns>
    Task<int> GetTotalEggCountByFlockIdAsync(Guid flockId);
}
