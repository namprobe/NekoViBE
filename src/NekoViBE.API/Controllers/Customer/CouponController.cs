using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Features.Coupon.Commands.CollectCoupon;
using NekoViBE.Application.Features.Coupon.Queries.GetAvailableCoupons;
using NekoViBE.Application.Features.Coupon.Queries.GetUserCoupons;

namespace NekoViBE.API.Controllers.Customer;

[Authorize]
[Route("api/customer/coupons")]
[ApiController]
public class CouponController : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách coupon khả dụng trong hệ thống (chưa hết hạn, còn slot)
    /// </summary>
    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableCoupons(CancellationToken cancellationToken)
    {
        var query = new GetAvailableCouponsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách coupon mà user đã collect
    /// </summary>
    [HttpGet("my-coupons")]
    public async Task<IActionResult> GetUserCoupons(CancellationToken cancellationToken)
    {
        var query = new GetUserCouponsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Collect coupon (thêm vào UserCoupon)
    /// </summary>
    [HttpPost("collect")]
    public async Task<IActionResult> CollectCoupon(
        [FromBody] CollectCouponRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CollectCouponCommand
        {
            CouponId = request.CouponId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
