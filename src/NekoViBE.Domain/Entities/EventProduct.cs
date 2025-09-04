using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class EventProduct : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid ProductId { get; set; }
    public bool IsFeatured { get; set; } = false;
    public decimal DiscountPercentage { get; set; } = 0;
    
    // navigation properties
    public virtual Event Event { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
