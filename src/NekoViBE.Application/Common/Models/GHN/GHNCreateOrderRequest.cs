using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

public class GHNCreateOrderRequest
{
    [JsonPropertyName("payment_type_id")]
    public int PaymentTypeId { get; set; } = 2;

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("required_note")]
    public string RequiredNote { get; set; } = "KHONGCHOXEMHANG";

    [JsonPropertyName("return_phone")]
    public string? ReturnPhone { get; set; }

    [JsonPropertyName("return_address")]
    public string? ReturnAddress { get; set; }

    [JsonPropertyName("return_district_id")]
    public int? ReturnDistrictId { get; set; }

    [JsonPropertyName("return_ward_code")]
    public string? ReturnWardCode { get; set; }

    [JsonPropertyName("client_order_code")]
    public string? ClientOrderCode { get; set; }

    [JsonPropertyName("from_name")]
    public string? FromName { get; set; }

    [JsonPropertyName("from_phone")]
    public string? FromPhone { get; set; }

    [JsonPropertyName("from_address")]
    public string? FromAddress { get; set; }

    [JsonPropertyName("from_ward_name")]
    public string? FromWardName { get; set; }

    [JsonPropertyName("from_district_name")]
    public string? FromDistrictName { get; set; }

    [JsonPropertyName("from_province_name")]
    public string? FromProvinceName { get; set; }

    [JsonPropertyName("to_name")]
    public string ToName { get; set; } = string.Empty;

    [JsonPropertyName("to_phone")]
    public string ToPhone { get; set; } = string.Empty;

    [JsonPropertyName("to_address")]
    public string ToAddress { get; set; } = string.Empty;

    [JsonPropertyName("to_ward_name")]
    public string ToWardName { get; set; } = string.Empty;

    [JsonPropertyName("to_district_name")]
    public string ToDistrictName { get; set; } = string.Empty;

    [JsonPropertyName("to_province_name")]
    public string ToProvinceName { get; set; } = string.Empty;

    [JsonPropertyName("cod_amount")]
    public int? CodAmount { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("cod_failed_amount")]
    public int? CodFailedAmount { get; set; }

    [JsonPropertyName("pick_station_id")]
    public int? PickStationId { get; set; }

    [JsonPropertyName("deliver_station_id")]
    public int? DeliverStationId { get; set; }

    [JsonPropertyName("insurance_value")]
    public int? InsuranceValue { get; set; }

    [JsonPropertyName("service_type_id")]
    public int ServiceTypeId { get; set; } = 2;

    [JsonPropertyName("coupon")]
    public string? Coupon { get; set; }

    [JsonPropertyName("pickup_time")]
    public long? PickupTime { get; set; }

    [JsonPropertyName("pick_shift")]
    public List<int>? PickShift { get; set; }

    [JsonPropertyName("items")]
    public List<GHNOrderItem>? Items { get; set; }
}

public class GHNOrderItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("category")]
    public GHNOrderItemCategory? Category { get; set; }
}

public class GHNOrderItemCategory
{
    [JsonPropertyName("level1")]
    public string? Level1 { get; set; }

    [JsonPropertyName("level2")]
    public string? Level2 { get; set; }

    [JsonPropertyName("level3")]
    public string? Level3 { get; set; }
}

