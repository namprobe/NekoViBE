using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.Interfaces;

/// <summary>
/// Factory interface for shipping service providers
/// Similar to IPaymentGatewayFactory pattern
/// </summary>
public interface IShippingServiceFactory
{
    /// <summary>
    /// Get shipping service by provider type
    /// </summary>
    IShippingService GetShippingService(ShippingProviderType providerType);
}

