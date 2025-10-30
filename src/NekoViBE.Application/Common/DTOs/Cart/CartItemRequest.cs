namespace NekoViBE.Application.Common.DTOs.Cart;

public class CartItemRequest
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    
}