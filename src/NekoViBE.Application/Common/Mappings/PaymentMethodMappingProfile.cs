using AutoMapper;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings;

public class PaymentMethodMappingProfile : Profile
{
    public PaymentMethodMappingProfile()
    {
        CreateMap<PaymentMethodRequest, PaymentMethod>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToString()))
            .ForMember(dest => dest.IconPath, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<PaymentMethod, PaymentMethodResponse>();
        CreateMap<PaymentMethod, PaymentMethodItem>();
        CreateMap<PaymentMethod, PaymentMethodResponse>();
    }
}