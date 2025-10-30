// NekoViBE.Application.Common.DTOs.BlogPost/BlogPostFilter.cs
using NekoViBE.Application.Common.Models;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.BlogPost
{
    public class BlogPostFilter : BasePaginationFilter
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("categoryId")]
        public Guid? CategoryId { get; set; }

        [JsonPropertyName("tagIds")]
        public List<Guid>? TagIds { get; set; }

        [JsonPropertyName("isPublished")]
        public bool? IsPublished { get; set; }
    }
}