using AutoMapper;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings;

public class ShippingMethodMappingProfile : Profile
{
    public ShippingMethodMappingProfile()
    {
        // ShippingMethodRequest -> ShippingMethod (for Create and Update)
        CreateMap<ShippingMethodRequest, ShippingMethod>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost))
            .ForMember(dest => dest.EstimatedDays, opt => opt.MapFrom(src => src.EstimatedDays))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.OrderShippingMethods, opt => opt.Ignore());

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

