using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs;

public class PaymentMethodItem : BaseResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("iconPath")]
    public string? IconPath { get; set; }
    [JsonPropertyName("isOnlinePayment")]
    public bool IsOnlinePayment { get; set; }
    [JsonPropertyName("processingFee")]
    public decimal ProcessingFee { get; set; }
}