using AutoMapper;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;

namespace Chickquita.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping Tenant entities to DTOs.
/// </summary>
public sealed class TenantMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantMappingProfile"/> class.
    /// </summary>
    public TenantMappingProfile()
    {
        // Tenant -> TenantDto
        CreateMap<Tenant, TenantDto>();

        // Tenant -> UserDto (for backwards compatibility)
        CreateMap<Tenant, UserDto>();
    }
}
