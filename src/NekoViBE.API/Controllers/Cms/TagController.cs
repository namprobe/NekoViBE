using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Tag.Commands.CreateTag;
using NekoViBE.Application.Features.Tag.Commands.DeleteTag;
using NekoViBE.Application.Features.Tag.Commands.UpdateTag;
using NekoViBE.Application.Features.Tag.Queries.GetSelectList;
using NekoViBE.Application.Features.Tag.Queries.GetTag;
using NekoViBE.Application.Features.Tag.Queries.GetTagList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Cms;

/// <summary>
/// Controller for managing Tags in CMS
/// </summary>
[ApiController]
[Route("api/cms/tags")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_Tags")]
[SwaggerTag("This API is used for Tag management in CMS")]
public class TagController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all Tags with pagination and filtering
    /// </summary>
    /// <remarks>
    /// API này trả về danh sách Tags phân trang với các tùy chọn lọc.
    /// Yêu cầu quyền Admin hoặc Staff.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/tags?page=1&pageSize=10&search=electronics
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="filter">Tag filter parameters</param>
    /// <returns>Paginated list of Tags</returns>
    [HttpGet]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(PaginationResult<TagItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<TagItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<TagItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<TagItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all Tags with pagination and filtering",
        Description = "This API retrieves a paginated list of Tags with filtering options",
        OperationId = "GetTags",
        Tags = new[] { "CMS", "CMS_Tags" }
    )]
    public async Task<IActionResult> GetTags([FromQuery] TagFilter filter)
    {
        var query = new GetTagsQuery(filter);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get Tag by ID
    /// </summary>
    /// <remarks>
    /// API này trả về thông tin chi tiết của một Tag theo ID.
    /// Yêu cầu quyền Admin hoặc Staff.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/tags/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="id">Tag ID</param>
    /// <returns>Tag details</returns>
    [HttpGet("{id}")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result<TagResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<TagResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<TagResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<TagResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<TagResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get Tag by ID",
        Description = "This API retrieves details of a specific Tag by its ID",
        OperationId = "GetTag",
        Tags = new[] { "CMS", "CMS_Tags" }
    )]
    public async Task<IActionResult> GetTag(Guid id)
    {
        var query = new GetTagQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new Tag
    /// </summary>
    /// <remarks>
    /// API này tạo một Tag mới. Yêu cầu quyền Admin.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/cms/tags
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "name": "Electronics",
    ///        "description": "Tag for electronic products",
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="request">Tag creation request</param>
    /// <returns>Creation result</returns>
    [HttpPost]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new Tag",
        Description = "This API creates a new Tag. Requires Admin role",
        OperationId = "CreateTag",
        Tags = new[] { "CMS", "CMS_Tags" }
    )]
    public async Task<IActionResult> CreateTag([FromForm] TagRequest request)
    {
        var command = new CreateTagCommand(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Update an existing Tag
    /// </summary>
    /// <remarks>
    /// API này cập nhật thông tin của một Tag hiện có. Yêu cầu quyền Admin.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/cms/tags/123e4567-e89b-12d3-a456-426614174000
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "name": "Electronics Updated",
    ///        "description": "Updated tag for electronic products",
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="id">Tag ID</param>
    /// <param name="request">Tag update request</param>
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
        Summary = "Update an existing Tag",
        Description = "This API updates an existing Tag. Requires Admin role",
        OperationId = "UpdateTag",
        Tags = new[] { "CMS", "CMS_Tags" }
    )]
    public async Task<IActionResult> UpdateTag(Guid id, [FromForm] TagRequest request)
    {
        var command = new UpdateTagCommand(id, request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a Tag
    /// </summary>
    /// <remarks>
    /// API này xóa một Tag. Yêu cầu quyền Admin.
    /// Tag không thể xóa nếu đang được sử dụng bởi Product hoặc Post.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/cms/tags/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer <access_token>
    /// </remarks>
    /// <param name="id">Tag ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete a Tag",
        Description = "This API deletes a Tag. Requires Admin role",
        OperationId = "DeleteTag",
        Tags = new[] { "CMS", "CMS_Tags" }
    )]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        var command = new DeleteTagCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }


    [HttpGet("select-list")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(List<TagSelectItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<TagSelectItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(List<TagSelectItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(List<TagSelectItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get tags for select list",
        Description = "This API retrieves a list of tags for use in dropdowns or select lists",
        OperationId = "GetTagSelectList",
        Tags = new[] { "CMS", "CMS_Tag" }
    )]
    public async Task<IActionResult> GetTagSelectList([FromQuery] string? search)
    {
        var query = new GetTagSelectListQuery { Search = search };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}