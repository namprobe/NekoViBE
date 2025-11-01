// File: Application/Common/DTOs/BlogPost/BlogPostRequest.cs
using Microsoft.AspNetCore.Http;
using NekoViBE.Domain.Enums;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.BlogPost
{
    public class BlogPostRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("postCategoryId")]
        public Guid? PostCategoryId { get; set; }

        [JsonPropertyName("publishDate")]
        public DateTime? PublishDate { get; set; }

        [JsonPropertyName("isPublished")]
        public bool IsPublished { get; set; } = true;

        [JsonPropertyName("featuredImageFile")]
        public IFormFile? FeaturedImageFile { get; set; }

        [JsonPropertyName("tagIds")]
        public List<Guid> TagIds { get; set; } = new();

        [JsonPropertyName("status")]
        public EntityStatusEnum Status { get; set; } = EntityStatusEnum.Active;
    }
}