using MediatR;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Models;


namespace NekoViBE.Application.Features.Coupon.Queries.GetCoupons
{
    public record GetCouponsQuery : IRequest<Result<CouponsResponse>>;

}
