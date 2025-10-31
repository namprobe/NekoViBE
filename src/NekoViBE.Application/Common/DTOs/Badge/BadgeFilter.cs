using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.Badge
{
    public class BadgeFilter : BasePaginationFilter
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("conditionType")]
        public ConditionTypeEnum? ConditionType { get; set; }

        [JsonPropertyName("isTimeLimited")]
        public bool? IsTimeLimited { get; set; }

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

        [JsonPropertyName("hasUsers")]
        public bool? HasUsers { get; set; }
    }
}