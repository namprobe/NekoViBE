using NekoViBE.Application.Common.DTOs.Product;

namespace NekoViBE.Application.Common.DTOs.Wishlist;

public class WishlistItemDto
{
    public Guid WishlistItemId { get; set; }
    public Guid ProductId { get; set; }
    public ProductItem Product { get; set; } = null!;
    public DateTime AddedAt { get; set; }
}
