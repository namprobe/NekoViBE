using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class Badge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconPath { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public ConditionTypeEnum ConditionType { get; set; }
    public string ConditionValue { get; set; } = string.Empty;
    public bool IsTimeLimited { get; set; } = false;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Link to auto-generated coupon for this badge
    /// This allows the badge discount to be applied through the existing coupon system
    /// </summary>
    public Guid? LinkedCouponId { get; set; }
    
    // navigation properties
    public virtual Coupon? LinkedCoupon { get; set; }
    public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
