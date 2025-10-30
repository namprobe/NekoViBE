namespace NekoViBE.Application.Common.DTOs.Cart;

public class CartItemResponse : BaseResponse
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public bool IsPreOrder { get; set; } = false;
    public DateTime? PreOrderReleaseDate { get; set; }
}