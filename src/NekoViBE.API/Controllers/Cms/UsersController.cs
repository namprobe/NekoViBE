using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Features.User.Commands.CreateUser;
using NekoViBE.Application.Features.User.Queries.GetUser;


namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var query = new GetUsersQuery();
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
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
