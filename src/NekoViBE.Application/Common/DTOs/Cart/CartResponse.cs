namespace NekoViBE.Application.Common.DTOs.Cart;
public class CartResponse : BaseResponse
{
    public Guid UserId { get; set; }
    public List<CartItemResponse> CartItems { get; set; } = new List<CartItemResponse>();
}
