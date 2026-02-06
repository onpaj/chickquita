using AutoMapper;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;

namespace Chickquita.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping Coop entities to DTOs.
/// </summary>
public sealed class CoopMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CoopMappingProfile"/> class.
    /// </summary>
    public CoopMappingProfile()
    {
        // Coop -> CoopDto
        CreateMap<Coop, CoopDto>();
    }
}
