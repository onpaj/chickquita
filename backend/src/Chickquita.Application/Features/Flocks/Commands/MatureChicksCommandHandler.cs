using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Handler for MatureChicksCommand.
/// Converts chicks to adult hens/roosters and records the change in flock history.
/// </summary>
public sealed class MatureChicksCommandHandler : IRequestHandler<MatureChicksCommand, Result<FlockDto>>
{
    private readonly IFlockRepository _flockRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<MatureChicksCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public MatureChicksCommandHandler(
        IFlockRepository flockRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<MatureChicksCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _flockRepository = flockRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<FlockDto>> Handle(MatureChicksCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing MatureChicksCommand - FlockId: {FlockId}, ChicksToMature: {ChicksToMature}",
            request.FlockId,
            request.ChicksToMature);

        try
        {
            var tenantId = _currentUserService.TenantId;

            // Load flock with history so UpdateComposition can add the new entry
            var flock = await _flockRepository.GetByIdAsync(request.FlockId);
            if (flock == null)
            {
                _logger.LogWarning(
                    "MatureChicksCommand: Flock {FlockId} not found for tenant {TenantId}",
                    request.FlockId,
                    tenantId.Value);
                return Result<FlockDto>.Failure(Error.NotFound("Flock not found"));
            }

            if (!flock.IsActive)
            {
                return Result<FlockDto>.Failure(Error.Validation("Cannot mature chicks in an archived flock"));
            }

            if (flock.CurrentChicks < request.ChicksToMature)
            {
                return Result<FlockDto>.Failure(
                    Error.Validation($"Cannot mature {request.ChicksToMature} chicks: flock only has {flock.CurrentChicks} chicks"));
            }

            if (request.Hens + request.Roosters != request.ChicksToMature)
            {
                return Result<FlockDto>.Failure(
                    Error.Validation("The sum of hens and roosters must equal ChicksToMature"));
            }

            // Calculate new composition after maturation
            var newHens = flock.CurrentHens + request.Hens;
            var newRoosters = flock.CurrentRoosters + request.Roosters;
            var newChicks = flock.CurrentChicks - request.ChicksToMature;

            // Use the domain method which also creates a FlockHistory entry
            flock.UpdateComposition(
                hens: newHens,
                roosters: newRoosters,
                chicks: newChicks,
                reason: "Maturation",
                notes: request.Notes);

            var updatedFlock = await _flockRepository.UpdateAsync(flock);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Matured {ChicksToMature} chicks in flock {FlockId}: +{Hens} hens, +{Roosters} roosters",
                request.ChicksToMature,
                request.FlockId,
                request.Hens,
                request.Roosters);

            var flockDto = _mapper.Map<FlockDto>(updatedFlock);
            return Result<FlockDto>.Success(flockDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while maturing chicks in flock {FlockId}",
                request.FlockId);

            return Result<FlockDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
