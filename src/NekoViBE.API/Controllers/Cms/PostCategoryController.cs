// NekoViBE.API.Controllers.Cms/PostCategoryController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.PostCategory.Commands.CreatePostCategory;
using NekoViBE.Application.Features.PostCategory.Commands.DeletePostCategory;
using NekoViBE.Application.Features.PostCategory.Commands.UpdatePostCategory;
using NekoViBE.Application.Features.PostCategory.Queries.GetPostCategory;
using NekoViBE.Application.Features.PostCategory.Queries.GetPostCategoryList;
using NekoViBE.Application.Features.PostCategory.Queries.GetSelectList;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Cms;

[ApiController]
[Route("api/cms/post-categories")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_PostCategory")]
[SwaggerTag("This API is used for Post Category management in CMS")]
public class PostCategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostCategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(PaginationResult<PostCategoryItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<PostCategoryItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<PostCategoryItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<PostCategoryItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all post categories with pagination and filtering",
        Description = "This API retrieves a paginated list of post categories with filtering options",
        OperationId = "GetPostCategoryList",
        Tags = new[] { "CMS", "CMS_PostCategory" }
    )]
    public async Task<IActionResult> GetPostCategoryList([FromQuery] PostCategoryFilter filter)
    {
        var query = new GetPostCategoryListQuery(filter);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result<PostCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PostCategoryResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<PostCategoryResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<PostCategoryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<PostCategoryResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get post category by ID",
        Description = "This API retrieves a specific post category by its ID",
        OperationId = "GetPostCategory",
        Tags = new[] { "CMS", "CMS_PostCategory" }
    )]
    public async Task<IActionResult> GetPostCategory(Guid id)
    {
        var query = new GetPostCategoryQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    [HttpPost]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new post category",
        Description = "This API creates a new post category. Requires Admin/Staff role",
        OperationId = "CreatePostCategory",
        Tags = new[] { "CMS", "CMS_PostCategory" }
    )]
    public async Task<IActionResult> CreatePostCategory([FromForm] PostCategoryRequest request)
    {
        var command = new CreatePostCategoryCommand(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    [HttpPut("{id}")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update an existing post category",
        Description = "This API updates an existing post category. Requires Admin/Staff role",
        OperationId = "UpdatePostCategory",
        Tags = new[] { "CMS", "CMS_PostCategory" }
    )]
    public async Task<IActionResult> UpdatePostCategory(Guid id, [FromForm] PostCategoryRequest request)
    {
        var command = new UpdatePostCategoryCommand(id, request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete a post category",
        Description = "This API deletes a post category. Requires Admin/Staff role",
        OperationId = "DeletePostCategory",
        Tags = new[] { "CMS", "CMS_PostCategory" }
    )]
    public async Task<IActionResult> DeletePostCategory(Guid id)
    {
        var command = new DeletePostCategoryCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    [HttpGet("select-list")]
    [ProducesResponseType(typeof(List<PostCategorySelectItem>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get post categories for select list",
        Description = "Used for dropdowns in CMS",
        OperationId = "GetPostCategorySelectList",
        Tags = new[] { "CMS", "CMS_PostCategory" }
    )]
    public async Task<IActionResult> GetPostCategorySelectList([FromQuery] string? search)
    {
        var query = new GetPostCategorySelectListQuery { Search = search };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}