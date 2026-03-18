using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Domain.Entities;

namespace Chickquita.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping EggSale entities to DTOs.
/// </summary>
public sealed class EggSaleMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EggSaleMappingProfile"/> class.
    /// </summary>
    public EggSaleMappingProfile()
    {
        // EggSale -> EggSaleDto
        // TotalRevenue is a computed field (Quantity * PricePerUnit) not stored on the entity.
        CreateMap<EggSale, EggSaleDto>()
            .ForMember(
                dest => dest.TotalRevenue,
                opt => opt.MapFrom(src => src.Quantity * src.PricePerUnit));
    }
}
