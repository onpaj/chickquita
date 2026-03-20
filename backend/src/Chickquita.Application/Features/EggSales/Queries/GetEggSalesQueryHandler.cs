using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.EggSales.Queries;

/// <summary>
/// Handler for GetEggSalesQuery that retrieves egg sales with optional date range filters.
/// </summary>
public sealed class GetEggSalesQueryHandler : IRequestHandler<GetEggSalesQuery, Result<List<EggSaleDto>>>
{
    private readonly IEggSaleRepository _eggSaleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEggSalesQueryHandler> _logger;

    public GetEggSalesQueryHandler(
        IEggSaleRepository eggSaleRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetEggSalesQueryHandler> logger)
    {
        _eggSaleRepository = eggSaleRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<EggSaleDto>>> Handle(GetEggSalesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing GetEggSalesQuery - FromDate: {FromDate}, ToDate: {ToDate}",
            request.FromDate,
            request.ToDate);

        try
        {
            var tenantId = _currentUserService.TenantId;

            var eggSales = await _eggSaleRepository.GetWithFiltersAsync(
                request.FromDate,
                request.ToDate);

            _logger.LogInformation(
                "Retrieved {Count} egg sales for tenant: {TenantId}",
                eggSales.Count,
                tenantId.Value);

            var dtos = _mapper.Map<List<EggSaleDto>>(eggSales);

            return Result<List<EggSaleDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving egg sales");
            return Result<List<EggSaleDto>>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
