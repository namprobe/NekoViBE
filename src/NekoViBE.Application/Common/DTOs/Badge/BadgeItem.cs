using System.Text.Json.Serialization;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Badge
{
    public class BadgeItem : BaseResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("iconPath")]
        public string? IconPath { get; set; }

        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonPropertyName("conditionType")]
        public ConditionTypeEnum ConditionType { get; set; }

        [JsonPropertyName("conditionValue")]
        public string ConditionValue { get; set; } = string.Empty;

        [JsonPropertyName("isTimeLimited")]
        public bool IsTimeLimited { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("userCount")]
        public int UserCount { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("isExpired")]
        public bool IsExpired => IsTimeLimited && EndDate.HasValue && DateTime.UtcNow > EndDate.Value;

        [JsonPropertyName("isValid")]
        public bool IsValid => IsActive && (!IsTimeLimited || !IsExpired);
    }
}