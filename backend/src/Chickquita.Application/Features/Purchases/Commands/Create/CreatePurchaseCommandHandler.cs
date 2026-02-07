using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Purchases.Commands.Create;

/// <summary>
/// Handler for CreatePurchaseCommand that creates a new purchase for the current tenant.
/// </summary>
public sealed class CreatePurchaseCommandHandler : IRequestHandler<CreatePurchaseCommand, Result<PurchaseDto>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePurchaseCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePurchaseCommandHandler"/> class.
    /// </summary>
    /// <param name="purchaseRepository">The purchase repository.</param>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public CreatePurchaseCommandHandler(
        IPurchaseRepository purchaseRepository,
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreatePurchaseCommandHandler> logger)
    {
        _purchaseRepository = purchaseRepository;
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreatePurchaseCommand by creating a new purchase.
    /// </summary>
    /// <param name="request">The create purchase command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the purchase DTO.</returns>
    public async Task<Result<PurchaseDto>> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing CreatePurchaseCommand - Name: {Name}, Type: {Type}, Amount: {Amount}",
            request.Name,
            request.Type,
            request.Amount);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("CreatePurchaseCommand: User is not authenticated");
                return Result<PurchaseDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("CreatePurchaseCommand: Tenant ID not found");
                return Result<PurchaseDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Validate coop reference if provided
            if (request.CoopId.HasValue)
            {
                var coop = await _coopRepository.GetByIdAsync(request.CoopId.Value);
                if (coop == null)
                {
                    _logger.LogWarning(
                        "CreatePurchaseCommand: Coop with ID {CoopId} not found",
                        request.CoopId.Value);
                    return Result<PurchaseDto>.Failure(Error.NotFound($"Coop with ID {request.CoopId.Value} not found"));
                }
            }

            // Create the purchase entity
            var purchase = Purchase.Create(
                tenantId.Value,
                request.Name,
                request.Type,
                request.Amount,
                request.Quantity,
                request.Unit,
                request.PurchaseDate,
                request.CoopId,
                request.ConsumedDate,
                request.Notes);

            // Save to database
            var addedPurchase = await _purchaseRepository.AddAsync(purchase);

            _logger.LogInformation(
                "Created new purchase with ID: {PurchaseId} for tenant: {TenantId}",
                addedPurchase.Id,
                tenantId.Value);

            var purchaseDto = _mapper.Map<PurchaseDto>(addedPurchase);

            return Result<PurchaseDto>.Success(purchaseDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error while creating purchase: {Message}",
                ex.Message);

            return Result<PurchaseDto>.Failure(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while creating purchase with name: {Name}",
                request.Name);

            return Result<PurchaseDto>.Failure(
                Error.Failure($"Failed to create purchase: {ex.Message}"));
        }
    }
}
