using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Order
{
    /// <summary>
    /// Lightweight order summary for CMS table view.
    /// Optimized for displaying order list with essential information.
    /// </summary>
    public class OrderListItem : BaseResponse
    {
        // === USER INFO ===
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
        
        // === FINANCIAL BREAKDOWN ===
        [JsonPropertyName("subtotalOriginal")]
        public decimal SubtotalOriginal { get; set; }

        [JsonPropertyName("productDiscountAmount")]
        public decimal ProductDiscountAmount { get; set; }

        [JsonPropertyName("subtotalAfterProductDiscount")]
        public decimal SubtotalAfterProductDiscount { get; set; }

        [JsonPropertyName("couponDiscountAmount")]
        public decimal CouponDiscountAmount { get; set; }

        [JsonPropertyName("totalProductAmount")]
        public decimal TotalProductAmount { get; set; }

        [JsonPropertyName("shippingFeeOriginal")]
        public decimal ShippingFeeOriginal { get; set; }

        [JsonPropertyName("shippingDiscountAmount")]
        public decimal ShippingDiscountAmount { get; set; }

        [JsonPropertyName("shippingFeeActual")]
        public decimal ShippingFeeActual { get; set; }

        [JsonPropertyName("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonPropertyName("finalAmount")]
        public decimal FinalAmount { get; set; }
        
        // === STATUS ===
        [JsonPropertyName("paymentStatus")]
        public PaymentStatusEnum PaymentStatus { get; set; }

        [JsonPropertyName("orderStatus")]
        public OrderStatusEnum OrderStatus { get; set; }
        
        // === SUMMARY ===
        [JsonPropertyName("itemCount")]
        public int ItemCount { get; set; }
    }
    
    
}
