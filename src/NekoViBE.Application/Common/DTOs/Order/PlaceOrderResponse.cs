using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

public class PlaceOrderResponse
{
    public Guid OrderId { get; set; }
    public PaymentGatewayType? PaymentGateway { get; set; }
    public string? PaymentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal FinalAmount { get; set; }
}

