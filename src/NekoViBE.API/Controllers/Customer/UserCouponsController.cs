using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.UserCoupon;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.UserCoupon.Queries.GetUserCouponById;
using NekoViBE.Application.Features.UserCoupon.Queries.GetUserCoupons;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer;

/// <summary>
/// Controller quản lý coupon mà khách hàng đang sở hữu/được tặng
/// </summary>
[ApiController]
[Route("api/customer/user-coupons")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Customer", "Customer_UserCoupons")]
[SwaggerTag("API cho phép khách hàng xem danh sách coupon đã sưu tầm hoặc được tặng")]
public class UserCouponsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserCouponsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách coupon của người dùng hiện tại
    /// </summary>
    [HttpGet]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(PaginationResult<UserCouponItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<UserCouponItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<UserCouponItem>), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Lấy danh sách coupon của người dùng hiện tại",
        Description = "Danh sách coupon mà khách hàng đã collect hoặc được tặng",
        OperationId = "Customer_GetUserCoupons",
        Tags = new[] { "Customer", "Customer_UserCoupons" }
    )]
    public async Task<IActionResult> GetUserCoupons([FromQuery] UserCouponFilter filter)
    {
        filter.IsCurrentUser = true;
        var result = await _mediator.Send(new GetUserCouponsQuery(filter));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một coupon của người dùng
    /// </summary>
    [HttpGet("{id:guid}")]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result<UserCouponDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<UserCouponDetail>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<UserCouponDetail>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<UserCouponDetail>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Lấy thông tin chi tiết coupon",
        Description = "Dùng để hiển thị chi tiết coupon khách hàng đang sở hữu",
        OperationId = "Customer_GetUserCoupon",
        Tags = new[] { "Customer", "Customer_UserCoupons" }
    )]
    public async Task<IActionResult> GetUserCoupon(Guid id)
    {
        var result = await _mediator.Send(new GetUserCouponByIdQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

