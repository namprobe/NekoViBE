using System;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Common.DTOs.UserCoupon;

public class UserCouponFilter : BasePaginationFilter
{
    public Guid? UserId { get; set; }
    public bool? IsCurrentUser { get; set; } = true;
    public bool? IsUsed { get; set; }
    public bool? IsExpired { get; set; }
    public Guid? CouponId { get; set; }
    public bool? OnlyActiveCoupons { get; set; }
}

