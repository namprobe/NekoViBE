using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

public class GHNAvailableServiceRequest
{
    [JsonPropertyName("shop_id")]
    public int ShopId { get; set; }

    [JsonPropertyName("from_district")]
    public int FromDistrict { get; set; }

    [JsonPropertyName("to_district")]
    public int ToDistrict { get; set; }
}

