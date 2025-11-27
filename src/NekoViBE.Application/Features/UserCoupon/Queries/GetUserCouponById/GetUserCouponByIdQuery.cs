using System;
using MediatR;
using NekoViBE.Application.Common.DTOs.UserCoupon;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserCoupon.Queries.GetUserCouponById;

public record GetUserCouponByIdQuery(Guid Id) : IRequest<Result<UserCouponDetail>>;

