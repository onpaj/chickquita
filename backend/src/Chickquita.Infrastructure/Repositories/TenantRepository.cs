using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of ITenantRepository
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByClerkUserIdAsync(string clerkUserId)
    {
        if (string.IsNullOrWhiteSpace(clerkUserId))
        {
            throw new ArgumentException("Clerk user ID cannot be null or whitespace", nameof(clerkUserId));
        }

        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.ClerkUserId == clerkUserId);
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or whitespace", nameof(email));
        }

        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Email == email);
    }

    /// <inheritdoc />
    public async Task<Tenant> AddAsync(Tenant tenant)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        return tenant;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Tenant tenant)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByClerkUserIdAsync(string clerkUserId)
    {
        if (string.IsNullOrWhiteSpace(clerkUserId))
        {
            throw new ArgumentException("Clerk user ID cannot be null or whitespace", nameof(clerkUserId));
        }

        return await _context.Tenants
            .AnyAsync(t => t.ClerkUserId == clerkUserId);
    }
}
