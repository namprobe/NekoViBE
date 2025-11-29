using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

public class GHNPreviewOrderResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public GHNPreviewOrderData? Data { get; set; }
}

public class GHNPreviewOrderData
{
    [JsonPropertyName("order_code")]
    public string OrderCode { get; set; } = string.Empty;

    [JsonPropertyName("fee")]
    public GHNOrderFee? Fee { get; set; }

    [JsonPropertyName("total_fee")]
    public int TotalFee { get; set; }

    [JsonPropertyName("expected_delivery_time")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? ExpectedDeliveryTime { get; set; }
}

