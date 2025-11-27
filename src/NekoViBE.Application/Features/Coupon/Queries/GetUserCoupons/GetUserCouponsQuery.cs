using MediatR;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Coupon.Queries.GetUserCoupons;

public class GetUserCouponsQuery : IRequest<Result<UserCouponsResponse>>
{
}
