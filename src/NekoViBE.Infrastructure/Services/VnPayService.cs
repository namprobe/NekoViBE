using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;

namespace NekoViBE.Infrastructure.Services;

public class VnPayService : IPaymentGatewayService
{
    private readonly ILogger<VnPayService> _logger;
    private readonly IVnpayClient _vnpayClient;

    public VnPayService(ILogger<VnPayService> logger, IVnpayClient vnpayClient)
    {
        _logger = logger;
        _vnpayClient = vnpayClient;
    }
    public string BuildSignature(Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public Task<object?> CreatePaymentIntentAsync(Application.Common.Models.PaymentRequest request)
    {
        //1. build VnPay request
        var vnPayRequest = new VnpayPaymentRequest
        {
            Money = (double)request.Amount,
            BankCode = BankCode.ANY,
            Description = request.Description,
            Language = DisplayLanguage.Vietnamese,
        };

        //2. Build payment url
        var paymentUrlInfo = _vnpayClient.CreatePaymentUrl(vnPayRequest);
        var paymentUrl = paymentUrlInfo.Url;
        return Task.FromResult<object?>(paymentUrl);
    }

    public string GetProviderName()
    {
        return PaymentGatewayType.VnPay.ToString();
    }

    public Task<object?> QueryTransactionAsync(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<object?> RefundPaymentAsync(string transactionId, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool VerifyIpnRequest(object ipnRequest)
    {
        if (ipnRequest is not IQueryCollection queryCollection)
        {
            return false;
        }
        return _vnpayClient.GetPaymentResult(queryCollection);
    }
}