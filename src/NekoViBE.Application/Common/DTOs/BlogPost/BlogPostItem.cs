// NekoViBE.Application.Common.DTOs.BlogPost/BlogPostItem.cs
using NekoViBE.Application.Common.DTOs.PostCategory;
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

        [JsonPropertyName("publishDate")]
        public DateTime PublishDate { get; set; }

        [JsonPropertyName("isPublished")]
        public bool IsPublished { get; set; }

        [JsonPropertyName("featuredImagePath")]
        public string? FeaturedImagePath { get; set; }

        [JsonPropertyName("tags")]
        public List<TagItem> Tags { get; set; } = new();
    }
}