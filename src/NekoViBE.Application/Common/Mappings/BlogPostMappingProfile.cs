using AutoMapper;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.DTOs.PostTag;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings
{
    public class BlogPostMappingProfile : Profile
    {
        public BlogPostMappingProfile()
        {
            // 1. BlogPostRequest → BlogPost (Update)
            CreateMap<BlogPostRequest, BlogPost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.FeaturedImagePath, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.PostCategory, opt => opt.Ignore())
                .ForMember(dest => dest.PostCategoryId, opt => opt.MapFrom(src => src.PostCategoryId)); ;

            // 2. BlogPost → BlogPostRequest (Audit log) ← CHỈ MAP NHỮNG FIELD CẦN
            CreateMap<BlogPost, BlogPostRequest>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.PostCategoryId, opt => opt.MapFrom(src => src.PostCategoryId))
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.PublishDate))
                .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
                .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.PostTags.Select(pt => pt.TagId).ToList()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.FeaturedImageFile, opt => opt.Ignore()); // Không map file

            // 3. BlogPost → BlogPostItem (List)
            CreateMap<BlogPost, BlogPostItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.UserName : null))
                .ForMember(dest => dest.AuthorAvatar, opt => opt.MapFrom(src => src.Author != null ? src.Author.AvatarPath : null))
                .ForMember(dest => dest.PostCategory, opt => opt.MapFrom(src => src.PostCategory != null
                    ? new PostCategoryItem { Id = src.PostCategory.Id.ToString(), Name = src.PostCategory.Name }
                    : null))
                .ForMember(dest => dest.PostTags, opt => opt.MapFrom(src => src.PostTags));

            // 4. BlogPost → BlogPostResponse (Detail)
            CreateMap<BlogPost, BlogPostResponse>()
                .IncludeBase<BlogPost, BlogPostItem>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.FeaturedImage, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore()); ; // Sẽ set sau

            // 5. PostTag → PostTagItem
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