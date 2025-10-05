using MediatR;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Coupon.Commands.CreateCoupon
{
    public record CreateCouponCommand(CreateCouponRequest Request) : IRequest<Result<CouponDto>>;

}
