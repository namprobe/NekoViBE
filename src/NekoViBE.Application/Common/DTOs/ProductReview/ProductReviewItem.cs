using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductReview
{
    public class ProductReviewItem : BaseResponse
    {
        [JsonPropertyName("id")] 
        public Guid Id { get; set; }

        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("orderId")]
        public Guid? OrderId { get; set; }

        [JsonPropertyName("userName")] 
        public string? UserName { get; set; } // Từ User

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }
}
