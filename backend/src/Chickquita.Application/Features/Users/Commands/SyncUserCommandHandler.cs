using AutoMapper;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Users.Commands;

/// <summary>
/// Handler for SyncUserCommand that creates or retrieves a tenant for a Clerk user.
/// This handler implements idempotent behavior - it will return the existing tenant
/// if one already exists for the given Clerk user ID.
/// </summary>
public sealed class SyncUserCommandHandler : IRequestHandler<SyncUserCommand, Result<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncUserCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncUserCommandHandler"/> class.
    /// </summary>
    /// <param name="tenantRepository">The tenant repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger instance.</param>
    public SyncUserCommandHandler(
        ITenantRepository tenantRepository,
        IMapper mapper,
        ILogger<SyncUserCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the SyncUserCommand by creating a new tenant or returning an existing one.
    /// </summary>
    /// <param name="request">The sync user command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the tenant DTO.</returns>
    public async Task<Result<TenantDto>> Handle(SyncUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing SyncUserCommand for Clerk user ID: {ClerkUserId}, Email: {Email}",
            request.ClerkUserId,
            request.Email);

        try
        {
            // Check if tenant already exists for this Clerk user ID (idempotent behavior)
            var existingTenant = await _tenantRepository.GetByClerkUserIdAsync(request.ClerkUserId);

            if (existingTenant is not null)
            {
                _logger.LogInformation(
                    "Tenant already exists for Clerk user ID: {ClerkUserId}, returning existing tenant ID: {TenantId}",
                    request.ClerkUserId,
                    existingTenant.Id);

                // Update email if it has changed
                if (existingTenant.Email != request.Email)
                {
                    _logger.LogInformation(
                        "Updating email for tenant {TenantId} from {OldEmail} to {NewEmail}",
                        existingTenant.Id,
                        existingTenant.Email,
                        request.Email);

                    existingTenant.UpdateEmail(request.Email);
                    await _tenantRepository.UpdateAsync(existingTenant);
                }

                var existingTenantDto = _mapper.Map<TenantDto>(existingTenant);
                return Result<TenantDto>.Success(existingTenantDto);
            }

            // Create new tenant
            var newTenant = Tenant.Create(request.ClerkUserId, request.Email);

            var addedTenant = await _tenantRepository.AddAsync(newTenant);

            _logger.LogInformation(
                "Created new tenant with ID: {TenantId} for Clerk user ID: {ClerkUserId}",
                addedTenant.Id,
                request.ClerkUserId);

            var tenantDto = _mapper.Map<TenantDto>(addedTenant);
            return Result<TenantDto>.Success(tenantDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while syncing user with Clerk user ID: {ClerkUserId}",
                request.ClerkUserId);

            return Result<TenantDto>.Failure(
                Error.Failure($"Failed to sync user: {ex.Message}"));
        }
    }
}
