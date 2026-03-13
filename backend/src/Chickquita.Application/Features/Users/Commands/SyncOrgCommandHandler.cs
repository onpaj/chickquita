using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Chickquita.Application.Features.Users.Commands;

/// <summary>
/// Handler for SyncOrgCommand that creates or updates a tenant for a Clerk organization.
/// Implements idempotent behavior - calling multiple times with same data creates no duplicates.
/// </summary>
public sealed class SyncOrgCommandHandler : IRequestHandler<SyncOrgCommand, Result<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncOrgCommandHandler> _logger;

    public SyncOrgCommandHandler(
        ITenantRepository tenantRepository,
        IMapper mapper,
        ILogger<SyncOrgCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TenantDto>> Handle(SyncOrgCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing SyncOrgCommand for Clerk org ID: {ClerkOrgId}, Name: {Name}",
            request.ClerkOrgId,
            request.Name);

        try
        {
            var existing = await _tenantRepository.GetByClerkOrgIdAsync(request.ClerkOrgId);

            if (existing is not null)
            {
                if (existing.Name != request.Name)
                {
                    _logger.LogInformation(
                        "Updating name for tenant {TenantId} from '{OldName}' to '{NewName}'",
                        existing.Id, existing.Name, request.Name);
                    existing.UpdateName(request.Name);
                    await _tenantRepository.UpdateAsync(existing);
                }
                return Result<TenantDto>.Success(_mapper.Map<TenantDto>(existing));
            }

            var tenant = Tenant.Create(request.ClerkOrgId, request.Name);
            var added = await _tenantRepository.AddAsync(tenant);

            _logger.LogInformation(
                "Created tenant {TenantId} for org {ClerkOrgId}",
                added.Id, request.ClerkOrgId);

            return Result<TenantDto>.Success(_mapper.Map<TenantDto>(added));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing org {ClerkOrgId}", request.ClerkOrgId);
            return Result<TenantDto>.Failure(Error.Failure("An unexpected error occurred. Please try again."));
        }
    }
}
