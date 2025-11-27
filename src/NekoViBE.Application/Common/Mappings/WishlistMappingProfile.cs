using AutoMapper;
using NekoViBE.Application.Common.DTOs.Wishlist;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings;

public class WishlistMappingProfile : Profile
{
    public WishlistMappingProfile()
    {
        CreateMap<WishlistItem, WishlistItemDto>()
            .ForMember(dest => dest.WishlistItemId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AddedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}
