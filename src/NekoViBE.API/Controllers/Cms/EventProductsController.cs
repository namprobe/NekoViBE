using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.EventProduct.Commands.CreateEventProduct;
using NekoViBE.Application.Features.EventProduct.Commands.DeleteEventProduct;
using NekoViBE.Application.Features.EventProduct.Commands.UpdateEventProduct;
using NekoViBE.Application.Features.EventProduct.Queries.GetEventProduct;
using NekoViBE.Application.Features.EventProduct.Queries.GetEventProductList;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/event-products")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_EventProducts")]
    [SwaggerTag("This API is used for EventProduct management in CMS")]
    public class EventProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(PaginationResult<EventProductItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<EventProductItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<EventProductItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<EventProductItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all event products with pagination and filtering",
            Description = "This API retrieves a paginated list of event products with filtering options",
            OperationId = "GetEventProductList",
            Tags = new[] { "CMS", "CMS_EventProducts" }
        )]
        public async Task<IActionResult> GetEventProductList([FromQuery] EventProductFilter filter)
        {
            var query = new GetEventProductListQuery(filter);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result<EventProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<EventProductResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<EventProductResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<EventProductResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<EventProductResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get event product by ID",
            Description = "This API retrieves a specific event product by its ID",
            OperationId = "GetEventProduct",
            Tags = new[] { "CMS", "CMS_EventProducts" }
        )]
        public async Task<IActionResult> GetEventProduct(Guid id)
        {
            var query = new GetEventProductQuery(id);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPost]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create a new event product",
            Description = "This API creates a new event product using form data. It requires Admin or Staff role access",
            OperationId = "CreateEventProduct",
            Tags = new[] { "CMS", "CMS_EventProducts" }
        )]
        public async Task<IActionResult> CreateEventProduct([FromForm] EventProductRequest request)
        {
            var command = new CreateEventProductCommand(request);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }


        [HttpPut("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Update an existing event product",
            Description = "This API updates an existing event product using form data. It requires Admin or Staff role access",
            OperationId = "UpdateEventProduct",
            Tags = new[] { "CMS", "CMS_EventProducts" }
        )]
        public async Task<IActionResult> UpdateEventProduct(Guid id, [FromForm] EventProductRequest request)
        {
            var command = new UpdateEventProductCommand(id, request);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        

        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete an event product",
            Description = "This API deletes an event product. It requires Admin or Staff role access",
            OperationId = "DeleteEventProduct",
            Tags = new[] { "CMS", "CMS_EventProducts" }
        )]
        public async Task<IActionResult> DeleteEventProduct(Guid id)
        {
            var command = new DeleteEventProductCommand(id);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
