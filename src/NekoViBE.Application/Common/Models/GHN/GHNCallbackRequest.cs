using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

/// <summary>
/// GHN Webhook Callback Request Model
/// Reference: https://api.ghn.vn/home/docs/detail?id=84
/// </summary>
public class GHNCallbackRequest
{
    [JsonPropertyName("CODAmount")]
    public decimal? CODAmount { get; set; }

    [JsonPropertyName("CODTransferDate")]
    public string? CODTransferDate { get; set; }

    [JsonPropertyName("ClientOrderCode")]
    public string? ClientOrderCode { get; set; }

    [JsonPropertyName("ConvertedWeight")]
    public int? ConvertedWeight { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Fee")]
    public GHNCallbackFee? Fee { get; set; }

    [JsonPropertyName("Height")]
    public int? Height { get; set; }

    [JsonPropertyName("IsPartialReturn")]
    public bool? IsPartialReturn { get; set; }

    [JsonPropertyName("Length")]
    public int? Length { get; set; }

    [JsonPropertyName("OrderCode")]
    public string OrderCode { get; set; } = string.Empty;

    [JsonPropertyName("PartialReturnCode")]
    public string? PartialReturnCode { get; set; }

    [JsonPropertyName("PaymentType")]
    public int? PaymentType { get; set; }

    [JsonPropertyName("Reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("ReasonCode")]
    public string? ReasonCode { get; set; }

    [JsonPropertyName("ShopID")]
    public int? ShopID { get; set; }

    [JsonPropertyName("Status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("Time")]
    public string? Time { get; set; }

    [JsonPropertyName("TotalFee")]
    public decimal? TotalFee { get; set; }

    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Warehouse")]
    public string? Warehouse { get; set; }

    [JsonPropertyName("Weight")]
    public int? Weight { get; set; }

    [JsonPropertyName("Width")]
    public int? Width { get; set; }
}

/// <summary>
/// GHN Callback Fee Structure
/// </summary>
public class GHNCallbackFee
{
    [JsonPropertyName("CODFailedFee")]
    public decimal? CODFailedFee { get; set; }

    [JsonPropertyName("CODFee")]
    public decimal? CODFee { get; set; }

    [JsonPropertyName("Coupon")]
    public decimal? Coupon { get; set; }

    [JsonPropertyName("DeliverRemoteAreasFee")]
    public decimal? DeliverRemoteAreasFee { get; set; }

    [JsonPropertyName("DocumentReturn")]
    public decimal? DocumentReturn { get; set; }

    [JsonPropertyName("DoubleCheck")]
    public decimal? DoubleCheck { get; set; }

    [JsonPropertyName("Insurance")]
    public decimal? Insurance { get; set; }

    [JsonPropertyName("MainService")]
    public decimal? MainService { get; set; }

    [JsonPropertyName("PickRemoteAreasFee")]
    public decimal? PickRemoteAreasFee { get; set; }

    [JsonPropertyName("R2S")]
    public decimal? R2S { get; set; }

    [JsonPropertyName("Return")]
    public decimal? Return { get; set; }

    [JsonPropertyName("StationDO")]
    public decimal? StationDO { get; set; }

    [JsonPropertyName("StationPU")]
    public decimal? StationPU { get; set; }

    [JsonPropertyName("Total")]
    public decimal? Total { get; set; }
}
