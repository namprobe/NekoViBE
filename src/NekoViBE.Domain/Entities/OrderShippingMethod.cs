using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class OrderShippingMethod : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ShippingMethodId { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    
    // navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual ShippingMethod ShippingMethod { get; set; } = null!;
}
