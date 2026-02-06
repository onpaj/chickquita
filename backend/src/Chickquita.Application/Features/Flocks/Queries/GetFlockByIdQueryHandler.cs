using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Flocks.Queries;

/// <summary>
/// Handler for GetFlockByIdQuery that retrieves a single flock by ID with history.
/// </summary>
public sealed class GetFlockByIdQueryHandler : IRequestHandler<GetFlockByIdQuery, Result<FlockDto>>
{
    private readonly IFlockRepository _flockRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFlockByIdQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFlockByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public GetFlockByIdQueryHandler(
        IFlockRepository flockRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetFlockByIdQueryHandler> logger)
    {
        _flockRepository = flockRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFlockByIdQuery by retrieving the flock with the specified ID.
    /// </summary>
    /// <param name="request">The get flock by ID query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the flock DTO with history.</returns>
    public async Task<Result<FlockDto>> Handle(GetFlockByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing GetFlockByIdQuery for FlockId: {FlockId}",
            request.FlockId);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetFlockByIdQuery: User is not authenticated");
                return Result<FlockDto>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetFlockByIdQuery: Tenant ID not found");
                return Result<FlockDto>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Retrieve flock by ID with history
            // Tenant isolation is handled by RLS and global query filter in the repository
            var flock = await _flockRepository.GetByIdAsync(request.FlockId);

            // Return NOT_FOUND if flock doesn't exist or belongs to different tenant
            if (flock == null)
            {
                _logger.LogWarning(
                    "GetFlockByIdQuery: Flock not found with ID: {FlockId}",
                    request.FlockId);
                return Result<FlockDto>.Failure(Error.NotFound("Flock not found"));
            }

            _logger.LogInformation(
                "Retrieved flock with ID: {FlockId}, TenantId: {TenantId}, History entries: {HistoryCount}",
                flock.Id,
                tenantId.Value,
                flock.History.Count);

            // Map to DTO (includes history ordered by ChangeDate descending)
            var flockDto = _mapper.Map<FlockDto>(flock);

            return Result<FlockDto>.Success(flockDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving flock with ID: {FlockId}",
                request.FlockId);

            return Result<FlockDto>.Failure(
                Error.Failure($"Failed to retrieve flock: {ex.Message}"));
        }
    }
}
