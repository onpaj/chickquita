using Chickquita.Domain.Common;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.EggSales.Commands.Delete;

/// <summary>
/// Handler for DeleteEggSaleCommand that deletes an egg sale for the current tenant.
/// </summary>
public sealed class DeleteEggSaleCommandHandler : IRequestHandler<DeleteEggSaleCommand, Result<bool>>
{
    private readonly IEggSaleRepository _eggSaleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteEggSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEggSaleCommandHandler(
        IEggSaleRepository eggSaleRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteEggSaleCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _eggSaleRepository = eggSaleRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<bool>> Handle(DeleteEggSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing DeleteEggSaleCommand - ID: {EggSaleId}", request.EggSaleId);

        try
        {
            var tenantId = _currentUserService.TenantId;

            var eggSale = await _eggSaleRepository.GetByIdAsync(request.EggSaleId);
            if (eggSale == null)
            {
                _logger.LogWarning("DeleteEggSaleCommand: EggSale with ID {EggSaleId} not found", request.EggSaleId);
                return Result<bool>.Failure(Error.NotFound($"Egg sale with ID {request.EggSaleId} not found"));
            }

            if (eggSale.TenantId != tenantId.Value)
            {
                _logger.LogWarning(
                    "DeleteEggSaleCommand: EggSale {EggSaleId} does not belong to tenant {TenantId}",
                    request.EggSaleId,
                    tenantId.Value);
                return Result<bool>.Failure(Error.Forbidden("You do not have permission to delete this egg sale"));
            }

            await _eggSaleRepository.DeleteAsync(request.EggSaleId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Deleted egg sale with ID: {EggSaleId} for tenant: {TenantId}",
                request.EggSaleId,
                tenantId.Value);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting egg sale with ID: {EggSaleId}", request.EggSaleId);
            return Result<bool>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
