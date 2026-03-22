using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Settings.Commands;

/// <summary>
/// Handler for UpdateTenantSettingsCommand that updates settings for the current tenant.
/// </summary>
public sealed class UpdateTenantSettingsCommandHandler : IRequestHandler<UpdateTenantSettingsCommand, Result<bool>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTenantSettingsCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTenantSettingsCommandHandler"/> class.
    /// </summary>
    /// <param name="tenantRepository">The tenant repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateTenantSettingsCommandHandler(
        ITenantRepository tenantRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateTenantSettingsCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger;
    }

    /// <summary>
    /// Handles the UpdateTenantSettingsCommand by updating the current tenant's settings.
    /// </summary>
    /// <param name="request">The update tenant settings command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result<bool>> Handle(UpdateTenantSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UpdateTenantSettingsCommand - SingleCoopMode: {SingleCoopMode}, RevenueTrackingEnabled: {RevenueTrackingEnabled}, Currency: {Currency}",
            request.SingleCoopMode,
            request.RevenueTrackingEnabled,
            request.Currency);

        try
        {
            var tenantId = _currentUserService.TenantId;
            if (tenantId == null)
            {
                _logger.LogWarning("UpdateTenantSettingsCommand: tenant could not be determined");
                return Result<bool>.Failure(Error.Unauthorized("Tenant could not be determined"));
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value);
            if (tenant == null)
            {
                _logger.LogWarning("UpdateTenantSettingsCommand: tenant not found for ID {TenantId}", tenantId.Value);
                return Result<bool>.Failure(Error.NotFound("Tenant not found"));
            }

            var updateResult = tenant.UpdateSettings(request.SingleCoopMode, request.RevenueTrackingEnabled, request.Currency);
            if (updateResult.IsFailure)
                return Result<bool>.Failure(updateResult.Error);

            await _tenantRepository.UpdateAsync(tenant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated settings for tenant {TenantId} - SingleCoopMode: {SingleCoopMode}, RevenueTrackingEnabled: {RevenueTrackingEnabled}, Currency: {Currency}",
                tenantId.Value,
                request.SingleCoopMode,
                request.RevenueTrackingEnabled,
                tenant.Currency);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating tenant settings");
            return Result<bool>.Failure(Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
