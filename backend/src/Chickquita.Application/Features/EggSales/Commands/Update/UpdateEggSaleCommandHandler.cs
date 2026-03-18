using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.EggSales.Commands.Update;

/// <summary>
/// Handler for UpdateEggSaleCommand that updates an existing egg sale record for the current tenant.
/// </summary>
public sealed class UpdateEggSaleCommandHandler : IRequestHandler<UpdateEggSaleCommand, Result<EggSaleDto>>
{
    private readonly IEggSaleRepository _eggSaleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateEggSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateEggSaleCommandHandler"/> class.
    /// </summary>
    /// <param name="eggSaleRepository">The egg sale repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateEggSaleCommandHandler(
        IEggSaleRepository eggSaleRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateEggSaleCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _eggSaleRepository = eggSaleRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Handles the UpdateEggSaleCommand by updating an existing egg sale record.
    /// </summary>
    /// <param name="request">The update egg sale command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the updated egg sale DTO.</returns>
    public async Task<Result<EggSaleDto>> Handle(UpdateEggSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UpdateEggSaleCommand - ID: {EggSaleId}, Date: {Date}, Quantity: {Quantity}, PricePerUnit: {PricePerUnit}",
            request.Id,
            request.Date,
            request.Quantity,
            request.PricePerUnit);

        try
        {
            var tenantId = _currentUserService.TenantId;

            // Fetch existing egg sale
            var eggSale = await _eggSaleRepository.GetByIdAsync(request.Id);
            if (eggSale == null)
            {
                _logger.LogWarning(
                    "UpdateEggSaleCommand: Egg sale with ID {EggSaleId} not found",
                    request.Id);
                return Result<EggSaleDto>.Failure(Error.NotFound($"Egg sale with ID {request.Id} not found"));
            }

            // Validate tenant ownership
            if (eggSale.TenantId != tenantId!.Value)
            {
                _logger.LogWarning(
                    "UpdateEggSaleCommand: Egg sale {EggSaleId} does not belong to tenant {TenantId}",
                    request.Id,
                    tenantId.Value);
                return Result<EggSaleDto>.Failure(Error.Forbidden("You do not have permission to update this egg sale"));
            }

            // Update the egg sale entity
            var updateResult = eggSale.Update(
                request.Date,
                request.Quantity,
                request.PricePerUnit,
                request.BuyerName,
                request.Notes);

            if (updateResult.IsFailure)
                return Result<EggSaleDto>.Failure(updateResult.Error);

            // Persist changes
            var updatedEggSale = await _eggSaleRepository.UpdateAsync(eggSale);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated egg sale with ID: {EggSaleId} for tenant: {TenantId}",
                updatedEggSale.Id,
                tenantId.Value);

            var eggSaleDto = _mapper.Map<EggSaleDto>(updatedEggSale);

            return Result<EggSaleDto>.Success(eggSaleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating egg sale with ID: {EggSaleId}",
                request.Id);

            return Result<EggSaleDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
