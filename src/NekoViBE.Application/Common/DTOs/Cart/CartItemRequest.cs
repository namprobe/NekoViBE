namespace NekoViBE.Application.Common.DTOs.Cart;

public class CartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    
}