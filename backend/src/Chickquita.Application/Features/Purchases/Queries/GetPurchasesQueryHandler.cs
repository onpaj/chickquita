using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Purchases.Queries;

/// <summary>
/// Handler for GetPurchasesQuery that retrieves purchases with optional filters.
/// </summary>
public sealed class GetPurchasesQueryHandler : IRequestHandler<GetPurchasesQuery, Result<List<PurchaseDto>>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IFlockRepository _flockRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPurchasesQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPurchasesQueryHandler"/> class.
    /// </summary>
    /// <param name="purchaseRepository">The purchase repository.</param>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public GetPurchasesQueryHandler(
        IPurchaseRepository purchaseRepository,
        IFlockRepository flockRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetPurchasesQueryHandler> logger)
    {
        _purchaseRepository = purchaseRepository;
        _flockRepository = flockRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetPurchasesQuery by retrieving filtered purchases.
    /// </summary>
    /// <param name="request">The get purchases query with filters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of purchase DTOs.</returns>
    public async Task<Result<List<PurchaseDto>>> Handle(GetPurchasesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing GetPurchasesQuery - FromDate: {FromDate}, ToDate: {ToDate}, Type: {Type}, FlockId: {FlockId}",
            request.FromDate,
            request.ToDate,
            request.Type,
            request.FlockId);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetPurchasesQuery: User is not authenticated");
                return Result<List<PurchaseDto>>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetPurchasesQuery: Tenant ID not found");
                return Result<List<PurchaseDto>>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Resolve CoopId from FlockId if FlockId filter is provided
            Guid? coopId = null;
            if (request.FlockId.HasValue)
            {
                var flock = await _flockRepository.GetByIdAsync(request.FlockId.Value);
                if (flock == null)
                {
                    _logger.LogWarning(
                        "GetPurchasesQuery: Flock with ID {FlockId} not found",
                        request.FlockId.Value);
                    return Result<List<PurchaseDto>>.Failure(
                        Error.NotFound($"Flock with ID {request.FlockId.Value} not found"));
                }
                coopId = flock.CoopId;
            }

            // Retrieve purchases with filters (tenant isolation is handled by RLS and global query filter)
            var purchases = await _purchaseRepository.GetWithFiltersAsync(
                request.FromDate,
                request.ToDate,
                request.Type,
                coopId);

            _logger.LogInformation(
                "Retrieved {Count} purchases for tenant: {TenantId}",
                purchases.Count,
                tenantId.Value);

            var purchaseDtos = _mapper.Map<List<PurchaseDto>>(purchases);

            return Result<List<PurchaseDto>>.Success(purchaseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving purchases");

            return Result<List<PurchaseDto>>.Failure(
                Error.Failure($"Failed to retrieve purchases: {ex.Message}"));
        }
    }
}
