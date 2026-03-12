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
    /// Start date for the statistics period (inclusive). Null means no lower bound (all time).
    /// </summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>
    /// End date for the statistics period (inclusive). Null means no upper bound (all time).
    /// </summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>
    /// Optional coop ID to filter statistics to a specific coop.
    /// </summary>
    public Guid? CoopId { get; init; }

    /// <summary>
    /// Optional flock ID to filter statistics to a specific flock.
    /// </summary>
    public Guid? FlockId { get; init; }
}
