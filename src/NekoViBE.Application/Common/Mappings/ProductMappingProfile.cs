using AutoMapper;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            CreateMap<Product, ProductItem>();

            CreateMap<Product, ProductResponse>();

            CreateMap<Product, ProductRequest>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore());
        }
    }
}
