using AutoMapper;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings;

public class ShippingMethodMappingProfile : Profile
{
    public ShippingMethodMappingProfile()
    {
        // ShippingMethod -> ShippingMethodItem
        CreateMap<ShippingMethod, ShippingMethodItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost))
            .ForMember(dest => dest.EstimatedDays, opt => opt.MapFrom(src => src.EstimatedDays));

        // ShippingMethod -> ShippingMethodResponse
        CreateMap<ShippingMethod, ShippingMethodResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost))
            .ForMember(dest => dest.EstimatedDays, opt => opt.MapFrom(src => src.EstimatedDays));
    }
}

