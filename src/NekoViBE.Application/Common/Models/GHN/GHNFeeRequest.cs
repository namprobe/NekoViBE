using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

/// <summary>
/// GHN Fee Request Model
/// Reference: https://api.ghn.vn/home/docs/detail?id=87
/// </summary>
public class GHNFeeRequest
{
    [JsonPropertyName("from_district_id")]
    public int? FromDistrictId { get; set; }

    [JsonPropertyName("from_ward_code")]
    public string? FromWardCode { get; set; }

    [JsonPropertyName("to_district_id")]
    public int ToDistrictId { get; set; }

    [JsonPropertyName("to_ward_code")]
    public string ToWardCode { get; set; } = string.Empty;

    [JsonPropertyName("service_type_id")]
    public int ServiceTypeId { get; set; } = 2; // 2: Hàng nhẹ, 5: Hàng nặng

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("length")]
    public int? Length { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("insurance_value")]
    public int? InsuranceValue { get; set; }

    [JsonPropertyName("cod_value")]
    public int? CodValue { get; set; }

    [JsonPropertyName("coupon")]
    public string? Coupon { get; set; }

    [JsonPropertyName("cod_failed_amount")]
    public int? CodFailedAmount { get; set; }

    [JsonPropertyName("items")]
    public List<GHNFeeItem>? Items { get; set; }
}

/// <summary>
/// GHN Fee Item (required for service_type_id = 5 - Hàng nặng)
/// </summary>
public class GHNFeeItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }
}

