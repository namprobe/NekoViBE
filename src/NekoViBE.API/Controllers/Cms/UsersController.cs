using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.User.Commands.CreateUser;
using NekoViBE.Application.Features.User.Commands.DeleteUser;
using NekoViBE.Application.Features.User.Queries.GetUser;
using NekoViBE.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;


namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/users")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_User")]
    [SwaggerTag("This API is used for User management in CMS")]

    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(PaginationResult<AppUser>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<AppUser>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<AppUser>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<AppUser>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
           Summary = "Get all users with pagination and filtering",
           Description = "This API retrieves a paginated list of users with filtering options",
           OperationId = "GetUserList",
           Tags = new[] { "CMS", "CMS_User" }
       )]
        public async Task<IActionResult> GetAllUsers()
        {
            var query = new GetUsersQuery();
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
           Summary = "Create a new user",
           Description = "This API creates a new user. It requires Admin role access",
           OperationId = "CreateU   ser",
           Tags = new[] { "CMS", "CMS_User" }
       )]
        public async Task<IActionResult> CreateUser([FromForm] CreateUserCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var command = new DeleteUserCommand(id);
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }

            return Ok(result);
        }

        //[HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> GetUserById(Guid id)
        //{
        //    var query = new GetUserByIdQuery(id);
        //    var result = await _mediator.Send(query);
        //    return StatusCode(result.GetHttpStatusCode(), result);
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var result = await _mediator.Send(command);
        //    return StatusCode(result.GetHttpStatusCode(), result);
        //}

        //[HttpPut]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand command)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var result = await _mediator.Send(command);
        //    return StatusCode(result.GetHttpStatusCode(), result);
        //}

        //[HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> DeleteUser(Guid id)
        //{
        //    var command = new DeleteUserCommand(id);
        //    var result = await _mediator.Send(command);
        //    return StatusCode(result.GetHttpStatusCode(), result);
        //}
    }
}
