using AutoMapper;
using System.Linq;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings;

public class UserAddressMappingProfile : Profile
{
    public UserAddressMappingProfile()
    {
        CreateMap<UserAddress, UserAddressDetail>();
        CreateMap<UserAddress, UserAddressItem>()
            .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src =>
                string.Join(", ", new[]
                {
                    src.Address,
                    src.WardName,
                    src.DistrictName,
                    src.ProvinceName
                }.Where(part => !string.IsNullOrWhiteSpace(part)))))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));
        CreateMap<UserAddressRequest, UserAddress>()
        .ForMember(dest => dest.User, opt => opt.Ignore()) //ignore user navigation property
        .IgnoreBaseEntityFields();
    }
}