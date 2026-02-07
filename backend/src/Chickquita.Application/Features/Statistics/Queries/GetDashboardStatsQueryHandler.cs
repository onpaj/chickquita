using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Statistics.Queries;

/// <summary>
/// Handler for GetDashboardStatsQuery that retrieves aggregated statistics for the current tenant's dashboard.
/// Optimized for performance with a single database round-trip.
/// </summary>
public sealed class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IStatisticsRepository _statisticsRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDashboardStatsQueryHandler"/> class.
    /// </summary>
    /// <param name="statisticsRepository">The statistics repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    public GetDashboardStatsQueryHandler(
        IStatisticsRepository statisticsRepository,
        ICurrentUserService currentUserService,
        ILogger<GetDashboardStatsQueryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetDashboardStatsQuery by aggregating statistics across coops and flocks.
    /// </summary>
    /// <param name="request">The get dashboard stats query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the dashboard statistics DTO.</returns>
    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing GetDashboardStatsQuery");

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetDashboardStatsQuery: User is not authenticated");
                return Result<DashboardStatsDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetDashboardStatsQuery: Tenant ID not found");
                return Result<DashboardStatsDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Retrieve dashboard statistics (tenant isolation is handled by RLS and repository)
            var stats = await _statisticsRepository.GetDashboardStatsAsync();

            _logger.LogInformation(
                "Retrieved dashboard stats for tenant: {TenantId} - Coops: {TotalCoops}, Flocks: {ActiveFlocks}, Hens: {TotalHens}, Total Animals: {TotalAnimals}",
                tenantId.Value,
                stats.TotalCoops,
                stats.ActiveFlocks,
                stats.TotalHens,
                stats.TotalAnimals);

            return Result<DashboardStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving dashboard statistics");

            return Result<DashboardStatsDto>.Failure(
                Error.Failure($"Failed to retrieve dashboard statistics: {ex.Message}"));
        }
    }
}
