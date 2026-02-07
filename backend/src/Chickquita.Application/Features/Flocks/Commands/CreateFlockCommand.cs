using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Command to create a new flock within a coop.
/// </summary>
public sealed record CreateFlockCommand : IRequest<Result<FlockDto>>
{
    /// <summary>
    /// Gets or sets the ID of the coop this flock belongs to.
    /// </summary>
    public Guid CoopId { get; set; }

    /// <summary>
    /// Gets or sets the user-defined identifier for the flock.
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the hatch date of the flock.
    /// </summary>
    public DateTime HatchDate { get; init; }

    /// <summary>
    /// Gets or sets the initial number of hens.
    /// </summary>
    public int InitialHens { get; init; }

    /// <summary>
    /// Gets or sets the initial number of roosters.
    /// </summary>
    public int InitialRoosters { get; init; }

    /// <summary>
    /// Gets or sets the initial number of chicks.
    /// </summary>
    public int InitialChicks { get; init; }

    /// <summary>
    /// Gets or sets optional notes for the initial composition.
    /// </summary>
    public string? Notes { get; init; }
}
