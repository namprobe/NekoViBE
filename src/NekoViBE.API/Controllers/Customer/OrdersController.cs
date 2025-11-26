using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Order.Commands.PlaceOrder;
using NekoViBE.Application.Features.Order.Queries.GetCustomerOrderDetail;
using NekoViBE.Application.Features.Order.Queries.GetCustomerOrderList;
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
        [ProducesResponseType(typeof(PaginationResult<CustomerOrderListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<CustomerOrderListItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<CustomerOrderListItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<CustomerOrderListItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all orders with pagination and filtering",
            Description = "This API retrieves a paginated list of orders with filtering options",
            OperationId = "GetOrderList",
            Tags = new[] { "Customer", "Customer_Order" }
        )]
        public async Task<IActionResult> GetOrderList([FromQuery] OrderFilter filter, CancellationToken cancellationToken)
        {
            var query = new GetCustomerOrderListQuery(filter);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve order list: {Error}", result.ErrorCode);
                return StatusCode(result.GetHttpStatusCode(), result);
            }

            return Ok(result);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(Result<CustomerOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<CustomerOrderDetailDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<CustomerOrderDetailDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<CustomerOrderDetailDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<CustomerOrderDetailDto>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get order detail",
            Description = "Returns order detail for the current customer including payment and items",
            OperationId = "GetCustomerOrderDetail",
            Tags = new[] { "Customer", "Customer_Order" }
        )]
        public async Task<IActionResult> GetOrderDetail(Guid orderId, CancellationToken cancellationToken)
        {
            if (orderId == Guid.Empty)
            {
                return BadRequest(Result<CustomerOrderDetailDto>.Failure("Invalid order id", ErrorCodeEnum.InvalidInput));
            }

            var query = new GetCustomerOrderDetailQuery(orderId);
            var result = await _mediator.Send(query, cancellationToken);
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    
    }
}

