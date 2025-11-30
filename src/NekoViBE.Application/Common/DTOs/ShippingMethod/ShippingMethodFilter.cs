using System.Text.Json.Serialization;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Common.DTOs.ShippingMethod;

public class ShippingMethodFilter : BasePaginationFilter
{
    [JsonPropertyName("minCost")]
    public decimal? MinCost { get; set; }

    [JsonPropertyName("maxCost")]
    public decimal? MaxCost { get; set; }
    
    [JsonPropertyName("maxEstimatedDays")]
    public int? MaxEstimatedDays { get; set; }
}

