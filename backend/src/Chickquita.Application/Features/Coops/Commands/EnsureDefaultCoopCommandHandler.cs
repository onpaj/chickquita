using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Handler for EnsureDefaultCoopCommand.
/// Returns the first existing coop, or creates one named "Default" if none exist.
/// </summary>
public sealed class EnsureDefaultCoopCommandHandler : IRequestHandler<EnsureDefaultCoopCommand, Result<CoopDto>>
{
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<EnsureDefaultCoopCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public EnsureDefaultCoopCommandHandler(
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<EnsureDefaultCoopCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CoopDto>> Handle(EnsureDefaultCoopCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var coops = await _coopRepository.GetAllAsync(includeArchived: false);
        if (coops.Count > 0)
        {
            _logger.LogInformation(
                "EnsureDefaultCoopCommand: Tenant {TenantId} already has {Count} coop(s), returning first.",
                tenantId.Value, coops.Count);

            var existingDto = _mapper.Map<CoopDto>(coops[0]);
            existingDto.FlocksCount = await _coopRepository.GetFlocksCountAsync(coops[0].Id);
            return Result<CoopDto>.Success(existingDto);
        }

        _logger.LogInformation(
            "EnsureDefaultCoopCommand: No coops found for tenant {TenantId}. Creating default coop.",
            tenantId.Value);

        var coopResult = Coop.Create(tenantId.Value, "Default");
        if (coopResult.IsFailure)
            return Result<CoopDto>.Failure(coopResult.Error);

        var addedCoop = await _coopRepository.AddAsync(coopResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "EnsureDefaultCoopCommand: Created default coop {CoopId} for tenant {TenantId}.",
            addedCoop.Id, tenantId.Value);

        var coopDto = _mapper.Map<CoopDto>(addedCoop);
        coopDto.FlocksCount = 0;
        return Result<CoopDto>.Success(coopDto);
    }
}
