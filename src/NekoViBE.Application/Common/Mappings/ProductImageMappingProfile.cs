using AutoMapper;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Mappings
{
    public class ProductImageMappingProfile : Profile
    {
        public ProductImageMappingProfile()
        {
            CreateMap<ProductImageRequest, ProductImage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImagePath, opt => opt.Ignore()) // Handled in handler
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<ProductImage, ProductImageItem>();
            CreateMap<ProductImage, ProductImageResponse>();

            CreateMap<ProductImage, ProductImageRequest>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Image, opt => opt.Ignore()) // IFormFile not mapped
                .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary))
                .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        }
    }
}
