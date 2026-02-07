using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.DailyRecords.Queries;

/// <summary>
/// Handler for GetDailyRecordsQuery that retrieves daily records with optional filtering.
/// </summary>
public sealed class GetDailyRecordsQueryHandler : IRequestHandler<GetDailyRecordsQuery, Result<List<DailyRecordDto>>>
{
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly IFlockRepository _flockRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetDailyRecordsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDailyRecordsQueryHandler"/> class.
    /// </summary>
    /// <param name="dailyRecordRepository">The daily record repository.</param>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public GetDailyRecordsQueryHandler(
        IDailyRecordRepository dailyRecordRepository,
        IFlockRepository flockRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetDailyRecordsQueryHandler> logger)
    {
        _dailyRecordRepository = dailyRecordRepository;
        _flockRepository = flockRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetDailyRecordsQuery by retrieving daily records for the current tenant.
    /// Supports filtering by flock ID and date range.
    /// </summary>
    /// <param name="request">The get daily records query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of daily record DTOs ordered by RecordDate descending.</returns>
    public async Task<Result<List<DailyRecordDto>>> Handle(GetDailyRecordsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing GetDailyRecordsQuery for FlockId: {FlockId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.FlockId?.ToString() ?? "All",
            request.StartDate?.ToString("yyyy-MM-dd") ?? "None",
            request.EndDate?.ToString("yyyy-MM-dd") ?? "None");

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetDailyRecordsQuery: User is not authenticated");
                return Result<List<DailyRecordDto>>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetDailyRecordsQuery: Tenant ID not found");
                return Result<List<DailyRecordDto>>.Failure(Error.Unauthorized("Tenant not found"));
            }

            List<DailyRecord> dailyRecords;

            // Case 1: FlockId + Date Range
            if (request.FlockId.HasValue && request.StartDate.HasValue && request.EndDate.HasValue)
            {
                // Verify the flock exists and belongs to the current tenant
                var flock = await _flockRepository.GetByIdAsync(request.FlockId.Value);
                if (flock == null)
                {
                    _logger.LogWarning(
                        "GetDailyRecordsQuery: Flock not found with ID: {FlockId}",
                        request.FlockId);
                    return Result<List<DailyRecordDto>>.Failure(Error.NotFound("Flock not found"));
                }

                dailyRecords = await _dailyRecordRepository.GetByFlockIdAndDateRangeAsync(
                    request.FlockId.Value,
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            // Case 2: FlockId only (with possible partial date range)
            else if (request.FlockId.HasValue)
            {
                // Verify the flock exists and belongs to the current tenant
                var flock = await _flockRepository.GetByIdAsync(request.FlockId.Value);
                if (flock == null)
                {
                    _logger.LogWarning(
                        "GetDailyRecordsQuery: Flock not found with ID: {FlockId}",
                        request.FlockId);
                    return Result<List<DailyRecordDto>>.Failure(Error.NotFound("Flock not found"));
                }

                // Get all records for the flock
                var allFlockRecords = await _dailyRecordRepository.GetByFlockIdAsync(request.FlockId.Value);

                // Apply date range filtering if provided
                dailyRecords = ApplyDateRangeFilter(allFlockRecords, request.StartDate, request.EndDate);
            }
            // Case 3: Date Range only (no FlockId)
            else if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                // Get all records for the tenant
                var allRecords = await _dailyRecordRepository.GetAllAsync();

                // Apply date range filtering
                dailyRecords = ApplyDateRangeFilter(allRecords, request.StartDate, request.EndDate);
            }
            // Case 4: No filters - all records for the tenant
            else
            {
                dailyRecords = await _dailyRecordRepository.GetAllAsync();
            }

            _logger.LogInformation(
                "Retrieved {Count} daily records for FlockId: {FlockId}, TenantId: {TenantId}, StartDate: {StartDate}, EndDate: {EndDate}",
                dailyRecords.Count,
                request.FlockId?.ToString() ?? "All",
                tenantId.Value,
                request.StartDate?.ToString("yyyy-MM-dd") ?? "None",
                request.EndDate?.ToString("yyyy-MM-dd") ?? "None");

            // Map to DTOs (already ordered by RecordDate DESC from repository)
            var dailyRecordDtos = _mapper.Map<List<DailyRecordDto>>(dailyRecords);

            return Result<List<DailyRecordDto>>.Success(dailyRecordDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving daily records for FlockId: {FlockId}, StartDate: {StartDate}, EndDate: {EndDate}",
                request.FlockId?.ToString() ?? "All",
                request.StartDate?.ToString("yyyy-MM-dd") ?? "None",
                request.EndDate?.ToString("yyyy-MM-dd") ?? "None");

            return Result<List<DailyRecordDto>>.Failure(
                Error.Failure($"Failed to retrieve daily records: {ex.Message}"));
        }
    }

    /// <summary>
    /// Applies date range filtering to a list of daily records.
    /// Handles partial date ranges (StartDate only, EndDate only, or both).
    /// </summary>
    /// <param name="records">The records to filter.</param>
    /// <param name="startDate">Optional start date (inclusive).</param>
    /// <param name="endDate">Optional end date (inclusive).</param>
    /// <returns>Filtered list of daily records ordered by RecordDate descending.</returns>
    private static List<DailyRecord> ApplyDateRangeFilter(
        List<DailyRecord> records,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = records.AsQueryable();

        if (startDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(d => d.RecordDate >= startDateUtc);
        }

        if (endDate.HasValue)
        {
            var endDateUtc = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(d => d.RecordDate <= endDateUtc);
        }

        return query
            .OrderByDescending(d => d.RecordDate)
            .ToList();
    }
}
