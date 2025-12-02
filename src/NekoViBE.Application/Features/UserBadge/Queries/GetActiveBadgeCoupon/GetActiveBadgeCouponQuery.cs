using MediatR;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserBadge.Queries.GetActiveBadgeCoupon
{
    /// <summary>
    /// Query to get the coupon associated with the user's currently equipped badge
    /// Returns null if no badge is equipped or badge has no linked coupon
    /// </summary>
    public class GetActiveBadgeCouponQuery : IRequest<Result<CouponItem?>>
    {
    }
}
