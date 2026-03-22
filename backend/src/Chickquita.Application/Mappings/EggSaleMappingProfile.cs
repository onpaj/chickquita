using AutoMapper;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;

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
        CreateMap<EggSale, EggSaleDto>();
    }
}
