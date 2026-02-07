using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Purchases.Commands.Update;

/// <summary>
/// Handler for UpdatePurchaseCommand that updates an existing purchase for the current tenant.
/// </summary>
public sealed class UpdatePurchaseCommandHandler : IRequestHandler<UpdatePurchaseCommand, Result<PurchaseDto>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePurchaseCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePurchaseCommandHandler"/> class.
    /// </summary>
    /// <param name="purchaseRepository">The purchase repository.</param>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdatePurchaseCommandHandler(
        IPurchaseRepository purchaseRepository,
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdatePurchaseCommandHandler> logger)
    {
        _purchaseRepository = purchaseRepository;
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdatePurchaseCommand by updating an existing purchase.
    /// </summary>
    /// <param name="request">The update purchase command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the updated purchase DTO.</returns>
    public async Task<Result<PurchaseDto>> Handle(UpdatePurchaseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UpdatePurchaseCommand - ID: {PurchaseId}, Name: {Name}, Type: {Type}, Amount: {Amount}",
            request.Id,
            request.Name,
            request.Type,
            request.Amount);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("UpdatePurchaseCommand: User is not authenticated");
                return Result<PurchaseDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("UpdatePurchaseCommand: Tenant ID not found");
                return Result<PurchaseDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Fetch existing purchase
            var purchase = await _purchaseRepository.GetByIdAsync(request.Id);
            if (purchase == null)
            {
                _logger.LogWarning(
                    "UpdatePurchaseCommand: Purchase with ID {PurchaseId} not found",
                    request.Id);
                return Result<PurchaseDto>.Failure(Error.NotFound($"Purchase with ID {request.Id} not found"));
            }

            // Validate tenant ownership
            if (purchase.TenantId != tenantId.Value)
            {
                _logger.LogWarning(
                    "UpdatePurchaseCommand: Purchase {PurchaseId} does not belong to tenant {TenantId}",
                    request.Id,
                    tenantId.Value);
                return Result<PurchaseDto>.Failure(Error.Forbidden("You do not have permission to update this purchase"));
            }

            // Validate coop reference if provided
            if (request.CoopId.HasValue)
            {
                var coop = await _coopRepository.GetByIdAsync(request.CoopId.Value);
                if (coop == null)
                {
                    _logger.LogWarning(
                        "UpdatePurchaseCommand: Coop with ID {CoopId} not found",
                        request.CoopId.Value);
                    return Result<PurchaseDto>.Failure(Error.NotFound($"Coop with ID {request.CoopId.Value} not found"));
                }
            }

            // Update the purchase entity
            purchase.Update(
                request.Name,
                request.Type,
                request.Amount,
                request.Quantity,
                request.Unit,
                request.PurchaseDate,
                request.CoopId,
                request.ConsumedDate,
                request.Notes);

            // Persist changes
            var updatedPurchase = await _purchaseRepository.UpdateAsync(purchase);

            _logger.LogInformation(
                "Updated purchase with ID: {PurchaseId} for tenant: {TenantId}",
                updatedPurchase.Id,
                tenantId.Value);

            var purchaseDto = _mapper.Map<PurchaseDto>(updatedPurchase);

            return Result<PurchaseDto>.Success(purchaseDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error while updating purchase: {Message}",
                ex.Message);

            return Result<PurchaseDto>.Failure(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating purchase with ID: {PurchaseId}",
                request.Id);

            return Result<PurchaseDto>.Failure(
                Error.Failure($"Failed to update purchase: {ex.Message}"));
        }
    }
}
