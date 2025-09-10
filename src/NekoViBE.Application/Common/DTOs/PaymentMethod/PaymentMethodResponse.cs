using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs;

public class PaymentMethodResponse : PaymentMethodItem
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("processorName")]
    public string? ProcessorName { get; set; }
    [JsonPropertyName("configuration")]
    public string? Configuration { get; set; }
}