using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Handler for ArchiveFlockCommand that archives (soft deletes) a flock.
/// Sets IsActive = false while preserving all flock data and history.
/// </summary>
public sealed class ArchiveFlockCommandHandler : IRequestHandler<ArchiveFlockCommand, Result<FlockDto>>
{
    private readonly IFlockRepository _flockRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<ArchiveFlockCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveFlockCommandHandler"/> class.
    /// </summary>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public ArchiveFlockCommandHandler(
        IFlockRepository flockRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<ArchiveFlockCommandHandler> logger)
    {
        _flockRepository = flockRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the ArchiveFlockCommand by setting IsActive = false.
    /// Returns a validation error if the flock is already archived.
    /// </summary>
    /// <param name="request">The archive flock command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the archived flock DTO.</returns>
    public async Task<Result<FlockDto>> Handle(ArchiveFlockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing ArchiveFlockCommand - FlockId: {FlockId}",
            request.FlockId);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("ArchiveFlockCommand: User is not authenticated");
                return Result<FlockDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("ArchiveFlockCommand: Tenant ID not found");
                return Result<FlockDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Get the flock (without history to improve performance)
            var flock = await _flockRepository.GetByIdWithoutHistoryAsync(request.FlockId);
            if (flock == null)
            {
                _logger.LogWarning(
                    "ArchiveFlockCommand: Flock with ID {FlockId} not found for tenant {TenantId}",
                    request.FlockId,
                    tenantId.Value);
                return Result<FlockDto>.Failure(Error.NotFound("Flock not found"));
            }

            // If already archived, return success (idempotent)
            if (!flock.IsActive)
            {
                _logger.LogInformation(
                    "ArchiveFlockCommand: Flock with ID {FlockId} is already archived, returning success",
                    request.FlockId);
                var alreadyArchivedDto = _mapper.Map<FlockDto>(flock);
                return Result<FlockDto>.Success(alreadyArchivedDto);
            }

            flock.Archive();

            // Save to database
            var archivedFlock = await _flockRepository.UpdateAsync(flock);

            _logger.LogInformation(
                "Archived flock with ID: {FlockId} for tenant: {TenantId}",
                archivedFlock.Id,
                tenantId.Value);

            var flockDto = _mapper.Map<FlockDto>(archivedFlock);

            return Result<FlockDto>.Success(flockDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while archiving flock with ID: {FlockId}",
                request.FlockId);

            return Result<FlockDto>.Failure(
                Error.Failure($"Failed to archive flock: {ex.Message}"));
        }
    }
}
