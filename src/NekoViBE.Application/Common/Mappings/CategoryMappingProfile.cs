using AutoMapper;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Mappings
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<CategoryRequest, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ImagePath, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<Category, CategoryItem>()
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath ?? ""));

            CreateMap<Category, CategoryResponse>()
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath ?? ""));

            CreateMap<Category, CategoryRequest>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore());
        }
    }
}
