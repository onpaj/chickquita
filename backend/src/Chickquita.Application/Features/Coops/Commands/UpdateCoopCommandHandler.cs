using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Handler for UpdateCoopCommand that updates an existing coop for the current tenant.
/// </summary>
public sealed class UpdateCoopCommandHandler : IRequestHandler<UpdateCoopCommand, Result<CoopDto>>
{
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCoopCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCoopCommandHandler"/> class.
    /// </summary>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateCoopCommandHandler(
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateCoopCommandHandler> logger)
    {
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateCoopCommand by updating an existing coop.
    /// </summary>
    /// <param name="request">The update coop command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the updated coop DTO.</returns>
    public async Task<Result<CoopDto>> Handle(UpdateCoopCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UpdateCoopCommand - Id: {Id}, Name: {Name}, Location: {Location}",
            request.Id,
            request.Name,
            request.Location);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("UpdateCoopCommand: User is not authenticated");
                return Result<CoopDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("UpdateCoopCommand: Tenant ID not found");
                return Result<CoopDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Get the existing coop
            var coop = await _coopRepository.GetByIdAsync(request.Id);
            if (coop == null)
            {
                _logger.LogWarning(
                    "UpdateCoopCommand: Coop with ID {CoopId} not found",
                    request.Id);
                return Result<CoopDto>.Failure(Error.NotFound("Coop not found"));
            }

            // Check if the name is being changed and if it conflicts with another coop
            if (coop.Name != request.Name)
            {
                var nameExists = await _coopRepository.ExistsByNameAsync(request.Name);
                if (nameExists)
                {
                    _logger.LogWarning(
                        "UpdateCoopCommand: Coop with name '{Name}' already exists for tenant {TenantId}",
                        request.Name,
                        tenantId.Value);
                    return Result<CoopDto>.Failure(Error.Conflict("A coop with this name already exists"));
                }
            }

            // Update the coop entity
            coop.Update(request.Name, request.Location);

            // Save to database
            await _coopRepository.UpdateAsync(coop);

            _logger.LogInformation(
                "Updated coop with ID: {CoopId} for tenant: {TenantId}",
                coop.Id,
                tenantId.Value);

            var coopDto = _mapper.Map<CoopDto>(coop);

            // Populate flocks count
            coopDto.FlocksCount = await _coopRepository.GetFlocksCountAsync(coopDto.Id);

            return Result<CoopDto>.Success(coopDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error while updating coop: {Message}",
                ex.Message);

            return Result<CoopDto>.Failure(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating coop with ID: {Id}",
                request.Id);

            return Result<CoopDto>.Failure(
                Error.Failure($"Failed to update coop: {ex.Message}"));
        }
    }
}
