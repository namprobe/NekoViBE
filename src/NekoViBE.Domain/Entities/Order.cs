using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

/// <summary>
/// Entity lưu thông tin đơn hàng
/// </summary>
public class Order : BaseEntity
{
    // === THÔNG TIN KHÁCH HÀNG ===
    public Guid? UserId { get; set; }
    public bool IsOneClick { get; set; } = false;
    public string? GuestEmail { get; set; }
    public string? GuestFirstName { get; set; }
    public string? GuestLastName { get; set; }
    public string? GuestPhone { get; set; }
    public string? OneClickAddress { get; set; }
    
    // === BREAKDOWN PHÍ (theo logic Shopee) ===
    
    /// <summary>
    /// Tổng giá gốc (Price × Quantity) - chưa bất kỳ discount nào
    /// Formula: Σ(UnitPriceOriginal × Quantity)
    /// </summary>
    public decimal SubtotalOriginal { get; set; }
    
    /// <summary>
    /// Tổng discount từ product (Price - DiscountPrice) × Quantity
    /// Formula: Σ((UnitPriceOriginal - UnitPriceAfterDiscount) × Quantity)
    /// </summary>
    public decimal ProductDiscountAmount { get; set; } = 0;
    
    /// <summary>
    /// Tổng tiền hàng SAU KHI TRỪ product discount
    /// Đây là BASE để tính coupon Fixed/Percentage
    /// Formula: SubtotalOriginal - ProductDiscountAmount
    /// </summary>
    public decimal SubtotalAfterProductDiscount { get; set; }
    
    /// <summary>
    /// Discount từ coupon Fixed/Percentage
    /// Tính trên SubtotalAfterProductDiscount (KHÔNG phải SubtotalOriginal)
    /// </summary>
    public decimal CouponDiscountAmount { get; set; } = 0;
    
    /// <summary>
    /// Tổng tiền hàng SAU TẤT CẢ discount
    /// Formula: SubtotalAfterProductDiscount - CouponDiscountAmount
    /// </summary>
    public decimal TotalProductAmount { get; set; }
    
    /// <summary>
    /// Phí ship gốc từ shipping provider
    /// </summary>
    public decimal ShippingFeeOriginal { get; set; } = 0;
    
    /// <summary>
    /// Giảm giá ship (từ FreeShip coupon)
    /// </summary>
    public decimal ShippingDiscountAmount { get; set; } = 0;
    
    /// <summary>
    /// Phí ship thực tế phải trả
    /// Formula: ShippingFeeOriginal - ShippingDiscountAmount
    /// </summary>
    public decimal ShippingFeeActual { get; set; } = 0;
    
    /// <summary>
    /// Thuế (nếu có)
    /// </summary>
    public decimal TaxAmount { get; set; } = 0;
    
    /// <summary>
    /// TỔNG THANH TOÁN CUỐI CÙNG
    /// Formula: TotalProductAmount + ShippingFeeActual + TaxAmount
    /// </summary>
    public decimal FinalAmount { get; set; }
    
    // === METADATA ===
    public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Pending;
    public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Processing;
    public string? Notes { get; set; }
    
    // === NAVIGATION PROPERTIES ===
    public virtual AppUser? User { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderShippingMethod> OrderShippingMethods { get; set; } = new List<OrderShippingMethod>();
    public virtual ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
    public virtual Payment? Payment { get; set; }
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

}
