using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.UserAddress.Queries.GetUserAddressById;
using NekoViBE.Application.Features.UserAddress.Queries.GetUserAddresses;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Cms;

/// <summary>
/// Controller quản lý User Addresses cho CMS (Read-only)
/// Admin chỉ được xem thông tin địa chỉ của user, không được sửa/xóa
/// </summary>
[ApiController]
[Route("api/cms/user-addresses")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_UserAddresses")]
[SwaggerTag("This API is used for User Address management in CMS (Read-only)")]
public class UserAddressController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserAddressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all user addresses with pagination and filtering (Admin view)
    /// </summary>
    /// <remarks>
    /// This API retrieves a paginated list of user addresses with filtering options.
    /// Admin can view all users' addresses for management purposes.
    /// **Note:** Admin CANNOT create, update, or delete user addresses (read-only access).
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/user-addresses?page=1&amp;pageSize=10&amp;userId=123e4567-e89b-12d3-a456-426614174000&amp;sortBy=createdDate&amp;isAscending=false
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="filter">User address filter parameters</param>
    /// <returns>Paginated list of user addresses</returns>
    /// <response code="200">User addresses retrieved successfully</response>
    /// <response code="401">Failed to retrieve user addresses (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to retrieve user addresses (internal server error)</response>
    [HttpGet]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(PaginationResult<UserAddressItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<UserAddressItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<UserAddressItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<UserAddressItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all user addresses with pagination and filtering (Admin view)",
        Description = "This API retrieves a paginated list of user addresses. Admin can view all users' addresses",
        OperationId = "CMS_GetUserAddresses",
        Tags = new[] { "CMS", "CMS_UserAddresses" }
    )]
    public async Task<IActionResult> GetUserAddresses([FromQuery] UserAddressFilter filter)
    {
        var query = new GetPagedUserAddressQuery(filter);
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get user address by ID (Admin view)
    /// </summary>
    /// <remarks>
    /// This API retrieves a specific user address by its ID.
    /// Admin can view any user's address for management purposes.
    /// **Note:** Admin CANNOT create, update, or delete user addresses (read-only access).
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/user-addresses/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">User address ID</param>
    /// <returns>User address details</returns>
    /// <response code="200">User address retrieved successfully</response>
    /// <response code="401">Failed to retrieve user address (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="404">User address not found</response>
    /// <response code="500">Failed to retrieve user address (internal server error)</response>
    [HttpGet("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<UserAddressDetail>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get user address by ID (Admin view)",
        Description = "This API retrieves a specific user address by its ID. Admin can view any user's address",
        OperationId = "CMS_GetUserAddress",
        Tags = new[] { "CMS", "CMS_UserAddresses" }
    )]
    public async Task<IActionResult> GetUserAddress(Guid id)
    {
        var query = new GetUserAddressByIdQuery(id);
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

