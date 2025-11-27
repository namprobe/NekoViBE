namespace NekoViBE.Application.Common.DTOs.Coupon;

public class AvailableCouponsResponse
{
    public List<AvailableCouponItem> Coupons { get; set; } = new();
}

public class AvailableCouponItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int CurrentUsage { get; set; }
    public int RemainingSlots { get; set; }
    public bool IsCollected { get; set; } // Có phải user đã collect chưa
}
