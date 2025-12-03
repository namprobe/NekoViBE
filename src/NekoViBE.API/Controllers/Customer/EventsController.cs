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


namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/events")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_Events")]
    [SwaggerTag("This API is used for Event management in Customer")]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<EventItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<EventItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<EventItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<EventItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all events with pagination and filtering",
            Description = "This API retrieves a paginated list of events with filtering options",
            OperationId = "GetEventList",
            Tags = new[] { "Customer", "Customer_Events" }
        )]
        public async Task<IActionResult> GetEventList([FromQuery] EventFilter filter)
        {
            var query = new GetEventListQuery(filter);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<EventResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get event by ID",
            Description = "This API retrieves a specific event by its ID",
            OperationId = "GetEvent",
            Tags = new[] { "Customer", "Customer_Events" }
        )]
        public async Task<IActionResult> GetEvent(Guid id)
        {
            var query = new GetEventQuery(id);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
