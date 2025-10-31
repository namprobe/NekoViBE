using MediatR;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Coupon.Commands.UpdateCoupon
{
    public record UpdateCouponCommand(Guid Id, UpdateCouponRequest Request) : IRequest<Result<CouponDto>>;

}
