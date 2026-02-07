using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Purchases.Queries;

/// <summary>
/// Query to get all purchases for the current tenant with optional filters.
/// </summary>
public sealed record GetPurchasesQuery : IRequest<Result<List<PurchaseDto>>>
{
    /// <summary>
    /// Gets or sets the start date for filtering purchases (inclusive).
    /// If null, no lower date bound is applied.
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Gets or sets the end date for filtering purchases (inclusive).
    /// If null, no upper date bound is applied.
    /// </summary>
    public DateTime? ToDate { get; init; }

    /// <summary>
    /// Gets or sets the purchase type filter.
    /// If null, all purchase types are included.
    /// </summary>
    public PurchaseType? Type { get; init; }

    /// <summary>
    /// Gets or sets the flock ID filter.
    /// If null, purchases for all coops/flocks are included.
    /// Note: Purchases are associated with coops, not flocks directly.
    /// This filter will match purchases associated with the coop that contains this flock.
    /// </summary>
    public Guid? FlockId { get; init; }
}
