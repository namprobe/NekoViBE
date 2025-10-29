using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductReview
{
    public class ProductReviewRequest
    {
        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; } // 1-5

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("status")]
        public EntityStatusEnum Status { get; set; }
    }
}
