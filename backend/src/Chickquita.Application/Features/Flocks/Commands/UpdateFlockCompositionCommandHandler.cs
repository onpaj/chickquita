using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Handler for UpdateFlockCompositionCommand that updates flock composition.
/// Creates an immutable FlockHistory entry with reason "Manual update".
/// </summary>
public sealed class UpdateFlockCompositionCommandHandler : IRequestHandler<UpdateFlockCompositionCommand, Result<FlockDto>>
{
    private readonly IFlockRepository _flockRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateFlockCompositionCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFlockCompositionCommandHandler"/> class.
    /// </summary>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateFlockCompositionCommandHandler(
        IFlockRepository flockRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateFlockCompositionCommandHandler> logger)
    {
        _flockRepository = flockRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateFlockCompositionCommand by updating flock composition and recording history.
    /// </summary>
    /// <param name="request">The update flock composition command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the updated flock DTO.</returns>
    public async Task<Result<FlockDto>> Handle(UpdateFlockCompositionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UpdateFlockCompositionCommand - FlockId: {FlockId}, Hens: {Hens}, Roosters: {Roosters}, Chicks: {Chicks}",
            request.FlockId,
            request.Hens,
            request.Roosters,
            request.Chicks);

        try
        {
            var tenantId = _currentUserService.TenantId;

            // Load flock with history so UpdateComposition can add the new entry
            var flock = await _flockRepository.GetByIdAsync(request.FlockId);
            if (flock == null)
            {
                _logger.LogWarning(
                    "UpdateFlockCompositionCommand: Flock with ID {FlockId} not found for tenant {TenantId}",
                    request.FlockId,
                    tenantId.Value);
                return Result<FlockDto>.Failure(Error.NotFound("Flock not found"));
            }

            // Use the domain method which also creates a FlockHistory entry
            var compositionResult = flock.UpdateComposition(
                hens: request.Hens,
                roosters: request.Roosters,
                chicks: request.Chicks,
                reason: "Manual update",
                notes: request.Notes);

            if (compositionResult.IsFailure)
                return Result<FlockDto>.Failure(compositionResult.Error);

            // Save to database
            var updatedFlock = await _flockRepository.UpdateAsync(flock);

            _logger.LogInformation(
                "Updated composition for flock {FlockId}: Hens={Hens}, Roosters={Roosters}, Chicks={Chicks} for tenant: {TenantId}",
                updatedFlock.Id,
                request.Hens,
                request.Roosters,
                request.Chicks,
                tenantId.Value);

            var flockDto = _mapper.Map<FlockDto>(updatedFlock);

            return Result<FlockDto>.Success(flockDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error while updating flock composition: {Message}",
                ex.Message);

            return Result<FlockDto>.Failure(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating composition for flock with ID: {FlockId}",
                request.FlockId);

            return Result<FlockDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
