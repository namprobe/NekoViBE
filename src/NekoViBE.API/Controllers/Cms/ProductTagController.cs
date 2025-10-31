using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.Models;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Features.ProductTag.Commands.CreateProductTag;
using NekoViBE.Application.Features.ProductTag.Commands.UpdateProductTag;
using NekoViBE.Application.Features.ProductTag.Commands.DeleteProductTag;
using NekoViBE.Application.Features.ProductTag.Queries.GetProductTagList;
using NekoViBE.Application.Features.ProductTag.Queries.GetProductTag;
using Microsoft.AspNetCore.Authorization;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.DTOs.ProductTag;

namespace NekoViBE.API.Controllers.Cms;

/// <summary>
/// Controller for managing ProductTags in CMS
/// </summary>
[ApiController]
[Route("api/cms/product-tags")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_ProductTags")]
[SwaggerTag("This API is used for ProductTag management in CMS")]
public class ProductTagController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductTagController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all ProductTags with pagination and filtering
    /// </summary>
    /// <remarks>
    /// API này trả về danh sách ProductTags phân trang với các tùy chọn lọc.
    /// Yêu cầu quyền Admin hoặc Staff.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/product-tags?page=1&pageSize=10&productId=123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="filter">ProductTag filter parameters</param>
    /// <returns>Paginated list of ProductTags</returns>
    [HttpGet]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(PaginationResult<ProductTagItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<ProductTagItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<ProductTagItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<ProductTagItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all ProductTags with pagination and filtering",
        Description = "This API retrieves a paginated list of ProductTags with filtering options",
        OperationId = "GetProductTags",
        Tags = new[] { "CMS", "CMS_ProductTags" }
    )]
    public async Task<IActionResult> GetProductTags([FromQuery] ProductTagFilter filter)
    {
        var query = new GetProductTagsQuery(filter);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get ProductTag by ID
    /// </summary>
    /// <remarks>
    /// API này trả về thông tin chi tiết của một ProductTag theo ID.
    /// Yêu cầu quyền Admin hoặc Staff.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/product-tags/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="id">ProductTag ID</param>
    /// <returns>ProductTag details</returns>
    [HttpGet("{id}")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result<ProductTagResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProductTagResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProductTagResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProductTagResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ProductTagResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get ProductTag by ID",
        Description = "This API retrieves details of a specific ProductTag by its ID",
        OperationId = "GetProductTag",
        Tags = new[] { "CMS", "CMS_ProductTags" }
    )]
    public async Task<IActionResult> GetProductTag(Guid id)
    {
        var query = new GetProductTagQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new ProductTag
    /// </summary>
    /// <remarks>
    /// API này tạo một ProductTag mới. Yêu cầu quyền Admin.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/cms/product-tags
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "productId": "123e4567-e89b-12d3-a456-426614174000",
    ///        "tagId": "456e7890-e89b-12d3-a456-426614174000",
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="request">ProductTag creation request</param>
    /// <returns>Creation result</returns>
    [HttpPost]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new ProductTag",
        Description = "This API creates a new ProductTag. Requires Admin role",
        OperationId = "CreateProductTag",
        Tags = new[] { "CMS", "CMS_ProductTags" }
    )]
    public async Task<IActionResult> CreateProductTag([FromForm] ProductTagRequest request)
    {
        var command = new CreateProductTagCommand(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Update an existing ProductTag
    /// </summary>
    /// <remarks>
    /// API này cập nhật thông tin của một ProductTag hiện có. Yêu cầu quyền Admin.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/cms/product-tags/123e4567-e89b-12d3-a456-426614174000
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "productId": "123e4567-e89b-12d3-a456-426614174000",
    ///        "tagId": "789e0123-e89b-12d3-a456-426614174000",
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="id">ProductTag ID</param>
    /// <param name="request">ProductTag update request</param>
    /// <returns>Update result</returns>
    [HttpPut("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update an existing ProductTag",
        Description = "This API updates an existing ProductTag. Requires Admin role",
        OperationId = "UpdateProductTag",
        Tags = new[] { "CMS", "CMS_ProductTags" }
    )]
    public async Task<IActionResult> UpdateProductTag(Guid id, [FromForm] ProductTagRequest request)
    {
        var command = new UpdateProductTagCommand(id, request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a ProductTag
    /// </summary>
    /// <remarks>
    /// API này xóa một ProductTag. Yêu cầu quyền Admin.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/cms/product-tags/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="id">ProductTag ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete a ProductTag",
        Description = "This API deletes a ProductTag. Requires Admin role",
        OperationId = "DeleteProductTag",
        Tags = new[] { "CMS", "CMS_ProductTags" }
    )]
    public async Task<IActionResult> DeleteProductTag(Guid id)
    {
        var command = new DeleteProductTagCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }
}