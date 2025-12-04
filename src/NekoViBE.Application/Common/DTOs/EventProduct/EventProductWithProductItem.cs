// File: Application/Common/DTOs/EventProduct/EventProductWithProductItem.cs
using System.Text.Json.Serialization;
using NekoViBE.Application.Common.DTOs.Product;

namespace NekoViBE.Application.Common.DTOs.EventProduct
{
    public class EventProductWithProductItem : BaseResponse
    {
        [JsonPropertyName("eventId")]
        public Guid EventId { get; set; }

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; }

        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        // Thông tin Product đầy đủ
        [JsonPropertyName("product")]
        public ProductItem Product { get; set; } = null!;
    }
}