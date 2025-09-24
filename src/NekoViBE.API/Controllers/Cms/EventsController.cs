using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Event.Commands.CreateEvent;
using NekoViBE.Application.Features.Event.Commands.DeleteEvent;
using NekoViBE.Application.Features.Event.Commands.UpdateEvent;
using NekoViBE.Application.Features.Event.Queries.GetEvent;
using NekoViBE.Application.Features.Event.Queries.GetEventList;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;


namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/events")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_Events")]
    [SwaggerTag("This API is used for Event management in CMS")]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(PaginationResult<EventItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<EventItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<EventItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<EventItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all events with pagination and filtering",
            Description = "This API retrieves a paginated list of events with filtering options",
            OperationId = "GetEventList",
            Tags = new[] { "CMS", "CMS_Events" }
        )]
        public async Task<IActionResult> GetEventList([FromQuery] EventFilter filter)
        {
            var query = new GetEventListQuery(filter);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get event by ID",
            Description = "This API retrieves a specific event by its ID",
            OperationId = "GetEvent",
            Tags = new[] { "CMS", "CMS_Events" }
        )]
        public async Task<IActionResult> GetEvent(Guid id)
        {
            var query = new GetEventQuery(id);
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
            Summary = "Create a new event",
            Description = "This API creates a new event. It requires Admin or Staff role access",
            OperationId = "CreateEvent",
            Tags = new[] { "CMS", "CMS_Events" }
        )]
        public async Task<IActionResult> CreateEvent([FromForm] EventRequest request)
        {
            var command = new CreateEventCommand(request);
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
            Summary = "Update an existing event",
            Description = "This API updates an existing event. It requires Admin or Staff role access",
            OperationId = "UpdateEvent",
            Tags = new[] { "CMS", "CMS_Events" }
        )]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromForm] EventRequest request)
        {
            var command = new UpdateEventCommand(id, request);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }


        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete an event",
            Description = "This API deletes an event. It requires Admin or Staff role access",
            OperationId = "DeleteEvent",
            Tags = new[] { "CMS", "CMS_Events" }
        )]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var command = new DeleteEventCommand(id);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
