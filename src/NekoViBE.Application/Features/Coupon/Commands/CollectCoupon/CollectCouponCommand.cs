using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Coupon.Commands.CollectCoupon;

public class CollectCouponCommand : IRequest<Result>
{
    public Guid CouponId { get; set; }
}
