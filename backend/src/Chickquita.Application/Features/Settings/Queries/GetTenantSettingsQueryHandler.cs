using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Settings.Queries;

/// <summary>
/// Handler for GetTenantSettingsQuery that retrieves settings for the current tenant.
/// </summary>
public sealed class GetTenantSettingsQueryHandler : IRequestHandler<GetTenantSettingsQuery, Result<TenantSettingsDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTenantSettingsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTenantSettingsQueryHandler"/> class.
    /// </summary>
    /// <param name="tenantRepository">The tenant repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger instance.</param>
    public GetTenantSettingsQueryHandler(
        ITenantRepository tenantRepository,
        ICurrentUserService currentUserService,
        ILogger<GetTenantSettingsQueryHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetTenantSettingsQuery by retrieving the current tenant's settings.
    /// </summary>
    /// <param name="request">The get tenant settings query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the tenant settings DTO.</returns>
    public async Task<Result<TenantSettingsDto>> Handle(GetTenantSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing GetTenantSettingsQuery");

        try
        {
            var tenantId = _currentUserService.TenantId;
            if (tenantId == null)
            {
                _logger.LogWarning("GetTenantSettingsQuery: tenant could not be determined");
                return Result<TenantSettingsDto>.Failure(Error.Unauthorized("Tenant could not be determined"));
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value);
            if (tenant == null)
            {
                _logger.LogWarning("GetTenantSettingsQuery: tenant not found for ID {TenantId}", tenantId.Value);
                return Result<TenantSettingsDto>.Failure(Error.NotFound("Tenant not found"));
            }

            _logger.LogInformation("Retrieved settings for tenant {TenantId}", tenantId.Value);

            return Result<TenantSettingsDto>.Success(new TenantSettingsDto(tenant.SingleCoopMode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving tenant settings");
            return Result<TenantSettingsDto>.Failure(Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
