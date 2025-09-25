using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Order.Commands.CreateOrder;
using NekoViBE.Application.Features.Order.Queries.GetOrderById;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/cms/orders")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_Order")]
    [SwaggerTag("This API is used for order management in CMS")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create a new order",
            Description = "This API creates a new order. It requires Admin role access",
            OperationId = "CreateOrder",
            Tags = new[] { "CMS", "CMS_Order" }
        )]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var command = new CreateOrderCommand(request);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
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
