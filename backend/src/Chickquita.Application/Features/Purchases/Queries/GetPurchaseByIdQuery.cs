using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

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

/// <summary>
/// Handler for GetPurchaseByIdQuery that retrieves a single purchase.
/// </summary>
public sealed class GetPurchaseByIdQueryHandler : IRequestHandler<GetPurchaseByIdQuery, Result<PurchaseDto>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPurchaseByIdQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPurchaseByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="purchaseRepository">The purchase repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public GetPurchaseByIdQueryHandler(
        IPurchaseRepository purchaseRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetPurchaseByIdQueryHandler> logger)
    {
        _purchaseRepository = purchaseRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetPurchaseByIdQuery by retrieving a single purchase.
    /// </summary>
    /// <param name="request">The get purchase by ID query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the purchase DTO or NOT_FOUND error.</returns>
    public async Task<Result<PurchaseDto>> Handle(GetPurchaseByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing GetPurchaseByIdQuery for purchase {PurchaseId}", request.Id);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetPurchaseByIdQuery: User is not authenticated");
                return Result<PurchaseDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetPurchaseByIdQuery: Tenant ID not found");
                return Result<PurchaseDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Retrieve purchase by ID (tenant isolation is handled by RLS and global query filter)
            var purchase = await _purchaseRepository.GetByIdAsync(request.Id);

            if (purchase == null)
            {
                _logger.LogInformation("Purchase {PurchaseId} not found for tenant {TenantId}", request.Id, tenantId.Value);
                return Result<PurchaseDto>.Failure(Error.NotFound("Purchase not found"));
            }

            _logger.LogInformation(
                "Successfully retrieved purchase {PurchaseId} for tenant {TenantId}",
                request.Id,
                tenantId.Value);

            var purchaseDto = _mapper.Map<PurchaseDto>(purchase);

            return Result<PurchaseDto>.Success(purchaseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving purchase {PurchaseId}",
                request.Id);

            return Result<PurchaseDto>.Failure(
                Error.Failure($"Failed to retrieve purchase: {ex.Message}"));
        }
    }
}
