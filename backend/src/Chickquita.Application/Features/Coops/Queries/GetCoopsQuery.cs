using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Coops.Queries;

/// <summary>
/// Query to get all coops for the current tenant.
/// </summary>
public sealed record GetCoopsQuery : IRequest<Result<List<CoopDto>>>
{
    /// <summary>
    /// Gets or sets whether to include archived (inactive) coops.
    /// Defaults to false.
    /// </summary>
    public bool IncludeArchived { get; init; } = false;
}
