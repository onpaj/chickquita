using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.EggSales.Commands.Create;

/// <summary>
/// Handler for CreateEggSaleCommand that creates a new egg sale for the current tenant.
/// </summary>
public sealed class CreateEggSaleCommandHandler : IRequestHandler<CreateEggSaleCommand, Result<EggSaleDto>>
{
    private readonly IEggSaleRepository _eggSaleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEggSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEggSaleCommandHandler(
        IEggSaleRepository eggSaleRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateEggSaleCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _eggSaleRepository = eggSaleRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<EggSaleDto>> Handle(CreateEggSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing CreateEggSaleCommand - Date: {Date}, Quantity: {Quantity}, PricePerUnit: {PricePerUnit}",
            request.Date,
            request.Quantity,
            request.PricePerUnit);

        try
        {
            var tenantId = _currentUserService.TenantId;

            var eggSaleResult = EggSale.Create(
                tenantId.Value,
                request.Date,
                request.Quantity,
                request.PricePerUnit,
                request.BuyerName,
                request.Notes);

            if (eggSaleResult.IsFailure)
                return Result<EggSaleDto>.Failure(eggSaleResult.Error);

            var eggSale = eggSaleResult.Value;

            var added = await _eggSaleRepository.AddAsync(eggSale);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created new egg sale with ID: {EggSaleId} for tenant: {TenantId}",
                added.Id,
                tenantId.Value);

            var dto = _mapper.Map<EggSaleDto>(added);
            return Result<EggSaleDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating egg sale");
            return Result<EggSaleDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
