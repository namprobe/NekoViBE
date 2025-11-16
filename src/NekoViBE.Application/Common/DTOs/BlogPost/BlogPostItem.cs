// NekoViBE.Application.Common.DTOs.BlogPost/BlogPostItem.cs
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.DTOs.PostTag;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Models;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.BlogPost
{
    public class BlogPostItem : BaseResponse
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("postCategoryId")]
        public Guid? PostCategoryId { get; set; }

        [JsonPropertyName("postCategory")]
        public PostCategoryItem? PostCategory { get; set; }

        [JsonPropertyName("authorId")]
        public Guid? AuthorId { get; set; }

        [JsonPropertyName("authorName")]
        public string? AuthorName { get; set; }

        [JsonPropertyName("authorAvatar")]
        public string? AuthorAvatar { get; set; }

        [JsonPropertyName("publishDate")]
        public DateTime PublishDate { get; set; }

        [JsonPropertyName("isPublished")]
        public bool IsPublished { get; set; }

        [JsonPropertyName("featuredImage")]
        public string? FeaturedImage { get; set; } 

        [JsonPropertyName("postTags")]
        public List<PostTagItem> PostTags { get; set; } = new();

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

    }
}