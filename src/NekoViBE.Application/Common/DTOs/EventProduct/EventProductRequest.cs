using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.EventProduct
{
    // Class con chứa thông tin từng sản phẩm
    public class EventProductItemRequest
    {
        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; } = false;

        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; } = 0;
    }

    // Class chính dùng cho Controller
    public class EventProductRequest
    {
        [JsonPropertyName("eventId")]
        public Guid EventId { get; set; }

        [JsonPropertyName("products")]
        public List<EventProductItemRequest> Products { get; set; } = new List<EventProductItemRequest>();
    }
}