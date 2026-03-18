using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEggSalesQueryHandler"/> class.
    /// </summary>
    /// <param name="eggSaleRepository">The egg sale repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
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

    /// <summary>
    /// Handles the GetEggSalesQuery by retrieving filtered egg sales for the current tenant.
    /// </summary>
    /// <param name="request">The get egg sales query with optional date filters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of egg sale DTOs ordered by date descending.</returns>
    public async Task<Result<List<EggSaleDto>>> Handle(GetEggSalesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing GetEggSalesQuery - DateFrom: {DateFrom}, DateTo: {DateTo}",
            request.DateFrom,
            request.DateTo);

        try
        {
            var tenantId = _currentUserService.TenantId;

            var eggSales = await _eggSaleRepository.GetWithFiltersAsync(
                tenantId!.Value,
                request.DateFrom,
                request.DateTo);

            var eggSaleList = eggSales.ToList();

            _logger.LogInformation(
                "Retrieved {Count} egg sales for tenant: {TenantId}",
                eggSaleList.Count,
                tenantId.Value);

            var eggSaleDtos = _mapper.Map<List<EggSaleDto>>(eggSaleList);

            return Result<List<EggSaleDto>>.Success(eggSaleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving egg sales");

            return Result<List<EggSaleDto>>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
