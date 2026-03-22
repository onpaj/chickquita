using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.EggSales.Commands.Delete;

/// <summary>
/// Command to delete an existing egg sale record.
/// </summary>
public sealed record DeleteEggSaleCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// Gets or sets the ID of the egg sale to delete.
    /// </summary>
    public Guid EggSaleId { get; init; }
}
