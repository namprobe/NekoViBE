using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductReview
{
    public class ProductReviewResponse
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("rating")] public int Rating { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
        [JsonPropertyName("userName")] public string? UserName { get; set; } // Từ User
    }
}
