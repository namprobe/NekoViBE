using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Features.ShippingMethod.Commands.CreateShippingMethod;
using NekoViBE.Application.Features.ShippingMethod.Commands.UpdateShippingMethod;
using NekoViBE.Application.Features.ShippingMethod.Commands.DeleteShippingMethod;
using NekoViBE.Application.Features.ShippingMethod.Queries.GetShippingMethods;
using NekoViBE.Application.Features.ShippingMethod.Queries.GetShippingMethod;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Cms;

/// <summary>
/// Controller quản lý Shipping Methods cho CMS
/// </summary>
[ApiController]
[Route("api/cms/shipping-methods")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_ShippingMethods")]
[SwaggerTag("This API is used for Shipping Methods management in CMS")]
public class ShippingMethodController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingMethodController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all shipping methods with pagination and filtering
    /// </summary>
    /// <remarks>
    /// This API retrieves a paginated list of shipping methods with filtering options.
    /// It requires Admin or Staff role access.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/shipping-methods?page=1&amp;pageSize=10&amp;search=Express&amp;minCost=0&amp;maxCost=100&amp;sortBy=name&amp;isAscending=true
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="filter">Shipping method filter parameters</param>
    /// <returns>Paginated list of shipping methods</returns>
    /// <response code="200">Shipping methods retrieved successfully</response>
    /// <response code="401">Failed to retrieve shipping methods (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to retrieve shipping methods (internal server error)</response>
    [HttpGet]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(PaginationResult<ShippingMethodItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<ShippingMethodItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<ShippingMethodItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<ShippingMethodItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all shipping methods with pagination and filtering",
        Description = "This API retrieves a paginated list of shipping methods with filtering options",
        OperationId = "GetShippingMethods",
        Tags = new[] { "CMS", "CMS_ShippingMethods" }
    )]
    public async Task<IActionResult> GetShippingMethods([FromQuery] ShippingMethodFilter filter)
    {
        var query = new GetShippingMethodsQuery(filter);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get shipping method by ID
    /// </summary>
    /// <remarks>
    /// This API retrieves a specific shipping method by its ID.
    /// It requires Admin or Staff role access.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/shipping-methods/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Shipping method ID</param>
    /// <returns>Shipping method details</returns>
    /// <response code="200">Shipping method retrieved successfully</response>
    /// <response code="401">Failed to retrieve shipping method (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="404">Shipping method not found</response>
    /// <response code="500">Failed to retrieve shipping method (internal server error)</response>
    [HttpGet("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result<ShippingMethodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ShippingMethodResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ShippingMethodResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ShippingMethodResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ShippingMethodResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get shipping method by ID",
        Description = "This API retrieves a specific shipping method by its ID",
        OperationId = "GetShippingMethod",
        Tags = new[] { "CMS", "CMS_ShippingMethods" }
    )]
    public async Task<IActionResult> GetShippingMethod(Guid id)
    {
        var query = new GetShippingMethodQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new shipping method
    /// </summary>
    /// <remarks>
    /// This API creates a new shipping method. It requires Admin role access.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/cms/shipping-methods
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "name": "Express Delivery",
    ///        "description": "Fast delivery within 1-2 days",
    ///        "cost": 50000,
    ///        "estimatedDays": 2,
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="request">Shipping method creation data</param>
    /// <returns>Creation result</returns>
    /// <response code="200">Shipping method created successfully</response>
    /// <response code="400">Creation failed (validation error)</response>
    /// <response code="401">Failed to create shipping method (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="500">Failed to create shipping method (internal server error)</response>
    [HttpPost]
    [AuthorizeRoles("Admin")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new shipping method",
        Description = "This API creates a new shipping method. It requires Admin role access",
        OperationId = "CreateShippingMethod",
        Tags = new[] { "CMS", "CMS_ShippingMethods" }
    )]
    public async Task<IActionResult> CreateShippingMethod([FromBody] ShippingMethodRequest request)
    {
        var command = new CreateShippingMethodCommand(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Update an existing shipping method
    /// </summary>
    /// <remarks>
    /// This API updates an existing shipping method. It requires Admin role access.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/cms/shipping-methods/123e4567-e89b-12d3-a456-426614174000
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "name": "Express Delivery Updated",
    ///        "description": "Fast delivery within 1-2 days with tracking",
    ///        "cost": 55000,
    ///        "estimatedDays": 1,
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Shipping method ID</param>
    /// <param name="request">Shipping method update data</param>
    /// <returns>Update result</returns>
    /// <response code="200">Shipping method updated successfully</response>
    /// <response code="400">Update failed (validation error)</response>
    /// <response code="401">Failed to update shipping method (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="404">Shipping method not found</response>
    /// <response code="500">Failed to update shipping method (internal server error)</response>
    [HttpPut("{id}")]
    [AuthorizeRoles("Admin")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update an existing shipping method",
        Description = "This API updates an existing shipping method. It requires Admin role access",
        OperationId = "UpdateShippingMethod",
        Tags = new[] { "CMS", "CMS_ShippingMethods" }
    )]
    public async Task<IActionResult> UpdateShippingMethod(Guid id, [FromBody] ShippingMethodRequest request)
    {
        var command = new UpdateShippingMethodCommand(id, request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a shipping method
    /// </summary>
    /// <remarks>
    /// This API deletes a shipping method. It requires Admin role access.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/cms/shipping-methods/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Shipping method ID</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Shipping method deleted successfully</response>
    /// <response code="400">Deletion failed (shipping method in use)</response>
    /// <response code="401">Failed to delete shipping method (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="404">Shipping method not found</response>
    /// <response code="500">Failed to delete shipping method (internal server error)</response>
    [HttpDelete("{id}")]
    [AuthorizeRoles("Admin")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete a shipping method",
        Description = "This API deletes a shipping method. It requires Admin role access",
        OperationId = "DeleteShippingMethod",
        Tags = new[] { "CMS", "CMS_ShippingMethods" }
    )]
    public async Task<IActionResult> DeleteShippingMethod(Guid id)
    {
        var command = new DeleteShippingMethodCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }
}
