using AutoMapper;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;

namespace Chickquita.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping Flock entities to DTOs.
/// </summary>
public sealed class FlockMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlockMappingProfile"/> class.
    /// </summary>
    public FlockMappingProfile()
    {
        // FlockHistory -> FlockHistoryDto
        CreateMap<FlockHistory, FlockHistoryDto>();

        // Flock -> FlockDto
        CreateMap<Flock, FlockDto>()
            .ForMember(dest => dest.History,
                opt => opt.MapFrom(src => src.History.OrderByDescending(h => h.ChangeDate)));
    }
}
