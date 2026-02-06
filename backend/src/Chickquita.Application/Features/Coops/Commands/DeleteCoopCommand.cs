using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Command to delete a coop.
/// </summary>
public sealed record DeleteCoopCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// Gets or sets the ID of the coop to delete.
    /// </summary>
    public Guid Id { get; init; }
}
