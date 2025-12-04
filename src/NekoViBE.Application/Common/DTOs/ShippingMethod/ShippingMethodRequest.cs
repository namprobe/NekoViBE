using System.Text.Json.Serialization;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs;

public class ShippingMethodRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }
    
    [JsonPropertyName("estimatedDays")]
    public int? EstimatedDays { get; set; }
    
    [JsonPropertyName("status")]
    public EntityStatusEnum Status { get; set; }
}
