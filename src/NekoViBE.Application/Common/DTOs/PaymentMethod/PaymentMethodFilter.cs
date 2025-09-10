using System.Text.Json.Serialization;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Common.DTOs.PaymentMethod;

public class PaymentMethodFilter : BasePaginationFilter
{
    [JsonPropertyName("isOnlinePayment")]
    public bool? IsOnlinePayment { get; set; }

    [JsonPropertyName("minProcessingFee")]
    public decimal? MinProcessingFee { get; set; }

    [JsonPropertyName("maxProcessingFee")]
    public decimal? MaxProcessingFee { get; set; }
}
