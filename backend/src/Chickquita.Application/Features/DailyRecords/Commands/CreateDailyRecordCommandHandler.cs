using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.DailyRecords.Commands;

/// <summary>
/// Handler for CreateDailyRecordCommand that creates a new daily record for egg production.
/// </summary>
public sealed class CreateDailyRecordCommandHandler : IRequestHandler<CreateDailyRecordCommand, Result<DailyRecordDto>>
{
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly IFlockRepository _flockRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateDailyRecordCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDailyRecordCommandHandler"/> class.
    /// </summary>
    /// <param name="dailyRecordRepository">The daily record repository.</param>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public CreateDailyRecordCommandHandler(
        IDailyRecordRepository dailyRecordRepository,
        IFlockRepository flockRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateDailyRecordCommandHandler> logger)
    {
        _dailyRecordRepository = dailyRecordRepository;
        _flockRepository = flockRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateDailyRecordCommand by creating a new daily record.
    /// </summary>
    /// <param name="request">The create daily record command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the daily record DTO.</returns>
    public async Task<Result<DailyRecordDto>> Handle(CreateDailyRecordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing CreateDailyRecordCommand - FlockId: {FlockId}, RecordDate: {RecordDate}, EggCount: {EggCount}",
            request.FlockId,
            request.RecordDate,
            request.EggCount);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("CreateDailyRecordCommand: User is not authenticated");
                return Result<DailyRecordDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("CreateDailyRecordCommand: Tenant ID not found");
                return Result<DailyRecordDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Check if the flock exists and belongs to the current tenant
            var flock = await _flockRepository.GetByIdWithoutHistoryAsync(request.FlockId);
            if (flock == null)
            {
                _logger.LogWarning(
                    "CreateDailyRecordCommand: Flock with ID {FlockId} not found for tenant {TenantId}",
                    request.FlockId,
                    tenantId.Value);
                return Result<DailyRecordDto>.Failure(Error.NotFound("Flock not found"));
            }

            // Check if a daily record already exists for this flock and date (duplicate detection)
            var recordExists = await _dailyRecordRepository.ExistsForFlockAndDateAsync(
                request.FlockId,
                request.RecordDate);

            if (recordExists)
            {
                _logger.LogWarning(
                    "CreateDailyRecordCommand: Daily record already exists for flock {FlockId} on date {RecordDate}",
                    request.FlockId,
                    request.RecordDate);
                return Result<DailyRecordDto>.Failure(
                    Error.Conflict("A daily record already exists for this flock on the specified date"));
            }

            // Create the daily record entity
            var dailyRecord = DailyRecord.Create(
                tenantId: tenantId.Value,
                flockId: request.FlockId,
                recordDate: request.RecordDate,
                eggCount: request.EggCount,
                notes: request.Notes);

            // Save to database
            var addedDailyRecord = await _dailyRecordRepository.AddAsync(dailyRecord);

            _logger.LogInformation(
                "Created new daily record with ID: {DailyRecordId} for flock: {FlockId}, tenant: {TenantId}",
                addedDailyRecord.Id,
                request.FlockId,
                tenantId.Value);

            var dailyRecordDto = _mapper.Map<DailyRecordDto>(addedDailyRecord);

            return Result<DailyRecordDto>.Success(dailyRecordDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error while creating daily record: {Message}",
                ex.Message);

            return Result<DailyRecordDto>.Failure(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while creating daily record for flock: {FlockId} on date: {RecordDate}",
                request.FlockId,
                request.RecordDate);

            return Result<DailyRecordDto>.Failure(
                Error.Failure($"Failed to create daily record: {ex.Message}"));
        }
    }
}
