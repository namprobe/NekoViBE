using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Order
{
    public class OrderFilter : BasePaginationFilter
    {
        [JsonPropertyName("orderNumber")]
        public string? OrderNumber { get; set; }

        [JsonPropertyName("userId")]
        public Guid? UserId { get; set; }

        [JsonPropertyName("userEmail")]
        public string? UserEmail { get; set; }

        [JsonPropertyName("guestEmail")]
        public string? GuestEmail { get; set; }

        [JsonPropertyName("isOneClick")]
        public bool? IsOneClick { get; set; }

        [JsonPropertyName("paymentStatus")]
        public PaymentStatusEnum? PaymentStatus { get; set; }

        [JsonPropertyName("orderStatus")]
        public OrderStatusEnum? OrderStatus { get; set; }

        [JsonPropertyName("minAmount")]
        public decimal? MinAmount { get; set; }

        [JsonPropertyName("maxAmount")]
        public decimal? MaxAmount { get; set; }

        [JsonPropertyName("dateFrom")]
        public DateTime? DateFrom { get; set; }

        [JsonPropertyName("dateTo")]
        public DateTime? DateTo { get; set; }

        [JsonPropertyName("hasCoupon")]
        public bool? HasCoupon { get; set; }
    }
    
}
