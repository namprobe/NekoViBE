using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

/// <summary>
/// Entity lưu thông tin vận chuyển
/// </summary>
public class OrderShippingMethod : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ShippingMethodId { get; set; }
    
    /// <summary>
    /// Reference đến địa chỉ giao hàng (snapshot tại thời điểm đặt hàng)
    /// </summary>
    public Guid? UserAddressId { get; set; }
    
    /// <summary>
    /// Tên provider (GHN, GHTK, Viettel Post...)
    /// </summary>
    public string? ProviderName { get; set; }
    
    /// <summary>
    /// Mã tracking từ bên thứ 3
    /// </summary>
    public string? TrackingNumber { get; set; }
    
    /// <summary>
    /// Phí ship gốc từ provider
    /// </summary>
    public decimal ShippingFeeOriginal { get; set; }
    
    /// <summary>
    /// Discount ship (từ FreeShip coupon)
    /// </summary>
    public decimal ShippingDiscountAmount { get; set; } = 0;
    
    /// <summary>
    /// Phí ship thực tế phải trả
    /// Formula: ShippingFeeOriginal - ShippingDiscountAmount
    /// </summary>
    public decimal ShippingFeeActual { get; set; }
    
    /// <summary>
    /// Đánh dấu đơn hàng được freeship
    /// </summary>
    public bool IsFreeshipping { get; set; } = false;
    
    /// <summary>
    /// Ghi chú nguồn freeship (coupon, event, promotion...)
    /// </summary>
    public string? FreeshippingNote { get; set; }
    
    /// <summary>
    /// Phí COD (nếu thanh toán khi nhận hàng)
    /// </summary>
    public decimal CodFee { get; set; } = 0;
    
    /// <summary>
    /// Phí bảo hiểm (nếu có)
    /// </summary>
    public decimal InsuranceFee { get; set; } = 0;
    
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    
    // === NAVIGATION PROPERTIES ===
    public virtual Order Order { get; set; } = null!;
    public virtual ShippingMethod ShippingMethod { get; set; } = null!;
    public virtual UserAddress? UserAddress { get; set; }
}
