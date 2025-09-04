using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class UserCoupon : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid CouponId { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime? UsedDate { get; set; }
    
    // navigation properties
    public virtual AppUser? User { get; set; }
    public virtual Coupon Coupon { get; set; } = null!;
    public virtual Order? Order { get; set; }
}
