using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class ShoppingCart : BaseEntity
{
    public Guid UserId { get; set; }
    
    // navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
