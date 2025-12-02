// NekoViBE.Application.Common.Mappings/PostCategoryMappingProfile.cs
using AutoMapper;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings
{
    public class PostCategoryMappingProfile : Profile
    {
        public PostCategoryMappingProfile()
        {
            CreateMap<PostCategoryRequest, PostCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            CreateMap<PostCategory, PostCategoryItem>();
            CreateMap<PostCategory, PostCategoryResponse>();
            CreateMap<PostCategory, PostCategoryRequest>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        }
    }
}