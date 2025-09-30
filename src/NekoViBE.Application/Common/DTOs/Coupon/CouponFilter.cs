using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Coupon
{
    public class CouponFilter : BasePaginationFilter
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("discountType")]
        public DiscountTypeEnum? DiscountType { get; set; }

        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [JsonPropertyName("isExpired")]
        public bool? IsExpired { get; set; }

        [JsonPropertyName("isValid")]
        public bool? IsValid { get; set; }

        [JsonPropertyName("startDateFrom")]
        public DateTime? StartDateFrom { get; set; }

        [JsonPropertyName("startDateTo")]
        public DateTime? StartDateTo { get; set; }

        [JsonPropertyName("endDateFrom")]
        public DateTime? EndDateFrom { get; set; }

        [JsonPropertyName("endDateTo")]
        public DateTime? EndDateTo { get; set; }

        [JsonPropertyName("hasUsageLimit")]
        public bool? HasUsageLimit { get; set; }
    }
}
