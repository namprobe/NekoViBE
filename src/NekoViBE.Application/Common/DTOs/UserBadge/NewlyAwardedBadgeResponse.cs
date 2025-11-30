using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.UserBadge
{
    public class NewlyAwardedBadgeResponse
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
    }
}
