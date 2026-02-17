using AutoMapper;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;

namespace Chickquita.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping DailyRecord entities to DTOs.
/// </summary>
public sealed class DailyRecordMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DailyRecordMappingProfile"/> class.
    /// </summary>
    public DailyRecordMappingProfile()
    {
        // DailyRecord -> DailyRecordDto
        CreateMap<DailyRecord, DailyRecordDto>()
            .ForMember(dest => dest.FlockName,
                opt => opt.MapFrom(src => src.Flock != null ? src.Flock.Identifier : string.Empty))
            .ForMember(dest => dest.FlockCoopName,
                opt => opt.MapFrom(src => src.Flock != null && src.Flock.Coop != null ? src.Flock.Coop.Name : string.Empty));
    }
}
