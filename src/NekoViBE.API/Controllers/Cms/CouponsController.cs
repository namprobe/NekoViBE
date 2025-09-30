using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Coupon.Commands.CreateCoupon;
using NekoViBE.Application.Features.Coupon.Commands.DeleteCoupon;
using NekoViBE.Application.Features.Coupon.Commands.UpdateCoupon;
using NekoViBE.Application.Features.Coupon.Queries.GetCouponByCode;
using NekoViBE.Application.Features.Coupon.Queries.GetCouponById;
using NekoViBE.Application.Features.Coupon.Queries.GetCoupons;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/customer/coupons")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_Order")]
    [SwaggerTag("This API is used for order for Customer website")]
    public class CouponsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CouponsController> _logger;

        public CouponsController(IMediator mediator, ILogger<CouponsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<CouponItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<CouponItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<CouponItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<CouponItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
        Summary = "Get all coupons with pagination and filtering",
        Description = "This API retrieves a paginated list of coupons with filtering options",
        OperationId = "GetCouponList",
        Tags = new[] { "CMS", "CMS_Coupon" }
    )]
        public async Task<IActionResult> GetCouponList([FromQuery] CouponFilter filter, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for GetCouponList: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(Result.Failure("Invalid request parameters", ErrorCodeEnum.InvalidInput));
            }

            _logger.LogInformation("Retrieving coupon list with filter: {@Filter}", filter);
            var query = new GetCouponListQuery(filter);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve coupon list: {Error}", result.Errors);
                return StatusCode(result.GetHttpStatusCode(), result);
            }

            _logger.LogInformation("Coupon list retrieved successfully, TotalItems: {TotalItems}", result.TotalItems);
            return Ok(result);
        }


        //[HttpGet]
        //[AuthorizeRoles("Admin", "Staff")]
        //[ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        //[SwaggerOperation(
        //            Summary = "Get all coupons",
        //            Description = "This API returns all coupons. It requires Admin role access",
        //            OperationId = "GetAllCoupons",
        //            Tags = new[] { "CMS", "CMS_Coupon" }
        //        )]
        //public async Task<IActionResult> GetAllCoupons()
        //{
        //    var query = new GetCouponsQuery();
        //    var result = await _mediator.Send(query);
        //    return StatusCode(result.GetHttpStatusCode(), result);
        //}

        [HttpGet("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
                    Summary = "Get coupon by Id",
                    Description = "This API returns a coupon. It requires Admin role access",
                    OperationId = "GetCouponById",
                    Tags = new[] { "CMS", "CMS_Coupon" }
                )]
        public async Task<IActionResult> GetCouponById(Guid id)
        {
            var query = new GetCouponByIdQuery(id);
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("code/{code}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
                    Summary = "Get coupon by code",
                    Description = "This API returns a coupon. It requires Admin role access",
                    OperationId = "GetCouponByCode",
                    Tags = new[] { "CMS", "CMS_Coupon" }
                )]
        public async Task<IActionResult> GetCouponByCode(string code)
        {
            var query = new GetCouponByCodeQuery(code);
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPost]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create a new coupon",
            Description = "This API creates a new coupon. It requires Admin role access",
            OperationId = "CreateCoupon",
            Tags = new[] { "CMS", "CMS_Coupon" }
        )]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
        {
            var command = new CreateCouponCommand(request);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPut]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Update a new coupon",
            Description = "This API updates a new coupon. It requires Admin role access",
            OperationId = "UpdateCoupon",
            Tags = new[] { "CMS", "CMS_Coupon" }
        )]
        public async Task<IActionResult> UpdateCoupon([FromBody] UpdateCouponRequest request)
        {
            var command = new UpdateCouponCommand(request);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }


        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Dalete a coupon",
            Description = "This API delete a coupon. It requires Admin role access",
            OperationId = "DeleteCoupon",
            Tags = new[] { "CMS", "CMS_Coupon" }
        )]
        public async Task<IActionResult> DeleteCoupon(Guid id)
        {
            var command = new DeleteCouponCommand(id);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
