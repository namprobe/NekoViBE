namespace NekoViBE.Application.Common.DTOs.Coupon;

public class UserCouponsResponse
{
    public List<UserCouponItem> Coupons { get; set; } = new();
}

public class UserCouponItem
{
    public Guid UserCouponId { get; set; }
    public Guid CouponId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CollectedDate { get; set; }
    public DateTime? UsedDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsExpired { get; set; }
}
