using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tenant?> GetByClerkOrgIdAsync(string clerkOrgId)
    {
        if (string.IsNullOrWhiteSpace(clerkOrgId))
            throw new ArgumentException("Clerk org ID cannot be null or whitespace", nameof(clerkOrgId));

        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.ClerkOrgId == clerkOrgId);
    }

    public async Task<Tenant> AddAsync(Tenant tenant)
    {
        if (tenant == null)
            throw new ArgumentNullException(nameof(tenant));

        await _context.Tenants.AddAsync(tenant);
        return tenant;
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        if (tenant == null)
            throw new ArgumentNullException(nameof(tenant));

        _context.Tenants.Update(tenant);    }

    public async Task<bool> ExistsByClerkOrgIdAsync(string clerkOrgId)
    {
        if (string.IsNullOrWhiteSpace(clerkOrgId))
            throw new ArgumentException("Clerk org ID cannot be null or whitespace", nameof(clerkOrgId));

        return await _context.Tenants
            .AnyAsync(t => t.ClerkOrgId == clerkOrgId);
    }
}
