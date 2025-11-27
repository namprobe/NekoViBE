using MediatR;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Coupon.Queries.GetAvailableCoupons;

public class GetAvailableCouponsQuery : IRequest<Result<AvailableCouponsResponse>>
{
}
