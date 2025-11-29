using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    //product price at the time of order
    public decimal UnitPrice { get; set; }
    //product discount amount at the time of order
    public decimal DiscountAmount { get; set; } = 0;
    
    // navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
