using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.EventProduct
{
    public class EventProductSaveRequest
    {
        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; } = false;

        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; } // 0 -> 100
    }
}
