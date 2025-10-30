using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.UserAddress.Commands.CreateUserAddress;
using NekoViBE.Application.Features.UserAddress.Commands.DeleteUserAddress;
using NekoViBE.Application.Features.UserAddress.Commands.UpdateUserAddress;
using NekoViBE.Application.Features.UserAddress.Queries.GetUserAddressById;
using NekoViBE.Application.Features.UserAddress.Queries.GetUserAddresses;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer;

/// <summary>
/// Controller quản lý User Addresses cho Customer
/// </summary>
[ApiController]
[Route("api/customer/user-addresses")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Customer", "Customer_UserAddresses")]
[SwaggerTag("This API is used for User Address management by Customers")]
public class UserAddressController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserAddressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all user addresses with pagination and filtering
    /// </summary>
    /// <remarks>
    /// This API retrieves a paginated list of user addresses with filtering options.
    /// Customers can only view their own addresses.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/customer/user-addresses?page=1&amp;pageSize=10&amp;isCurrentUser=true&amp;sortBy=createdDate&amp;isAscending=false
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="filter">User address filter parameters</param>
    /// <returns>Paginated list of user addresses</returns>
    /// <response code="200">User addresses retrieved successfully</response>
    /// <response code="401">Failed to retrieve user addresses (not authorized)</response>
    /// <response code="403">No access (user is not a customer)</response>
    /// <response code="500">Failed to retrieve user addresses (internal server error)</response>
    [HttpGet]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(PaginationResult<UserAddressItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<UserAddressItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<UserAddressItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<UserAddressItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all user addresses with pagination and filtering",
        Description = "This API retrieves a paginated list of user addresses. Customers can only view their own addresses",
        OperationId = "Customer_GetUserAddresses",
        Tags = new[] { "Customer", "Customer_UserAddresses" }
    )]
    public async Task<IActionResult> GetUserAddresses([FromQuery] UserAddressFilter filter)
    {
        // Force isCurrentUser to true for customers (security)
        filter.IsCurrentUser = true;

        var query = new GetPagedUserAddressQuery(filter);
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get user address by ID
    /// </summary>
    /// <remarks>
    /// This API retrieves a specific user address by its ID.
    /// Customers can only view their own addresses.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/customer/user-addresses/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">User address ID</param>
    /// <returns>User address details</returns>
    /// <response code="200">User address retrieved successfully</response>
    /// <response code="401">Failed to retrieve user address (not authorized)</response>
    /// <response code="403">No access (user is not a customer)</response>
    /// <response code="404">User address not found</response>
    /// <response code="500">Failed to retrieve user address (internal server error)</response>
    [HttpGet("{id}")]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get user address by ID",
        Description = "This API retrieves a specific user address by its ID. Customers can only view their own addresses",
        OperationId = "Customer_GetUserAddress",
        Tags = new[] { "Customer", "Customer_UserAddresses" }
    )]
    public async Task<IActionResult> GetUserAddress(Guid id)
    {
        var query = new GetUserAddressByIdQuery(id);
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Create a new user address
    /// </summary>
    /// <remarks>
    /// This API creates a new user address. It requires Customer role access.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/customer/user-addresses
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "fullName": "Nguyen Van A",
    ///        "address": "123 Main Street, District 1",
    ///        "city": "Ho Chi Minh City",
    ///        "state": "Ho Chi Minh",
    ///        "postalCode": "700000",
    ///        "country": "Vietnam",
    ///        "phoneNumber": "0901234567",
    ///        "addressType": 1,
    ///        "isDefault": true,
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="request">User address creation request</param>
    /// <returns>Creation result</returns>
    /// <response code="200">User address created successfully</response>
    /// <response code="400">Creation failed (validation error)</response>
    /// <response code="401">Failed to create user address (not authorized)</response>
    /// <response code="403">No access (user is not a customer)</response>
    /// <response code="500">Failed to create user address (internal server error)</response>
    [HttpPost]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new user address",
        Description = "This API creates a new user address. It requires Customer role access",
        OperationId = "Customer_CreateUserAddress",
        Tags = new[] { "Customer", "Customer_UserAddresses" }
    )]
    public async Task<IActionResult> CreateUserAddress([FromBody] UserAddressRequest request)
    {
        var command = new CreateUserAddressCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update an existing user address
    /// </summary>
    /// <remarks>
    /// This API updates an existing user address. It requires Customer role access.
    /// Customers can only update their own addresses.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/customer/user-addresses/123e4567-e89b-12d3-a456-426614174000
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "fullName": "Nguyen Van A Updated",
    ///        "address": "456 Main Street, District 1",
    ///        "city": "Ho Chi Minh City",
    ///        "state": "Ho Chi Minh",
    ///        "postalCode": "700000",
    ///        "country": "Vietnam",
    ///        "phoneNumber": "0901234567",
    ///        "addressType": 1,
    ///        "isDefault": true,
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">User address ID</param>
    /// <param name="request">User address update request</param>
    /// <returns>Update result</returns>
    /// <response code="200">User address updated successfully</response>
    /// <response code="400">Update failed (validation error)</response>
    /// <response code="401">Failed to update user address (not authorized)</response>
    /// <response code="403">No access (user is not a customer or trying to update another user's address)</response>
    /// <response code="404">User address not found</response>
    /// <response code="500">Failed to update user address (internal server error)</response>
    [HttpPut("{id}")]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update an existing user address",
        Description = "This API updates an existing user address. Customers can only update their own addresses",
        OperationId = "Customer_UpdateUserAddress",
        Tags = new[] { "Customer", "Customer_UserAddresses" }
    )]
    public async Task<IActionResult> UpdateUserAddress(Guid id, [FromBody] UserAddressRequest request)
    {
        var command = new UpdateUserAddressCommand(id, request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Delete a user address
    /// </summary>
    /// <remarks>
    /// This API deletes a user address. It requires Customer role access.
    /// Customers can only delete their own addresses.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/customer/user-addresses/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">User address ID</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">User address deleted successfully</response>
    /// <response code="401">Failed to delete user address (not authorized)</response>
    /// <response code="403">No access (user is not a customer or trying to delete another user's address)</response>
    /// <response code="404">User address not found</response>
    /// <response code="500">Failed to delete user address (internal server error)</response>
    [HttpDelete("{id}")]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete a user address",
        Description = "This API deletes a user address. Customers can only delete their own addresses",
        OperationId = "Customer_DeleteUserAddress",
        Tags = new[] { "Customer", "Customer_UserAddresses" }
    )]
    public async Task<IActionResult> DeleteUserAddress(Guid id)
    {
        var command = new DeleteUserAddressCommand(id);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

