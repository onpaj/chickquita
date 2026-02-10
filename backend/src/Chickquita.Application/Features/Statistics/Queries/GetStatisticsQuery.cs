using Chickquita.Application.DTOs;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Statistics.Queries;

/// <summary>
/// Query to get detailed statistics for a date range.
/// Includes cost breakdown, production trends, and flock productivity.
/// </summary>
public sealed record GetStatisticsQuery : IRequest<Result<StatisticsDto>>
{
    /// <summary>
    /// Start date for the statistics period (inclusive).
    /// </summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// End date for the statistics period (inclusive).
    /// </summary>
    public required DateOnly EndDate { get; init; }
}
