using Chickquita.Domain.Common;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Handler for DeleteCoopCommand that deletes an existing coop for the current tenant.
/// </summary>
public sealed class DeleteCoopCommandHandler : IRequestHandler<DeleteCoopCommand, Result<bool>>
{
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteCoopCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCoopCommandHandler"/> class.
    /// </summary>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    public DeleteCoopCommandHandler(
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteCoopCommandHandler> logger)
    {
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteCoopCommand by deleting an existing coop.
    /// </summary>
    /// <param name="request">The delete coop command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing true if deletion was successful.</returns>
    public async Task<Result<bool>> Handle(DeleteCoopCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing DeleteCoopCommand - Id: {Id}",
            request.Id);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("DeleteCoopCommand: User is not authenticated");
                return Result<bool>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("DeleteCoopCommand: Tenant ID not found");
                return Result<bool>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Get the existing coop
            var coop = await _coopRepository.GetByIdAsync(request.Id);
            if (coop == null)
            {
                _logger.LogWarning(
                    "DeleteCoopCommand: Coop with ID {CoopId} not found",
                    request.Id);
                return Result<bool>.Failure(Error.NotFound("Coop not found"));
            }

            // Check if coop has any flocks
            var hasFlocks = await _coopRepository.HasFlocksAsync(request.Id);
            if (hasFlocks)
            {
                _logger.LogWarning(
                    "DeleteCoopCommand: Cannot delete coop {CoopId} because it has associated flocks",
                    request.Id);
                return Result<bool>.Failure(
                    Error.Validation("Cannot delete coop with existing flocks. Please delete or move all flocks first."));
            }

            // Delete the coop
            await _coopRepository.DeleteAsync(request.Id);

            _logger.LogInformation(
                "Deleted coop with ID: {CoopId} for tenant: {TenantId}",
                request.Id,
                tenantId.Value);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while deleting coop with ID: {Id}",
                request.Id);

            return Result<bool>.Failure(
                Error.Failure($"Failed to delete coop: {ex.Message}"));
        }
    }
}
