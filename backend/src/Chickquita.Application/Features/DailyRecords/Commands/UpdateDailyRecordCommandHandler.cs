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
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDailyRecordCommandHandler"/> class.
    /// </summary>
    /// <param name="dailyRecordRepository">The daily record repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateDailyRecordCommandHandler(
        IDailyRecordRepository dailyRecordRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateDailyRecordCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _dailyRecordRepository = dailyRecordRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
            var tenantId = _currentUserService.TenantId;

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

            // Parse optional collection time (null = preserve existing value)
            TimeSpan? collectionTime = null;
            if (!string.IsNullOrWhiteSpace(request.CollectionTime))
            {
                if (TimeSpan.TryParseExact(request.CollectionTime, @"hh\:mm", null, out var parsedTime))
                {
                    collectionTime = parsedTime;
                }
            }

            // Update the daily record
            var updateResult = dailyRecord.Update(request.EggCount, request.Notes, collectionTime);
            if (updateResult.IsFailure)
                return Result<DailyRecordDto>.Failure(updateResult.Error);

            // Save to database
            var updatedDailyRecord = await _dailyRecordRepository.UpdateAsync(dailyRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated daily record with ID: {DailyRecordId}, tenant: {TenantId}",
                updatedDailyRecord.Id,
                tenantId.Value);

            var dailyRecordDto = _mapper.Map<DailyRecordDto>(updatedDailyRecord);

            return Result<DailyRecordDto>.Success(dailyRecordDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while updating daily record: {DailyRecordId}",
                request.Id);

            return Result<DailyRecordDto>.Failure(
                Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
