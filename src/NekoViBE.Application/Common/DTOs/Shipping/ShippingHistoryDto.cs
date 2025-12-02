namespace NekoViBE.Application.Common.DTOs.Shipping;

public class ShippingHistoryDto
{
    public Guid Id { get; set; }
    public Guid OrderShippingMethodId { get; set; }
    public Guid OrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public int StatusCode { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public string? EventType { get; set; }
    public DateTime EventTime { get; set; }
    public string? AdditionalData { get; set; }
    public string? CallerIpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }
}

