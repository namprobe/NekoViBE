using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

/// <summary>
/// Entity quản lý coupon/voucher
/// </summary>
public class Coupon : BaseEntity
{
    /// <summary>
    /// Mã coupon (unique)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Loại discount: Percentage, Fixed, FreeShipping
    /// </summary>
    public DiscountTypeEnum DiscountType { get; set; }
    
    /// <summary>
    /// Giá trị discount
    /// - Percentage: 10 = 10%
    /// - Fixed: 50000 = 50,000đ
    /// - FreeShipping: 100 = 100%
    /// </summary>
    public decimal DiscountValue { get; set; }
    
    /// <summary>
    /// Giá trị tối đa được giảm (áp dụng cho Percentage)
    /// VD: Giảm 13% tối đa 80,000đ
    /// </summary>
    public decimal? MaxDiscountCap { get; set; }
    
    /// <summary>
    /// Giá trị đơn hàng tối thiểu để áp dụng coupon
    /// </summary>
    public decimal MinOrderAmount { get; set; } = 0;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Giới hạn số lần Collected (null = không giới hạn)
    /// </summary>
    public int? UsageLimit { get; set; }
    
    /// <summary>
    /// Số lần đã Collected
    /// </summary>
    public int CurrentUsage { get; set; } = 0;
    
    /// <summary>
    /// Indicates if this coupon was auto-generated for a badge
    /// Badge coupons cannot be collected manually - only applied when badge is equipped
    /// </summary>
    public bool IsBadgeCoupon { get; set; } = false;
    
    // === NAVIGATION PROPERTIES ===
    public virtual ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
    public virtual Badge? LinkedBadge { get; set; }
}
