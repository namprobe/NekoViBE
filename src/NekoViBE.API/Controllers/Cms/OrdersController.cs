using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Order.Queries.GetOrderById;
using NekoViBE.Application.Features.Order.Queries.GetOrderList;
using NekoViBE.Application.Features.Shipping.Queries.GetShippingHistory;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.API.Controllers.Cms;

[ApiController]
[Route("api/cms/orders")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_Order")]
[SwaggerTag("This API is used for Order management in CMS")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(PaginationResult<OrderListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<OrderListItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<OrderListItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<OrderListItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all orders with pagination and filtering",
        Description = "This API retrieves a paginated list of orders with filtering options for CMS. Supports filtering by user, status, amount range, date range, and more.",
        OperationId = "GetOrderList",
        Tags = new[] { "CMS", "CMS_Order" }
    )]
    public async Task<IActionResult> GetOrderList([FromQuery] OrderFilter filter, CancellationToken cancellationToken)
    {
        var query = new GetOrderListQuery(filter);
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpGet("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<OrderDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<OrderDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<OrderDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<OrderDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get order by ID",
        Description = "This API retrieves detailed order information by its ID for CMS. Includes order items, shipping, payment, and applied coupons.",
        OperationId = "GetOrderById",
        Tags = new[] { "CMS", "CMS_Order" }
    )]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpGet("{orderId}/shipping-history")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result<List<NekoViBE.Application.Common.DTOs.Shipping.ShippingHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<List<NekoViBE.Application.Common.DTOs.Shipping.ShippingHistoryDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<List<NekoViBE.Application.Common.DTOs.Shipping.ShippingHistoryDto>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<List<NekoViBE.Application.Common.DTOs.Shipping.ShippingHistoryDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<List<NekoViBE.Application.Common.DTOs.Shipping.ShippingHistoryDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get shipping history for an order",
        Description = "This API retrieves shipping history/tracking events for the specified order in CMS.",
        OperationId = "GetShippingHistoryCms",
        Tags = new[] { "CMS", "CMS_Order" }
    )]
    public async Task<IActionResult> GetShippingHistory(Guid orderId, CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest(Result<List<NekoViBE.Application.Common.DTOs.Shipping.ShippingHistoryDto>>.Failure(
                "Invalid order id", 
                ErrorCodeEnum.InvalidInput));
        }

        var query = new GetShippingHistoryQuery(orderId);
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

