using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

public class GHNCreateOrderResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public GHNCreateOrderData? Data { get; set; }

    [JsonPropertyName("message_display")]
    public string? MessageDisplay { get; set; }
}

public class GHNCreateOrderData
{
    [JsonPropertyName("order_code")]
    public string OrderCode { get; set; } = string.Empty;

    [JsonPropertyName("sort_code")]
    public string SortCode { get; set; } = string.Empty;

    [JsonPropertyName("trans_type")]
    public string TransType { get; set; } = string.Empty;

    [JsonPropertyName("ward_encode")]
    public string? WardEncode { get; set; }

    [JsonPropertyName("district_encode")]
    public string? DistrictEncode { get; set; }

    [JsonPropertyName("fee")]
    public GHNOrderFee? Fee { get; set; }

    [JsonPropertyName("total_fee")]
    public int TotalFee { get; set; }

    [JsonPropertyName("expected_delivery_time")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime? ExpectedDeliveryTime { get; set; }
}

public class GHNOrderFee
{
    [JsonPropertyName("main_service")]
    public int MainService { get; set; }

    [JsonPropertyName("insurance")]
    public int Insurance { get; set; }

    [JsonPropertyName("station_do")]
    public int StationDo { get; set; }

    [JsonPropertyName("station_pu")]
    public int StationPu { get; set; }

    [JsonPropertyName("return")]
    public int Return { get; set; }

    [JsonPropertyName("r2s")]
    public int R2S { get; set; }

    [JsonPropertyName("coupon")]
    public int Coupon { get; set; }

    [JsonPropertyName("cod_failed_fee")]
    public int CodFailedFee { get; set; }
}

