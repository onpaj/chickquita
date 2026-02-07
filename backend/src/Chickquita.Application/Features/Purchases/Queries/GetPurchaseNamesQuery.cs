using Chickquita.Domain.Common;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

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

/// <summary>
/// Handler for GetPurchaseNamesQuery that retrieves distinct purchase names for autocomplete.
/// </summary>
public sealed class GetPurchaseNamesQueryHandler : IRequestHandler<GetPurchaseNamesQuery, Result<List<string>>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPurchaseNamesQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPurchaseNamesQueryHandler"/> class.
    /// </summary>
    /// <param name="purchaseRepository">The purchase repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    public GetPurchaseNamesQueryHandler(
        IPurchaseRepository purchaseRepository,
        ICurrentUserService currentUserService,
        ILogger<GetPurchaseNamesQueryHandler> logger)
    {
        _purchaseRepository = purchaseRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetPurchaseNamesQuery by retrieving filtered purchase names.
    /// </summary>
    /// <param name="request">The get purchase names query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of distinct purchase names.</returns>
    public async Task<Result<List<string>>> Handle(GetPurchaseNamesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing GetPurchaseNamesQuery - Query: {Query}, Limit: {Limit}",
            request.Query,
            request.Limit);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetPurchaseNamesQuery: User is not authenticated");
                return Result<List<string>>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetPurchaseNamesQuery: Tenant ID not found");
                return Result<List<string>>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Return empty list if query is null or empty
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                _logger.LogInformation("GetPurchaseNamesQuery: Query is empty, returning empty list");
                return Result<List<string>>.Success(new List<string>());
            }

            // Retrieve distinct purchase names filtered by query (tenant isolation is handled by RLS and global query filter)
            var names = await _purchaseRepository.GetDistinctNamesByQueryAsync(request.Query, request.Limit);

            _logger.LogInformation(
                "Retrieved {Count} distinct purchase names for tenant: {TenantId}",
                names.Count,
                tenantId.Value);

            return Result<List<string>>.Success(names);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving purchase names");

            return Result<List<string>>.Failure(
                Error.Failure($"Failed to retrieve purchase names: {ex.Message}"));
        }
    }
}
