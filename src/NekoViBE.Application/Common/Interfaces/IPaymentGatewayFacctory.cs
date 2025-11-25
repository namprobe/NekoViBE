using NekoViBE.Application.Common.Enums;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.Interfaces;

public interface IPaymentGatewayFactory
{
    IPaymentGatewayService GetPaymentGatewayService(PaymentGatewayType gatewayName);
}
