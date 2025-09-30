using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Coupon
{
    public class CouponItem : BaseResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("discountType")]
        public DiscountTypeEnum DiscountType { get; set; }

        [JsonPropertyName("discountValue")]
        public decimal DiscountValue { get; set; }

        [JsonPropertyName("minOrderAmount")]
        public decimal MinOrderAmount { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("usageLimit")]
        public int? UsageLimit { get; set; }

        [JsonPropertyName("currentUsage")]
        public int CurrentUsage { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("remainingUses")]
        public int? RemainingUses => UsageLimit - CurrentUsage;

        [JsonPropertyName("isExpired")]
        public bool IsExpired => DateTime.UtcNow > EndDate;

        [JsonPropertyName("isValid")]
        public bool IsValid => IsActive && !IsExpired && (RemainingUses == null || RemainingUses > 0);
    }
}
