using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Auth.Commands.Role;
using NekoViBE.Application.Features.Auth.Queries.GetProfile;
using NekoViBE.Application.Features.Auth.Queries.GetRole;
using Swashbuckle.AspNetCore.Annotations;




namespace NekoViBE.API.Controllers.Cms
{

    [ApiController]
    [Route("api/cms/role")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_Role")]
    [SwaggerTag("This API is used for Role for CMS website")]
    public class RoleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoleController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("role")]
        //[AuthorizeRoles("Customer")]
        [ProducesResponseType(typeof(Result<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<RoleResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<RoleResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<RoleResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
        Summary = "Get all role of the website",
        Description = "This API retrieves the roles information",
        OperationId = "GetRole",
        Tags = new[] { "Role", "Role_caiGiDo" }
    )]
        public async Task<IActionResult> GetAllRoles()
        {
            var query = new GetRoleQuery();
            var result = await _mediator.Send(query);
            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")] // Restrict to admin users only
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
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

            return CreatedAtAction(nameof(GetAllRoles), result);
        }

    }



}
