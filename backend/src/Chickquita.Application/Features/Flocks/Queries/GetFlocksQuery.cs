using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Queries;

/// <summary>
/// Query to get flocks. Can retrieve all flocks for the tenant or filter by coop.
/// </summary>
public sealed record GetFlocksQuery : IRequest<Result<List<FlockDto>>>
{
    /// <summary>
    /// Gets or sets the optional ID of the coop to filter flocks.
    /// If null, returns all flocks for the current tenant.
    /// </summary>
    public Guid? CoopId { get; init; }

    /// <summary>
    /// Gets or sets whether to include inactive (archived) flocks.
    /// Defaults to false.
    /// </summary>
    public bool IncludeInactive { get; init; } = false;
}
