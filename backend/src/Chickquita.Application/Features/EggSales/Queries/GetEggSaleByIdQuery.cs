using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.EggSales.Queries;

/// <summary>
/// Query to get a single egg sale by its ID.
/// </summary>
public sealed record GetEggSaleByIdQuery : IRequest<Result<EggSaleDto>>
{
    /// <summary>
    /// Gets or sets the ID of the egg sale to retrieve.
    /// </summary>
    public Guid Id { get; init; }
}
