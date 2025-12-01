using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Common.Interfaces;

/// <summary>
/// Interface for shipping service integration
/// Abstracts shipping provider implementation (GHN, GHTK, etc.)
/// </summary>
public interface IShippingService
{
    /// <summary>
    /// Calculate shipping fee for an order
    /// </summary>
    Task<ShippingFeeResult> CalculateFeeAsync(ShippingFeeRequest request);

    /// <summary>
    /// Preview order information before creating
    /// </summary>
    Task<ShippingPreviewResult> PreviewOrderAsync(ShippingOrderRequest request);

    /// <summary>
    /// Get expected delivery time (lead time)
    /// </summary>
    Task<ShippingLeadTimeResult> GetLeadTimeAsync(ShippingLeadTimeRequest request);

    /// <summary>
    /// Create shipping order
    /// </summary>
    Task<ShippingOrderResult> CreateOrderAsync(ShippingOrderRequest request);

    /// <summary>
    /// Handle callback from shipping provider
    /// </summary>
    ShippingCallbackResult HandleCallback(object callbackRequest);

    /// <summary>
    /// Cancel shipping order
    /// </summary>
    Task<ShippingOrderResult> CancelOrderAsync(string orderCode);

    /// <summary>
    /// Get provider name
    /// </summary>
    string GetProviderName();
}

