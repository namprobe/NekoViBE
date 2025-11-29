using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

public class GHNLeadTimeResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public GHNLeadTimeData? Data { get; set; }
}

public class GHNLeadTimeData
{
    [JsonPropertyName("leadtime")]
    public long? LeadTimeUnix { get; set; }

    [JsonIgnore]
    public DateTime? LeadTimeUtc => LeadTimeUnix.HasValue
        ? DateTimeOffset.FromUnixTimeSeconds(LeadTimeUnix.Value).UtcDateTime
        : null;

    [JsonPropertyName("order_date")]
    public long? OrderDateUnix { get; set; }

    [JsonPropertyName("leadtime_order")]
    public GHNLeadTimeOrderRange? LeadTimeOrder { get; set; }
}

public class GHNLeadTimeOrderRange
{
    [JsonPropertyName("from_estimate_date")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? FromEstimateDate { get; set; }

    [JsonPropertyName("to_estimate_date")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? ToEstimateDate { get; set; }
}

