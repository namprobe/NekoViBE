using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Order
{
    public class OrderListItem : BaseResponse
    {

        [JsonPropertyName("userId")]
        public Guid? UserId { get; set; }

        [JsonPropertyName("userEmail")]
        public string? UserEmail { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("isOneClick")]
        public bool IsOneClick { get; set; }

        [JsonPropertyName("guestEmail")]
        public string? GuestEmail { get; set; }

        [JsonPropertyName("guestName")]
        public string? GuestName { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [JsonPropertyName("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonPropertyName("shippingAmount")]
        public decimal ShippingAmount { get; set; }

        [JsonPropertyName("finalAmount")]
        public decimal FinalAmount { get; set; }

        [JsonPropertyName("paymentStatus")]
        public PaymentStatusEnum PaymentStatus { get; set; }

        [JsonPropertyName("orderStatus")]
        public OrderStatusEnum OrderStatus { get; set; }

        [JsonPropertyName("itemCount")]
        public int ItemCount { get; set; }
    }
    
    
}
