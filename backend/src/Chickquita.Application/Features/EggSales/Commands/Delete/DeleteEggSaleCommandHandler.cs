using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.EggSales.Commands.Delete;

/// <summary>
/// Handler for DeleteEggSaleCommand that deletes an egg sale record for the current tenant.
/// </summary>
public sealed class DeleteEggSaleCommandHandler : IRequestHandler<DeleteEggSaleCommand, Result<bool>>
{
    private readonly IEggSaleRepository _eggSaleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteEggSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEggSaleCommandHandler"/> class.
    /// </summary>
    /// <param name="eggSaleRepository">The egg sale repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="unitOfWork">The unit of work.</param>
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

    /// <summary>
    /// Handles the DeleteEggSaleCommand by deleting an egg sale record.
    /// </summary>
    /// <param name="request">The delete egg sale command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result<bool>> Handle(DeleteEggSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing DeleteEggSaleCommand - ID: {EggSaleId}",
            request.Id);

        try
        {
            var tenantId = _currentUserService.TenantId;

            // Fetch existing egg sale to validate tenant ownership
            var eggSale = await _eggSaleRepository.GetByIdAsync(request.Id);
            if (eggSale == null)
            {
                _logger.LogWarning(
                    "DeleteEggSaleCommand: Egg sale with ID {EggSaleId} not found",
                    request.Id);
                return Result<bool>.Failure(Error.NotFound($"Egg sale with ID {request.Id} not found"));
            }

            // Validate tenant ownership
            if (eggSale.TenantId != tenantId!.Value)
            {
                _logger.LogWarning(
                    "DeleteEggSaleCommand: Egg sale {EggSaleId} does not belong to tenant {TenantId}",
                    request.Id,
                    tenantId.Value);
                return Result<bool>.Failure(Error.Forbidden("You do not have permission to delete this egg sale"));
            }

            // Delete the egg sale
            await _eggSaleRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Deleted egg sale with ID: {EggSaleId} for tenant: {TenantId}",
                request.Id,
                tenantId.Value);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while deleting egg sale with ID: {EggSaleId}",
                request.Id);

            return Result<bool>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
