using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Handler for UpdateFlockCommand that updates basic flock information.
/// Does not modify flock composition - use composition-specific handlers for that.
/// </summary>
public sealed class UpdateFlockCommandHandler : IRequestHandler<UpdateFlockCommand, Result<FlockDto>>
{
    private readonly IFlockRepository _flockRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateFlockCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFlockCommandHandler"/> class.
    /// </summary>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateFlockCommandHandler(
        IFlockRepository flockRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateFlockCommandHandler> logger)
    {
        _flockRepository = flockRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFlockCommand by updating basic flock information.
    /// </summary>
    /// <param name="request">The update flock command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the updated flock DTO.</returns>
    public async Task<Result<FlockDto>> Handle(UpdateFlockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UpdateFlockCommand - FlockId: {FlockId}, Identifier: {Identifier}, HatchDate: {HatchDate}",
            request.FlockId,
            request.Identifier,
            request.HatchDate);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("UpdateFlockCommand: User is not authenticated");
                return Result<FlockDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("UpdateFlockCommand: Tenant ID not found");
                return Result<FlockDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Get the flock (without history to improve performance)
            var flock = await _flockRepository.GetByIdWithoutHistoryAsync(request.FlockId);
            if (flock == null)
            {
                _logger.LogWarning(
                    "UpdateFlockCommand: Flock with ID {FlockId} not found for tenant {TenantId}",
                    request.FlockId,
                    tenantId.Value);
                return Result<FlockDto>.Failure(Error.NotFound("Flock not found"));
            }

            // Check identifier uniqueness within the coop (excluding current flock)
            var identifierExists = await _flockRepository.ExistsByIdentifierInCoopAsync(
                flock.CoopId,
                request.Identifier,
                request.FlockId);

            if (identifierExists)
            {
                _logger.LogWarning(
                    "UpdateFlockCommand: Flock with identifier '{Identifier}' already exists in coop {CoopId}",
                    request.Identifier,
                    flock.CoopId);
                return Result<FlockDto>.Failure(
                    Error.Conflict("A flock with this identifier already exists in the coop"));
            }

            // Update basic flock information
            flock.Update(request.Identifier, request.HatchDate);

            // Update composition if provided and changed
            if (request.CurrentHens.HasValue || request.CurrentRoosters.HasValue || request.CurrentChicks.HasValue)
            {
                var newHens = request.CurrentHens ?? flock.CurrentHens;
                var newRoosters = request.CurrentRoosters ?? flock.CurrentRoosters;
                var newChicks = request.CurrentChicks ?? flock.CurrentChicks;

                var compositionChanged =
                    newHens != flock.CurrentHens ||
                    newRoosters != flock.CurrentRoosters ||
                    newChicks != flock.CurrentChicks;

                if (compositionChanged)
                {
                    flock.UpdateComposition(newHens, newRoosters, newChicks, "Manual update");

                    _logger.LogInformation(
                        "Updated composition for flock {FlockId}: Hens={Hens}, Roosters={Roosters}, Chicks={Chicks}",
                        flock.Id,
                        newHens,
                        newRoosters,
                        newChicks);
                }
            }

            // Save to database
            var updatedFlock = await _flockRepository.UpdateAsync(flock);

            _logger.LogInformation(
                "Updated flock with ID: {FlockId} for tenant: {TenantId}",
                updatedFlock.Id,
                tenantId.Value);

            var flockDto = _mapper.Map<FlockDto>(updatedFlock);

            return Result<FlockDto>.Success(flockDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error while updating flock: {Message}",
                ex.Message);

            return Result<FlockDto>.Failure(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating flock with ID: {FlockId}",
                request.FlockId);

            return Result<FlockDto>.Failure(
                Error.Failure($"Failed to update flock: {ex.Message}"));
        }
    }
}
