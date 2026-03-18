using Chickquita.Application.DTOs;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.EggSales.Queries;

/// <summary>
/// Query to get all egg sales for the current tenant with optional date range filters.
/// </summary>
public sealed record GetEggSalesQuery : IRequest<Result<List<EggSaleDto>>>
{
    /// <summary>
    /// Gets the optional start date for filtering egg sales (inclusive).
    /// If null, no lower date bound is applied.
    /// </summary>
    public DateTime? DateFrom { get; init; }

    /// <summary>
    /// Gets the optional end date for filtering egg sales (inclusive).
    /// If null, no upper date bound is applied.
    /// </summary>
    public DateTime? DateTo { get; init; }
}
