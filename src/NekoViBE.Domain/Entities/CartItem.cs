using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    
    // navigation properties
    public virtual ShoppingCart Cart { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
