using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Purchases.Queries;

/// <summary>
/// Query to get a single purchase by its ID.
/// </summary>
public sealed record GetPurchaseByIdQuery : IRequest<Result<PurchaseDto>>
{
    /// <summary>
    /// Gets or sets the purchase ID.
    /// </summary>
    public Guid Id { get; init; }
}
