using AutoMapper;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<ProductRequest, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.AnimeSeries, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.ProductTags, opt => opt.Ignore())
                .ForMember(dest => dest.ProductReviews, opt => opt.Ignore())
                .ForMember(dest => dest.CartItems, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.WishlistItems, opt => opt.Ignore())
                .ForMember(dest => dest.EventProducts, opt => opt.Ignore())
                .ForMember(dest => dest.ProductInventories, opt => opt.Ignore());

            CreateMap<Product, ProductItem>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.AnimeSeriesId, opt => opt.MapFrom(src => src.AnimeSeriesId));// Ánh xạ trực tiếp

            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.ProductTags, opt => opt.MapFrom(src => src.ProductTags))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.AnimeSeriesId, opt => opt.MapFrom(src => src.AnimeSeriesId))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.ProductReviews))
                .ForMember(dest => dest.Events, opt => opt.MapFrom(src => src.EventProducts.Select(ep => ep.Event)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.DiscountPrice, opt => opt.MapFrom(src => src.DiscountPrice))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.IsPreOrder, opt => opt.MapFrom(src => src.IsPreOrder))
                .ForMember(dest => dest.PreOrderReleaseDate, opt => opt.MapFrom(src => src.PreOrderReleaseDate))
                .ForMember(dest => dest.TotalSales, opt => opt.Ignore())
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore());

            CreateMap<Product, ProductRequest>()
                .ForMember(dest => dest.ImageFiles, opt => opt.Ignore());

            // Mapping cho các entity con
            CreateMap<ProductImage, ProductImageResponse>();
            CreateMap<ProductReview, ProductReviewResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName)); // Giả sử User có UserName
            CreateMap<Tag, TagItem>();
            CreateMap<Event, EventItem>();

            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.AnimeSeries, opt => opt.Ignore())
                .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                .ForMember(dest => dest.ProductTags, opt => opt.Ignore())
                .ForMember(dest => dest.ProductReviews, opt => opt.Ignore())
                .ForMember(dest => dest.CartItems, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.WishlistItems, opt => opt.Ignore())
                .ForMember(dest => dest.EventProducts, opt => opt.Ignore())
                .ForMember(dest => dest.ProductInventories, opt => opt.Ignore());

            CreateMap<Product, UpdateProductDto>()
                .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
                .ForMember(dest => dest.ExistingImageIds, opt => opt.MapFrom(src => src.ProductImages.Select(img => img.Id)));
        }
    }
}