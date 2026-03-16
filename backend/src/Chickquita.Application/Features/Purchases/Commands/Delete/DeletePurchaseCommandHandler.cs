using Chickquita.Domain.Common;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Purchases.Commands.Delete;

/// <summary>
/// Handler for DeletePurchaseCommand that deletes a purchase for the current tenant.
/// </summary>
public sealed class DeletePurchaseCommandHandler : IRequestHandler<DeletePurchaseCommand, Result<bool>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeletePurchaseCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePurchaseCommandHandler"/> class.
    /// </summary>
    /// <param name="purchaseRepository">The purchase repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    public DeletePurchaseCommandHandler(
        IPurchaseRepository purchaseRepository,
        ICurrentUserService currentUserService,
        ILogger<DeletePurchaseCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _purchaseRepository = purchaseRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Handles the DeletePurchaseCommand by deleting a purchase.
    /// </summary>
    /// <param name="request">The delete purchase command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result<bool>> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing DeletePurchaseCommand - ID: {PurchaseId}",
            request.PurchaseId);

        try
        {
            var tenantId = _currentUserService.TenantId;

            // Fetch existing purchase to validate tenant ownership
            var purchase = await _purchaseRepository.GetByIdAsync(request.PurchaseId);
            if (purchase == null)
            {
                _logger.LogWarning(
                    "DeletePurchaseCommand: Purchase with ID {PurchaseId} not found",
                    request.PurchaseId);
                return Result<bool>.Failure(Error.NotFound($"Purchase with ID {request.PurchaseId} not found"));
            }

            // Validate tenant ownership
            if (purchase.TenantId != tenantId.Value)
            {
                _logger.LogWarning(
                    "DeletePurchaseCommand: Purchase {PurchaseId} does not belong to tenant {TenantId}",
                    request.PurchaseId,
                    tenantId.Value);
                return Result<bool>.Failure(Error.Forbidden("You do not have permission to delete this purchase"));
            }

            // Delete the purchase
            await _purchaseRepository.DeleteAsync(request.PurchaseId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Deleted purchase with ID: {PurchaseId} for tenant: {TenantId}",
                request.PurchaseId,
                tenantId.Value);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while deleting purchase with ID: {PurchaseId}",
                request.PurchaseId);

            return Result<bool>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
