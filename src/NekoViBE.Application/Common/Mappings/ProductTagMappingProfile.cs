using AutoMapper;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Mappings
{
    public class ProductTagMappingProfile : Profile
    {
        public ProductTagMappingProfile()
        {
            CreateMap<ProductTagRequest, ProductTag>()
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<ProductTag, ProductTagItem>()
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));

            CreateMap<ProductTag, ProductTagResponse>()
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));
        }
    }
}
