using Microsoft.Extensions.DependencyInjection;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Enums;
using NekoViBE.Infrastructure.Services;

namespace PaymentService.Infrastructure.Factories;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IPaymentGatewayService GetPaymentGatewayService(PaymentGatewayType gatewayName)
    {
        return gatewayName switch
        {
            PaymentGatewayType.VnPay => _serviceProvider.GetRequiredService<VnPayService>(),
            PaymentGatewayType.Momo => _serviceProvider.GetRequiredService<MomoService>(),
            // FUTURE: Add other payment gateways here
            // PaymentGatewayType.PayPal => new PayPalService(),
            _ => throw new ArgumentException("Invalid payment gateway type"),
        };
    }
}
