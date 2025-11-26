using AutoMapper;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Mappings.Resolvers;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings;

public class PaymentMethodMappingProfile : Profile
{
    public PaymentMethodMappingProfile()
    {
        CreateMap<PaymentMethodRequest, PaymentMethod>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToString()))
            .ForMember(dest => dest.IconPath, opt => opt.Ignore()) // IconPath is handled manually in handlers
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        
        // Map to response DTOs with IconPath converted to URL
        CreateMap<PaymentMethod, PaymentMethodItem>()
            .ForMember(dest => dest.IconPath, opt => opt.ConvertUsing<FilePathUrlConverter, string?>(src => src.IconPath));
        
        CreateMap<PaymentMethod, PaymentMethodResponse>()
            .ForMember(dest => dest.IconPath, opt => opt.ConvertUsing<FilePathUrlConverter, string?>(src => src.IconPath));
    }
}