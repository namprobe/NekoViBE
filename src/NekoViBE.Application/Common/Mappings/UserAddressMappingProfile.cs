using AutoMapper;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings;

public class UserAddressMappingProfile : Profile
{
    public UserAddressMappingProfile()
    {
        CreateMap<UserAddress, UserAddressDetail>();
        CreateMap<UserAddress, UserAddressItem>()
        .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => $"{src.Address}, {src.City}, {src.State}, {src.Country}"));
        CreateMap<UserAddressRequest, UserAddress>()
        .ForMember(dest => dest.User, opt => opt.Ignore()) //ignore user navigation property
        .IgnoreBaseEntityFields();
    }
}