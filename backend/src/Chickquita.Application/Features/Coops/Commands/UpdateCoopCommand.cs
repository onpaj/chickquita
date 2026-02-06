using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Command to update an existing coop.
/// </summary>
public sealed record UpdateCoopCommand : IRequest<Result<CoopDto>>
{
    /// <summary>
    /// Gets or sets the ID of the coop to update.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the name of the coop.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional location description.
    /// </summary>
    public string? Location { get; init; }
}
