// NekoViBE.Application.Common.Mappings/BlogPostMappingProfile.cs
using AutoMapper;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings
{
    public class BlogPostMappingProfile : Profile
    {
        public BlogPostMappingProfile()
        {
            CreateMap<BlogPostRequest, BlogPost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.FeaturedImagePath, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore());

            CreateMap<BlogPost, BlogPostItem>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.UserName : null))
                .ForMember(dest => dest.PostCategory, opt => opt.MapFrom(src => src.PostCategory))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PostTags.Select(pt => pt.Tag).ToList()));

            CreateMap<BlogPost, BlogPostResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.UserName : null))
                .ForMember(dest => dest.PostCategory, opt => opt.MapFrom(src => src.PostCategory))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PostTags.Select(pt => pt.Tag).ToList()));

            // Map navigation
            CreateMap<PostCategory, PostCategoryItem>();
            CreateMap<Tag, TagItem>();
        }
    }
}