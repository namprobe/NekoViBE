using MediatR;
using NekoViBE.Application.Common.DTOs.UserCoupon;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserCoupon.Queries.GetUserCoupons;

public record GetUserCouponsQuery(UserCouponFilter Filter) : IRequest<PaginationResult<UserCouponItem>>;

