using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.EggSales.Commands.Create;

/// <summary>
/// Handler for CreateEggSaleCommand that creates a new egg sale record for the current tenant.
/// </summary>
public sealed class CreateEggSaleCommandHandler : IRequestHandler<CreateEggSaleCommand, Result<EggSaleDto>>
{
    private readonly IEggSaleRepository _eggSaleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEggSaleCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEggSaleCommandHandler"/> class.
    /// </summary>
    /// <param name="eggSaleRepository">The egg sale repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="unitOfWork">The unit of work.</param>
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

    /// <summary>
    /// Handles the CreateEggSaleCommand by creating a new egg sale record.
    /// </summary>
    /// <param name="request">The create egg sale command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the egg sale DTO.</returns>
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

            // Create the egg sale entity
            var eggSaleResult = EggSale.Create(
                tenantId!.Value,
                request.Date,
                request.Quantity,
                request.PricePerUnit,
                request.BuyerName,
                request.Notes);

            if (eggSaleResult.IsFailure)
                return Result<EggSaleDto>.Failure(eggSaleResult.Error);

            var eggSale = eggSaleResult.Value;

            // Save to database
            var addedEggSale = await _eggSaleRepository.AddAsync(eggSale);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created new egg sale with ID: {EggSaleId} for tenant: {TenantId}",
                addedEggSale.Id,
                tenantId.Value);

            var eggSaleDto = _mapper.Map<EggSaleDto>(addedEggSale);

            return Result<EggSaleDto>.Success(eggSaleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while creating egg sale for date: {Date}",
                request.Date);

            return Result<EggSaleDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
