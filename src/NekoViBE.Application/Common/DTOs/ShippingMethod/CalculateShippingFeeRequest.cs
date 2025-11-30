using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.ShippingMethod;

/// <summary>
/// Request DTO for calculating shipping fee
/// </summary>
public class CalculateShippingFeeRequest
{
    /// <summary>
    /// Shipping method ID
    /// </summary>
    [JsonPropertyName("shippingMethodId")]
    public Guid ShippingMethodId { get; set; }

    /// <summary>
    /// User address ID (for authenticated users)
    /// </summary>
    [JsonPropertyName("userAddressId")]
    public Guid UserAddressId { get; set; }

    /// <summary>
    /// Case 1: Buy now - Product ID (if provided, will calculate fee for this product)
    /// </summary>
    [JsonPropertyName("productId")]
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Case 1: Buy now - Quantity (required if ProductId is provided)
    /// </summary>
    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }

    /// <summary>
    /// Insurance value (order total amount)
    /// </summary>
    [JsonPropertyName("insuranceValue")]
    public int? InsuranceValue { get; set; }

    /// <summary>
    /// COD amount (if payment is COD)
    /// </summary>
    [JsonPropertyName("codValue")]
    public int? CodValue { get; set; }

    /// <summary>
    /// Coupon code (optional)
    /// </summary>
    [JsonPropertyName("coupon")]
    public string? Coupon { get; set; }
}

