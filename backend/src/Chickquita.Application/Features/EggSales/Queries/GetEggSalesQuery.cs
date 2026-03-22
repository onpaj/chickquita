using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.EggSales.Queries;

/// <summary>
/// Query to get all egg sales for the current tenant with optional date range filters.
/// </summary>
public sealed record GetEggSalesQuery : IRequest<Result<List<EggSaleDto>>>
{
    /// <summary>
    /// Gets or sets the start date for filtering egg sales (inclusive).
    /// If null, no lower date bound is applied.
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Gets or sets the end date for filtering egg sales (inclusive).
    /// If null, no upper date bound is applied.
    /// </summary>
    public DateTime? ToDate { get; init; }
}
