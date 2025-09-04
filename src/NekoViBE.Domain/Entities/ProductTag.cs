using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class ProductTag : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid TagId { get; set; }
    
    // navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
