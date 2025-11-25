using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Order.Commands.CreateOrder;
using NekoViBE.Application.Features.Order.Commands.PlaceOrder;
using NekoViBE.Application.Features.Order.Queries.GetOrderById;
using NekoViBE.Application.Features.Order.Queries.GetOrderList;
using NekoViBE.Application.Features.OrderItem.Query.GetOrderItemsByOrderId;
using NekoViBE.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/orders")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_Order")]
    [SwaggerTag("This API is used for order management in Customer")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [AuthorizeRoles("Customer")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Place a new order",
            Description = "This API places a new order. It requires Customer role access",
            OperationId = "PlaceOrder",
            Tags = new[] { "Customer", "Customer_Order" }
        )]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            var command = new PlaceOrderCommand(request);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<OrderListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<OrderListItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<OrderListItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<OrderListItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all orders with pagination and filtering",
            Description = "This API retrieves a paginated list of orders with filtering options",
            OperationId = "GetOrderList",
            Tags = new[] { "Customer", "Customer_Order" }
        )]
        public async Task<IActionResult> GetOrderList([FromQuery] OrderFilter filter, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for GetOrderList: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(Result.Failure("Invalid request parameters", ErrorCodeEnum.InvalidInput));
            }

            _logger.LogInformation("Retrieving order list with filter: {@Filter}", filter);
            var query = new GetOrderListQuery(filter);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve order list: {Error}", result.ErrorCode);
                return StatusCode(result.GetHttpStatusCode(), result);
            }

            _logger.LogInformation("Order list retrieved successfully, TotalItems: {TotalItems}", result.TotalItems);
            return Ok(result);
        }
        


     [HttpGet("{orderId:guid}/items")]
        [ProducesResponseType(typeof(Result<List<OrderItemDetailDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<OrderItemDetailDTO>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<List<OrderItemDetailDTO>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<List<OrderItemDetailDTO>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<List<OrderItemDetailDTO>>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
        Summary = "Get all order items for a specific order",
        Description = "Retrieves all items belonging to a specific order with product details",
        OperationId = "GetOrderItemsByOrderId",
        Tags = new[] { "Customer", "Customer_Order" }
    )]
        public async Task<IActionResult> GetOrderItemsByOrderId(Guid orderId, CancellationToken cancellationToken)
        {
            if (orderId == Guid.Empty)
            {
                return BadRequest(Result<List<OrderItemDetailDTO>>.Failure("Invalid order ID", ErrorCodeEnum.InvalidInput));
            }

            _logger.LogInformation("Retrieving order items for order {OrderId}", orderId);

            var query = new GetOrderItemsByOrderIdQuery(orderId);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve order items for order {OrderId}: {Error}", orderId, result.ErrorCode);
                return StatusCode(result.GetHttpStatusCode(), result);
            }

            _logger.LogInformation("Successfully retrieved {Count} order items for order {OrderId}",
                result.Data?.Count, orderId);

            return Ok(result);
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetOrderById(Guid id)
        //{
        //    var query = new GetOrderByIdQuery(id);
        //    var result = await _mediator.Send(query);
        //    return StatusCode(result.GetHttpStatusCode(), result);
        //}

        //[HttpGet("my-orders")]
        //public async Task<IActionResult> GetUserOrders()
        //{
        //    var query = new GetUserOrdersQuery();
        //    var result = await _mediator.Send(query);
        //    return StatusCode(result.GetHttpStatusCode(), result);
        //}
    }
}

