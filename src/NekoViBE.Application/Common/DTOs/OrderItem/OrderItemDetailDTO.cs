using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.OrderItem
{
    public class OrderItemDetailDTO : BaseResponse
    {
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("orderItemId")]
        public Guid Id { get; set; }

        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; } 

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("productImage")]
        public string? ProductImage { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [JsonPropertyName("itemTotal")]
        public decimal ItemTotal => (UnitPrice * Quantity) - DiscountAmount;

        [JsonPropertyName("originalTotal")]
        public decimal OriginalTotal => UnitPrice * Quantity;
    }
}
