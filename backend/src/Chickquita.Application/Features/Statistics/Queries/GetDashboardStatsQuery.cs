using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Statistics.Queries;

/// <summary>
/// Query to get dashboard statistics for the current tenant.
/// Aggregates key metrics including coops, flocks, and animal counts.
/// </summary>
public sealed record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>
{
}
