using NekoViBE.Domain.Enums;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.UserBadge
{
    public class UserBadgeWalletItem
    {
        [JsonPropertyName("userBadgeId")]
        public Guid UserBadgeId { get; set; }

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

        [JsonPropertyName("earnedDate")]
        public DateTime EarnedDate { get; set; }

        [JsonPropertyName("isEquipped")]
        public bool IsEquipped { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("isTimeLimited")]
        public bool IsTimeLimited { get; set; }

        [JsonPropertyName("activatedFrom")]
        public DateTime? ActivatedFrom { get; set; }

        [JsonPropertyName("activatedTo")]
        public DateTime? ActivatedTo { get; set; }

        [JsonPropertyName("isExpired")]
        public bool IsExpired => IsTimeLimited && ActivatedTo.HasValue && DateTime.UtcNow > ActivatedTo.Value;

        [JsonPropertyName("status")]
        public string Status => IsExpired ? "Expired" : IsEquipped ? "Equipped" : "Unlocked";

        [JsonPropertyName("benefit")]
        public string Benefit => DiscountPercentage > 0 ? $"{DiscountPercentage}% Discount" : "No Discount";

        [JsonPropertyName("conditionType")]
        public ConditionTypeEnum ConditionType { get; set; }

        [JsonPropertyName("conditionValue")]
        public string ConditionValue { get; set; } = string.Empty;
    }
}
