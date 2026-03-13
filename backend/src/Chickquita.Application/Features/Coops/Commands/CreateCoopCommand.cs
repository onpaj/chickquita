using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;
using Chickquita.Application.Interfaces;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Command to create a new coop.
/// </summary>
public sealed record CreateCoopCommand : IRequest<Result<CoopDto>, IAuthorizedRequest>
{
    /// <summary>
    /// Gets or sets the name of the coop.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional location description.
    /// </summary>
    public string? Location { get; init; }
}
