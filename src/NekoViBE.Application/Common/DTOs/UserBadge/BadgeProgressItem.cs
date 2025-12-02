using NekoViBE.Domain.Enums;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.UserBadge
{
    public class BadgeProgressItem
    {
        [JsonPropertyName("badgeId")]
        public Guid BadgeId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }

        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Locked";

        [JsonPropertyName("progress")]
        public string? Progress { get; set; }

        [JsonPropertyName("currentValue")]
        public decimal CurrentValue { get; set; }

        [JsonPropertyName("targetValue")]
        public decimal TargetValue { get; set; }

        [JsonPropertyName("conditionType")]
        public ConditionTypeEnum ConditionType { get; set; }

        [JsonPropertyName("conditionValue")]
        public string ConditionValue { get; set; } = string.Empty;

        [JsonPropertyName("isUnlocked")]
        public bool IsUnlocked { get; set; }
    }
}
