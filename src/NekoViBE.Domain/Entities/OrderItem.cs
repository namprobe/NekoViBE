using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

/// <summary>
/// Entity lưu chi tiết từng sản phẩm trong đơn hàng
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    
    /// <summary>
    /// Giá gốc tại thời điểm đặt hàng (Product.Price)
    /// </summary>
    public decimal UnitPriceOriginal { get; set; }
    
    /// <summary>
    /// Giá sau product discount tại thời điểm đặt hàng
    /// = Product.DiscountPrice nếu có, ngược lại = Product.Price
    /// </summary>
    public decimal UnitPriceAfterDiscount { get; set; }
    
    /// <summary>
    /// Discount amount cho 1 unit
    /// Formula: UnitPriceOriginal - UnitPriceAfterDiscount
    /// </summary>
    public decimal UnitDiscountAmount { get; set; } = 0;
    
    /// <summary>
    /// Tổng tiền cho item này (sau product discount)
    /// Formula: UnitPriceAfterDiscount × Quantity
    /// </summary>
    public decimal LineTotal { get; set; }
    
    // === NAVIGATION PROPERTIES ===
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
