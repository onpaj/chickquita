using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Flocks.Queries;

/// <summary>
/// Handler for GetFlocksQuery that retrieves all flocks for a specific coop.
/// </summary>
public sealed class GetFlocksQueryHandler : IRequestHandler<GetFlocksQuery, Result<List<FlockDto>>>
{
    private readonly IFlockRepository _flockRepository;
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFlocksQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFlocksQueryHandler"/> class.
    /// </summary>
    /// <param name="flockRepository">The flock repository.</param>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public GetFlocksQueryHandler(
        IFlockRepository flockRepository,
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetFlocksQueryHandler> logger)
    {
        _flockRepository = flockRepository;
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFlocksQuery by retrieving flocks for the current tenant.
    /// Can optionally filter by coop ID.
    /// </summary>
    /// <param name="request">The get flocks query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of flock DTOs.</returns>
    public async Task<Result<List<FlockDto>>> Handle(GetFlocksQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing GetFlocksQuery for CoopId: {CoopId}, IncludeInactive: {IncludeInactive}",
            request.CoopId?.ToString() ?? "All",
            request.IncludeInactive);

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetFlocksQuery: User is not authenticated");
                return Result<List<FlockDto>>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetFlocksQuery: Tenant ID not found");
                return Result<List<FlockDto>>.Failure(Error.Unauthorized("Tenant not found"));
            }

            List<Flock> flocks;

            // If CoopId is provided, filter by coop
            if (request.CoopId.HasValue)
            {
                // Verify the coop exists and belongs to the current tenant
                var coop = await _coopRepository.GetByIdAsync(request.CoopId.Value);
                if (coop == null)
                {
                    _logger.LogWarning(
                        "GetFlocksQuery: Coop not found with ID: {CoopId}",
                        request.CoopId);
                    return Result<List<FlockDto>>.Failure(Error.NotFound("Coop not found"));
                }

                // Retrieve flocks for the specified coop
                flocks = await _flockRepository.GetByCoopIdAsync(request.CoopId.Value, request.IncludeInactive);
            }
            else
            {
                // Retrieve all flocks for the current tenant
                flocks = await _flockRepository.GetAllAsync(request.IncludeInactive);
            }

            _logger.LogInformation(
                "Retrieved {Count} flocks for CoopId: {CoopId}, TenantId: {TenantId} (IncludeInactive: {IncludeInactive})",
                flocks.Count,
                request.CoopId?.ToString() ?? "All",
                tenantId.Value,
                request.IncludeInactive);

            // Map to DTOs and sort by creation date (newest first)
            var flockDtos = _mapper.Map<List<FlockDto>>(flocks)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            return Result<List<FlockDto>>.Success(flockDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving flocks for CoopId: {CoopId}",
                request.CoopId?.ToString() ?? "All");

            return Result<List<FlockDto>>.Failure(
                Error.Failure($"Failed to retrieve flocks: {ex.Message}"));
        }
    }
}
