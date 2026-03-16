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
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCoopCommandHandler"/> class.
    /// </summary>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    public DeleteCoopCommandHandler(
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteCoopCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
            var tenantId = _currentUserService.TenantId;

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
                    Error.ValidationWithCode("HAS_FLOCKS", "Cannot delete coop with existing flocks. Please delete or move all flocks first."));
            }

            // Delete the coop
            await _coopRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
