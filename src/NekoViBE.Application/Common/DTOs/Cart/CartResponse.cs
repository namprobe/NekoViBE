namespace NekoViBE.Application.Common.DTOs.Cart;
public class CartResponse : BaseResponse
{
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; } // Total number of items in the cart
    public List<CartItemResponse> CartItems { get; set; } = new List<CartItemResponse>(); // List of cart ites base on the pagination filter
}
