using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class ShippingMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public int? EstimatedDays { get; set; }
    
    // navigation properties
    public virtual ICollection<OrderShippingMethod> OrderShippingMethods { get; set; } = new List<OrderShippingMethod>();
}
