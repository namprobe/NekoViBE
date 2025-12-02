using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

public class GHNAvailableServiceResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<GHNAvailableServiceData>? Data { get; set; }
}

public class GHNAvailableServiceData
{
    [JsonPropertyName("service_id")]
    public int ServiceId { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; } = string.Empty;

    [JsonPropertyName("service_type_id")]
    public int ServiceTypeId { get; set; }
}

