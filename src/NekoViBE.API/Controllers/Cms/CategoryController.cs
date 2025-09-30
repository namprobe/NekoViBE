using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Category.Commands.CreateCategory;
using NekoViBE.Application.Features.Category.Commands.DeleteCategory;
using NekoViBE.Application.Features.Category.Commands.UpdateCategory;
using NekoViBE.Application.Features.Category.Queries.GetCategory;
using NekoViBE.Application.Features.Category.Queries.GetCategoryList;
using NekoViBE.Application.Features.Category.Queries.GetSelectList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/categories")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_Category")]
    [SwaggerTag("This API is used for Category management in CMS")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(PaginationResult<CategoryItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<CategoryItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<CategoryItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<CategoryItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all categories with pagination and filtering",
            Description = "This API retrieves a paginated list of categories with filtering options",
            OperationId = "GetCategoryList",
            Tags = new[] { "CMS", "CMS_Category" }
        )]
        public async Task<IActionResult> GetCategoryList([FromQuery] CategoryFilter filter)
        {
            var query = new GetCategoryListQuery(filter);
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<CategoryResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<CategoryResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<CategoryResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<CategoryResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get category by ID",
            Description = "This API retrieves a specific category by its ID",
            OperationId = "GetCategory",
            Tags = new[] { "CMS", "CMS_Category" }
        )]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            var query = new GetCategoryQuery(id);
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPost]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create a new category",
            Description = "This API creates a new category. It requires Admin role access",
            OperationId = "CreateCategory",
            Tags = new[] { "CMS", "CMS_Category" }
        )]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryRequest request)
        {
            var command = new CreateCategoryCommand(request);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPut("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Update an existing category",
            Description = "This API updates an existing category. It requires Admin role access",
            OperationId = "UpdateCategory",
            Tags = new[] { "CMS", "CMS_Category" }
        )]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromForm] CategoryRequest request)
        {
            var command = new UpdateCategoryCommand(id, request);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete a category",
            Description = "This API deletes a category. It requires Admin role access",
            OperationId = "DeleteCategory",
            Tags = new[] { "CMS", "CMS_Category" }
        )]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var command = new DeleteCategoryCommand(id);
            var result = await _mediator.Send(command);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("select-list")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(List<CategorySelectItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategorySelectList([FromQuery] string? search)
        {
            var query = new GetCategorySelectListQuery { Search = search };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

    }
}
