using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs;

public class ShippingMethodItem : BaseResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }
    
    [JsonPropertyName("estimatedDays")]
    public int? EstimatedDays { get; set; }
}

