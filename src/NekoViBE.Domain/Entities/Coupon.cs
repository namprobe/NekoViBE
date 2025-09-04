using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountTypeEnum DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int CurrentUsage { get; set; } = 0;
    
    // navigation properties
    public virtual ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
}
