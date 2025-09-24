using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Badge;
using NekoViBE.Application.Common.DTOs.UserBadge;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Badge.Command.CreateBadge;
using NekoViBE.Application.Features.Badge.Command.DeleteBadge;
using NekoViBE.Application.Features.Badge.Command.UpdateBadge;
using NekoViBE.Application.Features.Badge.Queries.GetBadeById;
using NekoViBE.Application.Features.Badge.Queries.GetBadge;
using NekoViBE.Application.Features.UserBadge.Command.AssignBadge;
using NekoViBE.Application.Features.UserBadge.Command.RemoveBadgeFromUser;
using NekoViBE.Application.Features.UserBadge.Queries.GetUserBadge;
using Swashbuckle.AspNetCore.Annotations;



namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/badge")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_Badge")]
    [SwaggerTag("This API is used for Badge management in CMS")]
    public class BadgesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BadgesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
                Summary = "Get all badges",
                Description = "This API retrieve all badges. It requires Admin role access",
                OperationId = "GetAllBadges",
                Tags = new[] { "CMS", "CMS_Badge" }
            )]
        public async Task<IActionResult> GetAllBadges()
        {
            var query = new GetBadgesQuery();
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
        }



        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
        Summary = "Get badge from id",
        Description = "This API get badge from id. It requires Admin role access",
        OperationId = "GetBadgeById",
        Tags = new[] { "CMS", "CMS_Badge" }
        )]

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBadgeById(Guid id)
        {
            var query = new GetBadgeByIdQuery(id);
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
                Summary = "Create a new badge",
                Description = "This API creates a new badge. It requires Admin role access",
                OperationId = "CreateBadge",
                Tags = new[] { "CMS", "CMS_Badge" }
            )]
        public async Task<IActionResult> CreateBadge([FromForm] CreateBadgeRequest request)
        {
            var createCommand = new CreateBadgeCommand(request);
            var result = await _mediator.Send(createCommand);
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
                Summary = "Update a new badge",
                Description = "This API updates a new badge. It requires Admin role access",
                OperationId = "UpdateBadge",
                Tags = new[] { "CMS", "CMS_Badge" }
            )]
        public async Task<IActionResult> UpdateBadge(Guid id, [FromForm] UpdateBadgeRequest request)
        {
            var command = new UpdateBadgeCommand(id, request);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }




        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
                Summary = "Delete a badge",
                Description = "This API deletes a badge. It requires Admin role access",
                OperationId = "DeleteBadge",
                Tags = new[] { "CMS", "CMS_Badge" }
            )]

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBadge(Guid id)
        {
            var command = new DeleteBadgeCommand(id);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }


        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
        Summary = "Get user badge by id",
        Description = "This API retrives user's badge. It requires Admin role access",
        OperationId = "GetUserBadges",
        Tags = new[] { "CMS", "CMS_Badge" }
    )]

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBadges(Guid userId)
        {
            var query = new GetUserBadgesQuery(userId);
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
        }



        [HttpPost("assign")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
                Summary = "Assign badge to user",
                Description = "This API assign for user a new badge. It requires Admin role access",
                OperationId = "AssignBadge",
                Tags = new[] { "CMS", "CMS_Badge" }
            )]
        public async Task<IActionResult> AssignBadgeToUser([FromBody] AssignBadgeToUserRequest request)
        {
            var command = new AssignBadgeToUserCommand(request);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }


        [HttpDelete("user-badge/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
                Summary = "Remove badge to user",
                Description = "This API remove badge from user. It requires Admin role access",
                OperationId = "RemoveBadge",
                Tags = new[] { "CMS", "CMS_Badge" }
            )]
        
        public async Task<IActionResult> RemoveBadgeFromUser(Guid id)
        {
            var command = new RemoveBadgeFromUserCommand(id);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}