using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    
    // navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
