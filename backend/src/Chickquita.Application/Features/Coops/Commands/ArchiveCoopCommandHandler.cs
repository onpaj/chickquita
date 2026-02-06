using Chickquita.Domain.Common;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Handler for ArchiveCoopCommand that archives an existing coop for the current tenant (soft delete).
/// </summary>
public sealed class ArchiveCoopCommandHandler : IRequestHandler<ArchiveCoopCommand, Result<bool>>
{
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ArchiveCoopCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveCoopCommandHandler"/> class.
    /// </summary>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    public ArchiveCoopCommandHandler(
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        ILogger<ArchiveCoopCommandHandler> logger)
    {
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the ArchiveCoopCommand by archiving an existing coop (setting IsActive to false).
    /// </summary>
    /// <param name="request">The archive coop command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing true if archival was successful.</returns>
    public async Task<Result<bool>> Handle(ArchiveCoopCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing ArchiveCoopCommand - Id: {Id}",
            request.Id);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("ArchiveCoopCommand: User is not authenticated");
                return Result<bool>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("ArchiveCoopCommand: Tenant ID not found");
                return Result<bool>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Get the existing coop (including archived ones)
            var coop = await _coopRepository.GetByIdAsync(request.Id);
            if (coop == null)
            {
                _logger.LogWarning(
                    "ArchiveCoopCommand: Coop with ID {CoopId} not found",
                    request.Id);
                return Result<bool>.Failure(Error.NotFound("Coop not found"));
            }

            // Check if coop is already archived
            if (!coop.IsActive)
            {
                _logger.LogWarning(
                    "ArchiveCoopCommand: Coop {CoopId} is already archived",
                    request.Id);
                return Result<bool>.Failure(Error.Validation("Coop is already archived"));
            }

            // Archive the coop (soft delete)
            coop.Deactivate();
            await _coopRepository.UpdateAsync(coop);

            _logger.LogInformation(
                "Archived coop with ID: {CoopId} for tenant: {TenantId}",
                request.Id,
                tenantId.Value);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while archiving coop with ID: {Id}",
                request.Id);

            return Result<bool>.Failure(
                Error.Failure($"Failed to archive coop: {ex.Message}"));
        }
    }
}
