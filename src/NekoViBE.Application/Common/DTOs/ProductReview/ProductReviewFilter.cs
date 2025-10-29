using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductReview
{
    public class ProductReviewFilter : BasePaginationFilter
    {
        [JsonPropertyName("productId")]
        public Guid? ProductId { get; set; }

        [JsonPropertyName("userId")]
        public Guid? UserId { get; set; }

        [JsonPropertyName("rating")]
        public int? Rating { get; set; }
    }
}
