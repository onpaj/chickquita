using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Users.Queries;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        ITenantRepository tenantRepository,
        IMapper mapper,
        ILogger<GetCurrentUserQueryHandler> logger)
    {
        _currentUserService = currentUserService;
        _tenantRepository = tenantRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            _logger.LogWarning("Attempted to get current user for unauthenticated request");
            return Result<UserDto>.Failure(Error.Unauthorized("User is not authenticated"));
        }

        var tenantId = _currentUserService.TenantId;
        if (tenantId == null)
        {
            _logger.LogWarning("Authenticated user has no resolved tenant (missing org_id claim?)");
            return Result<UserDto>.Failure(Error.Unauthorized("Tenant could not be determined"));
        }

        var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value);

        if (tenant == null)
        {
            _logger.LogWarning("No tenant found for tenant ID: {TenantId}", tenantId);
            return Result<UserDto>.Failure(Error.NotFound("User not found"));
        }

        var userDto = _mapper.Map<UserDto>(tenant);
        _logger.LogInformation("Successfully retrieved current user: {UserId}", userDto.Id);
        return Result<UserDto>.Success(userDto);
    }
}
