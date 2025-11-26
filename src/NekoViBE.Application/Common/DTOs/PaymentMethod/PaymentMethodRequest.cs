using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs;

public class PaymentMethodRequest
{
    [JsonPropertyName("name")]
    public PaymentGatewayType Name { get; set; } = PaymentGatewayType.VnPay;
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("iconImage")]
    public IFormFile? IconImage { get; set; }
    [JsonPropertyName("isOnlinePayment")]
    public bool IsOnlinePayment { get; set; }
    [JsonPropertyName("processingFee")]
    public decimal ProcessingFee { get; set; } 
    [JsonPropertyName("processorName")]
    public string? ProcessorName { get; set; } // VnPay, PayPal, Stripe, etc.
    [JsonPropertyName("configuration")]
    public string? Configuration { get; set; } // JSON config for payment processor
    [JsonPropertyName("status")]
    public EntityStatusEnum Status { get; set; }
}