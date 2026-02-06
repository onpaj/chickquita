using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Users.Queries;

public record GetCurrentUserQuery : IRequest<Result<UserDto>>;

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
        // Check if user is authenticated
        if (!_currentUserService.IsAuthenticated)
        {
            _logger.LogWarning("Attempted to get current user for unauthenticated request");
            return Result<UserDto>.Failure(Error.Unauthorized("User is not authenticated"));
        }

        var clerkUserId = _currentUserService.ClerkUserId;
        if (string.IsNullOrEmpty(clerkUserId))
        {
            _logger.LogWarning("Authenticated user has no Clerk user ID");
            return Result<UserDto>.Failure(Error.Unauthorized("User identity could not be determined"));
        }

        // Fetch tenant from database using Clerk user ID
        var tenant = await _tenantRepository.GetByClerkUserIdAsync(clerkUserId);

        if (tenant == null)
        {
            _logger.LogWarning("No tenant found for Clerk user ID: {ClerkUserId}", clerkUserId);
            return Result<UserDto>.Failure(Error.NotFound("User not found"));
        }

        // Map tenant to UserDto
        var userDto = _mapper.Map<UserDto>(tenant);

        _logger.LogInformation("Successfully retrieved current user: {UserId}", userDto.Id);
        return Result<UserDto>.Success(userDto);
    }
}
