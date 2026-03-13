using Chickquita.Domain.Common;
using MediatR;
using Chickquita.Application.Interfaces;

namespace Chickquita.Application.Features.Purchases.Commands.Delete;

/// <summary>
/// Command to delete an existing purchase.
/// </summary>
public sealed record DeletePurchaseCommand : IRequest<Result<bool>>, IAuthorizedRequest
{
    /// <summary>
    /// Gets or sets the ID of the purchase to delete.
    /// </summary>
    public Guid PurchaseId { get; init; }
}
