// NekoViBE.Application.Common.DTOs.BlogPost/BlogPostResponse.cs
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.DTOs.Tag;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.BlogPost
{
    public class BlogPostResponse : BlogPostItem
    {
        [JsonPropertyName("products")]
        public List<ProductItem>? Products { get; set; } = new List<ProductItem>();
    }
}