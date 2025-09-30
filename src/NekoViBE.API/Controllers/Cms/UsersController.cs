using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Enums;
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
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, ILogger<UsersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
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


        [HttpGet]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(PaginationResult<UserItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<UserItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<UserItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<UserItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
        Summary = "Get all users with pagination and filtering",
        Description = "This API retrieves a paginated list of users with filtering options",
        OperationId = "GetUserList",
        Tags = new[] { "CMS", "CMS_User" }
        )]
        public async Task<IActionResult> GetUserList([FromQuery] UserFilter filter, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for GetUserList: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(Result.Failure("Invalid request parameters", ErrorCodeEnum.InvalidInput));
            }

            _logger.LogInformation("Retrieving user list with filter: {@Filter}", filter);
            var query = new GetUserListQuery(filter);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve user list: {Error}", result.Errors);
                return StatusCode(result.GetHttpStatusCode(), result);
            }

            _logger.LogInformation("User list retrieved successfully, TotalItems: {TotalItems}", result.TotalItems);
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
