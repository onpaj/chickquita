using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByClerkOrgIdAsync(string clerkOrgId);
    Task<Tenant> AddAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task<bool> ExistsByClerkOrgIdAsync(string clerkOrgId);
}
