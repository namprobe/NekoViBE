namespace NekoViBE.Application.Common.DTOs.Wishlist;

public class GetWishlistResponse
{
    public Guid WishlistId { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
}
