using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Coops.Queries;

public record GetCoopByIdQuery : IRequest<Result<CoopDto>>, IAuthorizedRequest
{
    public Guid Id { get; init; }
}

public class GetCoopByIdQueryHandler : IRequestHandler<GetCoopByIdQuery, Result<CoopDto>>
{
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCoopByIdQueryHandler> _logger;

    public GetCoopByIdQueryHandler(
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetCoopByIdQueryHandler> logger)
    {
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CoopDto>> Handle(GetCoopByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing GetCoopByIdQuery for coop {CoopId}", request.Id);

        try
        {
            var tenantId = _currentUserService.TenantId;
            // Retrieve coop by ID (tenant isolation is handled by RLS and global query filter)
            var coop = await _coopRepository.GetByIdAsync(request.Id);

            if (coop == null)
            {
                _logger.LogInformation("Coop {CoopId} not found for tenant {TenantId}", request.Id, tenantId.Value);
                return Result<CoopDto>.Failure(Error.NotFound("Coop not found"));
            }

            _logger.LogInformation(
                "Successfully retrieved coop {CoopId} for tenant {TenantId}",
                request.Id,
                tenantId.Value);

            var coopDto = _mapper.Map<CoopDto>(coop);

            // Populate flocks count
            coopDto.FlocksCount = await _coopRepository.GetFlocksCountAsync(coopDto.Id);

            return Result<CoopDto>.Success(coopDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving coop {CoopId}",
                request.Id);

            return Result<CoopDto>.Failure(
                Error.Failure($"Failed to retrieve coop: {ex.Message}"));
        }
    }
}
