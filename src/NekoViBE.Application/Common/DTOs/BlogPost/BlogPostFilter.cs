// File: Application/Common/DTOs/BlogPost/BlogPostFilter.cs
using NekoViBE.Application.Common.Models;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.BlogPost
{
    public class BlogPostFilter : BasePaginationFilter
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("postCategoryId")]
        public Guid? PostCategoryId { get; set; }

        [JsonPropertyName("authorId")]
        public Guid? AuthorId { get; set; }

        [JsonPropertyName("isPublished")]
        public bool? IsPublished { get; set; }

        [JsonPropertyName("tagIds")]
        public List<Guid>? TagIds { get; set; }
    }
}