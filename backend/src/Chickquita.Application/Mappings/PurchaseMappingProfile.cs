using AutoMapper;
using Chickquita.Domain.Entities;
using Chickquita.Application.DTOs;

namespace Chickquita.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping Purchase entities to DTOs.
/// </summary>
public sealed class PurchaseMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PurchaseMappingProfile"/> class.
    /// </summary>
    public PurchaseMappingProfile()
    {
        // Purchase -> PurchaseDto
        CreateMap<Purchase, PurchaseDto>();
    }
}
