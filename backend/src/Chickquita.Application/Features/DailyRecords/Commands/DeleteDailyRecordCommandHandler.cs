using Chickquita.Domain.Common;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.DailyRecords.Commands;

/// <summary>
/// Handler for DeleteDailyRecordCommand that deletes an existing daily record for the current tenant.
/// </summary>
public sealed class DeleteDailyRecordCommandHandler : IRequestHandler<DeleteDailyRecordCommand, Result<bool>>
{
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteDailyRecordCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDailyRecordCommandHandler"/> class.
    /// </summary>
    /// <param name="dailyRecordRepository">The daily record repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    public DeleteDailyRecordCommandHandler(
        IDailyRecordRepository dailyRecordRepository,
        ICurrentUserService currentUserService,
        ILogger<DeleteDailyRecordCommandHandler> logger)
    {
        _dailyRecordRepository = dailyRecordRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteDailyRecordCommand by deleting an existing daily record.
    /// </summary>
    /// <param name="request">The delete daily record command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing true if deletion was successful.</returns>
    public async Task<Result<bool>> Handle(DeleteDailyRecordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing DeleteDailyRecordCommand - Id: {Id}",
            request.Id);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("DeleteDailyRecordCommand: User is not authenticated");
                return Result<bool>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("DeleteDailyRecordCommand: Tenant ID not found");
                return Result<bool>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Get the existing daily record to verify it exists and belongs to this tenant
            var dailyRecord = await _dailyRecordRepository.GetByIdWithoutNavigationAsync(request.Id);
            if (dailyRecord == null)
            {
                _logger.LogWarning(
                    "DeleteDailyRecordCommand: Daily record with ID {DailyRecordId} not found for tenant {TenantId}",
                    request.Id,
                    tenantId.Value);
                return Result<bool>.Failure(Error.NotFound("Daily record not found"));
            }

            // Delete the daily record
            await _dailyRecordRepository.DeleteAsync(request.Id);

            _logger.LogInformation(
                "Deleted daily record with ID: {DailyRecordId} for tenant: {TenantId}",
                request.Id,
                tenantId.Value);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while deleting daily record with ID: {Id}",
                request.Id);

            return Result<bool>.Failure(
                Error.Failure($"Failed to delete daily record: {ex.Message}"));
        }
    }
}
