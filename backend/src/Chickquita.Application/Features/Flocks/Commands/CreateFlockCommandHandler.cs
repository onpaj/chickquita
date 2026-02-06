using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Handler for CreateFlockCommand that creates a new flock within a coop for the current tenant.
/// </summary>
public sealed class CreateFlockCommandHandler : IRequestHandler<CreateFlockCommand, Result<FlockDto>>
{
    private readonly IFlockRepository _flockRepository;
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFlockCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateFlockCommandHandler"/> class.
    /// </summary>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public CreateFlockCommandHandler(
        IFlockRepository flockRepository,
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateFlockCommandHandler> logger)
    {
        _flockRepository = flockRepository;
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateFlockCommand by creating a new flock.
    /// </summary>
    /// <param name="request">The create flock command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the flock DTO.</returns>
    public async Task<Result<FlockDto>> Handle(CreateFlockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing CreateFlockCommand - CoopId: {CoopId}, Identifier: {Identifier}, HatchDate: {HatchDate}",
            request.CoopId,
            request.Identifier,
            request.HatchDate);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("CreateFlockCommand: User is not authenticated");
                return Result<FlockDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("CreateFlockCommand: Tenant ID not found");
                return Result<FlockDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Check if the coop exists and belongs to the current tenant
            var coop = await _coopRepository.GetByIdAsync(request.CoopId);
            if (coop == null)
            {
                _logger.LogWarning(
                    "CreateFlockCommand: Coop with ID {CoopId} not found for tenant {TenantId}",
                    request.CoopId,
                    tenantId.Value);
                return Result<FlockDto>.Failure(Error.NotFound("Coop not found"));
            }

            // Check if a flock with this identifier already exists in the coop
            var identifierExists = await _flockRepository.ExistsByIdentifierInCoopAsync(
                request.CoopId,
                request.Identifier);

            if (identifierExists)
            {
                _logger.LogWarning(
                    "CreateFlockCommand: Flock with identifier '{Identifier}' already exists in coop {CoopId}",
                    request.Identifier,
                    request.CoopId);
                return Result<FlockDto>.Failure(
                    Error.Conflict("A flock with this identifier already exists in the coop"));
            }

            // Create the flock entity with initial history entry
            var flock = Flock.Create(
                tenantId: tenantId.Value,
                coopId: request.CoopId,
                identifier: request.Identifier,
                hatchDate: request.HatchDate,
                initialHens: request.InitialHens,
                initialRoosters: request.InitialRoosters,
                initialChicks: request.InitialChicks,
                notes: request.Notes);

            // Save to database
            var addedFlock = await _flockRepository.AddAsync(flock);

            _logger.LogInformation(
                "Created new flock with ID: {FlockId} for coop: {CoopId}, tenant: {TenantId}",
                addedFlock.Id,
                request.CoopId,
                tenantId.Value);

            var flockDto = _mapper.Map<FlockDto>(addedFlock);

            return Result<FlockDto>.Success(flockDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error while creating flock: {Message}",
                ex.Message);

            return Result<FlockDto>.Failure(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while creating flock with identifier: {Identifier} in coop: {CoopId}",
                request.Identifier,
                request.CoopId);

            return Result<FlockDto>.Failure(
                Error.Failure($"Failed to create flock: {ex.Message}"));
        }
    }
}
