using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Purchases.Queries;

/// <summary>
/// Query to get distinct purchase names for autocomplete functionality.
/// </summary>
public sealed record GetPurchaseNamesQuery : IRequest<Result<List<string>>>
{
    /// <summary>
    /// Gets or sets the search query string (case-insensitive).
    /// If null or empty, returns an empty list.
    /// </summary>
    public string? Query { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of results to return.
    /// Defaults to 20.
    /// </summary>
    public int Limit { get; init; } = 20;
}
