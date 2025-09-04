using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; set; }
    public Guid ProductId { get; set; }
    
    // navigation properties
    public virtual Wishlist Wishlist { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
