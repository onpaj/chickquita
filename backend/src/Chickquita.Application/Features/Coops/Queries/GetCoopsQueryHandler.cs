using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Coops.Queries;

/// <summary>
/// Handler for GetCoopsQuery that retrieves all coops for the current tenant.
/// </summary>
public sealed class GetCoopsQueryHandler : IRequestHandler<GetCoopsQuery, Result<List<CoopDto>>>
{
    private readonly ICoopRepository _coopRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCoopsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCoopsQueryHandler"/> class.
    /// </summary>
    /// <param name="coopRepository">The coop repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public GetCoopsQueryHandler(
        ICoopRepository coopRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetCoopsQueryHandler> logger)
    {
        _coopRepository = coopRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetCoopsQuery by retrieving all coops.
    /// </summary>
    /// <param name="request">The get coops query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of coop DTOs.</returns>
    public async Task<Result<List<CoopDto>>> Handle(GetCoopsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing GetCoopsQuery");

        try
        {
            // Verify user is authenticated
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetCoopsQuery: User is not authenticated");
                return Result<List<CoopDto>>.Failure(Error.Unauthorized("User is not authenticated"));
            }

            // Get current tenant ID
            var tenantId = _currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                _logger.LogWarning("GetCoopsQuery: Tenant ID not found");
                return Result<List<CoopDto>>.Failure(Error.Unauthorized("Tenant not found"));
            }

            // Retrieve all coops (tenant isolation is handled by RLS and global query filter)
            var coops = await _coopRepository.GetAllAsync(request.IncludeArchived);

            _logger.LogInformation(
                "Retrieved {Count} coops for tenant: {TenantId} (IncludeArchived: {IncludeArchived})",
                coops.Count,
                tenantId.Value,
                request.IncludeArchived);

            var coopDtos = _mapper.Map<List<CoopDto>>(coops);
            return Result<List<CoopDto>>.Success(coopDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while retrieving coops");

            return Result<List<CoopDto>>.Failure(
                Error.Failure($"Failed to retrieve coops: {ex.Message}"));
        }
    }
}
