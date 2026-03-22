using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.EggSales.Commands.Update;

/// <summary>
/// Handler for UpdateEggSaleCommand that updates an existing egg sale for the current tenant.
/// </summary>
public sealed class UpdateEggSaleCommandHandler : IRequestHandler<UpdateEggSaleCommand, Result<EggSaleDto>>
{
    private readonly IEggSaleRepository _eggSaleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateEggSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

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

    public async Task<Result<EggSaleDto>> Handle(UpdateEggSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UpdateEggSaleCommand - ID: {EggSaleId}, Quantity: {Quantity}, PricePerUnit: {PricePerUnit}",
            request.Id,
            request.Quantity,
            request.PricePerUnit);

        try
        {
            var tenantId = _currentUserService.TenantId;

            var eggSale = await _eggSaleRepository.GetByIdAsync(request.Id);
            if (eggSale == null)
            {
                _logger.LogWarning("UpdateEggSaleCommand: EggSale with ID {EggSaleId} not found", request.Id);
                return Result<EggSaleDto>.Failure(Error.NotFound($"Egg sale with ID {request.Id} not found"));
            }

            if (eggSale.TenantId != tenantId.Value)
            {
                _logger.LogWarning(
                    "UpdateEggSaleCommand: EggSale {EggSaleId} does not belong to tenant {TenantId}",
                    request.Id,
                    tenantId.Value);
                return Result<EggSaleDto>.Failure(Error.Forbidden("You do not have permission to update this egg sale"));
            }

            var updateResult = eggSale.Update(
                request.Date,
                request.Quantity,
                request.PricePerUnit,
                request.BuyerName,
                request.Notes);

            if (updateResult.IsFailure)
                return Result<EggSaleDto>.Failure(updateResult.Error);

            var updated = await _eggSaleRepository.UpdateAsync(eggSale);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated egg sale with ID: {EggSaleId} for tenant: {TenantId}",
                updated.Id,
                tenantId.Value);

            var dto = _mapper.Map<EggSaleDto>(updated);
            return Result<EggSaleDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating egg sale with ID: {EggSaleId}", request.Id);
            return Result<EggSaleDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
