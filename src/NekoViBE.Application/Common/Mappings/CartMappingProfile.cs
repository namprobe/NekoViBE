using AutoMapper;
using NekoViBE.Application.Common.DTOs.Cart;
using NekoViBE.Domain.Entities;
namespace NekoViBE.Application.Common.Mappings;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<ShoppingCart, CartResponse>();
        CreateMap<CartItem, CartItemResponse>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.DiscountPrice, opt => opt.MapFrom(src => src.Product.DiscountPrice))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore());
        CreateMap<CartItemRequest, CartItem>()
            .ForMember(dest => dest.CartId, opt => opt.Ignore())
            .IgnoreBaseEntityFields();
    }
}