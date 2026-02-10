using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Statistics.Queries;

/// <summary>
/// Handler for GetStatisticsQuery that retrieves detailed statistics for a date range.
/// </summary>
public sealed class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, Result<StatisticsDto>>
{
    private readonly IStatisticsRepository _statisticsRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetStatisticsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetStatisticsQueryHandler"/> class.
    /// </summary>
    public GetStatisticsQueryHandler(
        IStatisticsRepository statisticsRepository,
        ICurrentUserService currentUserService,
        ILogger<GetStatisticsQueryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetStatisticsQuery by aggregating statistics for the date range.
    /// </summary>
    public async Task<Result<StatisticsDto>> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing GetStatisticsQuery for period: {StartDate} to {EndDate}",
            request.StartDate,
            request.EndDate);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetStatisticsQuery: User is not authenticated");
                return Result<StatisticsDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetStatisticsQuery: Tenant ID not found");
                return Result<StatisticsDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Validate date range
            if (request.StartDate > request.EndDate)
            {
                _logger.LogWarning(
                    "GetStatisticsQuery: Invalid date range - StartDate ({StartDate}) > EndDate ({EndDate})",
                    request.StartDate,
                    request.EndDate);
                return Result<StatisticsDto>.Failure(
                    Error.Validation("StartDate must be before or equal to EndDate"));
            }

            // Retrieve statistics (tenant isolation is handled by RLS and repository)
            var stats = await _statisticsRepository.GetStatisticsAsync(request.StartDate, request.EndDate);

            _logger.LogInformation(
                "Retrieved statistics for tenant: {TenantId}, Period: {StartDate} to {EndDate}, Total Eggs: {TotalEggs}, Total Cost: {TotalCost}",
                tenantId.Value,
                request.StartDate,
                request.EndDate,
                stats.Summary.TotalEggs,
                stats.Summary.TotalCost);

            return Result<StatisticsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving statistics for period: {StartDate} to {EndDate}",
                request.StartDate,
                request.EndDate);

            return Result<StatisticsDto>.Failure(
                Error.Failure($"Failed to retrieve statistics: {ex.Message}"));
        }
    }
}
