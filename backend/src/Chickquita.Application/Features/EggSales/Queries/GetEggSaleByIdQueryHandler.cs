using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.EggSales.Queries;

/// <summary>
/// Handler for GetEggSaleByIdQuery that retrieves a single egg sale.
/// </summary>
public sealed class GetEggSaleByIdQueryHandler : IRequestHandler<GetEggSaleByIdQuery, Result<EggSaleDto>>
{
    private readonly IEggSaleRepository _eggSaleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEggSaleByIdQueryHandler> _logger;

    public GetEggSaleByIdQueryHandler(
        IEggSaleRepository eggSaleRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetEggSaleByIdQueryHandler> logger)
    {
        _eggSaleRepository = eggSaleRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<EggSaleDto>> Handle(GetEggSaleByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing GetEggSaleByIdQuery - ID: {EggSaleId}", request.Id);

        try
        {
            var tenantId = _currentUserService.TenantId;

            var eggSale = await _eggSaleRepository.GetByIdAsync(request.Id);
            if (eggSale == null)
            {
                _logger.LogWarning("GetEggSaleByIdQuery: EggSale with ID {EggSaleId} not found", request.Id);
                return Result<EggSaleDto>.Failure(Error.NotFound($"Egg sale with ID {request.Id} not found"));
            }

            if (eggSale.TenantId != tenantId.Value)
            {
                _logger.LogWarning(
                    "GetEggSaleByIdQuery: EggSale {EggSaleId} does not belong to tenant {TenantId}",
                    request.Id,
                    tenantId.Value);
                return Result<EggSaleDto>.Failure(Error.Forbidden("You do not have permission to access this egg sale"));
            }

            var dto = _mapper.Map<EggSaleDto>(eggSale);
            return Result<EggSaleDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving egg sale with ID: {EggSaleId}", request.Id);
            return Result<EggSaleDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
