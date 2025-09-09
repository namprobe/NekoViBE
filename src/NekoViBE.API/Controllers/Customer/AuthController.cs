using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Features.Auth.Commands.Login;
using NekoViBE.Application.Features.Auth.Commands.Logout;
using NekoViBE.Application.Features.Auth.Commands.Register;
using NekoViBE.API.Attributes;
using NekoViBE.Domain.Enums;
using NekoViBE.Application.Features.Auth.Queries.GetProfile;
using NekoViBE.Application.Features.Auth.Commands.VerifyOtp;
using NekoViBE.Application.Features.Auth.Commands.ResetPassword;


namespace NekoViBE.API.Controllers.Customer;

/// <summary>
/// Controller quản lý xác thực cho website Customer
/// </summary>
[ApiController]
[Route("api/customer/auth")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Customer", "Customer_Auth")]
[SwaggerTag("This API is used for Authentication for Customer website")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login to the Customer website
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/customer/auth/login
    ///     {
    ///        "email": "user@example.com",
    ///        "password": "User@123",
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
    /// <response code="500">Login failed (internal server error)</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Login to the Customer website",
        Description = "This API is used for Authentication for Customer website",
        OperationId = "Login",
        Tags = new[] { "Customer", "Customer_Auth" }
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
    /// Logout from the customer website
    /// </summary>
    /// <remarks>
    /// This API is used for Logging out from the customer website. It will clear the refresh token and refresh token expiry time in the database.
    /// Need access token in the header.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/customer/auth/logout
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Logout successfully</returns>
    /// <response code="200">Logout successfully</response>
    /// <response code="401">Logout failed (not authorized)</response>
    /// <response code="500">Logout failed (internal server error)</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Logout from the Customer website",
        Description = "This API is used for Logging out from the Customer website",
        OperationId = "Logout",
        Tags = new[] { "Customer", "Customer_Auth" }
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
    /// Register a new customer
    /// </summary>
    /// <remarks>
    /// This API is used for Registering a new customer. It will cache an OTP code and send it to the user's email or phone number for verification.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/customer/auth/register
    ///     Content-Type: multipart/form-data
    ///     
    /// Form fields (camelCase naming):
    /// - email (required): Email address
    /// - password (required): Password (min 6 characters)
    /// - confirmPassword (required): Password confirmation
    /// - firstName (required): First name (max 50 characters)
    /// - lastName (required): Last name (max 50 characters)
    /// - phoneNumber (required): Phone number (10-11 digits)
    /// - gender (optional): Gender (0=Male, 1=Female, 2=Other)
    /// - dateOfBirth (optional): Date of birth (YYYY-MM-DD format)
    /// 
    /// Note: Use camelCase for form field names to maintain consistency with React client naming conventions.
    /// </remarks>
    /// <response code="200">Register successfully</response>
    /// <response code="400">Register failed (validation error)</response>
    /// <response code="500">Register failed (internal server error)</response>
    [HttpPost("register")]
    [SkipModelValidation]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Register a new customer",
        Description = "This API is used for Registering a new customer",
        OperationId = "Register",
        Tags = new[] { "Customer", "Customer_Auth" }
    )]
    public async Task<IActionResult> Register(
        [FromForm(Name = "email")] string email,
        [FromForm(Name = "password")] string password,
        [FromForm(Name = "confirmPassword")] string confirmPassword,
        [FromForm(Name = "firstName")] string firstName,
        [FromForm(Name = "lastName")] string lastName,
        [FromForm(Name = "phoneNumber")] string phoneNumber,
        [FromForm(Name = "gender")] GenderEnum? gender = null,
        [FromForm(Name = "dateOfBirth")] DateTime? dateOfBirth = null)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Gender = gender,
            DateOfBirth = dateOfBirth,
        };

        var command = new RegisterCommand(request);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Reset password
    /// </summary>
    /// <remarks>
    /// This API is used for Resetting password. It will cache an OTP code and send it to the user's email or phone number for verification.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/customer/auth/reset-password
    ///     {
    ///        "contact": "user@example.com",
    ///        "newPassword": "User@123",
    ///        "otpSentChannel": 1
    ///     }
    /// 
    /// `otpSentChannel` default is 1 (Email), 2 (Phone). 
    /// `newPassword` is required
    /// `contact` is required
    /// </remarks>
    /// <response code="200">Reset password successfully</response>
    /// <response code="400">Reset password failed (validation error)</response>
    /// <response code="500">Reset password failed (internal server error)</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Reset password",
        Description = "This API is used for Resetting password",
        OperationId = "ResetPassword",
        Tags = new[] { "Customer", "Customer_Auth" }
    )]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Verify OTP for registration or password reset
    /// </summary>
    /// <remarks>
    /// This API is used for Verifying OTP for registration or password reset. It will verify the OTP code and register the user or reset the password.
    /// Sample request:
    /// 
    ///     POST /api/customer/auth/verify-otp
    ///     {
    ///        "contact": "user@example.com",
    ///        "otp": "123456",
    ///        "otpType": 1
    ///        "otpSentChannel": 1
    ///     }
    /// 
    /// `otpType` default is 1 (Registration), 2 (Password Reset)
    /// `otpSentChannel` default is 1 (Email), 2 (Phone)
    /// </remarks>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Verify OTP for registration",
        Description = "This API is used for Verifying OTP for registration",
        OperationId = "VerifyOtp",
        Tags = new[] { "Customer", "Customer_Auth" }
    )]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var command = new VerifyOtpCommand(request);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }
    
    /// <summary>
    /// Get profile of the logged-in user in customer website
    /// </summary>
    /// <remarks>
    /// This API retrieves the profile information of the currently authenticated user.
    /// It requires a valid access token in the request header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/customer/auth/profile
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>customer profile information</returns>
    /// <response code="200">Profile retrieved successfully</response>
    /// <response code="401">Failed to retrieve profile (not authorized)</response>
    /// <response code="403">No access (user is not a customer)</response>
    /// <response code="500">Failed to retrieve profile (internal server error)</response>
    [HttpGet("profile")]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProfileResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get profile of the logged-in user in customer website",
        Description = "This API retrieves the profile information of the currently customer authenticated user",
        OperationId = "GetProfile",
        Tags = new[] { "Customer", "Customer_Auth" }
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
}