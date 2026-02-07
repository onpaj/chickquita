using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.DailyRecords.Commands;

/// <summary>
/// Handler for UpdateDailyRecordCommand that updates an existing daily record.
/// Enforces same-day edit restriction: records can only be updated on the same day they were created.
/// </summary>
public sealed class UpdateDailyRecordCommandHandler : IRequestHandler<UpdateDailyRecordCommand, Result<DailyRecordDto>>
{
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateDailyRecordCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDailyRecordCommandHandler"/> class.
    /// </summary>
    /// <param name="dailyRecordRepository">The daily record repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateDailyRecordCommandHandler(
        IDailyRecordRepository dailyRecordRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateDailyRecordCommandHandler> logger)
    {
        _dailyRecordRepository = dailyRecordRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateDailyRecordCommand by updating an existing daily record.
    /// Enforces same-day edit restriction based on the RecordDate.
    /// </summary>
    /// <param name="request">The update daily record command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the updated daily record DTO.</returns>
    public async Task<Result<DailyRecordDto>> Handle(UpdateDailyRecordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UpdateDailyRecordCommand - DailyRecordId: {DailyRecordId}, EggCount: {EggCount}",
            request.Id,
            request.EggCount);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("UpdateDailyRecordCommand: User is not authenticated");
                return Result<DailyRecordDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("UpdateDailyRecordCommand: Tenant ID not found");
                return Result<DailyRecordDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Get the existing daily record
            var dailyRecord = await _dailyRecordRepository.GetByIdWithoutNavigationAsync(request.Id);
            if (dailyRecord == null)
            {
                _logger.LogWarning(
                    "UpdateDailyRecordCommand: Daily record with ID {DailyRecordId} not found for tenant {TenantId}",
                    request.Id,
                    tenantId.Value);
                return Result<DailyRecordDto>.Failure(Error.NotFound("Daily record not found"));
            }

            // Enforce same-day edit restriction:
            // The record can only be updated on the same day as its RecordDate
            var today = DateTime.UtcNow.Date;
            if (dailyRecord.RecordDate != today)
            {
                _logger.LogWarning(
                    "UpdateDailyRecordCommand: Same-day edit restriction violated. Record date: {RecordDate}, Today: {Today}",
                    dailyRecord.RecordDate,
                    today);
                return Result<DailyRecordDto>.Failure(
                    Error.Validation("Daily records can only be updated on the same day they were created (same-day edit restriction)"));
            }

            // Update the daily record
            dailyRecord.Update(request.EggCount, request.Notes);

            // Save to database
            var updatedDailyRecord = await _dailyRecordRepository.UpdateAsync(dailyRecord);

            _logger.LogInformation(
                "Updated daily record with ID: {DailyRecordId}, tenant: {TenantId}",
                updatedDailyRecord.Id,
                tenantId.Value);

            var dailyRecordDto = _mapper.Map<DailyRecordDto>(updatedDailyRecord);

            return Result<DailyRecordDto>.Success(dailyRecordDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error while updating daily record: {Message}",
                ex.Message);

            return Result<DailyRecordDto>.Failure(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating daily record: {DailyRecordId}",
                request.Id);

            return Result<DailyRecordDto>.Failure(
                Error.Failure($"Failed to update daily record: {ex.Message}"));
        }
    }
}
