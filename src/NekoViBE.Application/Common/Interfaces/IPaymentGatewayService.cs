using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Common.Interfaces;

/// <summary>
/// Interface for payment gateway integration
/// Abstracts payment provider implementation (VnPay, PayPal, Momo, etc.)
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Create a payment intent with payment provider
    /// </summary>
    Task<object?> CreatePaymentIntentAsync(
        PaymentRequest request);

    /// <summary>
    /// Verify IPN request from provider
    /// </summary>
    public bool VerifyIpnRequest(object ipnRequest);

    /// <summary>
    /// Refund a payment
    /// </summary>
    Task<object?> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment provider name
    /// </summary>
    string GetProviderName();

    /// <summary>
    /// Build signature for request
    /// </summary>
    string BuildSignature(Dictionary<string, string> parameters);

    /// <summary>
    /// Query transaction status from provider
    /// </summary>
    Task<object?> QueryTransactionAsync(object request, CancellationToken cancellationToken = default);
}

