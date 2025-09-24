using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Features.Auth.Commands.Login;
using NekoViBE.Application.Features.Auth.Commands.Logout;
using Microsoft.AspNetCore.Authorization;
using NekoViBE.Application.Features.Auth.Queries.GetProfile;
using NekoViBE.Application.Features.Auth.Commands.RefreshToken;

namespace NekoViBE.API.Controllers.Cms;

/// <summary>
/// Controller quản lý xác thực cho website CMS
/// </summary>
[ApiController]
[Route("api/cms/auth")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_Auth")]
[SwaggerTag("This API is used for Authentication for CMS website")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login to the CMS system
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/cms/auth/login
    ///     {
    ///        "email": "admin@example.com",
    ///        "password": "Admin@123",
    ///        "grantType": 0
    ///     }
    ///     
    /// `grantType` default is 0 (Password)
    /// </remarks>
    /// <param name="request">Login request</param>
    /// <returns>User information and authentication token</returns>
    /// <response code="200">Login successfully</response>
    /// <response code="400">Login failed (validation error)</response>
    /// <response code="401">Login failed (email or password is incorrect)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    [HttpPost("login")]
    [ServiceFilter(typeof(AdminRoleAccessFilter))]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Login to the CMS system",
        Description = "This API is used for Authentication for CMS website",
        OperationId = "Login",
        Tags = new[] { "CMS", "CMS_Auth" }
    )]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Logout from the CMS system
    /// </summary>
    /// <remarks>
    /// This API is used for Logging out from the CMS website. It will clear the refresh token and refresh token expiry time in the database.
    /// Need access token in the header.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/cms/auth/logout
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Logout successfully</returns>
    /// <response code="200">Logout successfully</response>
    /// <response code="401">Logout failed (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    [HttpPost("logout")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Logout from the CMS system",
        Description = "This API is used for Logging out from the CMS website",
        OperationId = "Logout",
        Tags = new[] { "CMS", "CMS_Auth" }
    )]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }
    
    /// <summary>
    /// Get profile of the logged-in user in cms system
    /// </summary>
    /// <remarks>
    /// This API retrieves the profile information of the currently authenticated user.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/auth/profile
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>admin or staff profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">Failed to retrieve profile (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to retrieve profile (internal server error)</response>
    [HttpGet("profile")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get profile of the logged-in user in cms system",
        Description = "This API retrieves the profile information of the currently cms authenticated user",
        OperationId = "GetProfile",
        Tags = new[] { "CMS", "CMS_Auth" }
    )]
    public async Task<IActionResult> GetProfile()
    {
        var query = new GetProfileQuery();
        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Refresh token of the logged-in user in cms system
    /// </summary>
    /// <remarks>
    /// This API refesh access token of the currently cms authenticated user.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/auth/refresh-token
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>refresh token for admin or staff</returns>
    /// <response code="200">Refresh token successfully</response>
    /// <response code="401">Failed to refresh token (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to refresh token (internal server error)</response>
    [HttpPost("refresh-token")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Refresh token for the logged-in user in cms system",
        Description = "This API refesh access token of the currently authenticated cms user",
        OperationId = "RefreshToken",
        Tags = new[] { "CMS", "CMS_Auth" }
    )]
    public async Task<IActionResult> RefreshToken()
    {
        var query = new RefreshTokenCommand();
        var result = await _mediator.Send(query);
        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }
}