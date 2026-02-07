using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Purchases.Commands.Delete;

/// <summary>
/// Command to delete an existing purchase.
/// </summary>
public sealed record DeletePurchaseCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// Gets or sets the ID of the purchase to delete.
    /// </summary>
    public Guid PurchaseId { get; init; }
}
