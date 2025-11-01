// File: Application/Common/Mappings/BlogPostMappingProfile.cs
using AutoMapper;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.DTOs.PostTag;
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
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.FeaturedImagePath, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore());

            CreateMap<BlogPost, BlogPostItem>()
    .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.UserName : null))
    .ForMember(dest => dest.PostTags, opt => opt.MapFrom(src => src.PostTags));


            CreateMap<BlogPost, BlogPostResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.UserName : null))
                .ForMember(dest => dest.PostTags, opt => opt.MapFrom(src => src.PostTags));


            CreateMap<PostTag, PostTagItem>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                    src.Tag != null
                        ? new List<TagItem>
                        {
                            new TagItem
                            {
                                Id = src.Tag.Id.ToString(),
                                Name = src.Tag.Name,
                                CreatedAt = (DateTime)src.Tag.CreatedAt,
                                Status = src.Tag.Status
                            }
                        }
                        : new List<TagItem>()
                ));
        }
    }
}